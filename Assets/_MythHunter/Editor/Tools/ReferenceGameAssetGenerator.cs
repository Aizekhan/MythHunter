// Assets/_MythHunter/Editor/Tools/ReferenceGameAssetGenerator.cs

using UnityEngine;
using UnityEditor;
using RPGFramework.Tools.QuestEditor;
using RPGFramework.Tools.LootTableDesigner;
using RPGFramework.Tools.ItemDatabaseManager;
using RPGFramework.Quests;
using RPGFramework.Items;
using System.IO;
using System.Collections.Generic;

namespace MythHunter.Editor.Tools
{
    /// <summary>
    /// Generates all ScriptableObject assets for Reference Game "Goblin Valley".
    /// Uses Inspector Tooling (Quest Editor, Loot Table Designer, Item Database Manager).
    /// </summary>
    public static class ReferenceGameAssetGenerator
    {
        private const string BASE_PATH = "Assets/_MythHunter/ReferenceGame";
        private const string QUESTS_PATH = BASE_PATH + "/Quests";
        private const string LOOT_TABLES_PATH = BASE_PATH + "/LootTables";
        private const string ITEMS_PATH = BASE_PATH + "/Items";
        private const string DATABASES_PATH = BASE_PATH + "/Databases";

        [MenuItem("Tools/Reference Game/Generate All Assets")]
        public static void GenerateAllAssets()
        {
            if (!EditorUtility.DisplayDialog(
                "Generate Reference Game Assets",
                "This will create all quests, items, and loot tables for the 'Goblin Valley' reference game.\n\n" +
                "Location: Assets/_MythHunter/ReferenceGame/\n\n" +
                "Continue?",
                "Generate",
                "Cancel"))
            {
                return;
            }

            CreateDirectories();

            var items = GenerateItems();
            var lootTables = GenerateLootTables();
            var quests = GenerateQuests();

            // Create databases
            var itemDatabase = CreateItemDatabase(items);
            var lootTableDatabase = CreateLootTableDatabase(lootTables);
            var questDatabase = CreateQuestDatabase(quests);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Generation Complete",
                $"Reference Game assets generated successfully!\n\n" +
                $"Items: {items.Count}\n" +
                $"Loot Tables: {lootTables.Count}\n" +
                $"Quests: {quests.Count}\n\n" +
                $"Location: {BASE_PATH}",
                "OK");

            Debug.Log($"Reference Game Assets Generated:\n" +
                     $"  Items: {items.Count}\n" +
                     $"  Loot Tables: {lootTables.Count}\n" +
                     $"  Quests: {quests.Count}\n" +
                     $"  Databases: 3\n" +
                     $"Location: {BASE_PATH}");
        }

        private static void CreateDirectories()
        {
            Directory.CreateDirectory(QUESTS_PATH);
            Directory.CreateDirectory(LOOT_TABLES_PATH);
            Directory.CreateDirectory(ITEMS_PATH);
            Directory.CreateDirectory(DATABASES_PATH);
        }

        #region Items

