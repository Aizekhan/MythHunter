// Assets/_MythHunter/Code/Systems/Lobby/ILobbySystem.cs
using MythHunter.Core.ECS;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MythHunter.Systems.Lobby
{
    /// <summary>
    /// Інтерфейс системи лоббі
    /// </summary>
    public interface ILobbySystem : ISystem
    {
        /// <summary>
        /// Ініціалізує лоббі з заданою кількістю гравців
        /// </summary>
        void InitializeLobby(int playerCount);

        /// <summary>
        /// Вибирає героя для поточного гравця
        /// </summary>
        bool SelectHero(string archetypeId);

        /// <summary>
        /// Підтверджує вибір героїв для поточного гравця
        /// </summary>
        void ConfirmSelection();

        /// <summary>
        /// Починає гру, якщо всі гравці готові
        /// </summary>
        UniTask<bool> StartGameAsync();

        /// <summary>
        /// Отримує список доступних героїв з врахуванням залишку мани
        /// </summary>
        List<string> GetAvailableHeroes();

        /// <summary>
        /// Отримує список вибраних героїв поточного гравця
        /// </summary>
        List<string> GetSelectedHeroes();

        /// <summary>
        /// Перевіряє, чи всі гравці готові почати гру
        /// </summary>
        bool AreAllPlayersReady();
    }
}
