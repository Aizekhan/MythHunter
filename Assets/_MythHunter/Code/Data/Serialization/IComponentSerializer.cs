// Assets/_MythHunter/Code/Data/Serialization/IComponentSerializer.cs
using RPGFramework.ECS;
namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Інтерфейс для серіалізації компонентів
    /// </summary>
    public interface IComponentSerializer<T> where T : struct, RPGFramework.ECS.IComponent
    {
        byte[] Serialize(T component);
        T Deserialize(byte[] data);
    }
}
