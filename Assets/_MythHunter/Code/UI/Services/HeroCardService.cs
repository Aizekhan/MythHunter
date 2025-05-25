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

        /// <summary>
        /// Перевіряє готовність сервісу
        /// </summary>
        bool IsReady
        {
            get;
        }
    }

    /// <summary>
    /// Сервіс для управління картками героїв
    /// Покладається на PreloadManager для створення пулів
    /// </summary>
    public class HeroCardService : IHeroCardService
    {
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        private readonly ISpriteService _spriteService;

        // ✅ Константа для пулу - має збігатися з preload конфігурацією
        private const string HERO_CARD_POOL_KEY = "HeroCardUI";

        public bool IsReady => _poolManager.HasPool(HERO_CARD_POOL_KEY);

        [Inject]
        public HeroCardService(
            IPoolManager poolManager,
            IMythLogger logger,
            IDIContainer container,
            ISpriteService spriteService)
        {
            _poolManager = poolManager;
            _logger = logger;
            _container = container;
            _spriteService = spriteService;

            _logger.LogInfo("HeroCardService створено - покладається на PreloadManager для пулів", nameof(HeroCardService));
        }

        /// <summary>
        /// Створює картку героя з пулу
        /// </summary>
        public async UniTask<HeroCardUI> CreateCardAsync(HeroCardModel model, Transform parent, bool interactive = true)
        {
            // ✅ ПЕРЕВІРЯЄМО ГОТОВНІСТЬ ПУЛУ (створеного PreloadManager)
            if (!_poolManager.HasPool(HERO_CARD_POOL_KEY))
            {
                _logger.LogError($"❌ Пул '{HERO_CARD_POOL_KEY}' не знайдено! " +
                    "Перевірте preload конфігурацію для LobbyScene.", nameof(HeroCardService));
                return null;
            }

            var go = _poolManager.GetFromPool<GameObject>(HERO_CARD_POOL_KEY);
            if (go == null)
            {
                _logger.LogError("❌ Не вдалося отримати HeroCard з пулу", nameof(HeroCardService));
                return null;
            }

            var card = go.GetComponent<HeroCardUI>();
            if (card == null)
            {
                _logger.LogError("❌ HeroCardUI компонент відсутній у префабі", nameof(HeroCardService));
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, go);
                return null;
            }

            // ✅ БЕЗПЕЧНЕ НАЛАШТУВАННЯ КАРТКИ
            try
            {
                // 1. Об'єкт вже деактивований в PoolManager.GetFromPool
                // 2. Встановлюємо батьківський трансформ
                go.transform.SetParent(parent, false);

                // 3. Ін'єкція залежностей та налаштування
                _container.InjectDependencies(card);
                card.Reset();
                card.SetupWithSpriteService(model, _spriteService);
                card.SetInteractable(interactive);

                // 4. Активуємо об'єкт
                go.SetActive(true);

                _logger.LogDebug($"✅ Створено картку для героя: {model.Name}", nameof(HeroCardService));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка налаштування картки: {ex.Message}", nameof(HeroCardService), ex);
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, go);
                return null;
            }

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
                // ✅ Деактивуємо об'єкт
                card.gameObject.SetActive(false);

                // ✅ Очищуємо стан картки
                card.Reset();

                // ✅ Повертаємо в пул
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, card.gameObject);

                _logger.LogDebug("✅ Картку повернено в пул", nameof(HeroCardService));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка при поверненні картки в пул: {ex.Message}", nameof(HeroCardService), ex);

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

            int returnedCount = 0;
            foreach (var card in cards)
            {
                if (card != null)
                {
                    ReturnCard(card);
                    returnedCount++;
                }
            }

            if (returnedCount > 0)
            {
                _logger.LogInfo($"✅ Повернено {returnedCount} карток в пул", nameof(HeroCardService));
            }
        }
    }
}
