// Assets/_MythHunter/Code/Components/Combat/CombatStateComponent.cs
using MythHunter.Core.ECS;
using RPGFramework.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Component tracking combat state (separate from stats and resources).
    /// Extracted from old CombatStatsComponent for clean separation of concerns.
    /// </summary>
    public struct CombatStateComponent : IComponent, ISerializableComponent
    {
        /// <summary>
        /// Whether entity is currently in combat
        /// </summary>
        public bool IsInCombat;

        /// <summary>
        /// Entity ID of current combat target (-1 if none)
        /// </summary>
        public int CombatTargetEntityId;

        /// <summary>
        /// Time of last attack (for attack speed calculations)
        /// </summary>
        public float LastAttackTime;

        /// <summary>
        /// Create combat state component with default values
        /// </summary>
        public static CombatStateComponent Create()
        {
            return new CombatStateComponent
            {
                IsInCombat = false,
                CombatTargetEntityId = -1,
                LastAttackTime = 0f
            };
        }

        /// <summary>
        /// Enter combat with specified target
        /// </summary>
        public void EnterCombat(int targetEntityId)
        {
            IsInCombat = true;
            CombatTargetEntityId = targetEntityId;
        }

        /// <summary>
        /// Exit combat
        /// </summary>
        public void ExitCombat()
        {
            IsInCombat = false;
            CombatTargetEntityId = -1;
        }

        /// <summary>
        /// Check if enough time passed to attack again
        /// </summary>
        public bool CanAttack(float currentTime, float attackSpeed)
        {
            float attackInterval = 1.0f / attackSpeed; // Convert attacks per second to interval
            return (currentTime - LastAttackTime) >= attackInterval;
        }

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(IsInCombat);
                writer.Write(CombatTargetEntityId);
                writer.Write(LastAttackTime);
                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                IsInCombat = reader.ReadBoolean();
                CombatTargetEntityId = reader.ReadInt32();
                LastAttackTime = reader.ReadSingle();
            }
        }
    }
}
