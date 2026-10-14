# EPIC 11 Phase 3: Loot System — Implementation Plan

**Date:** 2026-10-14
**Author:** Claude Code
**Status:** Ready for implementation
**Phase 2:** PR #72 (Item System) — ✅ COMPLETE
**Audit:** Documentation/EPIC-11-Gameplay-Domain-Audit.md

---

## 1. Executive Summary

**Goal:** Create a universal, reusable loot generation system for RPG Framework with weighted random drops, rarity distribution, and loot tables.

**Scope:**
- Loot table definitions with weighted entries
- Loot pool management for item grouping
- Rarity-based drop distribution
- Weighted random selection algorithm
- Quantity range generation (min/max stacks)
- Boss/mob-specific loot tables
- Luck/magic find modifiers

**Out of Scope (Later Phases):**
- Ability definitions (Phase 4)
- XP/progression rewards (Phase 5)
- MythHunter adapters and UI integration (Phase 6)

**Deliverables:**
- 12-15 Runtime C# files (interfaces, implementations, components, events)
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

**❌ NO LOOT SYSTEM FOUND**

- No LootTable classes
- No Drop classes
- No Reward classes
- No loot generation logic
- No rarity-based drop systems

**Inference:**
- Items likely manually placed or scripted
- No procedural loot generation
- No death-triggered loot drops

**Extraction Opportunity:**
- Build complete loot system from scratch as Framework module
- No legacy code to refactor
- Clean slate for best practices

---

## 3. Framework Design

### 3.1 Core Interfaces

#### ILootEntry

Represents a single entry in a loot table.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Represents a single entry in a loot table.
    /// Defines what item can drop, with what probability, and in what quantities.
    /// </summary>
    public interface ILootEntry
    {
        /// <summary>
        /// Item definition ID to drop.
        /// </summary>
        string ItemId { get; }

        /// <summary>
        /// Drop weight for weighted random selection.
        /// Higher weight = more likely to drop.
        /// </summary>
        int Weight { get; }

        /// <summary>
        /// Minimum quantity to drop.
        /// </summary>
        int MinQuantity { get; }

        /// <summary>
        /// Maximum quantity to drop.
        /// </summary>
        int MaxQuantity { get; }

        /// <summary>
        /// Drop chance percentage (0.0 to 1.0).
        /// Example: 0.5 = 50% chance to drop.
        /// </summary>
        float DropChance { get; }

        /// <summary>
        /// Minimum player/enemy level required for this drop.
        /// 0 = no requirement.
        /// </summary>
        int MinLevel { get; }

        /// <summary>
        /// Maximum player/enemy level for this drop.
        /// -1 = no maximum.
        /// </summary>
        int MaxLevel { get; }
    }
}
```

#### ILootTable

Represents a collection of loot entries.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Loot table containing multiple loot entries.
    /// Used for specific enemies, chests, bosses, etc.
    /// </summary>
    public interface ILootTable
    {
        /// <summary>
        /// Unique identifier for this loot table.
        /// </summary>
        string TableId { get; }

        /// <summary>
        /// Display name for debugging/UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// All loot entries in this table.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<ILootEntry> Entries { get; }

        /// <summary>
        /// Minimum number of items to drop.
        /// </summary>
        int MinDrops { get; }

        /// <summary>
        /// Maximum number of items to drop.
        /// </summary>
        int MaxDrops { get; }

        /// <summary>
        /// Whether items can be duplicated in a single roll.
        /// True = same item can drop multiple times.
        /// False = each item can only drop once per generation.
        /// </summary>
        bool AllowDuplicates { get; }

        /// <summary>
        /// Guaranteed item IDs that always drop.
        /// Used for quest items, boss-specific drops.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<string> GuaranteedDrops { get; }
    }
}
```

#### ILootGenerator