        private static List<ItemAsset> GenerateItems()
        {
            var items = new List<ItemAsset>();

            // Equipment - Weapons
            items.Add(CreateWeapon("Rusty Dagger", "rusty_dagger", 1, ItemRarity.Common, 5));
            items.Add(CreateWeapon("Iron Sword", "iron_sword", 2, ItemRarity.Uncommon, 15));
            items.Add(CreateWeapon("Steel Sword", "steel_sword", 4, ItemRarity.Rare, 30));
            items.Add(CreateWeapon("King's Blade", "kings_blade", 5, ItemRarity.Epic, 50, extraStats: new[] {
                new StatModifier { statName = "Health", value = 20 }
            }));

            // Equipment - Armor
            items.Add(CreateArmor("Leather Gloves", "leather_gloves", 1, ItemRarity.Common, 5, "Hands"));
            items.Add(CreateArmor("Leather Armor", "leather_armor", 2, ItemRarity.Uncommon, 15, "Chest"));
            items.Add(CreateArmor("Plate Armor", "plate_armor", 4, ItemRarity.Rare, 30, "Chest", extraStats: new[] {
                new StatModifier { statName = "Health", value = 10 }
            }));
            items.Add(CreateArmor("King's Crown", "kings_crown", 5, ItemRarity.Epic, 10, "Head", extraStats: new[] {
                new StatModifier { statName = "Health", value = 50 }
            }));

            // Equipment - Accessories
            items.Add(CreateAccessory("Simple Ring", "simple_ring", 1, ItemRarity.Common, new[] {
                new StatModifier { statName = "Health", value = 10 }
            }));
            items.Add(CreateAccessory("Enchanted Ring", "enchanted_ring", 3, ItemRarity.Rare, new[] {
                new StatModifier { statName = "Health", value = 30 },
                new StatModifier { statName = "Mana", value = 10 }
            }));
            items.Add(CreateAccessory("Goblin King's Treasure", "goblin_kings_treasure", 5, ItemRarity.Legendary, new[] {
                new StatModifier { statName = "Health", value = 100 },
                new StatModifier { statName = "Mana", value = 50 },
                new StatModifier { statName = "Damage", value = 20 }
            }));

            // Consumables
            items.Add(CreatePotion("Health Potion Minor", "health_potion_minor", 50, ItemRarity.Common));
            items.Add(CreatePotion("Health Potion", "health_potion", 100, ItemRarity.Common));
            items.Add(CreatePotion("Health Potion Greater", "health_potion_greater", 250, ItemRarity.Uncommon));
            items.Add(CreatePotion("Mana Potion", "mana_potion", 50, ItemRarity.Common));

            // Quest Items
            items.Add(CreateQuestItem("Goblin Ear", "goblin_ear", "A goblin ear for Elder Garrick"));
            items.Add(CreateQuestItem("Boss Soul Fragment", "boss_soul_fragment", "Soul fragment from the Goblin King"));

            // Crafting Materials
            items.Add(CreateCraftingMaterial("Leather Scrap", "leather_scrap", 999));
            items.Add(CreateCraftingMaterial("Arrows", "arrows", 999));

            // Currency
            items.Add(CreateCurrency("Gold Coins", "gold_coins", 9999));

            return items;
        }

        private static ItemAsset CreateWeapon(string name, string id, int level, ItemRarity rarity, float damage, StatModifier[] extraStats = null)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = level;
            item.itemType = ItemType.Equipment;
            item.rarity = rarity;
            item.equipmentSlot = "MainHand";
            item.maxStackSize = 1;
            item.description = $"A {rarity.ToString().ToLower()} weapon that deals {damage} damage.";
            item.iconId = $"icon_{id}";

            var stats = new List<StatModifier> { new StatModifier { statName = "Damage", value = damage } };
            if (extraStats != null) stats.AddRange(extraStats);
            item.statModifiers = stats.ToArray();

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreateArmor(string name, string id, int level, ItemRarity rarity, float defense, string slot, StatModifier[] extraStats = null)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = level;
            item.itemType = ItemType.Equipment;
            item.rarity = rarity;
            item.equipmentSlot = slot;
            item.maxStackSize = 1;
            item.description = $"A {rarity.ToString().ToLower()} armor piece that provides {defense} defense.";
            item.iconId = $"icon_{id}";

            var stats = new List<StatModifier> { new StatModifier { statName = "Defense", value = defense } };
            if (extraStats != null) stats.AddRange(extraStats);
            item.statModifiers = stats.ToArray();

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreateAccessory(string name, string id, int level, ItemRarity rarity, StatModifier[] stats)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = level;
            item.itemType = ItemType.Equipment;
            item.rarity = rarity;
            item.equipmentSlot = "Ring";
            item.maxStackSize = 1;
            item.description = $"A {rarity.ToString().ToLower()} accessory with magical properties.";
            item.iconId = $"icon_{id}";
            item.statModifiers = stats;

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreatePotion(string name, string id, float restoreAmount, ItemRarity rarity)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = 1;
            item.itemType = ItemType.Consumable;
            item.rarity = rarity;
            item.maxStackSize = 99;
            item.description = $"Restores {restoreAmount} health.";
            item.iconId = $"icon_{id}";
            item.useDuration = 0f;
            item.cooldown = 30f;
            item.consumeOnUse = true;

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreateQuestItem(string name, string id, string description)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = 1;
            item.itemType = ItemType.Quest;
            item.rarity = ItemRarity.Rare;
            item.maxStackSize = 1;
            item.description = description;
            item.iconId = $"icon_{id}";

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreateCraftingMaterial(string name, string id, int stackSize)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = 1;
            item.itemType = ItemType.Crafting;
            item.rarity = ItemRarity.Common;
            item.maxStackSize = stackSize;
            item.description = $"Crafting material for various recipes.";
            item.iconId = $"icon_{id}";

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        private static ItemAsset CreateCurrency(string name, string id, int stackSize)
        {
            var item = ScriptableObject.CreateInstance<ItemAsset>();
            item.itemName = name;
            item.itemId = id;
            item.itemLevel = 1;
            item.itemType = ItemType.Currency;
            item.rarity = ItemRarity.Common;
            item.maxStackSize = stackSize;
            item.description = "Universal currency.";
            item.iconId = $"icon_{id}";

            CreateAsset(item, ITEMS_PATH, $"{name.Replace(" ", "")}.asset");
            return item;
        }

