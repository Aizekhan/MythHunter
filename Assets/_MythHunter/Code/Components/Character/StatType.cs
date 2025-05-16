// Assets/_MythHunter/Code/Components/Character/StatType.cs

using System;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Типи характеристик героя
    /// </summary>
    public enum StatType
    {
        // Базові характеристики
        Level,
        HP,
        HpRegen,
        Stamina,
        StaminaRegen,

        // Магічні характеристики
        MagicChance,
        MagicPower,

        // Бойові характеристики - атака
        Damage,
        Weakness,
        AttackSpeed,
        AttackRate,
        MultipleAttackChance,
        AmountOfMultipleHits,
        Slowdown,

        // Бойові характеристики - захист
        ArmorDurability,
        ArmorResistance,
        ArmorReduction,
        ArmorCorrosion,
        BlockChance,
        BlockPenetrationChance,
        ArmorPenetrationChance,

        // Бойові характеристики - критичні удари
        CritChance,
        CritPower,

        // Бойові характеристики - ухилення/точність
        EvasionChance,
        AccuracyChance,

        // Бойові характеристики - станові ефекти
        StunChance,
        StunPower,
        ReturnDamageChance,
        ReturnDamagePower,
        CounterattackChance,
        DeathHitChance,

        // Бойові характеристики - ефекти отрути та кровотечі
        Poison,
        PoisonPower,
        Bleeding,
        BleedingPower,
        Plague,
        PlaguePower,

        // Бойові характеристики - вампіризм та крадіжка
        VampirikChance,
        VampirikPower,
        ThiefChance,

        // Економічні характеристики
        GoldPerTap,
        GoldPer8Hours,
        CommonItemChance,
        RareItemChance,
        EpicItemChance,
        LegendaryItemChance,
        UniqueItemChance,
        TotalUpgradeCost,
        UpgradeScale,

        // Інші характеристики
        BagSlots
    }

    /// <summary>
    /// Категорії характеристик для групування в редакторі та UI
    /// </summary>
    public enum StatCategory
    {
        Basic,      // Базові (рівень, здоров'я, витривалість)
        Combat,     // Бойові (атака, захист, крит тощо)
        Magic,      // Магічні (шанс магії, сила магії)
        Status,     // Ефекти статусу (отрута, кровотеча, чума)
        Economy,    // Економічні (золото, шанси предметів)
        Inventory   // Інвентар (слоти сумки тощо)
    }
}