Generates loot from loot tables.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Loot generator that creates items from loot tables.
    /// </summary>
    public interface ILootGenerator
    {
        /// <summary>
        /// Generate loot from a loot table.
        /// </summary>
        /// <param name="lootTable">Loot table to roll</param>
        /// <param name="level">Level for level-based filtering (default: 1)</param>
        /// <param name="luckModifier">Luck/magic find modifier (default: 1.0 = no bonus)</param>
        /// <param name="seed">Random seed for deterministic generation (default: time-based)</param>
        /// <returns>Loot generation result</returns>
        LootResult GenerateLoot(
            ILootTable lootTable,
            int level = 1,
            float luckModifier = 1.0f,
            int? seed = null);

        /// <summary>
        /// Generate loot from a loot table by ID.
        /// </summary>
        /// <param name="tableId">Loot table ID</param>
        /// <param name="level">Level for level-based filtering</param>
        /// <param name="luckModifier">Luck/magic find modifier</param>
        /// <param name="seed">Random seed for deterministic generation</param>
        /// <returns>Loot generation result, or null if table not found</returns>
        LootResult GenerateLootById(
            string tableId,
            int level = 1,
            float luckModifier = 1.0f,
            int? seed = null);
    }
}
```

#### ILootTableRegistry

Central registry for loot tables.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Registry for storing and retrieving loot tables.
    /// </summary>
    public interface ILootTableRegistry
    {
        /// <summary>
        /// Get loot table by ID.
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns>Loot table, or null if not found</returns>
        ILootTable GetTable(string tableId);

        /// <summary>
        /// Check if table exists.
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns>True if registered, false otherwise</returns>
        bool HasTable(string tableId);

        /// <summary>
        /// Register a loot table.
        /// </summary>
        /// <param name="table">Loot table to register</param>
        void RegisterTable(ILootTable table);

        /// <summary>
        /// Unregister a loot table.
        /// </summary>
        /// <param name="tableId">Table ID to remove</param>
        /// <returns>True if removed, false if not found</returns>
        bool UnregisterTable(string tableId);

        /// <summary>
        /// Get all registered tables.
        /// </summary>
        /// <returns>Read-only list of all loot tables</returns>
        System.Collections.Generic.IReadOnlyList<ILootTable> GetAllTables();

        /// <summary>
        /// Clear all registered tables.
        /// </summary>
        void Clear();
    }
}
```

### 3.2 Data Structures

#### LootDrop

Represents a single generated item drop.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Represents a single generated item drop.
    /// </summary>
    public struct LootDrop
    {
        /// <summary>
        /// Item definition ID.
        /// </summary>
        public string ItemId;

        /// <summary>
        /// Quantity dropped.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Source loot entry that generated this drop.
        /// Null for guaranteed drops.
        /// </summary>
        public ILootEntry SourceEntry;
    }
}
```

#### LootResult

Result of loot generation.

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Result of loot generation.
    /// Contains all items dropped and metadata about the roll.
    /// </summary>
    public struct LootResult
    {
        /// <summary>
        /// All items dropped.
        /// </summary>
        public System.Collections.Generic.List<LootDrop> Drops;

        /// <summary>
        /// Loot table that was rolled.
        /// </summary>
        public string TableId;

        /// <summary>
        /// Level used for generation.
        /// </summary>
        public int Level;

        /// <summary>
        /// Luck modifier applied.
        /// </summary>
        public float LuckModifier;

        /// <summary>
        /// Random seed used.
        /// </summary>
        public int Seed;

        /// <summary>
        /// Timestamp when loot was generated.
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// Total number of items dropped.
        /// </summary>
        public int TotalCount => Drops?.Count ?? 0;

        /// <summary>
        /// Whether any items were dropped.
        /// </summary>
        public bool HasLoot => TotalCount > 0;
    }
}
```

### 3.3 Events

#### LootGeneratedEvent

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Event raised when loot is generated.
    /// </summary>
    public struct LootGeneratedEvent
    {
        /// <summary>
        /// Loot table ID.
        /// </summary>
        public string TableId;

        /// <summary>
        /// Entity ID that triggered loot generation (killer, chest opener).
        /// -1 = no specific entity.
        /// </summary>
        public int TriggerEntityId;

        /// <summary>
        /// Source entity ID (killed enemy, opened chest).
        /// -1 = no source entity.
        /// </summary>
        public int SourceEntityId;

        /// <summary>
        /// Number of items generated.
        /// </summary>
        public int ItemCount;

        /// <summary>
        /// Timestamp when loot was generated.
        /// </summary>
        public float Timestamp;
    }
}
```

#### LootDroppedEvent

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Event raised when loot item is dropped in world.
    /// </summary>
    public struct LootDroppedEvent
    {
        /// <summary>
        /// Item ID dropped.
        /// </summary>
        public string ItemId;

        /// <summary>
        /// Quantity dropped.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Entity ID representing the dropped item in world.
        /// </summary>
        public int ItemEntityId;

        /// <summary>
        /// Source entity ID (killed enemy).
        /// </summary>
        public int SourceEntityId;

        /// <summary>
        /// Timestamp when loot was dropped.
        /// </summary>
        public float Timestamp;
    }
}
```

