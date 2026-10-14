# EPIC 11 Phase 2: Item System — Implementation Plan

**Date:** 2026-10-14
**Author:** Claude Code
**Status:** Ready for implementation
**Phase 1:** PR #71 (Inventory System) — ✅ COMPLETE
**Audit:** Documentation/EPIC-11-Gameplay-Domain-Audit.md

---

## 1. Executive Summary

**Goal:** Create a universal, reusable item system for RPG Framework that defines items, equipment, consumables, and their properties.

**Scope:**
- Core item interfaces (IItem, IEquipment, IConsumable)
- Item definition registry for centralized lookups
- Rarity and quality tier systems
- Equipment stat modifiers (integration with EPIC 10 Phase 2)
- Item instance management with unique IDs
- Stack size configuration for consumables

**Out of Scope (Later Phases):**
- Loot generation and drop tables — Phase 3
- Ability definitions and effects — Phase 4
- Progression/leveling requirements — Phase 5
- MythHunter adapters and UI integration — Phase 6

**Deliverables:**
- 15-18 Runtime C# files (interfaces, components, implementations)
- 1 Test C# file (~20-25 tests)
- .NET 8.0 test harness integration
- Static boundary validation
- GitHub Actions workflow
- Zero Unity dependencies
- Zero MythHunter coupling

**Estimated Effort:** 6-8 hours

---

## 2. Audit Findings Recap

From EPIC-11-Gameplay-Domain-Audit.md:

### Current MythHunter State

**Items are primitive:**
- Stored as string IDs in InventoryComponent
- UI layer has InventoryItem class (Name, Description, IsConsumable, Quantity)
- No item definitions, properties, or stat modifiers
- No rarity, quality, or tier systems
- No item requirements or restrictions

**Missing Features:**
- ❌ Item data definitions (ScriptableObjects, JSON, database)
- ❌ Item properties (damage, defense, stat bonuses)
- ❌ Item types/categories beyond "IsConsumable"
- ❌ Rarity/quality system
- ❌ Item level/tier system
- ❌ Durability or condition tracking
- ❌ Set bonuses or item relationships
- ❌ Item requirements (level, class, stats)
- ❌ Unique item instances (enchantments, sockets, modifications)

**Universal Patterns to Extract:**
1. **Item identity** (ID, Name, Description)
2. **Item categorization** (Equipment vs Consumable vs Quest)
3. **Item rarity** (Common, Rare, Epic, Legendary)
4. **Equipment slot types** (from Phase 1: generic TSlotType enum)
5. **Stack size limits** (from Phase 1: IItemStack interface)
6. **Stat modifiers** (from EPIC 10 Phase 2: IStatModifier)

---

## 3. Framework Design

### 3.1 Core Interfaces

#### IItem

Base interface for all items in the game.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Base interface for all items.
    /// Represents the definition/template of an item, not instances.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Unique identifier for this item type.
        /// Used for lookups in item registry.
        /// </summary>
        string ItemId { get; }

        /// <summary>
        /// Display name of the item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description text for tooltips/UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Category of this item (Equipment, Consumable, Quest, Crafting, etc.).
        /// </summary>
        ItemType ItemType { get; }

        /// <summary>
        /// Rarity tier (Common, Uncommon, Rare, Epic, Legendary, etc.).
        /// </summary>
        ItemRarity Rarity { get; }

        /// <summary>
        /// Quality tier (Normal, Magical, Rare, Unique, Set).
        /// </summary>
        ItemQuality Quality { get; }

        /// <summary>
        /// Maximum stack size for this item.
        /// 1 = not stackable, >1 = stackable.
        /// </summary>
        int MaxStackSize { get; }

        /// <summary>
        /// Item level/tier for requirements or scaling.
        /// </summary>
        int ItemLevel { get; }

        /// <summary>
        /// Icon identifier for UI rendering.
        /// Framework stores string ID, game layer maps to actual sprite.
        /// </summary>
        string IconId { get; }
    }
}
```

#### IEquipment

Equipment items that can be equipped in slots and provide stat modifiers.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Equipment item that can be equipped and provides stat bonuses.
    /// </summary>
    /// <typeparam name="TSlotType">Equipment slot type (Helmet, Chest, Weapon, etc.)</typeparam>
    /// <typeparam name="TStatType">Stat type for modifiers (Strength, Defense, etc.)</typeparam>
    /// <typeparam name="TValue">Stat value type (float, int, etc.)</typeparam>
    public interface IEquipment<TSlotType, TStatType, TValue> : IItem
        where TSlotType : System.Enum
        where TStatType : struct, System.IConvertible
        where TValue : struct, System.IComparable<TValue>
    {
        /// <summary>
        /// Equipment slot this item can be equipped in.
        /// </summary>
        TSlotType SlotType { get; }

        /// <summary>
        /// Stat modifiers provided when equipped.
        /// </summary>
        IReadOnlyList<RPGFramework.Stats.IStatModifier<TStatType, TValue>> StatModifiers { get; }

        /// <summary>
        /// Minimum level required to equip this item.
        /// 0 = no requirement.
        /// </summary>
        int MinimumLevel { get; }

        /// <summary>
        /// Maximum durability points.
        /// -1 = indestructible, 0 = already broken, >0 = has durability.
        /// </summary>
        int MaxDurability { get; }

        /// <summary>
        /// Whether this item can be repaired when durability drops.
        /// </summary>
        bool IsRepairable { get; }
    }
}
```

