// Файл: Assets/_MythHunter/Code/Data/ScriptableObjects/PhaseConfig.cs

using UnityEngine;
using MythHunter.Events.Domain;

namespace MythHunter.Data.ScriptableObjects
{
    /// <summary>
    /// Конфігурація фаз гри
    /// </summary>
    [CreateAssetMenu(fileName = "PhaseConfig", menuName = "MythHunter/Configs/PhaseConfig")]
    public class PhaseConfig : ScriptableObject
    {
        [Header("Тривалість фаз (в секундах)")]
        [SerializeField] private float _runePhaseTime = 10f;
        [SerializeField] private float _planningPhaseTime = 15f;
        [SerializeField] private float _movementPhaseTime = 20f;
        [SerializeField] private float _combatPhaseTime = 30f;
        [SerializeField] private float _freezePhaseTime = 5f;

        [Header("Налаштування фаз")]
        [SerializeField] private bool _autoProgressPhases = true;
        [SerializeField] private bool _enableTimerUpdates = true;
        [SerializeField] private float _timerUpdateInterval = 0.5f;

        // Геттери
        public float RunePhaseTime => _runePhaseTime;
        public float PlanningPhaseTime => _planningPhaseTime;
        public float MovementPhaseTime => _movementPhaseTime;
        public float CombatPhaseTime => _combatPhaseTime;
        public float FreezePhaseTime => _freezePhaseTime;

        public bool AutoProgressPhases => _autoProgressPhases;
        public bool EnableTimerUpdates => _enableTimerUpdates;
        public float TimerUpdateInterval => _timerUpdateInterval;

        /// <summary>
        /// Отримати тривалість фази за типом
        /// </summary>
        public float GetPhaseDuration(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Rune => _runePhaseTime,
                GamePhase.Planning => _planningPhaseTime,
                GamePhase.Movement => _movementPhaseTime,
                GamePhase.Combat => _combatPhaseTime,
                GamePhase.Freeze => _freezePhaseTime,
                _ => 0f
            };
        }

        /// <summary>
        /// Отримати наступну фазу за поточною
        /// </summary>
        public GamePhase GetNextPhase(GamePhase currentPhase)
        {
            return currentPhase switch
            {
                GamePhase.Rune => GamePhase.Planning,
                GamePhase.Planning => GamePhase.Movement,
                GamePhase.Movement => GamePhase.Combat,
                GamePhase.Combat => GamePhase.Freeze,
                GamePhase.Freeze => GamePhase.Rune,
                _ => GamePhase.Rune
            };
        }
    }
}
