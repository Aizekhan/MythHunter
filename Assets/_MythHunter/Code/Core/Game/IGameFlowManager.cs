using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Інтерфейс централізованого сервісу управління ігровими переходами
    /// </summary>
    public interface IGameFlowManager
    {
        /// <summary>
        /// Запускає перехід від Boot до Lobby
        /// </summary>
        UniTask EnterLobbyAsync();

        /// <summary>
        /// Запускає перехід від Lobby до Gameplay
        /// </summary>
        UniTask EnterGameplayAsync(string[] selectedHeroArchetypes = null);

        /// <summary>
        /// Повертається з будь-якого стану до головного меню
        /// </summary>
        UniTask ReturnToMainMenuAsync();

        /// <summary>
        /// Перезавантажує поточну сцену
        /// </summary>
        UniTask ReloadCurrentSceneAsync();

        /// <summary>
        /// Запускає гру з самого початку (Boot)
        /// </summary>
        UniTask RestartGameAsync();
    }
}
