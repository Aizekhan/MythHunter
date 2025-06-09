// Assets/_MythHunter/Code/Components/Movement/MovementComponent.cs
using MythHunter.Core.ECS;
using UnityEngine;
namespace MythHunter.Components.Movement
{
    /// <summary>
    /// Компонент для руху сутності
    /// </summary>
    public struct MovementComponent : ISerializableComponent
    {
        public float MoveSpeed;         // Швидкість руху
        public float RotationSpeed;     // Швидкість повороту
        public Vector3 Direction;       // Поточний напрямок
        public bool IsMoving;           // Чи рухається зараз
        public float MovementPoints;    // Поточні очки руху (витривалість)
        public float MaxMovementPoints; // Максимальні очки руху

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
