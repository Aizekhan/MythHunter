namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для систем
    /// </summary>
    public abstract class SystemBase : ISystem
    {
        public virtual void Initialize() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Dispose() { }
    }
}