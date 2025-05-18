using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{

    public interface ISystemRegistry
    {
        void RegisterSystem(ISystem system);
        void RegisterSystemWithPriority(ISystem system, int priority);
        void UpdateAll(float deltaTime);
        void DisposeAll();
        IReadOnlyList<ISystem> GetAllSystems();
        void SetSystemActive(ISystem system, bool isActive);
        void InitializeAll();
        void InitializeSystemsByCategory(SystemInitializationCategory category);
        void LogInfo(string message);

        // Додамо метод для отримання DI контейнера
        IDIContainer GetContainer();
    }
}
