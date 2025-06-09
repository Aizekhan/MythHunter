// Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerRegistry.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Реєстр серіалізаторів компонентів
    /// </summary>
    public class ComponentSerializerRegistry : IComponentSerializerRegistry
    {
        private readonly Dictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IMythLogger _logger;

        [Inject]
        public ComponentSerializerRegistry(IMythLogger logger)
        {
            _logger = logger;
            RegisterDefaultSerializers();
        }

        public void RegisterSerializer<T, TSerializer>(TSerializer serializer)
            where T : struct, IComponent
            where TSerializer : IComponentSerializer<T>
        {
            _serializers[typeof(T)] = serializer;
            _logger.LogInfo($"Registered serializer for component {typeof(T).Name}", "Serialization");
        }

        public IComponentSerializer<T> GetSerializer<T>() where T : struct, IComponent
        {
            if (_serializers.TryGetValue(typeof(T), out var serializer))
            {
                return (IComponentSerializer<T>)serializer;
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

        private void RegisterDefaultSerializers()
        {
            // Реєструємо базові серіалізатори
           
            RegisterSerializer<Components.Core.IdComponent, Serializers.IdComponentSerializer>(
                new Serializers.IdComponentSerializer()
            );
            RegisterSerializer<Components.Core.NameComponent, Serializers.NameComponentSerializer>(
                new Serializers.NameComponentSerializer()
            );
            RegisterSerializer<Components.Core.DescriptionComponent, Serializers.DescriptionComponentSerializer>(
                new Serializers.DescriptionComponentSerializer()
            );
            RegisterSerializer<Components.Core.ValueComponent, Serializers.ValueComponentSerializer>(
                new Serializers.ValueComponentSerializer()
            );

            _logger.LogInfo("Default component serializers registered", "Serialization");
        }
    }
}
