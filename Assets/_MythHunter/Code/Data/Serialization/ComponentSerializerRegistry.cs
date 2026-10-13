// Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerRegistry.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using RPGFramework.ECS;
using RPGFramework.Serialization;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// MythHunter implementation of component serializer registry.
    /// Game-agnostic: serializers registered via installer.
    /// </summary>
    public class ComponentSerializerRegistry : IComponentSerializerRegistry
    {
        private readonly Dictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IMythLogger _logger;

        [Inject]
        public ComponentSerializerRegistry(IMythLogger logger)
        {
            _logger = logger;
        }

        public void RegisterSerializer<T, TSerializer>(TSerializer serializer)
            where T : struct, IComponent
            where TSerializer : RPGFramework.Serialization.IComponentSerializer<T>
        {
            _serializers[typeof(T)] = serializer;
            _logger.LogInfo($"Registered serializer for component {typeof(T).Name}", "Serialization");
        }

        public RPGFramework.Serialization.IComponentSerializer<T> GetSerializer<T>() where T : struct, IComponent
        {
            if (_serializers.TryGetValue(typeof(T), out var serializer))
            {
                return (RPGFramework.Serialization.IComponentSerializer<T>)serializer;
            }

            _logger.LogError($"No serializer found for component {typeof(T).Name}", "Serialization");
            return null;
        }

        public byte[] Serialize<T>(T component) where T : struct, IComponent
        {
            var serializer = GetSerializer<T>();
            return serializer?.Serialize(component);
        }

        public T Deserialize<T>(byte[] data) where T : struct, IComponent
        {
            var serializer = GetSerializer<T>();
            if (serializer != null)
            {
                return serializer.Deserialize(data);
            }
            return default;
        }

        public bool HasSerializer<T>() where T : struct, IComponent
        {
            return _serializers.ContainsKey(typeof(T));
        }
    }
}