        #endregion

        #region Loot Tables

        private static List<LootTableAsset> GenerateLootTables()
        {
            var tables = new List<LootTableAsset>();

            tables.Add(CreateGoblinScoutLoot());
            tables.Add(CreateGoblinWarriorLoot());
            tables.Add(CreateGoblinEliteLoot());
            tables.Add(CreateGoblinKingLoot());

            return tables;
        }

        private static LootTableAsset CreateGoblinScoutLoot()
        {
            var table = ScriptableObject.CreateInstance<LootTableAsset>();
            table.tableId = "goblin_scout_loot";
            table.tableName = "Goblin Scout Loot";
            table.minDrops = 1;
            table.maxDrops = 2;
            table.allowDuplicates = true;

            table.entries = new List<LootEntryData>
            {
                new LootEntryData { itemId = "gold_coins", weight = 100, dropChance = 0.8f, minQuantity = 5, maxQuantity = 15 },
                new LootEntryData { itemId = "health_potion_minor", weight = 60, dropChance = 0.5f, minQuantity = 1, maxQuantity = 1 },
                new LootEntryData { itemId = "leather_scrap", weight = 40, dropChance = 0.3f, minQuantity = 1, maxQuantity = 3 }
            };

            table.guaranteedDrops = new List<string> { "goblin_ear" };

            CreateAsset(table, LOOT_TABLES_PATH, "GoblinScoutLoot.asset");
            return table;
        }

        private static LootTableAsset CreateGoblinWarriorLoot()
        {
            var table = ScriptableObject.CreateInstance<LootTableAsset>();
            table.tableId = "goblin_warrior_loot";
            table.tableName = "Goblin Warrior Loot";
            table.minDrops = 1;
            table.maxDrops = 3;
            table.allowDuplicates = true;

            table.entries = new List<LootEntryData>
            {
                new LootEntryData { itemId = "gold_coins", weight = 100, dropChance = 0.9f, minQuantity = 15, maxQuantity = 30 },
                new LootEntryData { itemId = "health_potion", weight = 70, dropChance = 0.4f, minQuantity = 1, maxQuantity = 1 },
                new LootEntryData { itemId = "rusty_dagger", weight = 30, dropChance = 0.2f, minQuantity = 1, maxQuantity = 1 }
            };

            CreateAsset(table, LOOT_TABLES_PATH, "GoblinWarriorLoot.asset");
            return table;
        }

        private static LootTableAsset CreateGoblinEliteLoot()
        {
            var table = ScriptableObject.CreateInstance<LootTableAsset>();
            table.tableId = "goblin_elite_loot";
            table.tableName = "Goblin Elite Loot";
            table.minDrops = 2;
            table.maxDrops = 4;
            table.allowDuplicates = true;

            table.entries = new List<LootEntryData>
            {
                new LootEntryData { itemId = "gold_coins", weight = 100, dropChance = 1f, minQuantity = 50, maxQuantity = 100 },
                new LootEntryData { itemId = "health_potion_greater", weight = 80, dropChance = 0.7f, minQuantity = 1, maxQuantity = 1 },
                new LootEntryData { itemId = "iron_sword", weight = 40, dropChance = 0.3f, minQuantity = 1, maxQuantity = 1 },
                new LootEntryData { itemId = "enchanted_ring", weight = 20, dropChance = 0.1f, minQuantity = 1, maxQuantity = 1 }
            };

            CreateAsset(table, LOOT_TABLES_PATH, "GoblinEliteLoot.asset");
            return table;
        }

