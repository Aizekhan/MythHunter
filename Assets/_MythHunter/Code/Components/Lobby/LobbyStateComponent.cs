// Assets/_MythHunter/Code/Components/Lobby/LobbyStateComponent.cs
using MythHunter.Core.ECS;

namespace MythHunter.Components.Lobby
{
    /// <summary>
    /// Компонент стану лоббі
    /// </summary>
    public struct LobbyStateComponent : ISerializableComponent
    {
        public bool IsReady;             // Чи готовий гравець почати гру
        public int RemainingMana;        // Залишок мани для вибору героїв
        public int[] SelectedHeroIds;    // Ідентифікатори вибраних героїв
        public float SelectionTimeLeft;  // Час, що залишився для вибору

        // Методи серіалізації
        public byte[] Serialize()
        { /* ... */
            return new byte[0];
        }
        public void Deserialize(byte[] data)
        { /* ... */
        }
    }
}
