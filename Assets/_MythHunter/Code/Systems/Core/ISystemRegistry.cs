using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{

    public interface ISystemRegistry
    {
        //RegisterSystemWithPriority - в файлі  Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs
        //using MythHunter.Systems.Core; 
        void RegisterSystem(ISystem system);
        void InitializeAll();
        void InitializeSystemsByCategory(SystemInitializationCategory category);

        void UpdateAll(float deltaTime);
        void FixedUpdateAll(float fixedDeltaTime);
        void LateUpdateAll(float deltaTime);
        void DisposeAll();
    }
}
