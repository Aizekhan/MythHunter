using MythHunter.Systems.Core;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Реалізація світу ECS
    /// </summary>
    public class EcsWorld : IEcsWorld
    {
        private readonly IEntityManager _entityManager;
        private readonly SystemRegistry _systemRegistry;
        
        public IEntityManager EntityManager => _entityManager;
        
        public EcsWorld(IEntityManager entityManager, SystemRegistry systemRegistry)
        {
            _entityManager = entityManager;
            _systemRegistry = systemRegistry;
        }
        
        public void Initialize()
        {
            _systemRegistry.InitializeAll();
        }
        
        public void Update(float deltaTime)
        {
            _systemRegistry.UpdateAll(deltaTime);
        }
        
        public void Dispose()
        {
            _systemRegistry.DisposeAll();
        }
    }
}