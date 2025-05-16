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
    }

    public class GameSettingsService : IGameSettingsService
    {
        public int PlayerCount => 2; // 🔧 тимчасово хардкод, потім можна зробити меню налаштувань
    }
}
