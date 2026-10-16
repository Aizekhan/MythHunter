// Assets/_MythHunter/Code/GameLogic/Combat/MythHunterResourceType.cs

namespace MythHunter.GameLogic.Combat
{
    /// <summary>
    /// Types of resources used in MythHunter combat
    /// </summary>
    public enum MythHunterResourceType
    {
        /// <summary>Health points - entity dies when depleted</summary>
        Health,

        /// <summary>Mana points - used for abilities and spells</summary>
        Mana,

        /// <summary>Stamina - used for physical actions</summary>
        Stamina,

        /// <summary>Rage - builds up during combat, used for powerful attacks</summary>
        Rage,

        /// <summary>Concentration - used for focused abilities and planning</summary>
        Concentration
    }
}
