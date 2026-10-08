// MainMenuModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Модель даних головного меню
    /// </summary>
    public class MainMenuModel : IMainMenuModel
    {
        public string Title { get; set; } = "MythHunter";
        public bool IsPlayButtonEnabled { get; set; } = true;
        public bool IsSettingsButtonEnabled { get; set; } = true;
        public bool IsExitButtonEnabled { get; set; } = true;
    }
}
