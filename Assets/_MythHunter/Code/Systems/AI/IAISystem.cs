// IAISystem.cs
namespace MythHunter.Systems.AI
{
    /// <summary>
    /// Інтерфейс для системи штучного інтелекту
    /// </summary>
    public interface IAISystem : MythHunter.Core.ECS.ISystem
    {
        void AddBehavior(int entityId, AIBehaviorType behaviorType);
        void RemoveBehavior(int entityId, AIBehaviorType behaviorType);
        void UpdateAI(int entityId);
    }

    /// <summary>
    /// Типи поведінки ШІ
    /// </summary>
    public enum AIBehaviorType
    {
        Idle,
        Patrol,
        Attack,
        Flee,
        Follow
    }
}
