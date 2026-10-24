// Assets/_MythHunter/Code/Entities/Heroes/HeroFactory.cs
using RPGFramework.ECS;

using System;
using System.Collections.Generic;
using RPGFramework.DI;
using MythHunter.Entities.Archetypes;
using MythHunter.Components.Character;
using MythHunter.GameLogic.Inventory;
using MythHunter.GameLogic.Inventory.Components;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using MythHunter.Services.Heroes;
namespace MythHunter.Entities.Heroes
{
    public interface IHeroFactory
    {
        int CreateHero(string archetypeId, string heroName, int teamId);
        int CreateCustomHero(string archetypeId, Dictionary<Type, object> overrides = null);
        string[] GetAvailableHeroArchetypeIds();
        UniTask<int> CreateHeroFromDatabaseAsync(string heroId);
        UniTask SaveHeroToDatabaseAsync(int entityId, string heroId);
    }

    public class HeroFactory : IHeroFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly IHeroArchetypeRegistry _heroArchetypeRegistry;
        private readonly IHeroDataService _heroDataService;
        private readonly IMythLogger _logger;

        [Inject]
        public HeroFactory(
            IEntityManager entityManager,
            IArchetypeSystem archetypeSystem,
            IHeroArchetypeRegistry heroArchetypeRegistry,
            IHeroDataService heroDataService,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _archetypeSystem = archetypeSystem;
            _heroArchetypeRegistry = heroArchetypeRegistry;
            _heroDataService = heroDataService;
            _logger = logger;
        }

        // Створення героя з архетипу
        public int CreateHero(string archetypeId, string heroName, int teamId)
        {
            var overrides = new Dictionary<Type, object>();

            // Компонент ідентифікації
            overrides[typeof(HeroIdentityComponent)] = new HeroIdentityComponent
            {
                HeroID = Guid.NewGuid().ToString(),
                Name = heroName
            };

            // Командний компонент
            overrides[typeof(Components.Combat.TeamComponent)] = new Components.Combat.TeamComponent
            {
                TeamId = teamId
            };

            // Створюємо героя з архетипу через ArchetypeSystem
            int entityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created hero '{heroName}' with archetype '{archetypeId}' and ID {entityId}", "HeroFactory");
            }
            else
            {
                _logger.LogError($"Failed to create hero with archetype '{archetypeId}'", "HeroFactory");
            }

