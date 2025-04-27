namespace MythHunter.Core.DI
{
    /// <summary>
    /// Базовий клас для інсталяторів залежностей
    /// </summary>
    public abstract class DIInstaller : IDIInstaller
    {
        public abstract void InstallBindings(IDIContainer container);
        
        protected void BindSingleton<TService, TImplementation>(IDIContainer container) 
            where TImplementation : TService
        {
            container.RegisterSingleton<TService, TImplementation>();
        }
        
        protected void Bind<TService, TImplementation>(IDIContainer container) 
            where TImplementation : TService
        {
            container.Register<TService, TImplementation>();
        }
        
        protected void BindInstance<TService>(IDIContainer container, TService instance)
        {
            container.RegisterInstance<TService>(instance);
        }
    }
}