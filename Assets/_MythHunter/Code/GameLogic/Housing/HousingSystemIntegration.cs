using System;
using System.Collections.Generic;
using RPGFramework.Housing.Runtime.Contracts;
using RPGFramework.Housing.Runtime.Data;
using RPGFramework.Housing.Runtime.Implementations;

namespace MythHunter.GameLogic.Housing
{
    /// <summary>
    /// MythHunter integration layer for the Housing System.
    /// Bridges Framework housing with MythHunter game implementation.
    /// </summary>
    public class HousingSystemIntegration
    {
        private readonly IHousingManager _housingManager;
        private readonly Dictionary<string, FurnitureTemplate> _furnitureTemplates;
        private readonly Dictionary<string, HousePrefabConfig> _housePrefabs;

        /// <summary>
        /// Create a new housing system integration.
        /// </summary>
        public HousingSystemIntegration()
        {
            _housingManager = new HousingManager();
            _furnitureTemplates = new Dictionary<string, FurnitureTemplate>();
            _housePrefabs = new Dictionary<string, HousePrefabConfig>();

            InitializeFurnitureTemplates();
            InitializeHousePrefabs();
        }

        /// <summary>
        /// Get the housing manager.
        /// </summary>
        public IHousingManager HousingManager => _housingManager;

        /// <summary>
        /// Initialize furniture templates for MythHunter.
        /// </summary>
        private void InitializeFurnitureTemplates()
        {
            // Furniture Items
            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "wooden_chair",
                Name = "Wooden Chair",
                Category = FurnitureCategory.Furniture,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "wooden_table",
                Name = "Wooden Table",
                Category = FurnitureCategory.Furniture,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "comfortable_bed",
                Name = "Comfortable Bed",
                Category = FurnitureCategory.Furniture,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = false
            });

