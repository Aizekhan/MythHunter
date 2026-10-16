// Assets/_MythHunter/Code/Components/Combat/MythHunterResourceComponent.cs
using RPGFramework.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Component containing combat resources (HP, MP, Stamina, etc.) using direct fields.
    /// Custom wrapper for convenient resource access in MythHunter game layer.
    /// Replaces resource tracking from old CombatStatsComponent.
    /// </summary>
    public struct MythHunterResourceComponent : IComponent, ISerializableComponent
    {
        // Health resource
        public float HealthCurrent;
        public float HealthMax;

        // Mana resource
        public float ManaCurrent;
        public float ManaMax;

        // Stamina resource
        public float StaminaCurrent;
        public float StaminaMax;

        // Rage resource
        public float RageCurrent;
        public float RageMax;

        // Concentration resource
        public float ConcentrationCurrent;
        public float ConcentrationMax;

        /// <summary>
        /// Create resource component with default values
        /// </summary>
        public static MythHunterResourceComponent Create()
        {
            return new MythHunterResourceComponent
            {
                HealthCurrent = 100f,
                HealthMax = 100f,
                ManaCurrent = 50f,
                ManaMax = 50f,
                StaminaCurrent = 100f,
                StaminaMax = 100f,
                RageCurrent = 0f,
                RageMax = 100f,
                ConcentrationCurrent = 0f,
                ConcentrationMax = 100f
            };
        }

        /// <summary>
        /// Create resource component with custom health/mana values
        /// </summary>
        public static MythHunterResourceComponent CreateWithHealth(
            float maxHealth,
            float maxMana = 50f,
            float maxStamina = 100f)
        {
            return new MythHunterResourceComponent
            {
                HealthCurrent = maxHealth,
                HealthMax = maxHealth,
                ManaCurrent = maxMana,
                ManaMax = maxMana,
                StaminaCurrent = maxStamina,
                StaminaMax = maxStamina,
                RageCurrent = 0f,
                RageMax = 100f,
                ConcentrationCurrent = 0f,
                ConcentrationMax = 100f
            };
        }

        /// <summary>
        /// Check if entity is alive (has health > 0)
        /// </summary>
        public bool IsAlive()
        {
            return HealthCurrent > 0f;
        }

        /// <summary>
        /// Get health percentage (0.0 - 1.0)
        /// </summary>
        public float GetHealthPercentage()
        {
            return HealthMax > 0 ? HealthCurrent / HealthMax : 0f;
        }

        public byte[] Serialize()
        {
            // TODO: Implement serialization when needed
            return System.Array.Empty<byte>();
        }

        public void Deserialize(byte[] data)
        {
            // TODO: Implement deserialization when needed
        }
    }
}
