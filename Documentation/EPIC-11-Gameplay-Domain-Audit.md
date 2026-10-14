# EPIC 11: Gameplay Domain Modules — Full Codebase Audit

**Date:** 2026-10-14
**Author:** Claude Code
**Status:** Initial audit for EPIC 11 Phase 1
**Previous EPIC:** EPIC 10 (RPG Foundation Domain Modules) — ✅ COMPLETE

---

## Executive Summary

This audit examines MythHunter's gameplay systems (inventory, items, abilities, loot, progression) to identify candidates for extraction into universal RPG Framework modules. The goal is to create reusable, game-agnostic systems that follow the successful pattern established in EPIC 10.

**Key Findings:**

- ✅ **Inventory system exists** — Basic implementation with equipment slots and bag storage
- ⚠️ **Item system minimal** — Items stored as string IDs, no item definition or property system
- ⚠️ **Ability system exists** — Basic implementation but coupled to combat, limited cooldown tracking
- ❌ **No loot system** — No loot tables, drop pools, or reward generation
- ❌ **No progression system** — No XP, leveling, or unlock mechanics
- ⚠️ **Cooldown tracking primitive** — Single hardcoded cooldown in CombatSystem, not generic

**Extraction Potential:**

1. **High:** Inventory container logic, equipment slots, item stacking
2. **High:** Item definition contracts, stat modifiers, rarity/quality tiers
3. **Medium:** Ability definitions, cooldown management, resource costs
4. **High:** Loot generation (new, not yet implemented)
5. **High:** Progression/XP system (new, not yet implemented)

**Recommended Phase Order:**

1. **Phase 1:** Inventory System (containers, slots, capacity)
2. **Phase 2:** Item System (definitions, properties, equipment, stat integration)
3. **Phase 3:** Loot System (tables, drop pools, rarity-based generation)
4. **Phase 4:** Ability System (definitions, cooldowns, costs, effects)
5. **Phase 5:** Progression System (XP, levels, stat growth, unlocks)
6. **Phase 6:** Integration (MythHunter adapters, end-to-end validation)

---

## 1. Inventory System Analysis

### 1.1 Current Implementation

#### InventoryComponent.cs

**Location:** `Assets/_MythHunter/Code/Components/Character/InventoryComponent.cs`

**Structure:**
```csharp
public struct InventoryComponent : ISerializableComponent
{
    public List<string> Items;           // Generic item list (string IDs)
    public EquippedItems EquippedItems;  // Equipped gear
    public Bag Bag;                      // Consumables bag
}

public struct EquippedItems
{
    public Armor Armor;      // 4 armor slots
    public string Artifact;  // 1 artifact slot
    public Weapons Weapons;  // 3 weapon slots + current
}

public struct Armor
{
    public string Boots;
    public string Gloves;
    public string Chest;
    public string Helmet;
}

public struct Weapons
{
    public string MainWeapon;
    public string SecondaryWeapon;
    public string AuxiliaryWeapon;
    public string CurrentWeapon;  // Active weapon reference
}

public struct Bag
{
    public int Slots;           // Maximum capacity
    public List<string> Elixirs; // Consumable items
}
```

**Analysis:**

✅ **Universal Patterns:**
- Container concept (Items list, Bag)
- Slot-based equipment (Armor, Weapons, Artifact)
- Capacity constraints (Bag.Slots)
- Item references (string IDs)

⚠️ **Game-Specific Hardcoding:**
- Fixed armor slot names (Boots, Gloves, Chest, Helmet)
- Fixed weapon slot names (Main, Secondary, Auxiliary)
- "Artifact" as single special slot
- "Elixirs" as consumable category
- CurrentWeapon pointer (weapon switching logic)

❌ **Missing Generic Features:**
- No item stacking logic
- No weight/encumbrance system
- No slot type restrictions (can equip anything anywhere?)
- No multi-container support (single bag only)
- No item instance data (everything is just an ID)

**Serialization:**
- Manual binary serialization (writes each field)
- Not using EPIC 06 serialization framework
- Tightly coupled to specific field structure

