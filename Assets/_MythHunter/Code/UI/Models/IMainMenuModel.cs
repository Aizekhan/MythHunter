// IMainMenuModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Інтерфейс моделі головного меню
    /// </summary>
    public interface IMainMenuModel : MythHunter.UI.Core.IModel
    {
        string Title
        {
            get; set;
        }
        bool IsPlayButtonEnabled
        {
            get; set;
        }
        bool IsSettingsButtonEnabled
        {
            get; set;
        }
        bool IsExitButtonEnabled
        {
            get; set;
        }
    }
}
