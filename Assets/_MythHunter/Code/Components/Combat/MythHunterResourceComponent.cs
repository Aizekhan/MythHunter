// Assets/_MythHunter/Code/Components/Combat/MythHunterResourceComponent.cs
using MythHunter.Core.ECS;
using MythHunter.GameLogic.Combat;
using RPGFramework.Combat;
using RPGFramework.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Component containing combat resources (HP, MP, Stamina, etc.) using Framework ResourceComponent.
    /// Replaces resource tracking from old CombatStatsComponent.
    /// </summary>
    public struct MythHunterResourceComponent : IComponent, ISerializableComponent
    {
        /// <summary>
        /// Container for all combat resources
        /// </summary>
        public ResourceComponent<MythHunterResourceType> Resources;

        /// <summary>
        /// Create resource component with default values
        /// </summary>
        public static MythHunterResourceComponent Create()
        {
            var component = new MythHunterResourceComponent
            {
                Resources = new ResourceComponent<MythHunterResourceType>()
            };

            // Initialize default resources (current, min, max)
            component.Resources.SetResource(MythHunterResourceType.Health, 100f, 0f, 100f);
            component.Resources.SetResource(MythHunterResourceType.Mana, 50f, 0f, 50f);
            component.Resources.SetResource(MythHunterResourceType.Stamina, 100f, 0f, 100f);
            component.Resources.SetResource(MythHunterResourceType.Rage, 0f, 0f, 100f);
            component.Resources.SetResource(MythHunterResourceType.Concentration, 0f, 0f, 100f);

            return component;
        }

        /// <summary>
        /// Create resource component with custom health/mana values
        /// </summary>
        public static MythHunterResourceComponent CreateWithHealth(
            float maxHealth,
            float maxMana = 50f,
            float maxStamina = 100f)
        {
            var component = Create();
            component.Resources.SetResource(MythHunterResourceType.Health, maxHealth, 0f, maxHealth);
            component.Resources.SetResource(MythHunterResourceType.Mana, maxMana, 0f, maxMana);
            component.Resources.SetResource(MythHunterResourceType.Stamina, maxStamina, 0f, maxStamina);
            return component;
        }

        /// <summary>
        /// Check if entity is alive (has health > 0)
        /// </summary>
        public bool IsAlive()
        {
            return Resources.GetCurrent(MythHunterResourceType.Health) > 0f;
        }

        /// <summary>
        /// Get health percentage (0.0 - 1.0)
        /// </summary>
        public float GetHealthPercentage()
        {
            float current = Resources.GetCurrent(MythHunterResourceType.Health);
            float max = Resources.GetMax(MythHunterResourceType.Health);
            return max > 0 ? current / max : 0f;
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