#### UI Layer — InventoryModel.cs

**Location:** `Assets/_MythHunter/Code/UI/Models/InventoryModel.cs`

**Structure:**
```csharp
public class InventoryModel : IInventoryModel
{
    private const int MAX_ITEMS = 10;
    private List<InventoryItem> _items;

    public void AddItem(InventoryItem item);
    public void RemoveItem(int index);
    public void UseItem(int index);  // TODO: Apply effect
}

public class InventoryItem
{
    public string Id;
    public string Name;
    public string Description;
    public bool IsConsumable;
    public int Quantity;  // Not actually used for stacking!
}
```

**Analysis:**

✅ **Universal Patterns:**
- Item container with capacity limit
- Add/Remove operations
- Item metadata (Name, Description)
- Consumable flag
- Quantity tracking (though not implemented)

⚠️ **Issues:**
- UI Model, not a Framework-layer data structure
- Hardcoded MAX_ITEMS = 10
- UseItem() has TODO comment (not implemented)
- Quantity exists but no stacking logic
- No integration with InventoryComponent

❌ **Not Suitable for Framework Extraction:**
- This is UI/presentation layer
- Duplicates InventoryComponent data differently
- No shared contract with InventoryComponent

### 1.2 Framework Extraction Recommendations

**What to Extract:**

1. **IInventoryContainer<TItemType>**
   - Generic container interface
   - Add/Remove/Contains/Clear operations
   - Capacity constraints
   - Supports any item type via generics

2. **IEquipmentSlot<TSlotType, TItemType>**
   - Generic equipment slot
   - Slot type enum (configurable by game)
   - Equip/unequip operations
   - Slot restrictions by item type

3. **InventoryComponent<TItemType, TSlotType>**
   - Framework component with generic types
   - Multiple container support (bags, banks, etc.)
   - Equipment slot management
   - Stacking logic

4. **IItemStack**
   - Stack management (quantity, max stack size)
   - Split/merge operations
   - Stackable vs non-stackable items

**What to Leave in MythHunter:**

1. **Specific slot types** (Armor, Weapons, Artifact)
2. **Specific container types** (Elixir bag, Quest items)
3. **UI presentation logic** (InventoryModel, InventoryPresenter)
4. **CurrentWeapon tracking** (weapon switching is game-specific)

**Integration Points with EPIC 10:**

- Equipment slots can apply stat modifiers (Phase 2: Stat System)
- Consumables can restore resources (Phase 1: Resource System)
- Item effects can trigger status effects (Phase 4: Status Effects)

---

## 2. Item System Analysis

### 2.1 Current Implementation

**No dedicated item definition system found.**

Items are currently represented as:
- **String IDs** in InventoryComponent
- **InventoryItem class** in UI layer (Name, Description, IsConsumable, Quantity)

**Observations:**

❌ **Missing Core Features:**
- No item data definitions (ScriptableObjects, JSON, database)
- No item properties (damage, defense, stat bonuses)
- No item types/categories beyond "IsConsumable"
- No rarity/quality system
- No item level/tier system
- No durability or condition tracking
- No set bonuses or item relationships
- No item requirements (level, class, stats)

❌ **Missing Item Instance Management:**
- No unique item instances (everything is just an ID)
- No item instance properties (enchantments, sockets, modifications)
- No item history/provenance tracking

### 2.2 Framework Extraction Recommendations

**What to Extract:**

1. **IItem Interface**
   - Core item contract
   - ItemId, Name, Description
   - ItemType enum (equipment, consumable, quest, crafting)
   - Rarity enum (common, uncommon, rare, epic, legendary)
   - Quality/tier properties

2. **IEquipment Interface** (extends IItem)
   - Equipment slot type
   - Stat modifiers (integrates with EPIC 10 Phase 2)
   - Equipment restrictions (level, class, race)
   - Durability system

3. **IConsumable Interface** (extends IItem)
   - Use effects (restore HP, apply buff, etc.)
   - Stack size limits
   - Cooldown categories (potions, food, etc.)

