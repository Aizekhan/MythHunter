// Assets/_MythHunter/Code/GameLogic/Combat/CombatStatType.cs

namespace MythHunter.GameLogic.Combat
{
    /// <summary>
    /// Types of combat statistics used in MythHunter
    /// </summary>
    public enum CombatStatType
    {
        /// <summary>Base attack damage</summary>
        AttackPower,

        /// <summary>Damage reduction from physical attacks</summary>
        Defense,

        /// <summary>Attacks per second multiplier</summary>
        AttackSpeed,

        /// <summary>Chance to deal critical damage (0.0 - 1.0)</summary>
        CriticalChance,

        /// <summary>Damage multiplier on critical hit (e.g., 1.5x, 2.0x)</summary>
        CriticalMultiplier,

        /// <summary>Chance to hit target (0.0 - 1.0)</summary>
        Accuracy,

        /// <summary>Chance to avoid incoming attacks (0.0 - 1.0)</summary>
        DodgeChance,

        /// <summary>Chance to block incoming attacks (0.0 - 1.0)</summary>
        BlockChance,

        /// <summary>Chance to counter-attack when hit (0.0 - 1.0)</summary>
        CounterAttackChance
    }
}
