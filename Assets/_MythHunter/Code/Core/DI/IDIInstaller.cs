namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для інсталяторів залежностей
    /// </summary>
    public interface IDIInstaller
    {
        void InstallBindings(IDIContainer container);
    }
}