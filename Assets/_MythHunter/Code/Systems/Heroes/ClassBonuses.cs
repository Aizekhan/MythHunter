// Path: Assets/_MythHunter/Code/Systems/Heroes/ClassBonuses.cs
using System.Collections.Generic;
using MythHunter.Components.Character;

namespace MythHunter.Systems.Heroes
{
    /// <summary>
    /// Бонуси від класу героя
    /// </summary>
    public class ClassBonuses
    {
        /// <summary>
        /// Бонуси до характеристик
        /// </summary>
        public Dictionary<StatType, float> StatBonuses { get; set; } = new Dictionary<StatType, float>();

        /// <summary>
        /// Ідентифікатор пасивної здібності класу
        /// </summary>
        public string PassiveAbilityId
        {
            get; set;
        }

        /// <summary>
        /// Ідентифікатор активної здібності класу
        /// </summary>
        public string ActiveAbilityId
        {
            get; set;
        }
    }
}
