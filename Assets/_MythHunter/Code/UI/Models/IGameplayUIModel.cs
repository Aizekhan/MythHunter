// IGameplayUIModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Інтерфейс моделі ігрового UI
    /// </summary>
    public interface IGameplayUIModel : MythHunter.UI.Core.IModel
    {
        int CurrentPhase
        {
            get; set;
        }
        float PhaseTimeRemaining
        {
            get; set;
        }
        int RuneValue
        {
            get; set;
        }
        bool IsRunePhaseActive
        {
            get; set;
        }
    }
}
