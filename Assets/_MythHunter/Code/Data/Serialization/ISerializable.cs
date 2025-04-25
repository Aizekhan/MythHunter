namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Інтерфейс для серіалізації об'єктів
    /// </summary>
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}