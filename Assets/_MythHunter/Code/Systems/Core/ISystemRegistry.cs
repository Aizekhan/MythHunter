using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    public interface ISystemRegistry
    {
        void RegisterSystem(ISystem system);
        void InitializeAll();
        void UpdateAll(float deltaTime);
        void FixedUpdateAll(float fixedDeltaTime);
        void LateUpdateAll(float deltaTime);
        void DisposeAll();
    }
}
