using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Група систем, що виконується послідовно
    /// </summary>
    public class SystemGroup : ISystem
    {
        private readonly List<ISystem> _systems = new();

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void Initialize()
        {
            foreach (var system in _systems)
                system.Initialize();
        }

        public void Update(float deltaTime)
        {
            foreach (var system in _systems)
                system.Update(deltaTime);
        }

        public void Dispose()
        {
            foreach (var system in _systems)
                system.Dispose();
        }
    }
}