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

        // Метод для ін'єкції залежностей у об'єкт
        void InjectDependencies(object target);

        /// <summary>
        /// Реєструє лінивий сінглтон, який буде створено при першому зверненні
        /// </summary>
        void RegisterLazySingleton<TService, TImplementation>()
            where TImplementation : TService
            where TService : class;  

        /// <summary>
        /// Резолвить лінивий екземпляр залежності
        /// </summary>
        LazyDependency<TService> ResolveLazy<TService>() where TService : class;
        /// <summary>
        /// Реєструє транзієнтні залежності (створюються щоразу при запиті)
        /// </summary>
        void RegisterTransient<TService, TImplementation>() where TImplementation : TService;

        /// <summary>
        /// Реєструє скоупні залежності (існують на час життя скоупу)
        /// </summary>
        void RegisterScoped<TService, TImplementation>() where TImplementation : TService;

        /// <summary>
        /// Створює новий скоуп для залежностей
        /// </summary>
        DIScope CreateScope(string scopeId = null);

        /// <summary>
        /// Отримує поточний скоуп або створює новий, якщо немає
        /// </summary>
        DIScope GetCurrentScope();

        /// <summary>
        /// Встановлює поточний скоуп
        /// </summary>
        void SetCurrentScope(DIScope scope);
    }
}
