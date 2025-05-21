// Шлях: Assets/_MythHunter/Code/UI/Services/HeroCardService.cs

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

        private const string POOL_KEY = "HeroCardUI";
        private bool _initialized = false;
        private readonly ViewId _heroCardViewId = ViewId.HeroCard; // Припускаємо, що це значення додано в ViewId

        [Inject]
        public HeroCardService(
            IUIComponentFactory componentFactory,
            IPoolManager poolManager,
            IMythLogger logger,
            IViewConfigRegistry viewConfigRegistry)
        {
            _componentFactory = componentFactory;
            _poolManager = poolManager;
            _logger = logger;
            _viewConfigRegistry = viewConfigRegistry;
        }

        /// <summary>
        /// Ініціалізує сервіс, створюючи пул карток героїв
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (_initialized)
                return;

            var config = _viewConfigRegistry.Get(_heroCardViewId);
            if (config == null)
            {
                _logger.LogError($"Не знайдено конфігурацію для ViewId: {_heroCardViewId}", nameof(HeroCardService));

                // Запасний варіант, якщо конфігурація не знайдена
                var prefabLegacy = await _componentFactory.CreateComponentAsync<HeroCardUI>("UI/Prefabs/HeroCardUI");
                if (prefabLegacy == null)
                {
                    _logger.LogError("Не вдалося завантажити HeroCardUI префаб", nameof(HeroCardService));
                    return;
                }

                _poolManager.CreatePool<GameObject>(POOL_KEY, prefabLegacy.gameObject, 20);
            }
            else
            {
                // Використовуємо новий підхід з ViewId
                var prefabGO = await _componentFactory.CreateComponentAsync(_heroCardViewId);
                if (prefabGO == null)
                {
                    _logger.LogError($"Не вдалося створити компонент для ViewId: {_heroCardViewId}", nameof(HeroCardService));
                    return;
                }

                _poolManager.CreatePool<GameObject>(POOL_KEY, prefabGO, 20);
            }

            _logger.LogInfo("HeroCardUI пул ініціалізовано", nameof(HeroCardService));
            _initialized = true;
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

            if (!_poolManager.HasPool(POOL_KEY))
            {
                _logger.LogError("HeroCardUI пул не ініціалізовано", nameof(HeroCardService));
                return null;
            }

            var go = _poolManager.GetFromPool<GameObject>(POOL_KEY);
            if (go == null)
            {
                _logger.LogError("Не вдалося отримати HeroCard з пулу", nameof(HeroCardService));
                return null;
            }

            var card = go.GetComponent<HeroCardUI>();
            if (card == null)
            {
                _logger.LogError("HeroCardUI компонент відсутній у префабі", nameof(HeroCardService));
                _poolManager.ReturnToPool(POOL_KEY, go);
                return null;
            }

            card.Reset();
            card.Setup(model);
            go.transform.SetParent(parent, false);
            go.SetActive(true);

            // Налаштування інтерактивності
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

            card.Reset();
            card.gameObject.SetActive(false);
            _poolManager.ReturnToPool(POOL_KEY, card.gameObject);
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
