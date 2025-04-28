// IRuneSystem.cs
namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Інтерфейс для системи рун
    /// </summary>
    public interface IRuneSystem : MythHunter.Core.ECS.ISystem
    {
        void RollRune();
        int GetRuneValue();
        void ApplyRuneEffect(int entityId);
    }
}
