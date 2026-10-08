using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для систем з підтримкою всіх типів оновлення
    /// </summary>
    public abstract class UpdateableSystemBase : SystemBase, IUpdateSystem, IFixedUpdateSystem, ILateUpdateSystem
    {
        [Inject]
        protected UpdateableSystemBase(IMythLogger logger, IEventBus eventBus) : base(logger, eventBus)
        {
        }

        public virtual void FixedUpdate(float fixedDeltaTime)
        {
            // Базова реалізація - нічого
        }

        public virtual void LateUpdate(float deltaTime)
        {
            // Базова реалізація - нічого
        }
    }
}
