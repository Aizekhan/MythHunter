// Шлях: Assets/_MythHunter/Code/Core/Installers/SerializationInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Data.Serialization;
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
            BindSingleton<IComponentSerializerRegistry, ComponentSerializerRegistry>(container);

            // Register MythHunter component serializers
            var registry = container.Resolve<IComponentSerializerRegistry>();
            RegisterComponentSerializers(registry);

            logger.LogInfo("Встановлення залежностей Serialization завершено", "Installer");
        }

        private void RegisterComponentSerializers(RPGFramework.Serialization.IComponentSerializerRegistry registry)
        {
            // Register core component serializers
            registry.RegisterSerializer<Components.Core.IdComponent, Serializers.IdComponentSerializer>(
                new Serializers.IdComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.NameComponent, Serializers.NameComponentSerializer>(
                new Serializers.NameComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.DescriptionComponent, Serializers.DescriptionComponentSerializer>(
                new Serializers.DescriptionComponentSerializer()
            );
            registry.RegisterSerializer<Components.Core.ValueComponent, Serializers.ValueComponentSerializer>(
                new Serializers.ValueComponentSerializer()
            );
        }
    }
}
