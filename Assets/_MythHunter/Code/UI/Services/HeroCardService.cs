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
    public interface IHeroCardService
    {
        UniTask InitializeAsync();
        UniTask<HeroCardUI> CreateCardAsync(HeroCardModel model, Transform parent, bool interactive = true);
        void ReturnCard(HeroCardUI card);
        void ReturnAll(IEnumerable<HeroCardUI> cards);
    }

    public class HeroCardService : IHeroCardService
    {
        private readonly IUIComponentFactory _componentFactory;
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;

        private const string POOL_KEY = "HeroCardUI";
        private bool _initialized = false;

        [Inject]
        public HeroCardService(IUIComponentFactory componentFactory, IPoolManager poolManager, IMythLogger logger)
        {
            _componentFactory = componentFactory;
            _poolManager = poolManager;
            _logger = logger;
        }

        public async UniTask InitializeAsync()
        {
            if (_initialized)
                return;

            var prefab = await _componentFactory.CreateComponentAsync<HeroCardUI>("UI/Prefabs/HeroCardUI");
            if (prefab == null)
            {
                _logger.LogError("Не вдалося завантажити HeroCardUI префаб", nameof(HeroCardService));
                return;
            }

            _poolManager.CreatePool<GameObject>(POOL_KEY, prefab.gameObject, 20);
            _logger.LogInfo("HeroCardUI пул ініціалізовано", nameof(HeroCardService));
            _initialized = true;
        }

        public async UniTask<HeroCardUI> CreateCardAsync(HeroCardModel model, Transform parent, bool interactive = true)
        {
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

            return card;
        }

        public void ReturnCard(HeroCardUI card)
        {
            if (card == null)
                return;
            card.Reset();
            card.gameObject.SetActive(false);
            _poolManager.ReturnToPool(POOL_KEY, card.gameObject);
        }

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