4. **ItemInstance<TItem>**
   - Wraps item definition
   - Instance-specific properties (durability, enchantments)
   - Unique instance ID
   - Creation/modification timestamp

5. **IItemRegistry**
   - Item definition registry
   - Get item by ID
   - Query items by type/rarity/category
   - Validation and lookups

**Integration Points with EPIC 10:**

- Equipment stat modifiers → Phase 2: StatContainer
- Consumable effects → Phase 1: ResourceComponent (restore HP/MP)
- Buff consumables → Phase 4: StatusEffect system
- Weapon damage → Phase 3: DamageCalculator

**Integration Points with EPIC 11 Phase 1 (Inventory):**

- Items go into IInventoryContainer
- Equipment goes into IEquipmentSlot
- Consumables respect stack size limits
- Item requirements checked on equip

---

## 3. Ability System Analysis

### 3.1 Current Implementation

#### CombatAbilityComponent.cs

**Location:** `Assets/_MythHunter/Code/Components/Combat/CombatAbilityComponent.cs`

**Structure:**
```csharp
public enum CombatAbilityType
{
    Active,    // Manual use
    Passive,   // Automatic
    Reactive   // Trigger-based
}

public struct CombatAbilityComponent : IComponent, ISerializableComponent
{
    public string ActiveAbilityId;
    public bool IsActiveAbilityUsed;       // One-time use flag
    public float LastActiveUseTime;        // Simple cooldown tracking

    public string[] PassiveAbilityIds;

    public bool UseAutoActivation;         // Auto-use setting
    public float AutoActivationHealthThreshold;
}
```

**Analysis:**

✅ **Universal Patterns:**
- Active vs Passive vs Reactive ability types
- Ability ID references (string-based)
- Cooldown tracking (time-based)
- Auto-activation thresholds

⚠️ **Limited Implementation:**
- Only one active ability per entity
- IsActiveAbilityUsed is one-time flag (resets at combat end?)
- No generic cooldown system (just LastActiveUseTime)
- No resource cost tracking
- Passive abilities are just IDs (no application logic)

❌ **Missing Features:**
- No ability definitions (just IDs)
- No ability effects (damage, heal, buff, etc.)
- No multi-cooldown tracking (only active ability)
- No charges/uses system
- No ability upgrades/ranks
- No ability requirements (level, resources, state)

#### ICombatAbilitySystem.cs

**Location:** `Assets/_MythHunter/Code/Systems/Gameplay/Combat/ICombatAbilitySystem.cs`

**Interface:**
```csharp
public interface ICombatAbilitySystem : ISystem
{
    bool UseActiveAbility(int entityId, int targetEntityId = -1);
    bool CanUseActiveAbility(int entityId);
    void ResetActiveAbilityUse(int entityId);
    string GetEntityActiveAbilityId(int entityId);
    void UpdatePassiveEffects(int entityId);
}
```

**Analysis:**

✅ **Universal Patterns:**
- Use ability with optional target
- Can-use validation
- State reset

⚠️ **Limitations:**
- Tightly coupled to combat system
- Only active abilities (no passive management interface)
- No cooldown query methods
- No ability slot management (multiple abilities)

### 3.2 Cooldown System (Found in CombatSystem.cs)

**Location:** `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs:44-48`

**Implementation:**
```csharp
private const float RAGE_EXCHANGE_COOLDOWN = 3f;
private readonly Dictionary<int, float> _lastRageExchangeTime = new Dictionary<int, float>();
```

**Analysis:**

⚠️ **Very Basic:**
- Single hardcoded cooldown for rage exchange mechanic
- Per-entity tracking via dictionary
- No generic cooldown system
- Hardcoded 3-second cooldown

❌ **Not Reusable:**
- Specific to one mechanic (rage exchange)
- No multi-cooldown support
- No cooldown categories
- No cooldown modifiers (CDR - cooldown reduction)

### 3.3 Framework Extraction Recommendations

**What to Extract:**

