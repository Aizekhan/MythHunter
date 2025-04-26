using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Groups
{
    /// <summary>
    /// Група систем для послідовного виконання
    /// </summary>
    public class SystemGroup : SystemBase
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        
        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }
        
        public override void Initialize()
        {
            foreach (var system in _systems)
            {
                system.Initialize();
            }
        }
        
        public override void Update(float deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }
        
        public override void Dispose()
        {
            foreach (var system in _systems)
            {
                system.Dispose();
            }
            
            _systems.Clear();
        }
    }
}