### 3.4 Implementations

#### LootEntry

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Default implementation of ILootEntry.
    /// </summary>
    public class LootEntry : ILootEntry
    {
        public string ItemId { get; }
        public int Weight { get; }
        public int MinQuantity { get; }
        public int MaxQuantity { get; }
        public float DropChance { get; }
        public int MinLevel { get; }
        public int MaxLevel { get; }

        public LootEntry(
            string itemId,
            int weight = 1,
            int minQuantity = 1,
            int maxQuantity = 1,
            float dropChance = 1.0f,
            int minLevel = 0,
            int maxLevel = -1)
        {
            ItemId = itemId;
            Weight = weight;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
            DropChance = dropChance;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }
    }
}
```

#### LootTable

```csharp
namespace RPGFramework.Loot
{
    /// <summary>
    /// Default implementation of ILootTable.
    /// </summary>
    public class LootTable : ILootTable
    {
        public string TableId { get; }
        public string Name { get; }
        public System.Collections.Generic.IReadOnlyList<ILootEntry> Entries { get; }
        public int MinDrops { get; }
        public int MaxDrops { get; }
        public bool AllowDuplicates { get; }
        public System.Collections.Generic.IReadOnlyList<string> GuaranteedDrops { get; }

        public LootTable(
            string tableId,
            string name,
            System.Collections.Generic.IReadOnlyList<ILootEntry> entries,
            int minDrops = 1,
            int maxDrops = 3,
            bool allowDuplicates = false,
            System.Collections.Generic.IReadOnlyList<string> guaranteedDrops = null)
        {
            TableId = tableId;
            Name = name;
            Entries = entries ?? new System.Collections.Generic.List<ILootEntry>();
            MinDrops = minDrops;
            MaxDrops = maxDrops;
            AllowDuplicates = allowDuplicates;
            GuaranteedDrops = guaranteedDrops ?? new System.Collections.Generic.List<string>();
        }
    }
}
```

#### LootGenerator

```csharp
namespace RPGFramework.Loot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default implementation of ILootGenerator.
    /// Uses weighted random selection with optional luck modifiers.
    /// </summary>
    public class LootGenerator : ILootGenerator
    {
        private readonly ILootTableRegistry _registry;

        public LootGenerator(ILootTableRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public LootResult GenerateLoot(
            ILootTable lootTable,
            int level = 1,
            float luckModifier = 1.0f,
            int? seed = null)
        {
            if (lootTable == null)
                throw new ArgumentNullException(nameof(lootTable));

            int actualSeed = seed ?? Environment.TickCount;
            var random = new Random(actualSeed);

            var result = new LootResult
            {
                Drops = new List<LootDrop>(),
                TableId = lootTable.TableId,
                Level = level,
                LuckModifier = luckModifier,
                Seed = actualSeed,
                Timestamp = 0f // Game layer sets timestamp
            };

            // Add guaranteed drops
            foreach (var guaranteedId in lootTable.GuaranteedDrops)
            {
                result.Drops.Add(new LootDrop
                {
                    ItemId = guaranteedId,
                    Quantity = 1,
                    SourceEntry = null
                });
            }

            // Determine how many items to drop
            int dropCount = random.Next(lootTable.MinDrops, lootTable.MaxDrops + 1);

            // Filter entries by level
            var validEntries = lootTable.Entries
                .Where(e => level >= e.MinLevel && (e.MaxLevel == -1 || level <= e.MaxLevel))
                .ToList();

            if (validEntries.Count == 0)
                return result;

            // Calculate total weight with luck modifier
            float totalWeight = validEntries.Sum(e => e.Weight * luckModifier);

            // Track dropped items (for duplicate prevention)
            var droppedItems = new HashSet<string>();

            for (int i = 0; i < dropCount; i++)
            {
                // Weighted random selection
                float roll = (float)random.NextDouble() * totalWeight;
                float cumulative = 0f;

                foreach (var entry in validEntries)
                {
                    // Skip if duplicates not allowed and item already dropped
                    if (!lootTable.AllowDuplicates && droppedItems.Contains(entry.ItemId))
                        continue;

                    cumulative += entry.Weight * luckModifier;

                    if (roll <= cumulative)
                    {
                        // Drop chance roll
                        float chanceRoll = (float)random.NextDouble();
                        float adjustedChance = Math.Min(1.0f, entry.DropChance * luckModifier);

                        if (chanceRoll <= adjustedChance)
                        {
                            // Determine quantity
                            int quantity = random.Next(entry.MinQuantity, entry.MaxQuantity + 1);

                            result.Drops.Add(new LootDrop
                            {
                                ItemId = entry.ItemId,
                                Quantity = quantity,
                                SourceEntry = entry
                            });

                            droppedItems.Add(entry.ItemId);
                        }

                        break;
                    }
                }
            }

            return result;
        }

        public LootResult GenerateLootById(
            string tableId,
            int level = 1,
            float luckModifier = 1.0f,
            int? seed = null)
        {
            var table = _registry.GetTable(tableId);
            if (table == null)
                return default(LootResult);

            return GenerateLoot(table, level, luckModifier, seed);
        }
    }
}
```

#### LootTableRegistry

```csharp
namespace RPGFramework.Loot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default implementation of ILootTableRegistry.
    /// </summary>
    public class LootTableRegistry : ILootTableRegistry
    {
        private readonly Dictionary<string, ILootTable> _tables;

        public LootTableRegistry()
        {
            _tables = new Dictionary<string, ILootTable>();
        }

        public ILootTable GetTable(string tableId)
        {
            return _tables.TryGetValue(tableId, out var table) ? table : null;
        }

        public bool HasTable(string tableId)
        {
            return _tables.ContainsKey(tableId);
        }

        public void RegisterTable(ILootTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrEmpty(table.TableId))
                throw new ArgumentException("Table ID cannot be null or empty", nameof(table));

            _tables[table.TableId] = table;
        }

        public bool UnregisterTable(string tableId)
        {
            return _tables.Remove(tableId);
        }

        public IReadOnlyList<ILootTable> GetAllTables()
        {
            return _tables.Values.ToList();
        }

        public void Clear()
        {
            _tables.Clear();
        }
    }
}
```

---

## 4. Test Coverage Plan

### 4.1 Test File Structure

**File:** `Assets/_Framework/Loot/Tests/LootSystemTests.cs`

**Test Classes:**
1. `LootEntryTests` (4 tests)
2. `LootTableTests` (4 tests)
3. `LootGeneratorTests` (10 tests)
4. `LootTableRegistryTests` (5 tests)

**Total:** ~23 comprehensive tests

### 4.2 Test Cases

#### LootEntryTests (4 tests)

1. ✅ **CreateLootEntry_WithDefaults_UsesExpectedValues**
2. ✅ **CreateLootEntry_WithLevelRange_FiltersCorrectly**
3. ✅ **DropChance_ClampsTo_ZeroToOne**
4. ✅ **Weight_PositiveValue_IsValid**

#### LootTableTests (4 tests)

1. ✅ **CreateLootTable_WithEntries_StoresCorrectly**
2. ✅ **GuaranteedDrops_AlwaysIncluded**
3. ✅ **MinMaxDrops_ValidRange**
4. ✅ **AllowDuplicates_FlagWorks**

#### LootGeneratorTests (10 tests)

1. ✅ **GenerateLoot_ReturnsValidResult**
2. ✅ **GenerateLoot_RespectsMinMaxDrops**
3. ✅ **GenerateLoot_AddsGuaranteedDrops**
4. ✅ **GenerateLoot_FiltersEntriesByLevel**
5. ✅ **GenerateLoot_DeterministicWithSeed**
6. ✅ **GenerateLoot_LuckModifier_IncreasesDropRate**
7. ✅ **GenerateLoot_WeightedSelection_HigherWeightMoreLikely**
8. ✅ **GenerateLoot_NoDuplicates_WhenDisallowed**
9. ✅ **GenerateLoot_AllowsDuplicates_WhenEnabled**
10. ✅ **GenerateLootById_InvalidTableId_ReturnsDefault**

#### LootTableRegistryTests (5 tests)

1. ✅ **RegisterTable_AddsToRegistry**
2. ✅ **GetTable_ReturnsRegisteredTable**
3. ✅ **GetTable_InvalidId_ReturnsNull**
4. ✅ **UnregisterTable_RemovesFromRegistry**
5. ✅ **GetAllTables_ReturnsAllRegistered**

---

## 5. Integration Points

### 5.1 EPIC 11 Phase 2: Item System

**Generated Items Use Item Definitions:**
- `LootDrop.ItemId` references `IItem.ItemId`
- Loot generator creates `ItemInstance` via `IItemRegistry`

**Example Integration:**
```csharp
// Generate loot and create item instances
var lootResult = lootGenerator.GenerateLoot(table, level: 10);
var itemRegistry = GetItemRegistry();