        private static LootTableAsset CreateGoblinKingLoot()
        {
            var table = ScriptableObject.CreateInstance<LootTableAsset>();
            table.tableId = "goblin_king_loot";
            table.tableName = "Goblin King Boss Loot";
            table.minDrops = 5;
            table.maxDrops = 8;
            table.allowDuplicates = false;

            table.entries = new List<LootEntryData>
            {
                new LootEntryData { itemId = "gold_coins", weight = 100, dropChance = 1f, minQuantity = 500, maxQuantity = 1000 }
            };

            table.guaranteedDrops = new List<string>
            {
                "kings_crown",
                "goblin_kings_treasure",
                "boss_soul_fragment"
            };

            CreateAsset(table, LOOT_TABLES_PATH, "GoblinKingLoot.asset");
            return table;
        }

        #endregion

        #region Quests

        private static List<QuestAsset> GenerateQuests()
        {
            var quests = new List<QuestAsset>();

            // Tutorial Quests
            quests.Add(CreateQuest1_WelcomeToThornwood());
            quests.Add(CreateQuest2_FirstBlood());
            quests.Add(CreateQuest3_GatherResources());

            // Main Quest Chain
            quests.Add(CreateQuest4_TheGoblinThreat());
            quests.Add(CreateQuest5_RescueVillagers());
            quests.Add(CreateQuest6_AncientPower());
            quests.Add(CreateQuest7_ElitePatrol());
            quests.Add(CreateQuest8_TheGoblinKing());

            return quests;
        }

