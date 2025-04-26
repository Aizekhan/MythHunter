using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Реєстр систем для керування їх життєвим циклом
    /// </summary>
    public class SystemRegistry
    {
        private readonly List<ISystem> _updateSystems = new List<ISystem>();
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystems = new List<IFixedUpdateSystem>();
        private readonly List<ILateUpdateSystem> _lateUpdateSystems = new List<ILateUpdateSystem>();
        
        public void RegisterSystem(ISystem system)
        {
            _updateSystems.Add(system);
            
            if (system is IFixedUpdateSystem fixedUpdateSystem)
                _fixedUpdateSystems.Add(fixedUpdateSystem);
                
            if (system is ILateUpdateSystem lateUpdateSystem)
                _lateUpdateSystems.Add(lateUpdateSystem);
        }
        
        public void InitializeAll()
        {
            foreach (var system in _updateSystems)
            {
                system.Initialize();
            }
        }
        
        public void UpdateAll(float deltaTime)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(deltaTime);
            }
        }
        
        public void FixedUpdateAll(float fixedDeltaTime)
        {
            foreach (var system in _fixedUpdateSystems)
            {
                system.FixedUpdate(fixedDeltaTime);
            }
        }
        
        public void LateUpdateAll(float deltaTime)
        {
            foreach (var system in _lateUpdateSystems)
            {
                system.LateUpdate(deltaTime);
            }
        }
        
        public void DisposeAll()
        {
            foreach (var system in _updateSystems)
            {
                system.Dispose();
            }
            
            _updateSystems.Clear();
            _fixedUpdateSystems.Clear();
            _lateUpdateSystems.Clear();
        }
    }
}