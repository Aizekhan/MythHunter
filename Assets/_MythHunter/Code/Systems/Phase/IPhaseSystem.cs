// Файл: Assets/_MythHunter/Code/Systems/Phase/IPhaseSystem.cs (оновлений)
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
        float GetPhaseDuration(MythHunter.Events.Domain.GamePhase phase);
        float GetPhaseProgress();
        void SetPhaseDuration(MythHunter.Events.Domain.GamePhase phase, float duration);
    }
}
