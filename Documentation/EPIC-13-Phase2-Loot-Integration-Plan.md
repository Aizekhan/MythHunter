# EPIC 13 Phase 2 - Loot System Integration Plan

> Integration of EPIC 11 Phase 3 Loot System into MythHunter game layer.

**Status:** In Progress (Phase 2a)
**Date:** 2026-10-16
**Pattern:** Greenfield Integration (no legacy loot code to migrate)

---

## Overview

**Goal:** Integrate RPGFramework.Loot.Runtime into MythHunter game layer, enabling loot generation from enemies, chests, and bosses.

**Key Difference from Phase 1:** This is a **greenfield integration** - MythHunter has no existing loot system, so no migration utility is needed.

**Dependencies:**
- ✅ EPIC 11 Phase 3: Loot System (Framework) - Complete
- ✅ EPIC 13 Phase 1: Inventory & Items Integration - Complete
- Integration flow: Loot → Item Entities → Inventory Pickup

---

## Phase 2 Structure

### Phase 2a: Core Integration (Current)
- **Goal:** Create MythHunter wrappers and integration helpers
- **Deliverables:**
  - MythHunterLootComponent (ECS component for entities)
  - LootSystemIntegration (helper for loot generation)
  - Loot table builder utilities
- **No Migration Needed:** Greenfield integration

### Phase 2b: System Updates
- **Goal:** Integrate into Combat/Death systems
- **Deliverables:**
  - Enemy death → loot drop integration
  - Chest/container loot support
  - Boss-specific loot tables
  - Loot table registry setup

### Phase 2c: Tests & Validation
- **Goal:** Comprehensive testing
- **Deliverables:**
  - Integration tests (loot generation flow)
  - Loot-to-Item-to-Inventory tests
  - Edge case tests (luck modifiers, level filtering)

---

## Framework Loot System Analysis

### Core Interfaces (from EPIC 11)

**ILootTable:**
- TableId, Name
- Entries (ILootEntry list)
- MinDrops, MaxDrops, AllowDuplicates
- GuaranteedDrops (always drop, bypass RNG)

**ILootEntry:**
- ItemId (links to item registry)
- Weight (for weighted random selection)
- MinQuantity, MaxQuantity
- DropChance (0.0-1.0, even if selected)
- MinLevel, MaxLevel (level filtering)

**ILootGenerator:**
- GenerateLoot(lootTable, level, luckModifier, seed?)
- GenerateLootById(tableId, ...)
- Returns LootResult

**LootResult:**
- Drops (List<LootDrop>)
- TableId, Level, LuckModifier, Seed
- Timestamp
- HasLoot property

**LootDrop:**
- ItemId
- Quantity
- SourceEntry

### Integration Flow

```
Enemy Death / Chest Open
  ↓
MythHunterLootComponent (has TableId)
  ↓
LootSystemIntegration.GenerateLoot()
  ↓
Framework: ILootGenerator → LootResult
  ↓
Game Layer: Create Item Entities for each LootDrop
  ↓
Player: Pickup Item → Add to Inventory (Phase 1)
```

---

## Phase 2a: Core Integration Design

### 1. MythHunterLootComponent

**Purpose:** ECS component attached to entities that can drop loot (enemies, chests, bosses).

**Structure:**
```csharp
public struct MythHunterLootComponent : IComponent
{
    // Primary loot table ID
    public string LootTableId;

    // Entity level (for level-based filtering)
    public int Level;

    // Luck modifier (boss aura, player buffs, etc.)
    public float LuckModifier;

    // Whether loot has been generated (for chests/one-time drops)
    public bool LootGenerated;

    // Optional seed for deterministic loot (testing/replays)
    public int? CustomSeed;
}
```

**Factory Methods:**
- `CreateEnemy(tableId, level)` - Standard enemy loot
- `CreateBoss(tableId, level, luckBonus)` - Boss with luck modifier
- `CreateChest(tableId, level)` - One-time container loot
- `CreateEvent(tableId, level, seed)` - Deterministic event loot

### 2. LootSystemIntegration

**Purpose:** Helper class for generating loot and creating item entities.

**Key Methods:**
```csharp
// Generate loot from component
LootResult GenerateLoot(int entityId);

// Generate loot from table ID directly
LootResult GenerateLoot(string tableId, int level, float luckModifier);

// Generate loot and create item entities in world
List<int> GenerateLootAndCreateItems(
    int sourceEntityId,
    Vector3 dropPosition,
    IEntityManager entityManager);

// Check if entity has loot component
bool HasLootTable(int entityId);

// Get loot table ID from entity
string GetLootTableId(int entityId);

// Get all entities with loot tables
IEnumerable<int> GetEntitiesWithLoot();
```

**Dependencies:**
- `ILootGenerator` (Framework)
- `ILootTableRegistry` (Framework)
- `IEntityManager` (Framework ECS)
- `IMythLogger` (MythHunter logging)

### 3. Directory Structure

```
Assets/_MythHunter/Code/GameLogic/Loot/
  ├── Components/
  │   └── MythHunterLootComponent.cs
  ├── Adapters/
  │   └── LootSystemIntegration.cs
  └── Utilities/
      └── LootTableBuilder.cs (optional helper)
```

