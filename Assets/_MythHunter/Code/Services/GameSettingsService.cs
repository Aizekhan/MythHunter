// Шлях: Assets/_MythHunter/Code/Services/GameSettings/GameSettingsService.cs
using MythHunter.Core.DI;

namespace MythHunter.Services.GameSettings
{
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

        // ✅ ДОДАТИ ці два властивості:
        float SelectionTimeLimit
        {
            get;
        }
        string LobbyTimerId
        {
            get;
        }
    }

    public class GameSettingsService : IGameSettingsService
    {
        public int PlayerCount => 2; // 🔧 тимчасово хардкод, потім можна зробити меню налаштувань
        public int ManaPerPlayer => 4; // ← реалізація значення

        // ✅ ДОДАТИ ці дві реалізації:
        public float SelectionTimeLimit => 300f; // 5 хвилин на вибір героїв
        public string LobbyTimerId => "LobbySelectionTimer"; // ID таймера лобі
    }
}
