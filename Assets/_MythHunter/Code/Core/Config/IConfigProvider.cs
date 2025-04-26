namespace MythHunter.Core.Config
{
    /// <summary>
    /// Інтерфейс провайдера конфігурацій
    /// </summary>
    public interface IConfigProvider
    {
        T GetConfig<T>() where T : class;
        void SetConfig<T>(T config) where T : class;
        bool HasConfig<T>() where T : class;
    }
}