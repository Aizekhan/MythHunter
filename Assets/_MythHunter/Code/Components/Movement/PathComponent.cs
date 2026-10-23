// Assets/_MythHunter/Code/Components/Movement/PathComponent.cs
using MythHunter.Core.ECS;
using System.Collections.Generic;
using UnityEngine;
namespace MythHunter.Components.Movement
{
    /// <summary>
    /// Компонент шляху сутності
    /// </summary>
    public struct PathComponent : ISerializableComponent
    {
        public List<Vector3> Waypoints;     // Точки шляху
        public int CurrentWaypointIndex;    // Індекс поточної точки
        public bool IsPathComplete;         // Чи завершено шлях
        public float TimeOnPath;            // Час на поточному шляху

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
