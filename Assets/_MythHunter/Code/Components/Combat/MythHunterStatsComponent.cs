// Assets/_MythHunter/Code/Components/Combat/MythHunterStatsComponent.cs
using MythHunter.Core.ECS;
using MythHunter.GameLogic.Combat;
using RPGFramework.ECS;
using RPGFramework.Stats;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Component containing combat statistics using Framework StatContainer.
    /// Replaces old CombatStatsComponent with extensible, modifier-based system.
    /// </summary>
    public struct MythHunterStatsComponent : IComponent, ISerializableComponent
    {
        /// <summary>
        /// Container for all combat stats with modifier support
        /// </summary>
        public StatContainer<CombatStatType, float> Stats;

        /// <summary>
        /// Create stats component with default values
        /// </summary>
        public static MythHunterStatsComponent Create()
        {
            var component = new MythHunterStatsComponent
            {
                Stats = new StatContainer<CombatStatType, float>()
            };

            // Initialize default base stats
            component.Stats.SetBaseStat(CombatStatType.AttackPower, 10f);
            component.Stats.SetBaseStat(CombatStatType.Defense, 5f);
            component.Stats.SetBaseStat(CombatStatType.AttackSpeed, 1.0f);
            component.Stats.SetBaseStat(CombatStatType.CriticalChance, 0.05f); // 5%
            component.Stats.SetBaseStat(CombatStatType.CriticalMultiplier, 1.5f); // 150%
            component.Stats.SetBaseStat(CombatStatType.Accuracy, 0.95f); // 95%
            component.Stats.SetBaseStat(CombatStatType.DodgeChance, 0.05f); // 5%
            component.Stats.SetBaseStat(CombatStatType.BlockChance, 0.1f); // 10%
            component.Stats.SetBaseStat(CombatStatType.CounterAttackChance, 0.05f); // 5%

            return component;
        }

        /// <summary>
        /// Create stats component with custom base values
        /// </summary>
        public static MythHunterStatsComponent CreateWithStats(
            float attackPower,
            float defense,
            float attackSpeed = 1.0f,
            float criticalChance = 0.05f,
            float criticalMultiplier = 1.5f)
        {
            var component = Create();
            component.Stats.SetBaseStat(CombatStatType.AttackPower, attackPower);
            component.Stats.SetBaseStat(CombatStatType.Defense, defense);
            component.Stats.SetBaseStat(CombatStatType.AttackSpeed, attackSpeed);
            component.Stats.SetBaseStat(CombatStatType.CriticalChance, criticalChance);
            component.Stats.SetBaseStat(CombatStatType.CriticalMultiplier, criticalMultiplier);
            return component;
        }

        public byte[] Serialize()
        {
            // TODO: Implement serialization when needed
            // For now, return empty array
            return System.Array.Empty<byte>();
        }

        public void Deserialize(byte[] data)
        {
            // TODO: Implement deserialization when needed
        }
    }
}
