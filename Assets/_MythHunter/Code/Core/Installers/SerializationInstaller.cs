using MythHunter.Core.DI;
using MythHunter.Data.Serialization;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    public class SerializationInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей Serialization...", "Installer");

            BindSingleton<IComponentSerializerRegistry, ComponentSerializerRegistry>(container);

            logger.LogInfo("Встановлення залежностей Serialization завершено", "Installer");
        }
    }
}
