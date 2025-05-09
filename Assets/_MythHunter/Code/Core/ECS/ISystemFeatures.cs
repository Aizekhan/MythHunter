// Шлях: Assets/_MythHunter/Code/Core/ECS/ISystemFeatures.cs
using MythHunter.Events;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Позначає систему, яка підтримує звичайне оновлення
    /// </summary>
    public interface IUpdateSystem : ISystem
    {
        // Успадковує Update з ISystem
    }

    /// <summary>
    /// Позначає систему, яка підтримує фіксоване оновлення
    /// </summary>
    public interface IFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float fixedDeltaTime);
    }

    /// <summary>
    /// Позначає систему, яка підтримує пізнє оновлення
    /// </summary>
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate(float deltaTime);
    }

    /// <summary>
    /// Позначає систему, яка підтримує роботу з подіями
    /// </summary>
    public interface IEventSystem : ISystem, IEventSubscriber
    {
        // Наслідує всі методи з IEventSubscriber
    }
}
