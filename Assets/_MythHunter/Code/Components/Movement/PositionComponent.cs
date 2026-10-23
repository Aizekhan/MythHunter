// Assets/_MythHunter/Code/Components/Movement/PositionComponent.cs
using MythHunter.Core.ECS;
using UnityEngine;

namespace MythHunter.Components.Movement

{
    /// <summary>
    /// Компонент позиції сутності у світі
    /// </summary>
    public struct PositionComponent : ISerializableComponent
    {
        public Vector3 Position;
        public Vector3 PreviousPosition;
        public Quaternion Rotation;
        public Vector3 Scale;

        // Методи для серіалізації/десеріалізації
        public byte[] Serialize()
        { /* ... */
            return new byte[0];
        }
        public void Deserialize(byte[] data)
        { /* ... */
        }
    }
}
