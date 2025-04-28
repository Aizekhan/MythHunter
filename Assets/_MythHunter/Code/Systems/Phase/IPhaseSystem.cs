// IPhaseSystem.cs
namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Інтерфейс для системи фаз гри
    /// </summary>
    public interface IPhaseSystem : MythHunter.Core.ECS.ISystem
    {
        void StartPhase(MythHunter.Events.Domain.GamePhase phase);
        void EndPhase(MythHunter.Events.Domain.GamePhase phase);
        MythHunter.Events.Domain.GamePhase CurrentPhase
        {
            get;
        }
        float GetPhaseTimeRemaining();
    }
}
