// Path: Assets/_MythHunter/Code/Systems/Heroes/RacialBonuses.cs
using System.Collections.Generic;
using MythHunter.Components.Character;

namespace MythHunter.Systems.Heroes
{
    /// <summary>
    /// Бонуси від раси героя
    /// </summary>
    public class RacialBonuses
    {
        /// <summary>
        /// Бонуси до характеристик
        /// </summary>
        public Dictionary<StatType, float> StatBonuses { get; set; } = new Dictionary<StatType, float>();

        /// <summary>
        /// Ідентифікатор пасивної здібності раси
        /// </summary>
        public string PassiveAbilityId
        {
            get; set;
        }
    }
}
