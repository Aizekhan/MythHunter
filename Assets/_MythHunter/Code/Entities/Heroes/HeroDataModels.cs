// Assets/_MythHunter/Code/Entities/Heroes/HeroDataModels.cs

using System;

namespace MythHunter.Entities.Heroes
{
    /// <summary>
    /// Модель даних героя для серіалізації/десеріалізації
    /// </summary>
    [Serializable]
    public class HeroDataModel
    {
        public string HeroID
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Race
        {
            get; set;
        }
        public SocialSkillsModel SocialSkills { get; set; } = new SocialSkillsModel();
        public BaseStatsModel BaseStats { get; set; } = new BaseStatsModel();
        public BaseIncomeModel BaseIncome { get; set; } = new BaseIncomeModel();
        public string Image
        {
            get; set;
        }
        public string[] Skins { get; set; } = new string[0];
        public SkillModel[] PassiveSkills { get; set; } = new SkillModel[0];
        public SkillModel[] ActiveSkills { get; set; } = new SkillModel[0];
        public string[] Inventory { get; set; } = new string[0];
        public EquippedItemsModel EquippedItems { get; set; } = new EquippedItemsModel();
        public BagModel Bag { get; set; } = new BagModel();
    }

    /// <summary>
    /// Модель соціальних навичок
    /// </summary>
    [Serializable]
    public class SocialSkillsModel
    {
        public string Religion
        {
            get; set;
        }
        public string Ideology
        {
            get; set;
        }
        public string Class
        {
            get; set;
        }
        public string Profa1
        {
            get; set;
        }
        public string Profa2
        {
            get; set;
        }
        public string Profa3
        {
            get; set;
        }
    }

    /// <summary>
    /// Модель базових характеристик
    /// </summary>
    [Serializable]
    public class BaseStatsModel
    {
        public int Level { get; set; } = 1;
        public float HP { get; set; } = 100;
        public float HpRegen { get; set; } = 5;
        public float Stamina { get; set; } = 100;
        public float StaminaRegen { get; set; } = 5;
        public float MagicChance { get; set; } = 10;
        public float MagicPower { get; set; } = 50;
        public float Damage { get; set; } = 50;
        public float Weakness { get; set; } = 10;
        public float ArmorDurability { get; set; } = 100;
        public float ArmorResistance { get; set; } = 50;
        public float ArmorReduction { get; set; } = 10;
        public float ArmorCorrosion { get; set; } = 5;
        public float AttackSpeed { get; set; } = 1;
        public float AttackRate { get; set; } = 1;
        public float MultipleAttackChance { get; set; } = 5;
        public float AmountOfMultipleHits { get; set; } = 3;
        public float Slowdown { get; set; } = 2;
        public float BlockChance { get; set; } = 30;
        public float BlockPenetrationChance { get; set; } = 10;
        public float ArmorPenetrationChance { get; set; } = 20;
        public float CritChance { get; set; } = 15;
        public float CritPower { get; set; } = 50;
        public float EvasionChance { get; set; } = 25;
        public float AccuracyChance { get; set; } = 80;
        public float StunChance { get; set; } = 10;
        public float StunPower { get; set; } = 3;
        public float ReturnDamageChance { get; set; } = 10;
        public float ReturnDamagePower { get; set; } = 20;
        public float CounterattackChance { get; set; } = 5;
        public float DeathHitChance { get; set; } = 2;
        public float Poison { get; set; } = 10;
        public float PoisonPower { get; set; } = 5;
        public float Bleeding { get; set; } = 10;
        public float BleedingPower { get; set; } = 5;
        public float Plague { get; set; } = 10;
        public float PlaguePower { get; set; } = 5;
        public float VampirikChance { get; set; } = 10;
        public float VampirikPower { get; set; } = 5;
        public float ThiefChance { get; set; } = 5;
    }

    /// <summary>
    /// Модель економічних характеристик
    /// </summary>
    [Serializable]
    public class BaseIncomeModel
    {
        public float GoldPerTap { get; set; } = 10;
        public float GoldPer8Hours { get; set; } = 80;
        public float CommonItemChance { get; set; } = 10;
        public float RareItemChance { get; set; } = 5;
        public float EpicItemChance { get; set; } = 1;
        public float LegendaryItemChance { get; set; } = 0.5f;
        public float UniqueItemChance { get; set; } = 0.1f;
        public float TotalUpgradeCost { get; set; } = 1;
        public float UpgradeScale { get; set; } = 1;
    }

    /// <summary>
    /// Модель навички
    /// </summary>
    [Serializable]
    public class SkillModel
    {
        public string id
        {
            get; set;
        }
        public int level
        {
            get; set;
        }
    }

    /// <summary>
    /// Модель спорядженого обладнання
    /// </summary>
    [Serializable]
    public class EquippedItemsModel
    {
        public ArmorModel Armor { get; set; } = new ArmorModel();
        public string Artifact
        {
            get; set;
        }
        public WeaponsModel Weapons { get; set; } = new WeaponsModel();
    }

    /// <summary>
    /// Модель броні
    /// </summary>
    [Serializable]
    public class ArmorModel
    {
        public string Boots
        {
            get; set;
        }
        public string Gloves
        {
            get; set;
        }
        public string Chest
        {
            get; set;
        }
        public string Helmet
        {
            get; set;
        }
    }

    /// <summary>
    /// Модель зброї
    /// </summary>
    [Serializable]
    public class WeaponsModel
    {
        public string MainWeapon
        {
            get; set;
        }
        public string SecondaryWeapon
        {
            get; set;
        }
        public string AuxiliaryWeapon
        {
            get; set;
        }
        public string CurrentWeapon
        {
            get; set;
        }
    }

    /// <summary>
    /// Модель сумки
    /// </summary>
    [Serializable]
    public class BagModel
    {
        public int Slots { get; set; } = 2;
        public string[] Elixirs { get; set; } = new string[0];
    }
}