foreach (var drop in lootResult.Drops)
{
    var itemDef = itemRegistry.GetItem(drop.ItemId);
    if (itemDef != null)
    {
        var instance = new ItemInstance(drop.ItemId, quantity: drop.Quantity);
        // Add to inventory or spawn in world
    }
}
```

### 5.2 EPIC 11 Phase 1: Inventory System

**Generated Loot Goes Into Containers:**
- Loot items added to player inventory
- Excess loot dropped on ground as `ItemComponent`

### 5.3 EPIC 10 Phase 5: Death/Life State System

**Death Triggers Loot:**
- `DeathEvent` triggers loot generation
- Killed entity's loot table ID determines drops

**Example Integration:**
```csharp
// On entity death, generate loot
void OnEntityDeath(DeathEvent deathEvent)
{
    var lootTableId = GetLootTableId(deathEvent.EntityId);
    if (!string.IsNullOrEmpty(lootTableId))
    {
        var lootResult = lootGenerator.GenerateLootById(lootTableId, level: entityLevel);
        SpawnLootInWorld(lootResult, deathEvent.EntityId);
    }
}
```

---

## 6. File Structure

```
Assets/_Framework/Loot/
├── Runtime/
│   ├── Contracts/
│   │   ├── ILootEntry.cs
│   │   ├── ILootTable.cs
│   │   ├── ILootGenerator.cs
│   │   └── ILootTableRegistry.cs
│   ├── Implementations/
│   │   ├── LootEntry.cs
│   │   ├── LootTable.cs
│   │   ├── LootGenerator.cs
│   │   └── LootTableRegistry.cs
│   ├── Data/
│   │   ├── LootDrop.cs
│   │   └── LootResult.cs
│   ├── Events/
│   │   ├── LootGeneratedEvent.cs
│   │   └── LootDroppedEvent.cs
│   └── RPGFramework.Loot.Runtime.asmdef
├── Tests/
│   ├── LootSystemTests.cs
│   └── RPGFramework.Loot.Tests.asmdef
└── README.md