1. **IAbility Interface**
   - AbilityId, Name, Description
   - AbilityType (active, passive, reactive, toggle)
   - Cooldown duration
   - Resource costs (HP, MP, Stamina, etc.)
   - Cast time / channeling
   - Range / AOE properties
   - Requirements (level, resources, state)

2. **IAbilityEffect Interface**
   - Execute(caster, target, context)
   - Multiple effects per ability
   - Effect types: Damage, Heal, Buff, Debuff, Summon, etc.
   - Integration with EPIC 10 (damage, status effects, resources)

3. **ICooldownManager**
   - Track multiple cooldowns per entity
   - StartCooldown(entityId, cooldownId, duration)
   - IsOnCooldown(entityId, cooldownId)
   - GetRemainingTime(entityId, cooldownId)
   - Cooldown categories (global, ability, item)
   - Cooldown reduction modifiers

4. **AbilityComponent<TAbilityType>**
   - List of ability slots (not just one active)
   - Per-ability cooldown tracking
   - Charges/uses system
   - Passive ability states

5. **IAbilityManager**
   - UseAbility(entityId, abilityId, targetId)
   - CanUseAbility validation
   - Resource cost checking
   - Cooldown integration
   - Effect application

**Integration Points with EPIC 10:**

- Resource costs → Phase 1: ResourceComponent
- Stat requirements → Phase 2: StatContainer
- Damage effects → Phase 3: DamageCalculator
- Buff/debuff effects → Phase 4: StatusEffectManager
- Death triggers → Phase 5: LifeStateManager

**Integration Points with EPIC 11:**

- Item-granted abilities (Phase 2: Equipment)
- Consumable abilities (Phase 2: Consumables)
- Ability unlocks (Phase 5: Progression)

---

## 4. Loot System Analysis

### 4.1 Current Implementation

**❌ NOT FOUND**

No loot system implementation found in MythHunter codebase.

**Search Results:**
- No LootTable classes
- No Drop classes
- No Reward classes
- No loot generation logic
- No rarity-based drop systems

**Inference:**
- Items are likely manually placed or scripted
- No procedural loot generation
- No death-triggered loot drops

### 4.2 Framework Extraction Recommendations

**What to Build (New):**

1. **ILootTable Interface**
   - Weighted drop pools
   - Drop chance ranges
   - Min/max item counts
   - Rarity distribution curves

2. **ILootEntry**
   - Item reference (or item type)
   - Drop weight/chance
   - Quantity range (min, max)
   - Conditional drops (requirements)

3. **ILootGenerator**
   - GenerateLoot(lootTableId, context)
   - Rarity roll system
   - Luck/MagicFind modifiers
   - Level scaling

4. **LootDropEvent**
   - Event raised when loot is generated
   - Integrates with death system (Phase 5)
   - Can be triggered manually (chest, quest, etc.)

**Integration Points with EPIC 10:**

- Death triggers loot → Phase 5: DeathEvent
- Loot quality scales with enemy level/stats → Phase 2: StatContainer

**Integration Points with EPIC 11:**

- Generated items go into inventory → Phase 1: Inventory
- Item definitions from registry → Phase 2: IItemRegistry
- Ability books/scrolls → Phase 4: Abilities
- XP rewards → Phase 5: Progression

---

## 5. Progression System Analysis

### 5.1 Current Implementation

**❌ NOT FOUND**

No progression system implementation found in MythHunter codebase.

**Search Results:**
- No Level/Experience classes
- No XP tracking
- No level-up logic
- No stat growth systems
- No unlock/skill tree systems

**Inference:**
- Characters may have fixed stats
- No character advancement
- Progression not yet implemented

### 5.2 Framework Extraction Recommendations

**What to Build (New):**

1. **IProgressionComponent**
   - CurrentLevel
   - CurrentXP
   - XPToNextLevel
   - Prestige/tier system

2. **ILevelUpHandler**
   - OnLevelUp(entityId, newLevel)
   - Stat growth (integrates with StatContainer)
   - Ability unlocks
   - Rewards (items, currency)

