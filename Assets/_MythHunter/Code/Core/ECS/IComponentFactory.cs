using System;
using MythHunter.Core.ECS;

namespace MythHunter.Entities
{
    public interface IComponentFactory
    {
        void RegisterDefaultValue<T>(T defaultValue) where T : struct, IComponent;
        T GetDefaultValue<T>() where T : struct, IComponent;
        T CreateComponent<T>() where T : struct, IComponent;
        void AddComponentToEntity<T>(int entityId) where T : struct, IComponent;
        void AddComponentToEntity<T>(int entityId, Action<T> initializer) where T : struct, IComponent;
    }
}
