# EPIC 13 Phase 1 — Inventory + Items Integration Plan

**Date:** 2026-10-16
**Goal:** Integrate RPG Framework Inventory and Items modules into MythHunter game layer
**Pattern:** Follow EPIC 12 Combat Integration approach

---

## Context

### Completed Work
- **EPIC 10:** Extracted Combat domain modules (Stats, Resources, Damage, Status Effects, Death/Life)
- **EPIC 11:** Extracted Gameplay modules (Inventory, Items, Loot, Abilities, Progression)
- **EPIC 12:** Integrated Combat modules into MythHunter (all 4 phases complete)

### Current State
- Framework Inventory/Items modules exist but NOT integrated into MythHunter
- Old MythHunter `InventoryComponent` uses string IDs and custom serialization
- Only 3 files use old component: `HeroFactory.cs`, `HeroArchetypeRegistry.cs`, `InventoryComponent.cs`

---

## Framework Modules Overview

### RPGFramework.Inventory.Runtime
**Generic Contracts:**
- `IInventoryContainer<TItem>` — Container with capacity management
- `IEquipmentSlot<TSlotType, TItem>` — Equipment slot by type
- `IItemStack` — Stackable items with quantity
- `InventoryComponent<TItem, TSlotType>` — ECS component

**Features:**
- Unlimited or fixed capacity containers
- Multiple containers per entity (primary, bank, quest items, etc.)
- Equipment slots keyed by enum type
- Factory methods: `Create()`, `CreateUnlimited()`, `CreateSimple()`

### RPGFramework.Items.Runtime
**Contracts:**
- `IItem` — Base item definition
- `IItemInstance` — Runtime item instance
- `IEquipment` — Equipable items
- `IConsumable` — Consumable items
- `IItemRegistry` — Item database

**Enums:**
- `ItemType` — Weapon, Armor, Consumable, Quest, etc.
- `ItemRarity` — Common, Uncommon, Rare, Epic, Legendary
- `ItemQuality` — Poor, Normal, Superior, etc.

**Events:**
- `ItemCreatedEvent`, `ItemDestroyedEvent`, `ItemUsedEvent`

---

## Old MythHunter InventoryComponent Analysis

### Current Structure
```csharp
public struct InventoryComponent : ISerializableComponent
{
    public List<string> Items;              // Generic item list (string IDs)
    public EquippedItems EquippedItems;     // Hardcoded equipment structure
    public Bag Bag;                         // Backpack with elixirs
}

public struct EquippedItems
{
    public Armor Armor;      // Helmet, Chest, Gloves, Boots
    public string Artifact;
    public Weapons Weapons;  // Main, Secondary, Auxiliary, Current
}

public struct Bag
{
    public int Slots;
    public List<string> Elixirs;
}
```

### Issues with Old Component
1. **String-based IDs:** No type safety, hard to validate
2. **Hardcoded slots:** Cannot extend equipment types easily
3. **Custom serialization:** Manual binary serialization code
4. **No stacking:** Elixirs stored as separate string entries
5. **No item instances:** Cannot track item state (durability, enchantments, etc.)

### Usage Points
1. **HeroFactory.cs** — Creates heroes with initial inventory
2. **HeroArchetypeRegistry.cs** — Defines hero templates with starting items

---

## Integration Design

### 1. MythHunter Equipment Slot Enum

Define all possible equipment slots for MythHunter:

```csharp
namespace MythHunter.GameLogic.Inventory
{
    public enum MythHunterEquipmentSlot
    {
        // Armor slots
        Helmet = 0,
        Chest = 1,
        Gloves = 2,
        Boots = 3,

        // Weapon slots
        MainWeapon = 10,
        SecondaryWeapon = 11,
        AuxiliaryWeapon = 12,

        // Special slots
        Artifact = 20,

        // Future extensibility
        Ring1 = 30,
        Ring2 = 31,
        Necklace = 32,
        Belt = 33
    }
}
```

### 2. MythHunterInventoryComponent

Wrapper around Framework `InventoryComponent<string, MythHunterEquipmentSlot>`:

