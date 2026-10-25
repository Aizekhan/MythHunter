// Шлях: Assets/_MythHunter/Code/Core/Installers/SerializationInstaller.cs
using RPGFramework.DI;
using MythHunter.Data.Serialization;
using MythHunter.Data.Serialization.Serializers;
using MythHunter.Utils.Logging;
using RPGFramework.Serialization;

namespace MythHunter.Core.Installers
{
    public class SerializationInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей Serialization...", "Installer");

            // Register component serializer registry
            BindSingleton<RPGFramework.Serialization.IComponentSerializerRegistry, ComponentSerializerRegistry>(container);

            // Register MythHunter component serializers
            var registry = container.Resolve<RPGFramework.Serialization.IComponentSerializerRegistry>();
            RegisterComponentSerializers(registry);

            logger.LogInfo("Встановлення залежностей Serialization завершено", "Installer");
        }

        private void RegisterComponentSerializers(RPGFramework.Serialization.IComponentSerializerRegistry registry)
        {
            // Register core component serializers
            registry.RegisterSerializer<Components.Core.IdComponent, IdComponentSerializer>(
                new IdComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.NameComponent, NameComponentSerializer>(
                new NameComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.DescriptionComponent, DescriptionComponentSerializer>(
                new DescriptionComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.ValueComponent, ValueComponentSerializer>(
                new ValueComponentSerializer()
            );
        }
    }
}
