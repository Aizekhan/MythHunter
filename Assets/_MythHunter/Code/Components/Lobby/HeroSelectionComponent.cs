// Assets/_MythHunter/Code/Components/Lobby/HeroSelectionComponent.cs
using MythHunter.Core.ECS;

namespace MythHunter.Components.Lobby
{
    /// <summary>
    /// Компонент для вибору героя
    /// </summary>
    public struct HeroSelectionComponent : ISerializableComponent
    {
        public string ArchetypeId;     // Ідентифікатор архетипу героя
        public int ManaCost;           // Вартість героя в мані
        public bool IsSelected;        // Чи вибраний цей герой
        public int PlayerIndex;        // Індекс гравця, який вибрав героя

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
