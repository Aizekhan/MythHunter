using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{

    public interface ISystemRegistry
    {
        void RegisterSystem(ISystem system);
        void RegisterSystemWithPriority(ISystem system, int priority);
        void RegisterSystemWithPriority(ISystem system, int priority, SystemInitializationCategory category);
        void UpdateAll(float deltaTime);
        void DisposeAll();
        IReadOnlyList<ISystem> GetAllSystems();
        void SetSystemActive(ISystem system, bool isActive);
        void InitializeAll();
        void InitializeSystemsByCategory(SystemInitializationCategory category);
        void LogInfo(string message);
        T GetSystem<T>() where T : ISystem;
    }
}
