// Шлях: Assets/_MythHunter/Code/Core/DI/IDIContainer.cs
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

        // Новий метод для ін'єкції залежностей у об'єкт
        void InjectDependencies(object target);
    }
}
