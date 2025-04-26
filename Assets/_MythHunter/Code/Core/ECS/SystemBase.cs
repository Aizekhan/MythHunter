namespace MythHunter.Core.ECS
{
    public abstract class SystemBase : ISystem
    {
        public virtual void Initialize()
        {
        }
        public virtual void Update(float deltaTime)
        {
        }
        public virtual void Dispose()
        {
        }

        public virtual void Execute()
        {
        }
    }
}
