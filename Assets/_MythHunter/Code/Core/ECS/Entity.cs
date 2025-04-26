using System.Collections.Generic;
using System;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для Entity
    /// </summary>
    public class Entity
    {
        public int Id { get; }

        public Entity(int id)
        {
            Id = id;
        }
        private Dictionary<Type, IComponent> _components = new();

        public void AddComponent<T>(T component) where T : IComponent
        {
            _components[typeof(T)] = component;
        }

        public T GetComponent<T>() where T : IComponent
        {
            return (T)_components[typeof(T)];
        }
    }
}