3. **IXPSource**
   - Combat XP
   - Quest XP
   - Exploration XP
   - XP multipliers (rested, buffs, party)

4. **IProgressionCurve**
   - XP requirement curves
   - Exponential, linear, custom
   - Level caps
   - Prestige/rebirth mechanics

5. **IUnlockSystem**
   - Level-gated content
   - Ability unlocks
   - Equipment requirements
   - Skill trees (optional)

**Integration Points with EPIC 10:**

- Stat growth on level up → Phase 2: StatContainer.AddModifier
- XP from combat → Phase 5: DeathEvent (killer gets XP)
- Level requirements for equipment → Phase 2: Equipment restrictions

**Integration Points with EPIC 11:**

- XP rewards from loot → Phase 3: LootTable
- Level-gated abilities → Phase 4: Ability requirements
- Level-gated items → Phase 2: Item requirements

---

## 6. Cross-Cutting Concerns

### 6.1 Serialization

**Current State:**
- InventoryComponent: Manual binary serialization
- CombatAbilityComponent: Manual binary serialization
- Not using EPIC 06 serialization framework

**Recommendation:**
- Framework modules should use EPIC 06 `ISerializable` contracts
- Game layer can adapt to binary serialization if needed

### 6.2 Event Integration

**Current State:**
- CombatSystem uses EventBus for CombatStartRequestEvent, DamageAppliedEvent, EntityDeathEvent
- No inventory events
- No ability events (beyond combat)

**Recommendation:**
- Framework should define domain events:
  - ItemAddedEvent, ItemRemovedEvent
  - ItemEquippedEvent, ItemUnequippedEvent
  - AbilityUsedEvent, CooldownStartedEvent
  - LootGeneratedEvent
  - LevelUpEvent, XPGainedEvent

### 6.3 Dependency Injection

