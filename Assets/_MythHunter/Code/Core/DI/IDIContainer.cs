namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для DI контейнера
    /// </summary>
    public interface IDIContainer
    {
        void Register<TService, TImplementation>() where TImplementation : TService, new();
        void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new();
        void RegisterInstance<TService>(TService instance);
        TService Resolve<TService>();
    }
}