### 4. Integration with Items (Phase 1)

**Flow: Loot → Item Entities**

```csharp
// After generating loot
LootResult result = lootIntegration.GenerateLoot(entityId);

// Create item entities for each drop
foreach (var drop in result.Drops)
{
    // Use MythHunterItemComponent from Phase 1
    var itemComponent = MythHunterItemComponent.Create(
        drop.ItemId,
        drop.Quantity);

    int itemEntityId = entityManager.CreateEntity();
    entityManager.AddComponent(itemEntityId, itemComponent);

    // Add world position, physics, etc.
}
```

---

## Implementation Steps (Phase 2a)

### Step 1: Create Directory Structure
- [x] Analyze Framework Loot System
- [ ] Create `Assets/_MythHunter/Code/GameLogic/Loot/Components/`
- [ ] Create `Assets/_MythHunter/Code/GameLogic/Loot/Adapters/`

### Step 2: Create MythHunterLootComponent
- [ ] Implement component struct
- [ ] Add factory methods (CreateEnemy, CreateBoss, CreateChest, CreateEvent)
- [ ] Add helper methods (IsGenerated, SetGenerated, etc.)
- [ ] XML documentation

### Step 3: Create LootSystemIntegration
- [ ] Implement core generation methods
- [ ] Add entity query methods
- [ ] Add item creation helpers
- [ ] Integration with Item system (Phase 1)
- [ ] XML documentation

### Step 4: Compile and Validate
- [ ] Unity compilation (verify no errors)
- [ ] Verify Framework dependencies resolve
- [ ] Verify Phase 1 Item integration works
- [ ] Create .meta files

### Step 5: Commit
- [ ] Git add new files
- [ ] Create descriptive commit message
- [ ] Push to origin/dev

---

## Differences from Phase 1 (Inventory Integration)

| Aspect | Phase 1 (Inventory) | Phase 2 (Loot) |
|--------|---------------------|----------------|
| **Legacy Code** | Yes (InventoryComponent) | No |
| **Migration Utility** | Required | Not needed |
| **Dual-Component** | Yes (old + new) | No (greenfield) |
| **Complexity** | Higher (migration) | Lower (direct integration) |
| **Factory Methods** | 5 (Hero, Elite, etc.) | 4 (Enemy, Boss, Chest, Event) |
| **Integration Tests** | 15 tests | Estimate: 10-12 tests |

---

## Dependencies & Integration Points

### Framework Dependencies
- `RPGFramework.Loot.Runtime` (ILootGenerator, ILootTable, etc.)
- `RPGFramework.Items.Runtime` (ItemQuality, ItemRarity from Phase 1)
- `RPGFramework.ECS.Runtime` (IComponent, IEntityManager)
- `RPGFramework.Logging.Runtime` (LogSeverity)

### MythHunter Dependencies
- `MythHunterItemComponent` (Phase 1) - for creating item entities
- `MythHunter.Utils.Logging.IMythLogger` - logging

### Future Integration Points (Phase 2b)
- `CombatSystem` - enemy death triggers loot generation
- `InteractionSystem` - chest/container opening
- `BossSystem` - boss-specific loot tables with guaranteed drops

---

## Testing Strategy (Phase 2c)

### Core Tests
1. **Component Creation Tests**
   - CreateEnemy, CreateBoss, CreateChest, CreateEvent factory methods
   - Component property validation

2. **Loot Generation Tests**
   - Basic generation from table ID
   - Level filtering (min/max level)
   - Luck modifier effects
   - Guaranteed drops
   - Seed determinism

3. **Integration Tests**
   - Loot → Item entity creation
   - Item entity → Inventory pickup
   - Full flow: Enemy death → Loot → Item → Inventory

4. **Edge Cases**
   - Empty loot tables
   - Missing loot table IDs
   - Invalid level ranges
   - Extreme luck modifiers

---

## Success Criteria (Phase 2a)

- [ ] MythHunterLootComponent created with 4 factory methods
- [ ] LootSystemIntegration created with generation helpers
- [ ] Unity compilation successful (exit code 0)
- [ ] All .meta files generated
- [ ] Zero compilation errors
- [ ] Zero Unity dependencies in adapters
- [ ] Integration with Phase 1 Items validated
- [ ] Code committed and pushed to origin/dev

---

## Next Phases

**Phase 2b: System Updates**
- Enemy death handlers
- Chest interaction handlers
- Loot table registry population
- Boss-specific loot tables

**Phase 2c: Tests & Validation**
- Integration tests (10-12 tests)
- Loot-to-Item-to-Inventory flow tests
- Edge case validation

---

**Pattern:** Greenfield Integration (simpler than Phase 1)
**Estimated Files:** 4-5 files (vs 17 in Phase 1a)
**Estimated Lines:** 400-500 lines (vs 2054 in Phase 1a)
**Estimated Tests:** 10-12 tests (vs 15 in Phase 1c)

**Phase 2a Start:** 2026-10-16
