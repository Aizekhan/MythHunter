using MythHunter.Core.ECS;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Components.Core;
namespace MythHunter.Entities
{
    /// <summary>
    /// Базова фабрика сутностей
    /// </summary>
    public abstract class EntityFactory
    {
        protected readonly IEntityManager EntityManager;
        protected readonly IMythLogger Logger;
        
        [Inject]
        public EntityFactory(IEntityManager entityManager, IMythLogger logger)
        {
            EntityManager = entityManager;
            Logger = logger;
        }
        
        protected int CreateBaseEntity()
        {
            int entityId = EntityManager.CreateEntity();
            EntityManager.AddComponent(entityId, new NameComponent { Name = "Entity_" + entityId });
            EntityManager.AddComponent(entityId, new IdComponent { Id = entityId });
            
            Logger.LogDebug($"Created entity with ID {entityId}");
            
            return entityId;
        }
    }
}
