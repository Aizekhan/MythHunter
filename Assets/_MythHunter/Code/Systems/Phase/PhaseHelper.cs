// Файл: Assets/_MythHunter/Code/Systems/Phase/PhaseHelper.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Допоміжний клас для роботи з фазами
    /// </summary>
    public class PhaseHelper
    {
        private readonly IPhaseSystem _phaseSystem;
        private readonly IMythLogger _logger;
        private readonly Dictionary<GamePhase, List<Type>> _phaseComponentTypes = new Dictionary<GamePhase, List<Type>>();

        [Inject]
        public PhaseHelper(IPhaseSystem phaseSystem, IMythLogger logger)
        {
            _phaseSystem = phaseSystem;
            _logger = logger;

            // Ініціалізація словника
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                _phaseComponentTypes[phase] = new List<Type>();
            }

            // Заповнення стандартними компонентами для фаз
            RegisterDefaultComponentsForPhases();
        }

        /// <summary>
        /// Реєструє стандартні компоненти для фаз
        /// </summary>
        private void RegisterDefaultComponentsForPhases()
        {
            // Тут можна визначити, які компоненти потрібні в яких фазах
            // Наприклад:

            // Компоненти для фази руни
            // RegisterComponentForPhase<Components.Rune.RuneComponent>(GamePhase.Rune);

            // Компоненти для фази планування
            // RegisterComponentForPhase<Components.Planning.PlanComponent>(GamePhase.Planning);

            // Компоненти для фази руху
            // RegisterComponentForPhase<Components.Movement.MovementComponent>(GamePhase.Movement);
            // RegisterComponentForPhase<Components.Movement.PathComponent>(GamePhase.Movement);

            // Компоненти для фази бою
            // RegisterComponentForPhase<Components.Combat.CombatComponent>(GamePhase.Combat);
            // RegisterComponentForPhase<Components.Combat.HealthComponent>(GamePhase.Combat);

            // Компоненти для фази завмирання
            // RegisterComponentForPhase<Components.Freeze.FreezeComponent>(GamePhase.Freeze);

            _logger.LogInfo("Registered default components for phases", "Phase");
        }

        /// <summary>
        /// Реєструє компонент для конкретної фази
        /// </summary>
        public void RegisterComponentForPhase<T>(GamePhase phase) where T : struct, IComponent
        {
            _phaseComponentTypes[phase].Add(typeof(T));
            _logger.LogDebug($"Registered component {typeof(T).Name} for phase {phase}", "Phase");
        }

        /// <summary>
        /// Отримує всі типи компонентів для фази
        /// </summary>
        public List<Type> GetComponentTypesForPhase(GamePhase phase)
        {
            return _phaseComponentTypes[phase];
        }

        /// <summary>
        /// Перевіряє, чи тип компонента активний у фазі
        /// </summary>
        public bool IsComponentActiveInPhase<T>(GamePhase phase) where T : struct, IComponent
        {
            return _phaseComponentTypes[phase].Contains(typeof(T));
        }

        /// <summary>
        /// Перевіряє, чи тип компонента активний у поточній фазі
        /// </summary>
        public bool IsComponentActiveInCurrentPhase<T>() where T : struct, IComponent
        {
            return IsComponentActiveInPhase<T>(_phaseSystem.CurrentPhase);
        }

        /// <summary>
        /// Отримує поточну фазу гри
        /// </summary>
        public GamePhase GetCurrentPhase()
        {
            return _phaseSystem.CurrentPhase;
        }

        /// <summary>
        /// Отримує час, що залишився до кінця поточної фази
        /// </summary>
        public float GetPhaseTimeRemaining()
        {
            return _phaseSystem.GetPhaseTimeRemaining();
        }

        /// <summary>
        /// Отримує прогрес поточної фази (0.0f - 1.0f)
        /// </summary>
        public float GetPhaseProgress()
        {
            return _phaseSystem.GetPhaseProgress();
        }
    }
}