#### IConsumable

Consumable items that can be used for effects.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Consumable item that can be used once (potions, food, scrolls, etc.).
    /// </summary>
    public interface IConsumable : IItem
    {
        /// <summary>
        /// Cooldown category for shared cooldowns.
        /// Examples: "Potion", "Food", "Elixir"
        /// Items in same category share cooldown.
        /// Empty string = no cooldown category.
        /// </summary>
        string CooldownCategory { get; }

        /// <summary>
        /// Cooldown duration in seconds after use.
        /// 0 = no cooldown.
        /// </summary>
        float CooldownDuration { get; }

        /// <summary>
        /// Whether this consumable can be used in combat.
        /// </summary>
        bool UsableInCombat { get; }

        /// <summary>
        /// Whether this consumable is consumed on use (single-use).
        /// False = infinite uses (like quest items that trigger events).
        /// </summary>
        bool IsConsumedOnUse { get; }

        /// <summary>
        /// Effect type identifier for game-specific effect application.
        /// Examples: "RestoreHP", "RestoreMana", "ApplyBuff", "RemoveDébuff"
        /// Framework stores the ID, game layer implements effect logic.
        /// </summary>
        string EffectId { get; }

        /// <summary>
        /// Effect magnitude (heal amount, buff strength, etc.).
        /// Interpretation depends on EffectId.
        /// </summary>
        float EffectMagnitude { get; }
    }
}
```

#### IItemInstance

Represents a specific instance of an item with unique properties.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Represents a unique instance of an item.
    /// Wraps item definition with instance-specific data.
    /// </summary>
    public interface IItemInstance
    {
        /// <summary>
        /// Unique runtime instance ID.
        /// Generated when item is created/dropped.
        /// </summary>
        string InstanceId { get; }

        /// <summary>
        /// Reference to the item definition.
        /// </summary>
        string ItemDefinitionId { get; }

        /// <summary>
        /// Current stack quantity for stackable items.
        /// 1 for non-stackable items.
        /// </summary>
        int Quantity { get; set; }

        /// <summary>
        /// Current durability points (for equipment).
        /// -1 = not applicable, 0 = broken, >0 = current durability.
        /// </summary>
        int CurrentDurability { get; set; }

        /// <summary>
        /// Timestamp when this instance was created.
        /// </summary>
        float CreatedAt { get; }

        /// <summary>
        /// Custom instance properties (enchantments, sockets, etc.).
        /// Framework provides storage, game layer defines schema.
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> CustomProperties { get; }
    }
}
```

#### IItemRegistry

Central registry for item definitions.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Item definition registry for centralized lookups.
    /// Game layer populates with definitions from data sources.
    /// </summary>
    public interface IItemRegistry
    {
        /// <summary>
        /// Get item definition by ID.
        /// </summary>
        /// <returns>Item definition, or null if not found</returns>
        IItem GetItem(string itemId);

        /// <summary>
        /// Check if item exists in registry.
        /// </summary>
        bool HasItem(string itemId);

        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<IItem> GetItemsByType(ItemType itemType);

        /// <summary>
        /// Get all items of a specific rarity.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<IItem> GetItemsByRarity(ItemRarity rarity);

        /// <summary>
        /// Get all items of a specific quality.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<IItem> GetItemsByQuality(ItemQuality quality);

        /// <summary>
        /// Get all registered items.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<IItem> GetAllItems();

        /// <summary>
        /// Register a new item definition.
        /// </summary>
        void RegisterItem(IItem item);

        /// <summary>
        /// Unregister an item definition.
        /// </summary>
        bool UnregisterItem(string itemId);

        /// <summary>
        /// Clear all registered items.
        /// </summary>
        void Clear();
    }
}
```

### 3.2 Enumerations

#### ItemType

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Primary category of an item.
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// Equippable gear (weapons, armor, accessories).
        /// </summary>
        Equipment = 0,

        /// <summary>
        /// Single-use items (potions, food, scrolls).
        /// </summary>
        Consumable = 1,

        /// <summary>
        /// Quest-related items (not tradeable/droppable).
        /// </summary>
        Quest = 2,

        /// <summary>
        /// Crafting materials and resources.
        /// </summary>
        Crafting = 3,

        /// <summary>
        /// Currency items (gold, tokens, etc.).
        /// </summary>
        Currency = 4,

        /// <summary>
        /// Miscellaneous items that don't fit other categories.
        /// </summary>
        Miscellaneous = 5
    }
}
```