```csharp
namespace MythHunter.GameLogic.Inventory.Components
{
    using RPGFramework.Inventory.Runtime;
    using RPGFramework.ECS;

    /// <summary>
    /// MythHunter-specific inventory component.
    /// Wraps Framework InventoryComponent with string item IDs and MythHunter equipment slots.
    /// </summary>
    public struct MythHunterInventoryComponent : IComponent
    {
        /// <summary>
        /// Framework inventory component (uses string IDs for compatibility)
        /// </summary>
        public InventoryComponent<string, MythHunterEquipmentSlot> Inventory;

        /// <summary>
        /// Metadata: Current weapon (MainWeapon, SecondaryWeapon, etc.)
        /// Tracks which weapon slot is actively used in combat.
        /// </summary>
        public MythHunterEquipmentSlot CurrentWeaponSlot;

        /// <summary>
        /// Factory: Create hero inventory with standard equipment slots
        /// </summary>
        public static MythHunterInventoryComponent CreateHero(int bagCapacity)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.Create(
                    bagCapacity,
                    MythHunterEquipmentSlot.Helmet,
                    MythHunterEquipmentSlot.Chest,
                    MythHunterEquipmentSlot.Gloves,
                    MythHunterEquipmentSlot.Boots,
                    MythHunterEquipmentSlot.MainWeapon,
                    MythHunterEquipmentSlot.SecondaryWeapon,
                    MythHunterEquipmentSlot.AuxiliaryWeapon,
                    MythHunterEquipmentSlot.Artifact
                ),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon
            };
        }

        /// <summary>
        /// Factory: Create monster/NPC inventory (simpler, no equipment)
        /// </summary>
        public static MythHunterInventoryComponent CreateMonster(int lootCapacity)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.CreateSimple(lootCapacity),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon
            };
        }
    }
}
```

### 3. MythHunterItemComponent

Wrapper for item instances:

```csharp
namespace MythHunter.GameLogic.Inventory.Components
{
    using RPGFramework.Items.Runtime;
    using RPGFramework.ECS;

    /// <summary>
    /// Component for entities that ARE items (loot drops, world items)
    /// </summary>
    public struct MythHunterItemComponent : IComponent
    {
        public string ItemId;           // Reference to item definition
        public int Quantity;            // Stack size
        public ItemQuality Quality;     // Poor, Normal, Superior
        public ItemRarity Rarity;       // Common, Rare, Epic, Legendary

        // Future: Durability, Enchantments, CustomData
    }
}
```

### 4. Integration Helper

Similar to `CombatSystemIntegration` from EPIC 12:

```csharp
namespace MythHunter.GameLogic.Inventory.Adapters
{
    /// <summary>
    /// Helper for inventory operations with dual-component support.
    /// Supports both old InventoryComponent and new MythHunterInventoryComponent.
    /// </summary>
    public class InventorySystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        public InventorySystemIntegration(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        // Dual-component API methods
        public bool AddItem(int entityId, string itemId);
        public bool RemoveItem(int entityId, string itemId);
        public bool EquipItem(int entityId, string itemId, MythHunterEquipmentSlot slot);
        public bool UnequipItem(int entityId, MythHunterEquipmentSlot slot);
        public string GetEquippedItem(int entityId, MythHunterEquipmentSlot slot);
        public int GetInventoryCount(int entityId);
        public bool IsInventoryFull(int entityId);
        public IReadOnlyList<string> GetAllItems(int entityId);
    }
}
```

### 5. Migration Utility

Convert old `InventoryComponent` → new `MythHunterInventoryComponent`:

```csharp
namespace MythHunter.GameLogic.Inventory.Migration
{
    public class InventoryComponentMigration
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        public int MigrateAllEntities();
        public bool MigrateEntity(int entityId);
        public bool RollbackEntity(int entityId);
        public MigrationStatistics GetStatistics();

        // Migration logic:
        // 1. Read old InventoryComponent
        // 2. Create new MythHunterInventoryComponent
        // 3. Copy items to PrimaryContainer
        // 4. Map equipment slots:
        //    - Helmet → MythHunterEquipmentSlot.Helmet
        //    - Chest → MythHunterEquipmentSlot.Chest
        //    - MainWeapon → MythHunterEquipmentSlot.MainWeapon
        //    - etc.
        // 5. Preserve old component for safety
        // 6. Add new component
    }
}
```

---

## Implementation Phases

### Phase 1a: Core Components (This Phase)
**Goal:** Create MythHunter wrapper components and adapters

**Deliverables:**
1. `MythHunterEquipmentSlot` enum
2. `MythHunterInventoryComponent` struct with factory methods
3. `MythHunterItemComponent` struct
4. `InventorySystemIntegration` helper class
5. `InventoryComponentMigration` utility
6. Comprehensive unit tests

**Files to Create:**
- `Assets/_MythHunter/Code/GameLogic/Inventory/` (new directory)
  - `Enums/MythHunterEquipmentSlot.cs`
  - `Components/MythHunterInventoryComponent.cs`
  - `Components/MythHunterItemComponent.cs`
  - `Adapters/InventorySystemIntegration.cs`
  - `Migration/InventoryComponentMigration.cs`
