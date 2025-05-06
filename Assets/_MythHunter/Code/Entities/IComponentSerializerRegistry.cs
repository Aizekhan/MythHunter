using System;
using MythHunter.Core.ECS;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Контракт для реєстру серіалізаторів компонентів
    /// </summary>
    public interface IComponentSerializerRegistry
    {
        /// <summary>
        /// Реєструє серіалізатор для типу компонента
        /// </summary>
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <typeparam name="TSerializer">Тип серіалізатора</typeparam>
        /// <param name="serializer">Інстанс серіалізатора</param>
        void RegisterSerializer<T, TSerializer>(TSerializer serializer)
            where T : struct, IComponent
            where TSerializer : IComponentSerializer<T>;

        /// <summary>
        /// Повертає серіалізатор для конкретного типу компонента
        /// </summary>
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <returns>Серіалізатор або null, якщо не знайдено</returns>
        IComponentSerializer<T> GetSerializer<T>() where T : struct, IComponent;

        /// <summary>
        /// Серіалізує компонент у масив байтів
        /// </summary>
        byte[] Serialize<T>(T component) where T : struct, IComponent;

        /// <summary>
        /// Десеріалізує масив байтів у компонент
        /// </summary>
        T Deserialize<T>(byte[] data) where T : struct, IComponent;
    }
}