#### ItemRarity

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Rarity tier of an item.
    /// Affects drop rates, visual presentation, and value.
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>
        /// Common items (white/grey, abundant).
        /// </summary>
        Common = 0,

        /// <summary>
        /// Uncommon items (green, slightly better).
        /// </summary>
        Uncommon = 1,

        /// <summary>
        /// Rare items (blue, notable finds).
        /// </summary>
        Rare = 2,

        /// <summary>
        /// Epic items (purple, very rare).
        /// </summary>
        Epic = 3,

        /// <summary>
        /// Legendary items (orange/gold, extremely rare).
        /// </summary>
        Legendary = 4,

        /// <summary>
        /// Mythic items (red/pink, unique/artifact tier).
        /// </summary>
        Mythic = 5
    }
}
```

#### ItemQuality

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Quality tier of an item (orthogonal to rarity).
    /// Represents magical enhancement or crafting quality.
    /// </summary>
    public enum ItemQuality
    {
        /// <summary>
        /// Normal quality (no magical properties).
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Magical quality (minor enchantments).
        /// </summary>
        Magical = 1,

        /// <summary>
        /// Rare quality (multiple enchantments).
        /// </summary>
        Rare = 2,

        /// <summary>
        /// Unique quality (named items with special properties).
        /// </summary>
        Unique = 3,

        /// <summary>
        /// Set quality (part of item set with bonuses).
        /// </summary>
        Set = 4
    }
}
```

### 3.3 Components

#### ItemComponent

ECS component that attaches item data to entities.

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Component that marks an entity as representing an item in the world.
    /// Used for dropped items, loot bags, etc.
    /// </summary>
    public struct ItemComponent : RPGFramework.ECS.IComponent
    {
        /// <summary>
        /// Item instance data.
        /// </summary>
        public IItemInstance ItemInstance;

        /// <summary>
        /// Whether this item can be picked up by players.
        /// </summary>
        public bool IsPickupable;

        /// <summary>
        /// Entity ID of the owner (who dropped this item).
        /// -1 = no owner (world loot).
        /// </summary>
        public int OwnerEntityId;

        /// <summary>
        /// Timestamp when this item becomes available to all players.
        /// Used for loot reservation.
        /// </summary>
        public float AvailableAt;

        /// <summary>
        /// Timestamp when this item despawns.
        /// -1 = never despawns.
        /// </summary>
        public float DespawnAt;

        /// <summary>
        /// Factory: Create item component from instance.
        /// </summary>
        public static ItemComponent Create(IItemInstance instance, bool isPickupable = true)
        {
            return new ItemComponent
            {
                ItemInstance = instance,
                IsPickupable = isPickupable,
                OwnerEntityId = -1,
                AvailableAt = 0f,
                DespawnAt = -1f
            };
        }

        /// <summary>
        /// Factory: Create with owner and loot reservation.
        /// </summary>
        public static ItemComponent CreateWithOwner(
            IItemInstance instance,
            int ownerEntityId,
            float reservationDuration,
            float currentTime)
        {
            return new ItemComponent
            {
                ItemInstance = instance,
                IsPickupable = true,
                OwnerEntityId = ownerEntityId,
                AvailableAt = currentTime + reservationDuration,
                DespawnAt = -1f
            };
        }
    }
}
```

### 3.4 Events

#### ItemCreatedEvent

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Event raised when a new item instance is created.
    /// </summary>
    public struct ItemCreatedEvent
    {
        public string InstanceId;
        public string ItemDefinitionId;
        public int Quantity;
        public float Timestamp;
    }
}
```

#### ItemDestroyedEvent

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Event raised when an item instance is destroyed.
    /// </summary>
    public struct ItemDestroyedEvent
    {
        public string InstanceId;
        public string ItemDefinitionId;
        public int Quantity;
        public float Timestamp;
    }
}
```

#### ItemUsedEvent

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Event raised when a consumable item is used.
    /// </summary>
    public struct ItemUsedEvent
    {
        public int EntityId;          // Who used the item
        public string InstanceId;     // Which item instance
        public string ItemDefinitionId;
        public string EffectId;       // Effect applied
        public float EffectMagnitude;
        public float Timestamp;
    }
}
```

### 3.5 Implementations

