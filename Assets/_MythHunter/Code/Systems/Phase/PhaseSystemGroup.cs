// Шлях: Assets/_MythHunter/Code/Systems/Groups/PhaseSystemGroup.cs

using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Groups
{
    /// <summary>
    /// Група систем, яка активується тільки під час певних фаз
    /// </summary>
    public class PhaseSystemGroup : SystemGroup, IPhaseFilteredSystem
    {
        private readonly List<string> _activePhaseIds = new List<string>();
        private readonly IPhaseProvider _phaseProvider;

        // У класі PhaseSystemGroup:
        public PhaseSystemGroup(
            string name,
            int priority,
            IMythLogger logger,
            string[] activePhaseIds,
            IPhaseProvider phaseProvider) : base(name, logger)
        {
            // Перевіряємо і зберігаємо активні фази
            if (activePhaseIds != null && activePhaseIds.Length > 0)
            {
                _activePhaseIds.AddRange(activePhaseIds);
            }

            _phaseProvider = phaseProvider ?? throw new System.ArgumentNullException(nameof(phaseProvider));

            logger.LogInfo($"Created PhaseSystemGroup '{name}' for phases: {string.Join(", ", activePhaseIds ?? new string[0])}", "System");
        }

        public override void Initialize()
        {
            // Перевіряємо чи є активні фази
            if (_activePhaseIds.Count == 0)
            {
                _logger.LogWarning($"PhaseSystemGroup '{GroupName}' has no active phases defined", "System");
            }

            // Ініціалізуємо всі підсистеми
            base.Initialize();
        }

        public override void Update(float deltaTime)
        {
            // Перевіряємо, чи поточна фаза є однією з активних для цієї групи
            if (_phaseProvider != null && IsActiveInCurrentPhase())
            {
                // Викликаємо базовий метод для оновлення всіх підсистем
                base.Update(deltaTime);
            }
        }

        /// <summary>
        /// Перевіряє, чи активна ця група в поточній фазі
        /// </summary>
        private bool IsActiveInCurrentPhase()
        {
            // Якщо немає активних фаз, вважаємо що активна завжди
            if (_activePhaseIds.Count == 0)
                return true;

            string currentPhaseId = _phaseProvider.GetCurrentPhaseId();
            return _activePhaseIds.Contains(currentPhaseId);
        }

        #region IPhaseFilteredSystem Implementation

        public void SetActivePhaseIds(string[] phaseIds)
        {
            // Очищаємо попередні фази
            _activePhaseIds.Clear();

            // Додаємо нові фази
            if (phaseIds != null && phaseIds.Length > 0)
            {
                _activePhaseIds.AddRange(phaseIds);
                _logger.LogInfo($"Updated active phases for group '{GroupName}': {string.Join(", ", phaseIds)}", "System");
            }
            else
            {
                _logger.LogWarning($"No active phases set for group '{GroupName}'", "System");
            }
        }

        // Метод для сумісності зі старим IPhaseFilteredSystem інтерфейсом
        public void SetActivePhases(Events.Domain.GamePhase[] phases)
        {
            // Очищаємо попередні фази
            _activePhaseIds.Clear();

            // Конвертуємо GamePhase в string IDs і додаємо
            if (phases != null && phases.Length > 0)
            {
                foreach (var phase in phases)
                {
                    _activePhaseIds.Add(phase.ToString());
                }
                _logger.LogInfo($"Updated active phases for group '{GroupName}': {string.Join(", ", phases)}", "System");
            }
            else
            {
                _logger.LogWarning($"No active phases set for group '{GroupName}'", "System");
            }
        }

        public bool IsActiveInPhase(Events.Domain.GamePhase phase)
        {
            return _activePhaseIds.Contains(phase.ToString());
        }

        #endregion

        public override string ToString()
        {
            return $"PhaseSystemGroup '{GroupName}' (Active phases: {string.Join(", ", _activePhaseIds)})";
        }
    }
}
