using System;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Інтерфейс для об'єктів, які можна серіалізувати
    /// </summary>
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}