// Файл: Assets/_MythHunter/Code/Systems/Core/IPhaseFilteredSystem.cs
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Інтерфейс для систем, які активні тільки в певних фазах
    /// </summary>
    public interface IPhaseFilteredSystem : ISystem
    {
        /// <summary>
        /// Встановлює активні фази для системи
        /// </summary>
        void SetActivePhases(GamePhase[] phases);

        /// <summary>
        /// Перевіряє, чи активна система в поточній фазі
        /// </summary>
        bool IsActiveInPhase(GamePhase currentPhase);
    }
}