#### Item (Default IItem Implementation)

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Default implementation of IItem.
    /// Immutable item definition.
    /// </summary>
    public class Item : IItem
    {
        public string ItemId { get; }
        public string Name { get; }
        public string Description { get; }
        public ItemType ItemType { get; }
        public ItemRarity Rarity { get; }
        public ItemQuality Quality { get; }
        public int MaxStackSize { get; }
        public int ItemLevel { get; }
        public string IconId { get; }

        public Item(
            string itemId,
            string name,
            string description,
            ItemType itemType,
            ItemRarity rarity = ItemRarity.Common,
            ItemQuality quality = ItemQuality.Normal,
            int maxStackSize = 1,
            int itemLevel = 1,
            string iconId = "")
        {
            ItemId = itemId;
            Name = name;
            Description = description;
            ItemType = itemType;
            Rarity = rarity;
            Quality = quality;
            MaxStackSize = maxStackSize;
            ItemLevel = itemLevel;
            IconId = iconId;
        }
    }
}
```

#### Equipment (Default IEquipment Implementation)

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Default implementation of IEquipment.
    /// </summary>
    public class Equipment<TSlotType, TStatType, TValue> : Item, IEquipment<TSlotType, TStatType, TValue>
        where TSlotType : System.Enum
        where TStatType : struct, System.IConvertible
        where TValue : struct, System.IComparable<TValue>
    {
        public TSlotType SlotType { get; }
        public IReadOnlyList<RPGFramework.Stats.IStatModifier<TStatType, TValue>> StatModifiers { get; }
        public int MinimumLevel { get; }
        public int MaxDurability { get; }
        public bool IsRepairable { get; }

        public Equipment(
            string itemId,
            string name,
            string description,
            TSlotType slotType,
            IReadOnlyList<RPGFramework.Stats.IStatModifier<TStatType, TValue>> statModifiers,
            ItemRarity rarity = ItemRarity.Common,
            ItemQuality quality = ItemQuality.Normal,
            int itemLevel = 1,
            int minimumLevel = 0,
            int maxDurability = -1,
            bool isRepairable = true,
            string iconId = "")
            : base(itemId, name, description, ItemType.Equipment, rarity, quality, 1, itemLevel, iconId)
        {
            SlotType = slotType;
            StatModifiers = statModifiers ?? new List<RPGFramework.Stats.IStatModifier<TStatType, TValue>>();
            MinimumLevel = minimumLevel;
            MaxDurability = maxDurability;
            IsRepairable = isRepairable;
        }
    }
}
```

#### Consumable (Default IConsumable Implementation)

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Default implementation of IConsumable.
    /// </summary>
    public class Consumable : Item, IConsumable
    {
        public string CooldownCategory { get; }
        public float CooldownDuration { get; }
        public bool UsableInCombat { get; }
        public bool IsConsumedOnUse { get; }
        public string EffectId { get; }
        public float EffectMagnitude { get; }

        public Consumable(
            string itemId,
            string name,
            string description,
            string effectId,
            float effectMagnitude,
            ItemRarity rarity = ItemRarity.Common,
            ItemQuality quality = ItemQuality.Normal,
            int maxStackSize = 20,
            int itemLevel = 1,
            string cooldownCategory = "",
            float cooldownDuration = 0f,
            bool usableInCombat = true,
            bool isConsumedOnUse = true,
            string iconId = "")
            : base(itemId, name, description, ItemType.Consumable, rarity, quality, maxStackSize, itemLevel, iconId)
        {
            EffectId = effectId;
            EffectMagnitude = effectMagnitude;
            CooldownCategory = cooldownCategory;
            CooldownDuration = cooldownDuration;
            UsableInCombat = usableInCombat;
            IsConsumedOnUse = isConsumedOnUse;
        }
    }
}
```

#### ItemInstance

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Default implementation of IItemInstance.
    /// </summary>
    public class ItemInstance : IItemInstance
    {
        public string InstanceId { get; }
        public string ItemDefinitionId { get; }
        public int Quantity { get; set; }
        public int CurrentDurability { get; set; }
        public float CreatedAt { get; }
        public System.Collections.Generic.Dictionary<string, object> CustomProperties { get; }

        public ItemInstance(
            string itemDefinitionId,
            int quantity = 1,
            int currentDurability = -1,
            float createdAt = 0f)
        {
            InstanceId = System.Guid.NewGuid().ToString();
            ItemDefinitionId = itemDefinitionId;
            Quantity = quantity;
            CurrentDurability = currentDurability;
            CreatedAt = createdAt;
            CustomProperties = new System.Collections.Generic.Dictionary<string, object>();
        }
    }
}
```

#### ItemRegistry

```csharp
namespace RPGFramework.Items
{
    /// <summary>
    /// Default implementation of IItemRegistry.
    /// Thread-safe singleton or can be injected via DI.
    /// </summary>
    public class ItemRegistry : IItemRegistry
    {
        private readonly System.Collections.Generic.Dictionary<string, IItem> _items;

        public ItemRegistry()
        {
            _items = new System.Collections.Generic.Dictionary<string, IItem>();
        }

        public IItem GetItem(string itemId)
        {
            return _items.TryGetValue(itemId, out var item) ? item : null;
        }

        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId);
        }

        public System.Collections.Generic.IReadOnlyList<IItem> GetItemsByType(ItemType itemType)
        {
            return _items.Values.Where(item => item.ItemType == itemType).ToList();
        }

        public System.Collections.Generic.IReadOnlyList<IItem> GetItemsByRarity(ItemRarity rarity)
        {
            return _items.Values.Where(item => item.Rarity == rarity).ToList();
        }

        public System.Collections.Generic.IReadOnlyList<IItem> GetItemsByQuality(ItemQuality quality)
        {
            return _items.Values.Where(item => item.Quality == quality).ToList();
        }

        public System.Collections.Generic.IReadOnlyList<IItem> GetAllItems()
        {
            return _items.Values.ToList();
        }

        public void RegisterItem(IItem item)
        {
            if (item == null)
                throw new System.ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(item.ItemId))
                throw new System.ArgumentException("Item ID cannot be null or empty");

            _items[item.ItemId] = item;
        }

        public bool UnregisterItem(string itemId)
        {
            return _items.Remove(itemId);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
```

