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
            string[] activePhaseIds,  // Змінено з string phaseId
            IPhaseProvider phaseProvider) : base(name, logger)
        {
            // Зберігаємо активні фази
            _activePhaseIds.AddRange(activePhaseIds);
            _phaseProvider = phaseProvider;

            logger.LogDebug($"Created PhaseSystemGroup '{name}' for phases: {string.Join(", ", activePhaseIds)}", "System");
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
            string currentPhaseId = _phaseProvider.GetCurrentPhaseId();
            return _activePhaseIds.Contains(currentPhaseId);
        }

        #region IPhaseFilteredSystem Implementation

        public void SetActivePhaseIds(string[] phaseIds)
        {
            // Очищаємо попередні фази
            _activePhaseIds.Clear();

            // Додаємо нові фази
            if (phaseIds != null)
            {
                _activePhaseIds.AddRange(phaseIds);
            }
        }

        // Метод для сумісності зі старим IPhaseFilteredSystem інтерфейсом
        public void SetActivePhases(Events.Domain.GamePhase[] phases)
        {
            // Очищаємо попередні фази
            _activePhaseIds.Clear();

            // Конвертуємо GamePhase в string IDs і додаємо
            if (phases != null)
            {
                foreach (var phase in phases)
                {
                    _activePhaseIds.Add(phase.ToString());
                }
            }
        }

        public bool IsActiveInPhase(Events.Domain.GamePhase phase)
        {
            return _activePhaseIds.Contains(phase.ToString());
        }

        #endregion
    }
}
