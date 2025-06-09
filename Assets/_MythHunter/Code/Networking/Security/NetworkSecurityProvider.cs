// NetworkSecurityProvider.cs
using System;
using System.Security.Cryptography;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Security
{
    /// <summary>
    /// Базовий провайдер безпеки мережі
    /// </summary>
    public class NetworkSecurityProvider : INetworkSecurityProvider
    {
        private readonly IMythLogger _logger;
        private readonly HMACSHA256 _hmac;

        public NetworkSecurityProvider(IMythLogger logger)
        {
            _logger = logger;

            // В реальному проекті ключ має бути захищений і оновлюватися
            byte[] key = GetSecureKey();
            _hmac = new HMACSHA256(key);
        }

        /// <summary>
        /// Захищає пакет даних
        /// </summary>
        public byte[] SecurePacket(byte[] data)
        {
            byte[] hash = _hmac.ComputeHash(data);

            byte[] result = new byte[hash.Length + data.Length];
            Buffer.BlockCopy(hash, 0, result, 0, hash.Length);
            Buffer.BlockCopy(data, 0, result, hash.Length, data.Length);

            return result;
        }

        /// <summary>
        /// Перевіряє та повертає дані з захищеного пакету
        /// </summary>
        public bool VerifyAndExtractPacket(byte[] securedData, out byte[] data)
        {
            data = null;

            if (securedData.Length < _hmac.HashSize / 8)
            {
                _logger.LogWarning("Received packet is too small to contain hash", "Security");
                return false;
            }

            int hashSize = _hmac.HashSize / 8;

            byte[] receivedHash = new byte[hashSize];
            Buffer.BlockCopy(securedData, 0, receivedHash, 0, hashSize);

            data = new byte[securedData.Length - hashSize];
            Buffer.BlockCopy(securedData, hashSize, data, 0, data.Length);

            byte[] computedHash = _hmac.ComputeHash(data);

            // Порівнюємо хеші
            for (int i = 0; i < hashSize; i++)
            {
                if (receivedHash[i] != computedHash[i])
                {
                    _logger.LogWarning("Hash verification failed", "Security");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Отримує захищений ключ
        /// </summary>
        private byte[] GetSecureKey()
        {
            // В реальному проекті використовуйте безпечне сховище
            // Це спрощений приклад для демонстрації
            return new HMACSHA256().Key;
        }
    }

    /// <summary>
    /// Інтерфейс для провайдера безпеки
    /// </summary>
    public interface INetworkSecurityProvider
    {
        byte[] SecurePacket(byte[] data);
        bool VerifyAndExtractPacket(byte[] securedData, out byte[] data);
    }
}