            // Storage Items
            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "wooden_chest",
                Name = "Wooden Chest",
                Category = FurnitureCategory.Storage,
                PlacementMode = PlacementMode.Floor,
                HasStorage = true,
                StorageCapacity = 50,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "iron_chest",
                Name = "Iron Chest",
                Category = FurnitureCategory.Storage,
                PlacementMode = PlacementMode.Floor,
                HasStorage = true,
                StorageCapacity = 100,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "wardrobe",
                Name = "Wardrobe",
                Category = FurnitureCategory.Storage,
                PlacementMode = PlacementMode.Floor,
                HasStorage = true,
                StorageCapacity = 75,
                IsCraftingStation = false
            });

            // Crafting Stations
            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "blacksmith_forge",
                Name = "Blacksmith Forge",
                Category = FurnitureCategory.Crafting,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = true,
                CraftingStationType = "Blacksmithing"
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "alchemy_table",
                Name = "Alchemy Table",
                Category = FurnitureCategory.Crafting,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = true,
                CraftingStationType = "Alchemy"
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "enchanting_table",
                Name = "Enchanting Table",
                Category = FurnitureCategory.Crafting,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = true,
                CraftingStationType = "Enchanting"
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "cooking_pot",
                Name = "Cooking Pot",
                Category = FurnitureCategory.Crafting,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = true,
                CraftingStationType = "Cooking"
            });

            // Decorations
            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "painting_landscape",
                Name = "Landscape Painting",
                Category = FurnitureCategory.Decoration,
                PlacementMode = PlacementMode.Wall,
                HasStorage = false,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "carpet_red",
                Name = "Red Carpet",
                Category = FurnitureCategory.Decoration,
                PlacementMode = PlacementMode.Floor,
                HasStorage = false,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "chandelier",
                Name = "Chandelier",
                Category = FurnitureCategory.Functional,
                PlacementMode = PlacementMode.Ceiling,
                HasStorage = false,
                IsCraftingStation = false
            });

            // Trophies
            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "dragon_head_trophy",
                Name = "Dragon Head Trophy",
                Category = FurnitureCategory.Trophy,
                PlacementMode = PlacementMode.Wall,
                HasStorage = false,
                IsCraftingStation = false
            });

            RegisterFurnitureTemplate(new FurnitureTemplate
            {
                TemplateId = "weapon_display_rack",
                Name = "Weapon Display Rack",
                Category = FurnitureCategory.Trophy,
                PlacementMode = PlacementMode.Wall,
                HasStorage = false,
                IsCraftingStation = false
            });
        }

        /// <summary>
        /// Initialize house prefab configurations.
        /// </summary>
        private void InitializeHousePrefabs()
        {
            RegisterHousePrefab(new HousePrefabConfig
            {
                HouseType = HouseType.Apartment,
                PrefabName = "Apartment_Prefab",
                BaseCapacity = 20,
                BaseCost = 1000,
                Description = "A modest apartment in the city"
            });

            RegisterHousePrefab(new HousePrefabConfig
            {
                HouseType = HouseType.Cottage,
                PrefabName = "Cottage_Prefab",
                BaseCapacity = 40,
                BaseCost = 5000,
                Description = "A cozy cottage with a small garden"
            });

            RegisterHousePrefab(new HousePrefabConfig
            {
                HouseType = HouseType.Manor,
                PrefabName = "Manor_Prefab",
                BaseCapacity = 80,
                BaseCost = 20000,
                Description = "A spacious manor with multiple rooms"
            });

            RegisterHousePrefab(new HousePrefabConfig
            {
                HouseType = HouseType.Castle,
                PrefabName = "Castle_Prefab",
                BaseCapacity = 150,
                BaseCost = 100000,
                Description = "A fortified castle with extensive grounds"
            });

            RegisterHousePrefab(new HousePrefabConfig
            {
                HouseType = HouseType.GuildHall,
                PrefabName = "GuildHall_Prefab",
                BaseCapacity = 200,
                BaseCost = 50000,
                Description = "A large guild hall for organizations"
            });
        }

        /// <summary>
        /// Register a furniture template.
        /// </summary>
        private void RegisterFurnitureTemplate(FurnitureTemplate template)
        {
            _furnitureTemplates[template.TemplateId] = template;
        }

        /// <summary>
        /// Register a house prefab configuration.
        /// </summary>
        private void RegisterHousePrefab(HousePrefabConfig config)
        {
            _housePrefabs[config.HouseType.ToString()] = config;
        }

        /// <summary>
        /// Get a furniture template by ID.
        /// </summary>
        public FurnitureTemplate GetFurnitureTemplate(string templateId)
        {
            return _furnitureTemplates.TryGetValue(templateId, out var template) ? template : null;
        }

        /// <summary>
        /// Get all furniture templates.
        /// </summary>
        public IReadOnlyDictionary<string, FurnitureTemplate> GetAllFurnitureTemplates()
        {
            return _furnitureTemplates;
        }

        /// <summary>
        /// Get a house prefab configuration.
        /// </summary>
        public HousePrefabConfig GetHousePrefabConfig(HouseType houseType)
        {
            return _housePrefabs.TryGetValue(houseType.ToString(), out var config) ? config : null;
        }

        /// <summary>
        /// Create furniture from a template.
        /// </summary>
        public IFurniture CreateFurnitureFromTemplate(string templateId, float x, float y, float z, float rotation = 0f)
        {
            var template = GetFurnitureTemplate(templateId);
            if (template == null)
                return null;

            var data = new FurniturePlacementData(
                Guid.NewGuid(),
                template.TemplateId,
                template.Name,
                template.Category,
                template.PlacementMode,
                x, y, z,
                rotation,
                template.HasStorage,
                template.StorageCapacity,
                template.IsCraftingStation,
                template.CraftingStationType
            );

            return new Furniture(data);
        }
    }

    /// <summary>
    /// Template for furniture creation.
    /// </summary>
    public class FurnitureTemplate
    {
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public FurnitureCategory Category { get; set; }
        public PlacementMode PlacementMode { get; set; }
        public bool HasStorage { get; set; }
        public int StorageCapacity { get; set; }
        public bool IsCraftingStation { get; set; }
        public string CraftingStationType { get; set; }
    }

    /// <summary>
    /// Configuration for house prefabs.
    /// </summary>
    public class HousePrefabConfig
    {
        public HouseType HouseType { get; set; }
        public string PrefabName { get; set; }
        public int BaseCapacity { get; set; }
        public int BaseCost { get; set; }
        public string Description { get; set; }
    }
}
