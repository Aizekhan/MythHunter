// GameplayUIModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Модель даних ігрового UI
    /// </summary>
    public class GameplayUIModel : IGameplayUIModel
    {
        public int CurrentPhase { get; set; } = 0;
        public float PhaseTimeRemaining { get; set; } = 0f;
        public int RuneValue { get; set; } = 0;
        public bool IsRunePhaseActive { get; set; } = false;
    }
}
