// Assets/_MythHunter/Code/Data/Serialization/Serializers/CoreComponentSerializers.cs
using MythHunter.Components.Core;

namespace MythHunter.Data.Serialization.Serializers
{
    public class IdComponentSerializer : ComponentSerializerBase<IdComponent>
    {
        protected override void SerializeInternal(System.IO.BinaryWriter writer, IdComponent component)
        {
            writer.Write(component.Id);
        }

        protected override IdComponent DeserializeInternal(System.IO.BinaryReader reader)
        {
            return new IdComponent { Id = reader.ReadInt32() };
        }
    }

    public class NameComponentSerializer : ComponentSerializerBase<NameComponent>
    {
        protected override void SerializeInternal(System.IO.BinaryWriter writer, NameComponent component)
        {
            writer.Write(component.Name ?? string.Empty);
        }

        protected override NameComponent DeserializeInternal(System.IO.BinaryReader reader)
        {
            return new NameComponent { Name = reader.ReadString() };
        }
    }

    public class DescriptionComponentSerializer : ComponentSerializerBase<DescriptionComponent>
    {
        protected override void SerializeInternal(System.IO.BinaryWriter writer, DescriptionComponent component)
        {
            writer.Write(component.Description ?? string.Empty);
        }

        protected override DescriptionComponent DeserializeInternal(System.IO.BinaryReader reader)
        {
            return new DescriptionComponent { Description = reader.ReadString() };
        }
    }

    public class ValueComponentSerializer : ComponentSerializerBase<ValueComponent>
    {
        protected override void SerializeInternal(System.IO.BinaryWriter writer, ValueComponent component)
        {
            writer.Write(component.Value);
        }

        protected override ValueComponent DeserializeInternal(System.IO.BinaryReader reader)
        {
            return new ValueComponent { Value = reader.ReadInt32() };
        }
    }
}