        private static QuestAsset CreateQuest1_WelcomeToThornwood()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_welcome_to_thornwood";
            quest.title = "Welcome to Thornwood";
            quest.description = "Speak with the village elders to learn about Thornwood.";
            quest.minimumLevel = 1;

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "talk_to_elder",
                    type = ObjectiveType.Interact,
                    targetId = "npc_elder_garrick",
                    requiredCount = 1,
                    description = "Speak with Elder Garrick"
                },
                new QuestObjective
                {
                    objectiveId = "talk_to_merchant",
                    type = ObjectiveType.Interact,
                    targetId = "npc_merchant_mira",
                    requiredCount = 1,
                    description = "Speak with Merchant Mira"
                },
                new QuestObjective
                {
                    objectiveId = "talk_to_blacksmith",
                    type = ObjectiveType.Interact,
                    targetId = "npc_blacksmith_torvin",
                    requiredCount = 1,
                    description = "Speak with Blacksmith Torvin"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 50 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 10 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q1_WelcomeToThornwood.asset");
            return quest;
        }

        private static QuestAsset CreateQuest2_FirstBlood()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_first_blood";
            quest.title = "First Blood";
            quest.description = "Kill 3 goblins in Valley Entrance to prove your combat skills.";
            quest.minimumLevel = 1;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q1_WelcomeToThornwood.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "kill_goblins",
                    type = ObjectiveType.Kill,
                    targetId = "goblin_scout",
                    requiredCount = 3,
                    description = "Kill 3 Goblin Scouts"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 100 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 25 },
                new QuestReward { type = RewardType.Item, rewardId = "health_potion_minor", amount = 2 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q2_FirstBlood.asset");
            return quest;
        }

        private static QuestAsset CreateQuest3_GatherResources()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_gather_resources";
            quest.title = "Gather Resources";
            quest.description = "Collect 5 goblin ears for Elder Garrick.";
            quest.minimumLevel = 1;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q2_FirstBlood.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "collect_ears",
                    type = ObjectiveType.Collect,
                    targetId = "goblin_ear",
                    requiredCount = 5,
                    description = "Collect 5 Goblin Ears"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 150 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 50 },
                new QuestReward { type = RewardType.Item, rewardId = "leather_gloves", amount = 1 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q3_GatherResources.asset");
            return quest;
        }

        private static QuestAsset CreateQuest4_TheGoblinThreat()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_goblin_threat";
            quest.title = "The Goblin Threat";
            quest.description = "Kill 10 goblins to reduce their numbers.";
            quest.minimumLevel = 2;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q3_GatherResources.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "kill_any_goblins",
                    type = ObjectiveType.Kill,
                    targetId = "goblin_any",
                    requiredCount = 10,
                    description = "Kill 10 Goblins"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 250 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 100 },
                new QuestReward { type = RewardType.Item, rewardId = "iron_sword", amount = 1 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q4_TheGoblinThreat.asset");
            return quest;
        }

        private static QuestAsset CreateQuest5_RescueVillagers()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_rescue_villagers";
            quest.title = "Rescue the Villagers";
            quest.description = "Free 3 captured villagers from the Goblin Camp.";
            quest.minimumLevel = 3;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q4_TheGoblinThreat.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "free_villagers",
                    type = ObjectiveType.Interact,
                    targetId = "captured_villager",
                    requiredCount = 3,
                    description = "Free 3 Captured Villagers"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 300 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 150 },
                new QuestReward { type = RewardType.Item, rewardId = "health_potion", amount = 5 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q5_RescueVillagers.asset");
            return quest;
        }

        private static QuestAsset CreateQuest6_AncientPower()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_ancient_power";
            quest.title = "Ancient Power";
            quest.description = "Discover the Ancient Ruins hidden in Goblin Valley.";
            quest.minimumLevel = 3;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q5_RescueVillagers.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "discover_ruins",
                    type = ObjectiveType.Explore,
                    targetId = "ancient_ruins",
                    requiredCount = 1,
                    description = "Discover the Ancient Ruins"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 350 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 200 },
                new QuestReward { type = RewardType.Item, rewardId = "enchanted_ring", amount = 1 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q6_AncientPower.asset");
            return quest;
        }

        private static QuestAsset CreateQuest7_ElitePatrol()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_elite_patrol";
            quest.title = "Elite Patrol";
            quest.description = "Defeat 3 Goblin Elites in the Ancient Ruins.";
            quest.minimumLevel = 4;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q6_AncientPower.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "kill_elites",
                    type = ObjectiveType.Kill,
                    targetId = "goblin_elite",
                    requiredCount = 3,
                    description = "Defeat 3 Goblin Elites"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 500 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 300 },
                new QuestReward { type = RewardType.Item, rewardId = "plate_armor", amount = 1 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q7_ElitePatrol.asset");
            return quest;
        }

        private static QuestAsset CreateQuest8_TheGoblinKing()
        {
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            quest.questId = "quest_goblin_king";
            quest.title = "The Goblin King";
            quest.description = "Defeat the Goblin King and end the threat to Thornwood.";
            quest.minimumLevel = 5;
            quest.prerequisiteQuests = new[] { AssetDatabase.LoadAssetAtPath<QuestAsset>(QUESTS_PATH + "/Q7_ElitePatrol.asset") };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    objectiveId = "kill_goblin_king",
                    type = ObjectiveType.Kill,
                    targetId = "goblin_king",
                    requiredCount = 1,
                    description = "Defeat the Goblin King"
                }
            };

            quest.rewards = new[]
            {
                new QuestReward { type = RewardType.Experience, rewardId = "reward_xp", amount = 1000 },
                new QuestReward { type = RewardType.Currency, rewardId = "reward_gold", amount = 1000 }
            };

            CreateAsset(quest, QUESTS_PATH, "Q8_TheGoblinKing.asset");
            return quest;
        }

        #endregion

        #region Databases

        private static ItemDatabase CreateItemDatabase(List<ItemAsset> items)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            database.items = items;
            database.UpdateStatistics();

            CreateAsset(database, DATABASES_PATH, "ReferenceGame_ItemDatabase.asset");
            return database;
        }

        private static LootTableDatabase CreateLootTableDatabase(List<LootTableAsset> lootTables)
        {
            var database = ScriptableObject.CreateInstance<LootTableDatabase>();
            database.lootTables = lootTables;
            database.UpdateStatistics();

            CreateAsset(database, DATABASES_PATH, "ReferenceGame_LootTableDatabase.asset");
            return database;
        }

        private static QuestDatabase CreateQuestDatabase(List<QuestAsset> quests)
        {
            var database = ScriptableObject.CreateInstance<QuestDatabase>();
            database.quests = quests;
            database.UpdateStatistics();

            CreateAsset(database, DATABASES_PATH, "ReferenceGame_QuestDatabase.asset");
            return database;
        }

        #endregion

        private static void CreateAsset(ScriptableObject asset, string directory, string fileName)
        {
            string path = Path.Combine(directory, fileName);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
