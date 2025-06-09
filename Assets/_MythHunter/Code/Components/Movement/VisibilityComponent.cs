// Assets/_MythHunter/Code/Components/Movement/VisibilityComponent.cs
using MythHunter.Core.ECS;
using UnityEngine;
namespace MythHunter.Components.Movement
{
    /// <summary>
    /// Компонент видимості та огляду сутності
    /// </summary>
    public struct VisibilityComponent : ISerializableComponent
    {
        public float VisionRadius;      // Радіус огляду
        public float VisionAngle;       // Кут огляду (у градусах)
        public Vector3 LookDirection;   // Напрямок погляду
        public bool IsVisible;          // Чи видима сутність

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
