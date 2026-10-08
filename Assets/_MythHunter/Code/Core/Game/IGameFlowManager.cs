using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Інтерфейс централізованого сервісу управління ігровими переходами
    /// </summary>
    public interface IGameFlowManager : IDisposable
    {
        // ✅ ІСНУЮЧІ методи
        UniTask EnterLobbyAsync();
        UniTask EnterGameplayAsync(string[] selectedHeroArchetypes, string mapId = "default");
        UniTask ReturnToLobbyAsync();
        UniTask ReloadCurrentSceneAsync();
        UniTask RestartGameAsync();

        // ✅ НОВИЙ метод
        UniTask EnterMainMenuAsync();

        // ✅ ПЕРЕЙМЕНОВАНИЙ метод (якщо потрібно)
        UniTask ReturnToMainMenuAsync();

        UniTask EnterProfileAsync();
    }
}
