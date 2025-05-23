// Шлях: Assets/_MythHunter/Code/UI/Services/HeroCardService.cs

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Services
{
    /// <summary>
    /// Інтерфейс для сервісу управління картками героїв
    /// </summary>
    public interface IHeroCardService
    {
        /// <summary>
        /// Ініціалізує сервіс карток героїв
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// Створює картку героя
        /// </summary>
        /// <param name="model">Модель даних картки</param>
        /// <param name="parent">Батьківський трансформ</param>
        /// <param name="interactive">Чи буде картка інтерактивною</param>
        UniTask<HeroCardUI> CreateCardAsync(HeroCardModel model, Transform parent, bool interactive = true);

        /// <summary>
        /// Повертає картку в пул
        /// </summary>
        void ReturnCard(HeroCardUI card);

        /// <summary>
        /// Повертає набір карток в пул
        /// </summary>
        void ReturnAll(IEnumerable<HeroCardUI> cards);
    }

    /// <summary>
    /// Сервіс для управління картками героїв
    /// </summary>
    public class HeroCardService : IHeroCardService
    {
        private readonly IUIComponentFactory _componentFactory;
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IDIContainer _container;
        private readonly ISpriteService _spriteService;

        // ✅ Одна константа для пулу карток героїв
        private const string HERO_CARD_POOL_KEY = "HeroCardUI";
        private const int DEFAULT_POOL_SIZE = 25;

        private bool _initialized = false;
        private readonly object _initializationLock = new object();
        private readonly ViewId _heroCardViewId = ViewId.HeroCard;

        [Inject]
        public HeroCardService(
            IUIComponentFactory componentFactory,
            IPoolManager poolManager,
            IMythLogger logger,
            IViewConfigRegistry viewConfigRegistry,
            IDIContainer container,
            ISpriteService spriteService)
        {
            _componentFactory = componentFactory;
            _poolManager = poolManager;
            _logger = logger;
            _viewConfigRegistry = viewConfigRegistry;
            _container = container;
            _spriteService = spriteService;
        }

        /// <summary>
        /// Ініціалізує сервіс, створюючи пул карток героїв
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (_initialized)
            {
                _logger.LogInfo("HeroCardService вже ініціалізовано", nameof(HeroCardService));
                return;
            }

            lock (_initializationLock)
            {
                if (_initialized)
                    return;
                _initialized = true;
            }

            try
            {
                // ✅ Перевіряємо чи пул вже існує
                if (_poolManager.HasPool(HERO_CARD_POOL_KEY))
                {
                    _logger.LogWarning($"Пул {HERO_CARD_POOL_KEY} вже існує, пропускаємо створення", nameof(HeroCardService));
                    return;
                }

                // ✅ Створюємо пул через ViewConfig
                var config = _viewConfigRegistry.Get(_heroCardViewId);
                if (config == null)
                {
                    _logger.LogError($"Не знайдено конфігурацію для ViewId: {_heroCardViewId}", nameof(HeroCardService));
                    _initialized = false;
                    return;
                }

                // ✅ Створюємо префаб через фабрику компонентів
                var prefabGO = await _componentFactory.CreateComponentAsync(_heroCardViewId);
                if (prefabGO == null)
                {
                    _logger.LogError($"Не вдалося створити компонент для ViewId: {_heroCardViewId}", nameof(HeroCardService));
                    _initialized = false;
                    return;
                }

                // ✅ Створюємо пул
                _poolManager.CreatePool<GameObject>(HERO_CARD_POOL_KEY, prefabGO, DEFAULT_POOL_SIZE);
                _logger.LogInfo($"✅ HeroCardUI пул створено успішно ({DEFAULT_POOL_SIZE} штук)", nameof(HeroCardService));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка ініціалізації HeroCardService: {ex.Message}", nameof(HeroCardService), ex);
                lock (_initializationLock)
                {
                    _initialized = false;
                }
                throw;
            }
        }

        /// <summary>
        /// Створює картку героя з пулу
        /// </summary>
        public async UniTask<HeroCardUI> CreateCardAsync(HeroCardModel model, Transform parent, bool interactive = true)
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }

            if (!_poolManager.HasPool(HERO_CARD_POOL_KEY))
            {
                _logger.LogError($"Пул {HERO_CARD_POOL_KEY} не ініціалізовано", nameof(HeroCardService));
                return null;
            }

            // ✅ Отримуємо об'єкт з пулу
            var go = _poolManager.GetFromPool<GameObject>(HERO_CARD_POOL_KEY);
            if (go == null)
            {
                _logger.LogError("Не вдалося отримати HeroCard з пулу", nameof(HeroCardService));
                return null;
            }

            var card = go.GetComponent<HeroCardUI>();
            if (card == null)
            {
                _logger.LogError("HeroCardUI компонент відсутній у префабі", nameof(HeroCardService));
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, go);
                return null;
            }

            // ✅ Ін'єкція залежностей та налаштування
            _container.InjectDependencies(card);
            card.Reset();
            card.SetupWithSpriteService(model, _spriteService);

            // ✅ Встановлюємо батьківський трансформ та активуємо
            go.transform.SetParent(parent, false);
            go.SetActive(true);
            card.SetInteractable(interactive);

            return card;
        }

        /// <summary>
        /// Повертає картку в пул
        /// </summary>
        public void ReturnCard(HeroCardUI card)
        {
            if (card == null)
                return;

            try
            {
                card.Reset();
                card.gameObject.SetActive(false);
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, card.gameObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні картки в пул: {ex.Message}", nameof(HeroCardService), ex);
                // Запасний варіант - знищення об'єкта
                if (card != null && card.gameObject != null)
                {
                    UnityEngine.Object.Destroy(card.gameObject);
                }
            }
        }

        /// <summary>
        /// Повертає набір карток в пул
        /// </summary>
        public void ReturnAll(IEnumerable<HeroCardUI> cards)
        {
            if (cards == null)
                return;

            foreach (var card in cards)
            {
                ReturnCard(card);
            }
        }
    }
}