**Current State:**
- CombatSystem uses [Inject] for dependencies
- No inventory system dependencies (it's just a component)

**Recommendation:**
- Framework managers use constructor injection (same pattern as EPIC 10)
- Game adapters inject framework managers

---

## 7. Framework vs Game Boundaries

### 7.1 Clear Framework Candidates

✅ **Universal RPG Concepts:**

1. **Inventory:**
   - Container abstraction (capacity, add/remove)
   - Equipment slots (generic type-based)
   - Item stacking logic
   - Multi-container support

2. **Items:**
   - Item definitions (stats, types, rarity)
   - Equipment stat modifiers
   - Consumable effects
   - Item instance management

3. **Loot:**
   - Loot tables and drop pools
   - Weighted random generation
   - Rarity distribution
   - Conditional drops

4. **Abilities:**
   - Ability definitions (cost, cooldown, effects)
   - Cooldown management system
   - Resource cost validation
   - Effect application contracts

5. **Progression:**
   - XP tracking and level-up
   - Stat growth curves
   - Unlock systems
   - Level requirements

### 7.2 Game-Specific Adaptations

⚠️ **MythHunter-Specific:**

1. **Inventory:**
   - Specific slot names (Boots, Gloves, Helmet, Artifact)
   - CurrentWeapon tracking
   - "Elixir" category
   - Fixed 10-item UI capacity

2. **Items:**
   - Specific item types (weapons, armor, artifacts, elixirs)
   - MythHunter rarity naming (if different from standard)
   - Specific stat types (maps to MythHunter StatType)

3. **Abilities:**
   - One active ability limitation
   - Auto-activation thresholds
   - Rage exchange mechanic
   - MythHunter-specific ability effects

4. **Loot:**
   - Specific loot tables for MythHunter enemies
   - Game balance (drop rates, loot quality)

5. **Progression:**
   - MythHunter XP curve
   - MythHunter stat growth rules
   - Hero-specific unlock trees

### 7.3 Integration Strategy

**Adapter Pattern (same as EPIC 10):**

```
MythHunter (Game Layer)
    ↓ uses
MythHunterInventoryAdapter
    ↓ wraps
RPGFramework.Inventory.Runtime
    ↓ uses
RPGFramework.Stats.Runtime (EPIC 10 Phase 2)
RPGFramework.Combat.Runtime (EPIC 10 Phases 1-5)
```

**Example:**

```csharp
// Framework (universal)
namespace RPGFramework.Inventory.Runtime
{
    public interface IEquipmentSlot<TSlotType, TItem> where TSlotType : Enum
    {
        TSlotType SlotType { get; }
        TItem EquippedItem { get; }
        bool IsOccupied { get; }
        bool Equip(TItem item);
        TItem Unequip();
    }
}

// MythHunter (game-specific)
namespace MythHunter.Adapters.Inventory
{
    public enum ArmorSlotType { Boots, Gloves, Chest, Helmet }

    public class MythHunterEquipmentSlot : IEquipmentSlot<ArmorSlotType, MythHunterItem>
    {
        // Wraps framework, applies MythHunter rules
    }
}
```

---

## 8. Dependencies and Coupling

### 8.1 Current Coupling Issues

**InventoryComponent:**
- ❌ Uses List (acceptable, but allocates)
- ✅ No Unity dependencies
- ❌ Manual serialization (not using EPIC 06)

**CombatAbilityComponent:**
- ❌ Tightly coupled to combat system
- ✅ No Unity dependencies
- ❌ Manual serialization (not using EPIC 06)

**UI Layer:**
- ❌ InventoryModel duplicates InventoryComponent data
- ❌ No shared contract
- ⚠️ Hardcoded MAX_ITEMS

### 8.2 Decoupling Strategy

**Phase 1 (Inventory):**
- Extract IInventoryContainer (no Unity, no game logic)
- Extract IEquipmentSlot (generic slot types)
- MythHunter adapts to specific slot enums

**Phase 2 (Items):**
- Extract IItem, IEquipment, IConsumable contracts
- Item stat modifiers integrate with EPIC 10 StatContainer
- MythHunter provides item definitions (ScriptableObjects or JSON)

**Phase 3 (Loot):**
- Pure algorithmic (no Unity dependencies)
- References IItemRegistry (Phase 2)
- MythHunter provides loot tables

**Phase 4 (Abilities):**
- Decouple from CombatSystem
- Generic IAbilityManager and ICooldownManager
- MythHunter provides ability definitions

**Phase 5 (Progression):**
- Pure XP/level math (no Unity)
- Integrates with StatContainer for growth
- MythHunter provides curves and unlock rules

---

## 9. Test Coverage Strategy

### 9.1 Follow EPIC 10 Pattern

**Per Phase:**
- 20-35 comprehensive NUnit tests
- .NET 8.0 test harness (fast CI)
- Static boundary validation (no Unity references)
- GitHub Actions workflow

**Example Test Areas (Phase 1 — Inventory):**

1. **Container Tests** (8-10 tests)
   - Add item to container
   - Remove item from container
   - Container at capacity rejects item
   - Clear container
   - Contains check
   - Enumerate items

2. **Equipment Slot Tests** (6-8 tests)
   - Equip item to empty slot
   - Replace equipped item
   - Unequip item
   - Slot type restrictions
   - IsOccupied flag

3. **Stacking Tests** (6-8 tests)
   - Stack identical items
   - Split stack
   - Merge stacks
   - Max stack size enforcement
   - Non-stackable items

**Total: ~20-25 tests per phase**

### 9.2 Integration Tests (Phase 6)

After all phases complete:
- End-to-end item flow (loot → inventory → equip → stat bonuses)
- End-to-end ability flow (unlock → equip → use → cooldown)
- End-to-end progression (combat → XP → level up → stat growth → ability unlock)

---

## 10. Assembly Structure

### 10.1 Proposed Assemblies

Following EPIC 10 pattern (zero Unity dependencies):

```
Assets/_Framework/
├── Inventory/
│   ├── Runtime/
│   │   ├── RPGFramework.Inventory.Runtime.asmdef
│   │   ├── Contracts/ (interfaces)
│   │   ├── Components/
│   │   └── Implementations/
│   └── Tests/
│       └── RPGFramework.Inventory.Tests.asmdef
│
├── Items/
│   ├── Runtime/
│   │   ├── RPGFramework.Items.Runtime.asmdef
│   │   ├── Contracts/
│   │   ├── Components/
│   │   └── Implementations/
│   └── Tests/
│       └── RPGFramework.Items.Tests.asmdef
│
├── Loot/
│   ├── Runtime/
│   │   ├── RPGFramework.Loot.Runtime.asmdef
│   │   └── ...
│   └── Tests/
│       └── RPGFramework.Loot.Tests.asmdef
│
├── Abilities/
│   ├── Runtime/
│   │   ├── RPGFramework.Abilities.Runtime.asmdef
│   │   └── ...
│   └── Tests/
│       └── RPGFramework.Abilities.Tests.asmdef
│
└── Progression/
    ├── Runtime/
    │   ├── RPGFramework.Progression.Runtime.asmdef
    │   └── ...
    └── Tests/
        └── RPGFramework.Progression.Tests.asmdef
```

### 10.2 Assembly Dependencies

**Inventory → Stats** (equip stat bonuses)
**Items → Stats** (item stat modifiers)
**Abilities → Combat** (damage/heal/buff effects)
**Abilities → Stats** (stat requirements)
**Loot → Items** (generate item instances)
**Progression → Stats** (stat growth on level up)

**All reference:**
- RPGFramework.Core (events, logging)
- RPGFramework.ECS (components, entities)

---

## 11. Phase Breakdown and Estimates

### Phase 1: Inventory System (6-8 hours)

**Deliverables:**
- IInventoryContainer<TItemType>
- IEquipmentSlot<TSlotType, TItemType>
- InventoryComponent<TItemType, TSlotType>
- IItemStack, StackableItem
- Container capacity logic
- 20-25 comprehensive tests

**Files:** ~12-15 Runtime, 1 Test file

### Phase 2: Item System (6-8 hours)

**Deliverables:**
- IItem, IEquipment, IConsumable interfaces
- ItemInstance<TItem>
- IItemRegistry (lookup by ID)
- Rarity, Quality, ItemType enums
- Stat modifier integration (EPIC 10 Phase 2)
- Equipment requirements validation
- 20-25 comprehensive tests

**Files:** ~12-15 Runtime, 1 Test file

### Phase 3: Loot System (6-8 hours)

**Deliverables:**
- ILootTable, ILootEntry interfaces
- LootGenerator implementation
- Weighted random selection
- Rarity distribution curves
- Luck/MagicFind modifiers
- LootDropEvent integration
- 20-25 comprehensive tests

**Files:** ~10-12 Runtime, 1 Test file

### Phase 4: Ability System (8-10 hours)

**Deliverables:**
- IAbility, IAbilityEffect interfaces
- ICooldownManager (multi-cooldown tracking)
- IAbilityManager (use, validation)
- AbilityComponent<TAbilityType>
- Resource cost integration (EPIC 10 Phase 1)
- Effect contracts (damage, heal, buff, etc.)
- 25-30 comprehensive tests

**Files:** ~15-18 Runtime, 1 Test file

### Phase 5: Progression System (6-8 hours)

**Deliverables:**
- IProgressionComponent (level, XP)
- ILevelUpHandler (stat growth)
- IXPSource (combat, quest, exploration)
- IProgressionCurve (XP requirements)
- IUnlockSystem (level-gated content)
- LevelUpEvent, XPGainedEvent
- 20-25 comprehensive tests

**Files:** ~12-15 Runtime, 1 Test file

### Phase 6: Integration (4-6 hours)

**Deliverables:**
- MythHunter adapters for all systems
- End-to-end integration tests
- Documentation and examples
- Performance validation

**Files:** Adapters in MythHunter, integration tests

**Total Estimate:** ~40-55 hours across 6 phases

---

## 12. Risks and Mitigations

### Risk 1: Scope Creep

**Risk:** Each phase could expand with "nice to have" features.
**Mitigation:** Strict adherence to "universal RPG" filter. Defer game-specific features to adapters.

### Risk 2: Integration Complexity

**Risk:** 5 new assemblies + EPIC 10's 2 = 7 total Framework assemblies. Dependency graph could get complex.
**Mitigation:** Clear dependency rules (Inventory doesn't depend on Abilities, etc.). Static boundary tests.

### Risk 3: MythHunter Compatibility

**Risk:** Extracting systems may break existing MythHunter code (even though none exists for loot/progression).
**Mitigation:** Additive-only pattern (same as EPIC 10). MythHunter continues to use old InventoryComponent until adapters are ready.

### Risk 4: Performance

**Risk:** Generic containers, boxing/unboxing, dictionary lookups could impact performance.
**Mitigation:** Performance tests in .NET harness. Benchmark against EPIC 10 patterns (which are already proven).

---

## 13. Success Criteria

By end of EPIC 11:

✅ **5 new Framework assemblies** (Inventory, Items, Loot, Abilities, Progression)
✅ **100-150 comprehensive tests** (20-30 per phase)
✅ **Zero Unity dependencies** (noEngineReferences: true)
✅ **Zero MythHunter coupling** (pure .NET 8.0 C#)
✅ **GitHub Actions GREEN** (all phases validated)
✅ **Integration with EPIC 10** (Stats, Combat, Resources, Status Effects)
✅ **MythHunter adapters** (Phase 6 proves extractability)
✅ **Documentation** (implementation plans, API docs, usage examples)

---

## 14. Recommendations

### Immediate Next Steps

1. ✅ **This audit complete**
2. ⏭️ **Create EPIC-11-Phase1-Implementation-Plan.md** (~500-600 lines)
   - Detailed design for Inventory System
   - Interface definitions
   - Test coverage plan
   - Acceptance criteria

3. ⏭️ **Implement Phase 1** (create branch, build, test, PR)

4. ⏭️ **Validate and merge** (.NET tests, boundary check, CI)

5. ⏭️ **Repeat for Phases 2-6**

### Prioritization Rationale

**Why Inventory first?**
- Simplest domain (containers and slots)
- Least dependencies (only on ECS, no other Framework modules yet)
- Establishes pattern for other phases
- Immediate value (MythHunter already has InventoryComponent to adapt)

**Why Items second?**
- Depends on Inventory (items go into containers)
- Enables stat integration (EPIC 10 Phase 2)
- Required for Loot and Abilities

**Why Loot third?**
- Depends on Items (generates item instances)
- Showcases integration (death → loot → inventory)
- High reusability across RPGs

**Why Abilities fourth?**
- More complex (cooldowns, costs, effects)
- Depends on Items (item-granted abilities)
- Integrates with many EPIC 10 systems

**Why Progression fifth?**
- Depends on Abilities (unlocks)
- Depends on Stats (growth)
- Caps off the RPG gameplay layer

**Why Integration last?**
- Validates all phases work together
- MythHunter adapters prove extractability
- End-to-end testing

---

## 15. Conclusion

MythHunter's gameplay systems are in early stages, with basic inventory and ability components but no loot or progression. This presents an **ideal opportunity** to extract universal patterns before game-specific complexity accumulates.

**Key Advantages:**

1. **Clean slate for loot/progression** — No legacy code to migrate
2. **Simple inventory/ability bases** — Easy to identify universal patterns
3. **EPIC 10 integration points** — Stats, combat, resources ready to connect
4. **Proven extraction pattern** — Follow EPIC 10's successful approach

**Estimated Timeline:**

- **Per phase:** ~6-10 hours (1-2 days)
- **Total:** ~40-55 hours (1.5-2 weeks at moderate pace)
- **Similar to EPIC 10:** 5 phases, 2 weeks

**Confidence:** ⭐⭐⭐⭐⭐ (Very High)

EPIC 10 validated the extraction approach. EPIC 11 follows the exact same pattern with similar scope. Expect similar success rate.

---

**Ready to proceed with Phase 1: Inventory System implementation planning.**