- `Assets/_MythHunter/Tests/Inventory/` (new directory)
  - `MythHunterInventoryComponentTests.cs`
  - `InventorySystemIntegrationTests.cs`
  - `InventoryComponentMigrationTests.cs`

**Assembly References:**
- `RPGFramework.Inventory.Runtime`
- `RPGFramework.Items.Runtime`
- `RPGFramework.ECS`
- `MythHunter.Core` (existing)

### Phase 1b: System Updates
**Goal:** Update MythHunter systems to use new components

**Files to Update:**
- `HeroFactory.cs` — Use `MythHunterInventoryComponent.CreateHero()`
- `HeroArchetypeRegistry.cs` — Update hero templates

### Phase 1c: Migration & Validation
**Goal:** Migrate existing entities and validate

**Tasks:**
1. Unity Editor migration utility (menu commands)
2. Migrate entities in scenes/prefabs (if any)
3. Validate with tests
4. Document migration results

---

## Testing Strategy

### Unit Tests
1. **MythHunterInventoryComponent:**
   - Factory methods create correct structure
   - Equipment slots properly initialized
   - Current weapon tracking works

2. **InventorySystemIntegration:**
   - AddItem/RemoveItem work with both old and new components
   - EquipItem/UnequipItem handle equipment slots correctly
   - Dual-component support seamless

3. **InventoryComponentMigration:**
   - Migration preserves all items
   - Equipment slots map correctly
   - Old component preserved for rollback
   - Statistics tracking accurate

### Integration Tests
1. Create hero with new component
2. Add items to inventory
3. Equip items to slots
4. Unequip items
5. Switch weapons
6. Verify capacity limits
7. Test migration from old component

---

## Success Criteria

**Phase 1 Complete When:**
- ✅ All components created and tested
- ✅ Integration helper supports dual-component architecture
- ✅ Migration utility tested
- ✅ Unity compilation successful
- ✅ All tests passing (target: 30+ tests)
- ✅ HeroFactory updated to use new component
- ✅ Documentation created

**Exit Gates:**
- Unity EditMode tests: 100% pass rate
- No compilation errors
- Backward compatibility maintained
- Old component still works alongside new

---

## Known Limitations & Future Work

### Current Limitations
1. **String IDs only:** Not yet using `IItemInstance` from Framework
2. **No item registry:** Item definitions still hardcoded in game
3. **No stacking logic:** Each item separate in container
4. **No durability/enchantments:** Item state tracking deferred

### Future Enhancements (Later EPICs)
1. **Item Registry Integration:**
   - Use `IItemRegistry` to store item definitions
   - Replace string IDs with item instance objects

2. **Stack Support:**
   - Use `IItemStack` for stackable items (elixirs, arrows, etc.)
   - Automatic stack merging/splitting

3. **Item Instances:**
   - Track item state (durability, enchantments, sockets)
   - Unique items with custom properties

4. **Loot System Integration:**
   - Connect `RPGFramework.Loot.Runtime` for enemy drops
   - Rarity-based loot generation

---

## Rollback Plan

**If Issues Occur:**
1. Old `InventoryComponent` preserved during migration
2. Can remove new components and revert to old
3. Git revert to commit before integration
4. `InventoryComponentMigration.RollbackEntity()` available

**Safe Migration Strategy:**
- Keep both components simultaneously
- Gradual transition over multiple sessions
- Remove old component only after extended validation period

---

## Timeline Estimate

**Phase 1a (Core Components):** 2-3 hours
- Component creation: 30 min
- Adapter implementation: 45 min
- Migration utility: 45 min
- Tests: 60 min

**Phase 1b (System Updates):** 30 min
- HeroFactory update: 15 min
- HeroArchetypeRegistry update: 15 min

**Phase 1c (Migration & Validation):** 1 hour
- Unity compilation: 15 min
- Test execution: 15 min
- Migration validation: 30 min

**Total:** ~4 hours (can be split across sessions)

---

## References

**EPIC 12 Combat Integration:**
- Pattern established: Wrapper components + Adapters + Migration
- PR #76 (Phase 1): `c52304d`
- Phases 2-4: `23dadc6`, `1302b07`, `84d807c`

**Framework Modules:**
- `Assets/_Framework/Inventory/Runtime/`
- `Assets/_Framework/Items/Runtime/`

**Documentation:**
- `Documentation/EPIC-12-Phases-2-3-Summary.md`
- `Documentation/EPIC-12-Phase4-Validation-Results.md`

---

**Status:** Planning complete, ready to implement
**Next:** Begin Phase 1a implementation
