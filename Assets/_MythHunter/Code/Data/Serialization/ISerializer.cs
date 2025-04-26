namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Інтерфейс серіалізатора
    /// </summary>
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] data);
        string SerializeToString<T>(T obj);
        T DeserializeFromString<T>(string data);
    }
}