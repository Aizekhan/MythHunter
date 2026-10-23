// Шлях: Assets/_MythHunter/Code/Systems/Core/IPhaseFilteredSystem.cs

using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Система, яка активна лише в певних фазах
    /// </summary>
    public interface IPhaseFilteredSystem : ISystem
    {
        /// <summary>
        /// Встановлює фази, в яких система активна, використовуючи string ID
        /// </summary>
        void SetActivePhaseIds(string[] phaseIds);

        /// <summary>
        /// Встановлює фази, в яких система активна (для сумісності зі старим кодом)
        /// </summary>
        void SetActivePhases(Events.Domain.GamePhase[] phases);

        /// <summary>
        /// Перевіряє, чи система активна у вказаній фазі (для сумісності зі старим кодом)
        /// </summary>
        bool IsActiveInPhase(Events.Domain.GamePhase phase);
    }
}