Build/
├── RPGFramework.Loot.Runtime/
│   └── RPGFramework.Loot.Runtime.csproj
├── RPGFramework.Loot.Tests/
│   └── RPGFramework.Loot.Tests.csproj
└── validate_framework_loot_boundary.py

.github/workflows/
└── rpg-framework-loot.yml
```

---

## 7. Assembly Definition

**File:** `Assets/_Framework/Loot/Runtime/RPGFramework.Loot.Runtime.asmdef`

```json
{
    "name": "RPGFramework.Loot.Runtime",
    "rootNamespace": "RPGFramework.Loot",
    "references": [],
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

**Dependencies:** NONE (completely standalone module)

---

## 8. Success Criteria

- ✅ All 23 tests passing (100% pass rate)
- ✅ .NET 8.0 compilation clean (no errors, no warnings)
- ✅ Static boundary validation passed (zero Unity deps, zero MythHunter coupling)
- ✅ GitHub Actions workflow GREEN
- ✅ Zero dependencies on other Framework modules
- ✅ Deterministic loot generation with seeds
- ✅ Weighted random selection algorithm correct
- ✅ Documentation complete (implementation plan)

---

## 9. Implementation Sequence

### Phase 3A: Core Contracts and Data (2 hours)
1. Create folder structure
2. Implement data structs (LootDrop, LootResult)
3. Implement interfaces (ILootEntry, ILootTable, ILootGenerator, ILootTableRegistry)
4. Create assembly definition (no dependencies)

### Phase 3B: Implementations (3 hours)
5. Implement LootEntry class
6. Implement LootTable class
7. Implement LootTableRegistry class
8. Implement LootGenerator class (weighted random algorithm)

### Phase 3C: Events (30 min)
9. Implement LootGeneratedEvent
10. Implement LootDroppedEvent

### Phase 3D: Testing (2 hours)
11. Create test assembly definition
12. Create .NET test project
13. Write 23 comprehensive tests
14. Run tests locally (.NET)
15. Fix any failures

### Phase 3E: Validation and CI (1 hour)
16. Create boundary validation script
17. Run boundary validation
18. Create GitHub Actions workflow
19. Commit and push to feature branch
20. Verify CI passes

### Phase 3F: Documentation (30 min)
21. Create Phase 3 summary document
22. Update roadmap STATUS.md
23. Create PR for review

---

**Document Status:** Ready for implementation
**Estimated Total Effort:** 6-8 hours
**Dependencies:** NONE (standalone module)
**Approval:** Awaiting user confirmation to proceed
