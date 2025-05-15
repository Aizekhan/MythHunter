// Assets/_MythHunter/Code/UI/Models/HeroCardModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Модель картки героя
    /// </summary>
    public class HeroCardModel
    {
        public string ArchetypeId
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public string Race
        {
            get; set;
        }
        public string Class
        {
            get; set;
        }
        public int ManaCost
        {
            get; set;
        }
        public string IconPath
        {
            get; set;
        }
        public bool IsSelected
        {
            get; set;
        }
        public bool IsSelectable
        {
            get; set;
        }
    }
}
