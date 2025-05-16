// Шлях: Assets/_MythHunter/Code/Systems/Heroes/HeroSystem.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities.Heroes;
using MythHunter.Events;
using MythHunter.Events.Domain.Gameplay;
using MythHunter.Utils.Logging;
using MythHunter.Components.Combat;  // Для HealthComponent
using MythHunter.Components.Character; // Для StatsComponent
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Entities;
using System.Collections.Generic;

namespace MythHunter.Systems.Heroes
{
    public interface IHeroSystem : ISystem
    {
        int CreateHero(string archetypeId, string heroName, int teamId);
        int CreateHeroWithComponents(string archetypeId, Dictionary<Type, object> overrides);
        UniTask<int> LoadHeroAsync(string heroId);
        UniTask SaveHeroAsync(int entityId, string heroId);
        void DestroyHero(int entityId);
    }

    public class HeroSystem : SystemBase, IHeroSystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IHeroFactory _heroFactory;
        private readonly IRaceClassBonusSystem _bonusSystem;
        private readonly IComponentCacheRegistry _componentCacheRegistry;

        private ComponentCache<StatsComponent> _statsCache;
        private ComponentCache<HealthComponent> _healthCache;

        [Inject]
        public HeroSystem(
            IEntityManager entityManager,
            IHeroFactory heroFactory,
            IRaceClassBonusSystem bonusSystem,
            IComponentCacheRegistry componentCacheRegistry,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _heroFactory = heroFactory;
            _bonusSystem = bonusSystem;
            _componentCacheRegistry = componentCacheRegistry;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Ініціалізація кешу компонентів
            _statsCache = _componentCacheRegistry.GetCache<StatsComponent>();
            _healthCache = _componentCacheRegistry.GetCache<HealthComponent>();

            // Підписка на події
            SubscribeToEvents();
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<CreateHeroRequestEvent>(OnCreateHeroRequest);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<CreateHeroRequestEvent>(OnCreateHeroRequest);
        }

        private void OnCreateHeroRequest(CreateHeroRequestEvent evt)
        {
            var entityId = CreateHero(evt.ArchetypeId, evt.HeroName, evt.TeamId);
            // Додаткова логіка після створення героя
        }

        public int CreateHero(string archetypeId, string heroName, int teamId)
        {
            return _heroFactory.CreateHero(archetypeId, heroName, teamId);
        }

        public int CreateHeroWithComponents(string archetypeId, Dictionary<Type, object> overrides)
        {
            return _heroFactory.CreateCustomHero(archetypeId, overrides);
        }

        public async UniTask<int> LoadHeroAsync(string heroId)
        {
            return await _heroFactory.CreateHeroFromDatabaseAsync(heroId);
        }

        public async UniTask SaveHeroAsync(int entityId, string heroId)
        {
            await _heroFactory.SaveHeroToDatabaseAsync(entityId, heroId);
        }

        public void DestroyHero(int entityId)
        {
            // Додаткова логіка перед видаленням героя
            _entityManager.DestroyEntity(entityId);
        }

        public override void Update(float deltaTime)
        {
            // Оновлення всіх кешів компонентів
            _statsCache.Update();
            _healthCache.Update();

            // Додаткова логіка оновлення всіх героїв
        }
    }
}
