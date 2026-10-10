using System;
using RPGFramework.ECS;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Контракт для реєстру серіалізаторів компонентів
    /// </summary>
    public interface IComponentSerializerRegistry
    {
        void RegisterSerializer<T, TSerializer>(TSerializer serializer)
            where T : struct, IComponent
            where TSerializer : IComponentSerializer<T>;

        IComponentSerializer<T> GetSerializer<T>() where T : struct, IComponent;
        byte[] Serialize<T>(T component) where T : struct, IComponent;
        T Deserialize<T>(byte[] data) where T : struct, IComponent;
    }
}
