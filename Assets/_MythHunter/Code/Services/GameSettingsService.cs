// Шлях: Assets/_MythHunter/Code/Services/GameSettings/GameSettingsService.cs
using MythHunter.Core.DI;

namespace MythHunter.Services.GameSettings
{
    public enum GameMode
    {
        OnlinePvP,      // Гра онлайн
        LocalPvP,       // 2 гравці за 1 ПК  
        PvAI            // Гра з AI
    }
    public interface IGameSettingsService
    {
        int PlayerCount
        {
            get;
        }
        int ManaPerPlayer
        {
            get;
        }
        float SelectionTimeLimit
        {
            get;
        }
        string LobbyTimerId
        {
            get;
        }

        // ✅ НОВІ властивості
        GameMode CurrentGameMode
        {
            get; set;
        }
        bool IsAIEnabled
        {
            get;
        }
        bool IsLocalMultiplayer
        {
            get;
        }
        bool IsOnlineMode
        {
            get;
        }
    }

    public class GameSettingsService : IGameSettingsService
    {
        public GameMode CurrentGameMode { get; set; } = GameMode.PvAI; // За замовчуванням AI

        public int PlayerCount => CurrentGameMode == GameMode.PvAI ? 1 : 2;
        public int ManaPerPlayer => 4;
        public float SelectionTimeLimit => 300f;
        public string LobbyTimerId => "LobbySelectionTimer";

        // ✅ НОВІ властивості
        public bool IsAIEnabled => CurrentGameMode == GameMode.PvAI;
        public bool IsLocalMultiplayer => CurrentGameMode == GameMode.LocalPvP;
        public bool IsOnlineMode => CurrentGameMode == GameMode.OnlinePvP;
    }
}