---

## 4. Test Coverage Plan

### 4.1 Test File Structure

**File:** `Assets/_Framework/Items/Tests/ItemSystemTests.cs`

**Test Classes:**
1. `ItemTests` (6 tests)
2. `EquipmentTests` (6 tests)
3. `ConsumableTests` (5 tests)
4. `ItemInstanceTests` (4 tests)
5. `ItemRegistryTests` (6 tests)

**Total:** ~27 comprehensive tests

### 4.2 Test Cases

#### ItemTests

1. ✅ **CreateItem_WithValidParameters_SetsPropertiesCorrectly**
   - Create basic item with all parameters
   - Verify all properties match

2. ✅ **CreateItem_WithDefaults_UsesExpectedDefaults**
   - Create item with minimal parameters
   - Verify defaults: Rarity=Common, Quality=Normal, MaxStackSize=1, ItemLevel=1

3. ✅ **ItemType_Equipment_MatchesExpectedValue**
   - Create Equipment type item
   - Verify ItemType == ItemType.Equipment

4. ✅ **ItemType_Consumable_MatchesExpectedValue**
   - Create Consumable type item
   - Verify ItemType == ItemType.Consumable

5. ✅ **MaxStackSize_ForNonStackableItem_IsOne**
   - Create equipment (non-stackable)
   - Verify MaxStackSize == 1

6. ✅ **MaxStackSize_ForStackableItem_IsGreaterThanOne**
   - Create consumable with MaxStackSize=20
   - Verify MaxStackSize == 20

#### EquipmentTests

1. ✅ **CreateEquipment_WithStatModifiers_StoresModifiersCorrectly**
   - Create equipment with 2 stat modifiers
   - Verify StatModifiers.Count == 2

2. ✅ **EquipmentSlotType_MatchesConstructorParameter**
   - Create helmet equipment (SlotType=Head)
   - Verify SlotType == EquipmentSlot.Head

3. ✅ **MinimumLevel_SetCorrectly**
   - Create equipment with MinimumLevel=10
   - Verify MinimumLevel == 10

4. ✅ **MaxDurability_Indestructible_IsNegativeOne**
   - Create indestructible equipment (MaxDurability=-1)
   - Verify MaxDurability == -1

5. ✅ **MaxDurability_Destructible_IsPositive**
   - Create equipment with MaxDurability=100
   - Verify MaxDurability == 100

6. ✅ **EquipmentIsNotStackable_MaxStackSizeIsOne**
   - Create any equipment
   - Verify MaxStackSize == 1

#### ConsumableTests

1. ✅ **CreateConsumable_WithEffectId_StoresEffectCorrectly**
   - Create health potion (EffectId="RestoreHP", Magnitude=50)
   - Verify EffectId and EffectMagnitude

2. ✅ **CooldownCategory_SetCorrectly**
   - Create potion with CooldownCategory="Potion"
   - Verify CooldownCategory == "Potion"

3. ✅ **UsableInCombat_True_ByDefault**
   - Create consumable without specifying
   - Verify UsableInCombat == true

4. ✅ **IsConsumedOnUse_True_ByDefault**
   - Create consumable without specifying
   - Verify IsConsumedOnUse == true

5. ✅ **Consumable_IsStackable_MaxStackSizeGreaterThanOne**
   - Create consumable with MaxStackSize=20
   - Verify MaxStackSize == 20

#### ItemInstanceTests

1. ✅ **CreateItemInstance_GeneratesUniqueInstanceId**
   - Create two instances of same item definition
   - Verify InstanceId is different

2. ✅ **ItemInstance_InitialQuantity_MatchesConstructorParameter**
   - Create instance with Quantity=5
   - Verify Quantity == 5

3. ✅ **ItemInstance_CurrentDurability_MatchesConstructorParameter**
   - Create instance with CurrentDurability=75
   - Verify CurrentDurability == 75

4. ✅ **ItemInstance_CustomProperties_IsEmpty_Initially**
   - Create instance
   - Verify CustomProperties.Count == 0

#### ItemRegistryTests

1. ✅ **RegisterItem_AddsItemToRegistry**
   - Register item
   - Verify HasItem returns true

2. ✅ **GetItem_ReturnsRegisteredItem**
   - Register item
   - Verify GetItem returns same item

3. ✅ **GetItem_UnregisteredItem_ReturnsNull**
   - Try GetItem with non-existent ID
   - Verify returns null

4. ✅ **GetItemsByType_FiltersCorrectly**
   - Register 3 equipment, 2 consumables
   - Verify GetItemsByType(Equipment) returns 3 items