            return entityId;
        }

        // Створення героя з повністю власними параметрами
        public int CreateCustomHero(string archetypeId, Dictionary<Type, object> overrides = null)
        {
            int entityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created custom hero with archetype '{archetypeId}' and ID {entityId}", "HeroFactory");
            }
            else
            {
                _logger.LogError($"Failed to create custom hero with archetype '{archetypeId}'", "HeroFactory");
            }

            return entityId;
        }

        // Отримання доступних архетипів героїв
        public string[] GetAvailableHeroArchetypeIds()
        {
            var archetypes = _heroArchetypeRegistry.GetAllHeroArchetypes();
            var ids = new string[archetypes.Count];

            int index = 0;
            foreach (var kvp in archetypes)
            {
                ids[index++] = kvp.Key;
            }

            return ids;
        }

        // Створення героя з даних з бази даних
        public async UniTask<int> CreateHeroFromDatabaseAsync(string heroId)
        {
            try
            {
                // Завантаження даних героя з сервісу бази даних
                var heroData = await _heroDataService.GetHeroDataAsync(heroId);
                if (heroData == null)
                {
                    _logger.LogWarning($"Hero data not found for ID {heroId}", "HeroFactory");
                    return -1;
                }

                // Створення сутності
                int entityId = _entityManager.CreateEntity();

                // Компонент ідентифікації
                var identityComponent = new HeroIdentityComponent
                {
                    HeroID = heroData.HeroID,
                    Name = heroData.Name,
                    Race = heroData.Race,
                    Image = heroData.Image,
                    Skins = heroData.Skins != null ? new List<string>(heroData.Skins) : new List<string>()
                };
                _entityManager.AddComponent(entityId, identityComponent);

                // Компонент соціальних навичок
                var socialComponent = new SocialSkillsComponent
                {
                    Religion = heroData.SocialSkills.Religion,
                    Ideology = heroData.SocialSkills.Ideology,
                    Class = heroData.SocialSkills.Class,
                    Professions = new string[]
                    {
                        heroData.SocialSkills.Profa1,
                        heroData.SocialSkills.Profa2,
                        heroData.SocialSkills.Profa3
                    }
                };
                _entityManager.AddComponent(entityId, socialComponent);

                // Компонент характеристик
                var statsComponent = new StatsComponent();
                statsComponent.Values = new Dictionary<StatType, float>();

                // Заповнення базових характеристик
                foreach (var prop in heroData.BaseStats.GetType().GetProperties())
                {
                    if (Enum.TryParse<StatType>(prop.Name, out var statType))
                    {
                        var value = Convert.ToSingle(prop.GetValue(heroData.BaseStats));
                        statsComponent.SetStat(statType, value);
                    }
                }

                // Заповнення економічних характеристик
                foreach (var prop in heroData.BaseIncome.GetType().GetProperties())
                {
                    if (Enum.TryParse<StatType>(prop.Name, out var statType))
                    {
                        var value = Convert.ToSingle(prop.GetValue(heroData.BaseIncome));
                        statsComponent.SetStat(statType, value);
                    }
                }

                _entityManager.AddComponent(entityId, statsComponent);

                // Компонент навичок
                var skillsComponent = new SkillsComponent
                {
                    PassiveSkills = ConvertSkillArray(heroData.PassiveSkills),
                    ActiveSkills = ConvertSkillArray(heroData.ActiveSkills)
                };
                _entityManager.AddComponent(entityId, skillsComponent);

                // Компонент інвентаря (старий формат - для сумісності)
                var inventoryComponent = new InventoryComponent
                {
                    Items = heroData.Inventory != null ? new List<string>(heroData.Inventory) : new List<string>(),
                    EquippedItems = ConvertEquippedItems(heroData.EquippedItems),
                    Bag = ConvertBag(heroData.Bag)
                };
                _entityManager.AddComponent(entityId, inventoryComponent);

                // Новий компонент інвентаря (Framework-based, dual-component mode)
                int bagCapacity = heroData.Bag != null && heroData.Bag.Slots > 0 ? heroData.Bag.Slots : 50;
                var newInventoryComponent = MythHunterInventoryComponent.CreateHero(bagCapacity, autoSwitchWeapons: true);

                // Копіюємо items з старого компонента в новий
                if (heroData.Inventory != null)
                {
                    foreach (var itemId in heroData.Inventory)
                    {
                        if (!string.IsNullOrEmpty(itemId) && newInventoryComponent.Inventory.PrimaryContainer != null)
                        {
                            newInventoryComponent.Inventory.PrimaryContainer.Add(itemId);
                        }
                    }
                }

                // Копіюємо elixirs з Bag в новий контейнер
                if (heroData.Bag != null && heroData.Bag.Elixirs != null)
                {
                    foreach (var elixir in heroData.Bag.Elixirs)
                    {
                        if (!string.IsNullOrEmpty(elixir) && newInventoryComponent.Inventory.PrimaryContainer != null)
                        {
                            newInventoryComponent.Inventory.PrimaryContainer.Add(elixir);
                        }
                    }
                }

                // Копіюємо equipped items (мапимо зі старого формату в нові слоти)
                if (heroData.EquippedItems != null)
                {
                    var equipped = ConvertEquippedItems(heroData.EquippedItems);

                    // Armor
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Armor.Helmet, MythHunterEquipmentSlot.Helmet);
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Armor.Chest, MythHunterEquipmentSlot.Chest);
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Armor.Gloves, MythHunterEquipmentSlot.Gloves);
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Armor.Boots, MythHunterEquipmentSlot.Boots);

                    // Weapons
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Weapons.MainWeapon, MythHunterEquipmentSlot.MainWeapon);
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Weapons.SecondaryWeapon, MythHunterEquipmentSlot.SecondaryWeapon);
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Weapons.AuxiliaryWeapon, MythHunterEquipmentSlot.AuxiliaryWeapon);

                    // Artifact
                    EquipIfNotEmpty(ref newInventoryComponent, equipped.Artifact, MythHunterEquipmentSlot.Artifact);

                    // Set current weapon slot
                    if (!string.IsNullOrEmpty(equipped.Weapons.CurrentWeapon))
                    {
                        if (equipped.Weapons.CurrentWeapon == equipped.Weapons.MainWeapon)
                            newInventoryComponent.CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon;
                        else if (equipped.Weapons.CurrentWeapon == equipped.Weapons.SecondaryWeapon)
                            newInventoryComponent.CurrentWeaponSlot = MythHunterEquipmentSlot.SecondaryWeapon;
                        else if (equipped.Weapons.CurrentWeapon == equipped.Weapons.AuxiliaryWeapon)
                            newInventoryComponent.CurrentWeaponSlot = MythHunterEquipmentSlot.AuxiliaryWeapon;
                    }
                }

                _entityManager.AddComponent(entityId, newInventoryComponent);

                _logger.LogInfo($"Successfully created hero {heroData.Name} (ID: {heroData.HeroID}) from database with dual inventory components", "HeroFactory");
                return entityId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create hero from database: {ex.Message}", "HeroFactory", ex);
                return -1;
            }
        }

        // Збереження героя в базу даних
        public async UniTask SaveHeroToDatabaseAsync(int entityId, string heroId)
        {
            try
            {
                if (!_entityManager.HasComponent<HeroIdentityComponent>(entityId))
                {
                    _logger.LogWarning($"Entity {entityId} is not a hero", "HeroFactory");
                    return;
                }

                // Створення моделі даних героя для збереження
                var heroData = new HeroDataModel();

                // Отримання компонентів
                var identityComponent = _entityManager.GetComponent<HeroIdentityComponent>(entityId);
                var socialComponent = _entityManager.GetComponent<SocialSkillsComponent>(entityId);
                var statsComponent = _entityManager.GetComponent<StatsComponent>(entityId);
                var skillsComponent = _entityManager.GetComponent<SkillsComponent>(entityId);

                // Пробуємо отримати inventory з нового або старого компонента (dual-component support)
                InventoryComponent inventoryComponent;
                bool hasNewInventory = _entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv);

                if (hasNewInventory)
                {
                    // Конвертуємо з нового формату в старий для збереження в БД
                    inventoryComponent = ConvertNewInventoryToOld(newInv);
                }
                else
                {
                    // Використовуємо старий компонент якщо новий відсутній
                    inventoryComponent = _entityManager.GetComponent<InventoryComponent>(entityId);
                }

                // Заповнення базової інформації
                heroData.HeroID = heroId;
                heroData.Name = identityComponent.Name;
                heroData.Race = identityComponent.Race;
                heroData.Image = identityComponent.Image;
                heroData.Skins = identityComponent.Skins != null
                    ? identityComponent.Skins.ToArray()
                    : new string[0];

                // Заповнення соціальних навичок
                heroData.SocialSkills = new SocialSkillsModel
                {
                    Religion = socialComponent.Religion,
                    Ideology = socialComponent.Ideology,
                    Class = socialComponent.Class,
                    Profa1 = socialComponent.Professions.Length > 0 ? socialComponent.Professions[0] : "",
                    Profa2 = socialComponent.Professions.Length > 1 ? socialComponent.Professions[1] : "",
                    Profa3 = socialComponent.Professions.Length > 2 ? socialComponent.Professions[2] : ""
                };

                // Заповнення базових характеристик
                heroData.BaseStats = new BaseStatsModel();
                heroData.BaseIncome = new BaseIncomeModel();

                // Заповнення всіх характеристик
                foreach (var pair in statsComponent.Values)
                {
                    // Визначаємо, до якої категорії належить характеристика
                    var definition = MythHunter.Data.StatRegistry.GetDefinition(pair.Key);
                    if (definition != null)
                    {
                        if (definition.Category == StatCategory.Economy)
                        {
                            // Для економічних характеристик
                            var prop = typeof(BaseIncomeModel).GetProperty(pair.Key.ToString());
                            if (prop != null)
                            {
                                prop.SetValue(heroData.BaseIncome, pair.Value);
                            }
                        }
                        else
                        {
                            // Для інших характеристик
                            var prop = typeof(BaseStatsModel).GetProperty(pair.Key.ToString());
                            if (prop != null)
                            {
                                prop.SetValue(heroData.BaseStats, pair.Value);
                            }
                        }
                    }
                }

                // Заповнення навичок
                heroData.PassiveSkills = ConvertSkillList(skillsComponent.PassiveSkills);
                heroData.ActiveSkills = ConvertSkillList(skillsComponent.ActiveSkills);

                // Заповнення інвентаря
                heroData.Inventory = inventoryComponent.Items != null
                    ? inventoryComponent.Items.ToArray()
                    : new string[0];

                heroData.EquippedItems = ConvertEquippedItemsToModel(inventoryComponent.EquippedItems);
                heroData.Bag = ConvertBagToModel(inventoryComponent.Bag);

                // Збереження в базу даних
                await _heroDataService.SaveHeroDataAsync(heroData);

                _logger.LogInfo($"Successfully saved hero {heroData.Name} (ID: {heroData.HeroID}) to database", "HeroFactory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save hero to database: {ex.Message}", "HeroFactory", ex);
            }
        }

        #region Helper Methods

        private List<Skill> ConvertSkillArray(SkillModel[] skillModels)
        {
            if (skillModels == null)
                return new List<Skill>();

            var result = new List<Skill>(skillModels.Length);
            foreach (var model in skillModels)
            {
                result.Add(new Skill
                {
                    Id = model.id,
                    Level = model.level
                });
            }
            return result;
        }

        private SkillModel[] ConvertSkillList(List<Skill> skills)
        {
            if (skills == null)
                return new SkillModel[0];

            var result = new SkillModel[skills.Count];
            for (int i = 0; i < skills.Count; i++)
            {
                result[i] = new SkillModel
                {
                    id = skills[i].Id,
                    level = skills[i].Level
                };
            }
            return result;
        }

        private EquippedItems ConvertEquippedItems(EquippedItemsModel model)
        {
            if (model == null)
                return new EquippedItems();

            return new EquippedItems
            {
                Armor = new Armor
                {
                    // Продовження Assets/_MythHunter/Code/Entities/Heroes/HeroFactory.cs

                    Boots = model.Armor.Boots,
                    Gloves = model.Armor.Gloves,
                    Chest = model.Armor.Chest,
                    Helmet = model.Armor.Helmet
                },
                Artifact = model.Artifact,
                Weapons = new Weapons
                {
                    MainWeapon = model.Weapons.MainWeapon,
                    SecondaryWeapon = model.Weapons.SecondaryWeapon,
                    AuxiliaryWeapon = model.Weapons.AuxiliaryWeapon,
                    CurrentWeapon = model.Weapons.CurrentWeapon
                }
            };
        }

        private EquippedItemsModel ConvertEquippedItemsToModel(EquippedItems items)
        {
            return new EquippedItemsModel
            {
                Armor = new ArmorModel
                {
                    Boots = items.Armor.Boots,
                    Gloves = items.Armor.Gloves,
                    Chest = items.Armor.Chest,
                    Helmet = items.Armor.Helmet
                },
                Artifact = items.Artifact,
                Weapons = new WeaponsModel
                {
                    MainWeapon = items.Weapons.MainWeapon,
                    SecondaryWeapon = items.Weapons.SecondaryWeapon,
                    AuxiliaryWeapon = items.Weapons.AuxiliaryWeapon,
                    CurrentWeapon = items.Weapons.CurrentWeapon
                }
            };
        }

        private Bag ConvertBag(BagModel model)
        {
            if (model == null)
                return new Bag { Slots = 2, Elixirs = new List<string>() };

            return new Bag
            {
                Slots = model.Slots,
                Elixirs = model.Elixirs != null ? new List<string>(model.Elixirs) : new List<string>()
            };
        }

        private BagModel ConvertBagToModel(Bag bag)
        {
            return new BagModel
            {
                Slots = bag.Slots,
                Elixirs = bag.Elixirs != null ? bag.Elixirs.ToArray() : new string[0]
            };
        }

        private void EquipIfNotEmpty(ref MythHunterInventoryComponent inv, string itemId, MythHunterEquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(itemId))
                return;

            if (inv.Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
            {
                equipSlot.Equip(itemId);
            }
        }

        private InventoryComponent ConvertNewInventoryToOld(MythHunterInventoryComponent newInv)
        {
            var oldInv = new InventoryComponent
            {
                Items = new List<string>(),
                EquippedItems = new EquippedItems
                {
                    Armor = new Armor(),
                    Weapons = new Weapons()
                },
                Bag = new Bag
                {
                    Slots = newInv.Inventory.PrimaryContainer?.Capacity ?? 50,
                    Elixirs = new List<string>()
                }
            };

            // Копіюємо items з primary container
            if (newInv.Inventory.PrimaryContainer != null)
            {
                var allItems = newInv.Inventory.PrimaryContainer.GetAllItems();
                foreach (var item in allItems)
                {
                    oldInv.Items.Add(item);
                }
            }

            // Копіюємо equipped armor
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.Helmet, out var helmetSlot))
                oldInv.EquippedItems.Armor.Helmet = helmetSlot.EquippedItem;
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.Chest, out var chestSlot))
                oldInv.EquippedItems.Armor.Chest = chestSlot.EquippedItem;
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.Gloves, out var glovesSlot))
                oldInv.EquippedItems.Armor.Gloves = glovesSlot.EquippedItem;
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.Boots, out var bootsSlot))
                oldInv.EquippedItems.Armor.Boots = bootsSlot.EquippedItem;

            // Копіюємо equipped weapons
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.MainWeapon, out var mainWeaponSlot))
                oldInv.EquippedItems.Weapons.MainWeapon = mainWeaponSlot.EquippedItem;
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.SecondaryWeapon, out var secondaryWeaponSlot))
                oldInv.EquippedItems.Weapons.SecondaryWeapon = secondaryWeaponSlot.EquippedItem;
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.AuxiliaryWeapon, out var auxiliaryWeaponSlot))
                oldInv.EquippedItems.Weapons.AuxiliaryWeapon = auxiliaryWeaponSlot.EquippedItem;

            // Копіюємо current weapon
            if (newInv.Inventory.EquipmentSlots.TryGetValue(newInv.CurrentWeaponSlot, out var currentWeaponSlot))
                oldInv.EquippedItems.Weapons.CurrentWeapon = currentWeaponSlot.EquippedItem;

            // Копіюємо artifact
            if (newInv.Inventory.EquipmentSlots.TryGetValue(MythHunterEquipmentSlot.Artifact, out var artifactSlot))
                oldInv.EquippedItems.Artifact = artifactSlot.EquippedItem;

            return oldInv;
        }

        #endregion
    }
}
