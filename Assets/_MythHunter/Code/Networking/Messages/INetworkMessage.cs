using MythHunter.Data.Serialization;

namespace MythHunter.Networking.Messages
{
    /// <summary>
    /// Інтерфейс мережевого повідомлення
    /// </summary>
    public interface INetworkMessage : ISerializable
    {
        string GetMessageId();
    }
}