5. ✅ **GetItemsByRarity_FiltersCorrectly**
   - Register 2 Common, 1 Epic, 1 Legendary
   - Verify GetItemsByRarity(Epic) returns 1 item

6. ✅ **UnregisterItem_RemovesItemFromRegistry**
   - Register item, then unregister
   - Verify HasItem returns false

---

## 5. Integration Points

### 5.1 EPIC 10 Phase 2: Stat System

**Equipment Stat Modifiers:**
- Equipment implements `IEquipment<TSlotType, TStatType, TValue>`
- Provides `IReadOnlyList<IStatModifier<TStatType, TValue>>`
- When equipped, stat modifiers are added to entity's `StatContainer<TStatType, TValue>`
- When unequipped, modifiers are removed by ModifierId

**Example Integration (Game Layer):**
```csharp
// Equip armor piece
void EquipItem(int entityId, IEquipment<EquipmentSlot, StatType, float> equipment)
{
    var statContainer = entityManager.GetComponent<StatContainer<StatType, float>>(entityId);

    foreach (var modifier in equipment.StatModifiers)
    {
        statContainer.AddModifier(modifier.StatType, modifier);
    }
}
```

### 5.2 EPIC 10 Phase 1: Resource System

**Consumables Restore Resources:**
- Consumable with EffectId="RestoreHP" modifies `ResourceComponent<ResourceType>`
- Game layer interprets EffectMagnitude as restore amount

**Example Integration (Game Layer):**
```csharp
// Use health potion
void UseConsumable(int entityId, IConsumable consumable)
{
    if (consumable.EffectId == "RestoreHP")
    {
        var resource = entityManager.GetComponent<ResourceComponent<ResourceType>>(entityId);
        resource.AddCurrent(ResourceType.Health, (int)consumable.EffectMagnitude);
    }
}
```

### 5.3 EPIC 10 Phase 4: Status Effect System

**Consumables Apply Status Effects:**
- Consumable with EffectId="ApplyBuff" creates `IStatusEffect`
- Game layer maps EffectId to specific status effect type

**Example Integration (Game Layer):**
```csharp
// Use buff potion
void UseConsumable(int entityId, IConsumable consumable)
{
    if (consumable.EffectId == "ApplyBuff")
    {
        var statusEffectManager = GetStatusEffectManager(entityId);
        var buffEffect = CreateBuffFromEffectId(consumable.EffectId, consumable.EffectMagnitude);
        statusEffectManager.ApplyEffect(buffEffect);
    }
}
```

### 5.4 EPIC 11 Phase 1: Inventory System

**Items Go Into Containers:**
- `ItemInstance` stored in `IInventoryContainer<ItemInstance>`
- Equipment equipped in `IEquipmentSlot<TSlotType, ItemInstance>`
- Stack sizes enforced via `IItemStack`

**Example Integration:**
```csharp
// Add item to inventory
bool AddItemToInventory(int entityId, ItemInstance itemInstance, IItemRegistry registry)
{
    var inventory = entityManager.GetComponent<InventoryComponent<ItemInstance, EquipmentSlot>>(entityId);

    // Check if item is stackable
    var itemDef = registry.GetItem(itemInstance.ItemDefinitionId);
    if (itemDef.MaxStackSize > 1)
    {
        // Try to stack with existing
        foreach (var existingItem in inventory.PrimaryContainer.GetAllItems())
        {
            if (existingItem.ItemDefinitionId == itemInstance.ItemDefinitionId)
            {
                // Stack logic here
            }
        }
    }

    return inventory.PrimaryContainer.Add(itemInstance);
}
```

---

## 6. File Structure

```
Assets/_Framework/Items/
├── Runtime/
│   ├── Contracts/
│   │   ├── IItem.cs
│   │   ├── IEquipment.cs
│   │   ├── IConsumable.cs
│   │   ├── IItemInstance.cs
│   │   └── IItemRegistry.cs
│   ├── Enums/
│   │   ├── ItemType.cs
│   │   ├── ItemRarity.cs
│   │   └── ItemQuality.cs
│   ├── Components/
│   │   └── ItemComponent.cs
│   ├── Events/
│   │   ├── ItemCreatedEvent.cs
│   │   ├── ItemDestroyedEvent.cs
│   │   └── ItemUsedEvent.cs
│   ├── Implementations/
│   │   ├── Item.cs
│   │   ├── Equipment.cs
│   │   ├── Consumable.cs
│   │   ├── ItemInstance.cs
│   │   └── ItemRegistry.cs
│   └── RPGFramework.Items.Runtime.asmdef
├── Tests/
│   ├── ItemSystemTests.cs
│   └── RPGFramework.Items.Tests.asmdef
└── README.md

Build/
├── RPGFramework.Items.Runtime/
│   └── RPGFramework.Items.Runtime.csproj
├── RPGFramework.Items.Tests/
│   └── RPGFramework.Items.Tests.csproj
└── validate_framework_items_boundary.py

.github/workflows/
└── rpg-framework-items.yml
```

