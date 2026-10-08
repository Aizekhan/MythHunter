using MythHunter.Networking.Messages;
namespace MythHunter.Networking.Serialization
{
    /// <summary>
    /// Інтерфейс мережевого серіалізатора
    /// </summary>
    public interface INetworkSerializer
    {
        byte[] Serialize<T>(T message) where T : INetworkMessage;
        T Deserialize<T>(byte[] data) where T : INetworkMessage, new();
        INetworkMessage Deserialize(byte[] data, System.Type messageType);
    }
}