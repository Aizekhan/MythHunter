namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс контейнера залежностей
    /// </summary>
    public interface IDIContainer
    {
        void Register<TService, TImplementation>() where TImplementation : TService;
        void RegisterSingleton<TService, TImplementation>() where TImplementation : TService;
        void RegisterInstance<TService>(TService instance);
        TService Resolve<TService>();
        bool IsRegistered<TService>();
        void AnalyzeDependencies();
    }
}