---

## 7. Assembly Definition

**File:** `Assets/_Framework/Items/Runtime/RPGFramework.Items.Runtime.asmdef`

```json
{
    "name": "RPGFramework.Items.Runtime",
    "rootNamespace": "RPGFramework.Items",
    "references": [
        "RPGFramework.ECS.Runtime",
        "RPGFramework.Stats.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

**Dependencies:**
- `RPGFramework.ECS.Runtime` — For IComponent interface
- `RPGFramework.Stats.Runtime` — For IStatModifier in equipment

---

## 8. Build Infrastructure

### 8.1 .NET Runtime Project

**File:** `Build/RPGFramework.Items.Runtime/RPGFramework.Items.Runtime.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>RPGFramework.Items</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="../../Assets/_Framework/Items/Runtime/Contracts/*.cs" Link="Contracts/%(Filename)%(Extension)" />
    <Compile Include="../../Assets/_Framework/Items/Runtime/Enums/*.cs" Link="Enums/%(Filename)%(Extension)" />
    <Compile Include="../../Assets/_Framework/Items/Runtime/Components/*.cs" Link="Components/%(Filename)%(Extension)" />
    <Compile Include="../../Assets/_Framework/Items/Runtime/Events/*.cs" Link="Events/%(Filename)%(Extension)" />
    <Compile Include="../../Assets/_Framework/Items/Runtime/Implementations/*.cs" Link="Implementations/%(Filename)%(Extension)" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../RPGFramework.ECS.Runtime/RPGFramework.ECS.Runtime.csproj" />
    <ProjectReference Include="../RPGFramework.Stats.Runtime/RPGFramework.Stats.Runtime.csproj" />
  </ItemGroup>
</Project>
```

### 8.2 .NET Test Project

**File:** `Build/RPGFramework.Items.Tests/RPGFramework.Items.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <RootNamespace>RPGFramework.Items.Tests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="../../Assets/_Framework/Items/Tests/ItemSystemTests.cs" Link="Tests/ItemSystemTests.cs" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../RPGFramework.Items.Runtime/RPGFramework.Items.Runtime.csproj" />
  </ItemGroup>
</Project>
```

### 8.3 Stats Runtime Project (Dependency)

**File:** `Build/RPGFramework.Stats.Runtime/RPGFramework.Stats.Runtime.csproj`

*(Already exists from EPIC 10 Phase 2, but verify it's configured correctly)*

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>RPGFramework.Stats</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="../../Assets/_Framework/Stats/Runtime/**/*.cs" Link="%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../RPGFramework.ECS.Runtime/RPGFramework.ECS.Runtime.csproj" />
  </ItemGroup>
</Project>
```

### 8.4 Boundary Validation Script

**File:** `Build/validate_framework_items_boundary.py`

```python
#!/usr/bin/env python3
"""
Static boundary validation for RPGFramework.Items.Runtime module.
Ensures zero Unity dependencies and zero MythHunter coupling.
"""

import os
import sys
from pathlib import Path

ITEMS_RUNTIME_PATH = Path("Assets/_Framework/Items/Runtime")

FORBIDDEN_IMPORTS = [
    "using UnityEngine",
    "using Unity.",
    "using MythHunter",
]

def scan_file(file_path):
    """Scan a single C# file for forbidden imports."""
    violations = []
    with open(file_path, 'r', encoding='utf-8') as f:
        for line_num, line in enumerate(f, start=1):
            for forbidden in FORBIDDEN_IMPORTS:
                if forbidden in line:
                    violations.append((line_num, forbidden, line.strip()))
    return violations

def main():
    print("[PASS] RPGFramework.Items.Runtime boundary validation")
    print(f"Scanning: {ITEMS_RUNTIME_PATH}")

    if not ITEMS_RUNTIME_PATH.exists():
        print(f"[FAIL] Path not found: {ITEMS_RUNTIME_PATH}")
        return 1

    cs_files = list(ITEMS_RUNTIME_PATH.rglob("*.cs"))
    print(f"Found {len(cs_files)} C# files")

    total_violations = 0
    for cs_file in cs_files:
        violations = scan_file(cs_file)
        if violations:
            print(f"\n[FAIL] {cs_file.relative_to('Assets/_Framework/Items/Runtime')}")
            for line_num, forbidden, line in violations:
                print(f"  Line {line_num}: {forbidden}")
                print(f"    {line}")
            total_violations += len(violations)

    if total_violations > 0:
        print(f"\n[FAIL] Found {total_violations} boundary violations")
        return 1

    print("\n[PASS] Zero Unity dependencies")
    print("[PASS] Zero MythHunter coupling")
    print("[PASS] Boundary validation complete")
    return 0

if __name__ == "__main__":
    sys.exit(main())
```

---

## 9. GitHub Actions Workflow

**File:** `.github/workflows/rpg-framework-items.yml`

```yaml
name: RPG Framework - Items Module

on:
  push:
    branches: [ dev, 'feature/epic11-phase2-*' ]
    paths:
      - 'Assets/_Framework/Items/**'
      - 'Build/RPGFramework.Items.*/**'
      - 'Build/validate_framework_items_boundary.py'
  pull_request:
    branches: [ dev ]
    paths:
      - 'Assets/_Framework/Items/**'
      - 'Build/RPGFramework.Items.*/**'
      - 'Build/validate_framework_items_boundary.py'

jobs:
  test-items-module:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Boundary Validation
        run: python Build/validate_framework_items_boundary.py

      - name: Restore Dependencies
        run: dotnet restore Build/RPGFramework.Items.Tests/RPGFramework.Items.Tests.csproj

      - name: Build
        run: dotnet build Build/RPGFramework.Items.Tests/RPGFramework.Items.Tests.csproj --no-restore

      - name: Run Tests
        run: dotnet test Build/RPGFramework.Items.Tests/RPGFramework.Items.Tests.csproj --no-build --verbosity normal
```

---

## 10. Implementation Sequence

### Phase 2A: Core Contracts and Enums (2 hours)
1. Create folder structure
2. Implement enums (ItemType, ItemRarity, ItemQuality)
3. Implement interfaces (IItem, IEquipment, IConsumable, IItemInstance, IItemRegistry)
4. Create assembly definition (reference ECS + Stats)
5. Create .NET runtime project

### Phase 2B: Implementations (2 hours)
6. Implement Item class
7. Implement Equipment class
8. Implement Consumable class
9. Implement ItemInstance class
10. Implement ItemRegistry class

### Phase 2C: Components and Events (1 hour)
11. Implement ItemComponent
12. Implement events (ItemCreatedEvent, ItemDestroyedEvent, ItemUsedEvent)

### Phase 2D: Testing (2-3 hours)
13. Create test assembly definition
14. Create .NET test project
15. Write 27 comprehensive tests
16. Run tests locally (.NET)
17. Fix any failures

### Phase 2E: Validation and CI (1 hour)
18. Create boundary validation script
19. Run boundary validation
20. Create GitHub Actions workflow
21. Commit and push to feature branch
22. Verify CI passes

### Phase 2F: Documentation (30 min)
23. Create Phase 2 summary document
24. Update roadmap STATUS.md
25. Create PR for review

---

## 11. Success Criteria

- ✅ All 27 tests passing (100% pass rate)
- ✅ .NET 8.0 compilation clean (no errors, no warnings)
- ✅ Static boundary validation passed (zero Unity deps, zero MythHunter coupling)
- ✅ GitHub Actions workflow GREEN
- ✅ Zero breaking changes to Phase 1 (Inventory)
- ✅ Zero breaking changes to EPIC 10 (Combat, Stats)
- ✅ Assembly definition configured with `noEngineReferences: true`
- ✅ All interfaces use generic type parameters for reusability
- ✅ Documentation complete (implementation plan + summary)

---

## 12. Risks and Mitigations

**Risk 1: Stat System Dependency**
- **Issue:** Equipment needs IStatModifier from Stats module
- **Mitigation:** Phase 1 already set precedent for framework-to-framework dependencies
- **Action:** Reference RPGFramework.Stats.Runtime in assembly definition

**Risk 2: Generic Type Complexity**
- **Issue:** IEquipment has 3 type parameters (TSlotType, TStatType, TValue)
- **Mitigation:** Provide concrete implementation classes to reduce boilerplate
- **Action:** Equipment<TSlotType, TStatType, TValue> handles complexity, game layer uses simpler types

**Risk 3: Item Registry Singleton**
- **Issue:** Global registry might conflict with DI patterns
- **Mitigation:** IItemRegistry interface allows injection
- **Action:** Provide default implementation, game layer can replace

**Risk 4: NUnit 4.x Assertion Syntax**
- **Issue:** Classic assertions deprecated (learned from Phase 1)
- **Mitigation:** Use constraint-based assertions from start
- **Action:** All tests use `Assert.That(actual, Is.EqualTo(expected))` pattern

---

## 13. Next Steps After Phase 2

**EPIC 11 Phase 3: Loot System**
- Loot tables and drop pools
- Rarity-based drop distribution
- Boss/mob-specific loot
- Random item generation

**EPIC 11 Phase 4: Ability System**
- Ability definitions
- Cooldown management system
- Resource cost tracking
- Ability effects framework

**EPIC 11 Phase 5: Progression System**
- XP and leveling
- Stat growth curves
- Unlock conditions
- Skill trees (optional)

**EPIC 11 Phase 6: Integration**
- MythHunter adapters
- Unity ScriptableObject integration
- UI model adapters
- End-to-end validation

---

**Document Status:** Ready for implementation
**Estimated Total Effort:** 6-8 hours
**Dependencies:** EPIC 10 Phase 2 (Stats), EPIC 11 Phase 1 (Inventory)
**Approval:** Awaiting user confirmation to proceed
