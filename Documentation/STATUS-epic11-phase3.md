# Project Status

> Operational pointer only. Roadmap scope/history live in [RPG_FRAMEWORK_MASTER_PLAN.md](RPG_FRAMEWORK_MASTER_PLAN.md). Do not duplicate milestone histories here.
> Last reconciled against GitHub: 2026-10-14.

## Repositories
- Roadmap: [Aizekhan/RPG-Framework-Roadmap](https://github.com/Aizekhan/RPG-Framework-Roadmap)
- Source: [Aizekhan/MythHunter](https://github.com/Aizekhan/MythHunter)
- Source branch: `dev`
- Current source head: `3e717b7` — Latest merged: [PR #73 EPIC 11 Phase 3](https://github.com/Aizekhan/MythHunter/pull/73).

## Verified completed sequence

### EPIC 05 (Framework Runtime Modules)
- EPIC 05.4: PR #25 merged as `df83fdbc438071bfe714978ade7416f9cb2a4146`; Unity EditMode 33/33 and integration checks confirmed.
- EPIC 05.5: PR #27 merged as `81e621ca98aa4ef409ead5cae7b6380328689b4c`; Unity EditMode 36/36 and integration checks confirmed.
- EPIC 05.6: PR #28 merged as `4830f7735dce6a12ecbf2eee3d5c2379c9dda340`; Unity EditMode 41/41 and integration checks confirmed.
- EPIC 05.7: PR #29 merged as `3b2783bd8005c58a3454dfad7dc132272fccdbd4`.
- EPIC 05.8: PR #30 merged as `e7ecbd854a7ff2c8b30a8308597b452ec570fd7c`.
- EPIC 05.9: PR #54 merged as `e8f724f` — ComponentCache OnDemand positive-membership semantics.

### EPIC 06 (Serialization Foundation) — ✅ COMPLETE
- **PR #63** merged as `39af13b` — All 5 phases complete:
  - **Phase 1:** Core contracts (`ISerializable`, `ITypeRegistry`, `SerializableTypeAttribute`, `IMigrator`)
  - **Phase 2:** VersionedSerializer with migration support
  - **Phase 3:** Component serialization (Framework-agnostic registry)
  - **Phase 4:** Delta compression module (optional)
  - **Phase 5:** Network serialization refactored (stable type IDs)
- Files: 26 changed (+2016 −281), 20 new Framework files, 24 tests
- Assemblies: `RPGFramework.Serialization.Runtime`, `RPGFramework.DeltaCompression.Runtime`
- Compiler fix: `dece04d` — Fixed EventBus test struct definitions

### EPIC 07 (Modular Game Composition System) — ✅ COMPLETE
- **PR #58** merged as `0862793` — Phase 1: Module infrastructure, FrameworkCoreModule, GameLayerModule, InfrastructureModule, GameplayModule, NetworkingModule, LobbyModule, DebugToolsModule. Unity EditMode 58/60 (96.7%).
- **PR #59** merged as `92c9abe` — Phase 2: CoreInstaller split into FrameworkCoreInstaller and GameCoreInstaller, module migration, LazyDI deduplication, InstallerRegistry removal.

### EPIC 08 (Dependency Debt and Lifecycle Stabilization) — ✅ COMPLETE
- **PR #60** merged as `8faddb9` — Phases 1 & 2
- **PR #61** merged as `cad8439` — Phase 3
- **PR #62** merged as `e8bd55a` — Phase 4

### EPIC 09 (Optional Runtime Services and Adapters) — ✅ COMPLETE (Phase 1)
- **PR #64** merged as `9adfb80` — Phase 1: Generic Pooling Foundation
  - Created `RPGFramework.Pooling.Runtime` assembly
  - Extracted `IObjectPool<T>`, `ObjectPool<T>`, `PoolStatistics` (20 tests)
  - Refactored `GameObjectPool` to Unity adapter pattern
  - Audit: `EPIC-09-Services-Audit.md` (590 lines)
  - Hotfix: Domain events interface compliance
  - Stats: 72 files (+1988 −61)
- **Status:** Phase 1 complete ✅, Phases 2-3 deferred until proven need

### EPIC 10 (RPG Foundation Domain Modules) — ✅ **COMPLETE** 🎉
All 5 phases successfully extracted RPG combat domain into universal Framework modules.

- **PR #65** merged as `8b5dc22` — Phase 1: Resource System extraction
- **PR #66** merged as `b9652f8` — Test Infrastructure Fixes
- **PR #67** merged as `999405c` — Phase 2: Stat System extraction
- **PR #68** merged as `2fd0b99` — Phase 3: Damage Calculation System
- **PR #69** merged as `f26bad5` — Phase 4: Status Effect System
- **PR #70** merged as `1475b2c` — Phase 5: Death/Life State System

**EPIC 10 Summary:**
- **Completed:** All 5 phases (Resources → Stats → Damage → Status Effects → Death/Life State)
- **Total Files:** 112 files (+9274 insertions, −248 deletions)
- **Total Tests:** 124 comprehensive tests (100% pass rate)
- **Assemblies Created:** `RPGFramework.Combat.Runtime`, `RPGFramework.Stats.Runtime`
- **Framework Quality:** Zero Unity dependencies, zero MythHunter coupling, pure .NET 8.0 C#

### EPIC 11 (Optional Gameplay Modules) — 🚧 **IN PROGRESS**

Extracting gameplay systems (inventory, items, abilities, loot, progression) into universal RPG Framework modules.

- **PR #71** merged as `58fa490` — Phase 1: Inventory System extraction ✅ CI PASSED
  - Created `RPGFramework.Inventory.Runtime` assembly
  - Generic inventory containers: `IInventoryContainer<TItem>` with capacity management
  - Equipment slot management: `IEquipmentSlot<TSlotType, TItem>` with generic slot types
  - Item stacking support: `IItemStack` with split/merge operations
  - Inventory component: `InventoryComponent<TItem, TSlotType>` with factory methods
  - Domain events: `ItemAddedEvent`, `ItemRemovedEvent`, `ItemEquippedEvent`, `ItemUnequippedEvent`
  - 24 comprehensive NUnit tests (100% pass rate: 24/24 in 52ms)
  - .NET 8.0 test harness + static boundary validator
  - GitHub Actions workflow: `rpg-framework-inventory.yml`
  - Audit: `EPIC-11-Gameplay-Domain-Audit.md` (760 lines)
  - Implementation plan: `EPIC-11-Phase1-Implementation-Plan.md` (660 lines)
  - Stats: 43 files (+3647 insertions)
  - Validation: .NET tests ✅, Static boundary ✅, GitHub Actions GREEN ✅
  - Zero Unity dependencies (noEngineReferences: true)
  - Zero MythHunter changes (additive only)

- **PR #72** merged as `9842621` — Phase 2: Item System extraction ✅ CI PASSED
  - Created `RPGFramework.Items.Runtime` assembly
  - Core interfaces: `IItem`, `IEquipment<TSlotType, TStatType, TValue>`, `IConsumable`, `IItemInstance`, `IItemRegistry`
  - Enumerations: `ItemType` (6 categories), `ItemRarity` (6 tiers), `ItemQuality` (5 tiers)
  - Implementations: `Item`, `Equipment<>`, `Consumable`, `ItemInstance`, `ItemRegistry`
  - Component: `ItemComponent` (dropped items with pickup/despawn logic)
  - Events: `ItemCreatedEvent`, `ItemDestroyedEvent`, `ItemUsedEvent`
  - 27 comprehensive NUnit tests (100% pass rate: 27/27 in 745ms)
  - .NET 8.0 test harness + static boundary validator
  - GitHub Actions workflow: `rpg-framework-items.yml`
  - Implementation plan: `EPIC-11-Phase2-Implementation-Plan.md` (1483 lines)
  - Stats: 27 files (+3106 insertions)
  - Validation: .NET tests ✅ (27/27 PASSED), Static boundary ✅ (17 files), GitHub Actions GREEN ✅
  - Zero Unity dependencies (noEngineReferences: true)
  - Zero MythHunter changes (additive only)
  - Integration with EPIC 10 Phase 2 (Stats): Equipment provides stat modifiers
  - Integration with Phase 1 (Inventory): Items stored in containers, equipped in slots

- **PR #73** merged as `3e717b7` — Phase 3: Loot System extraction ✅ CI PASSED
  - Created `RPGFramework.Loot.Runtime` assembly
  - Core interfaces: `ILootEntry`, `ILootTable`, `ILootGenerator`, `ILootTableRegistry`
  - Data structures: `LootDrop` (single drop result), `LootResult` (complete generation result with metadata)
  - Weighted random selection algorithm with deterministic seeding
  - Level-based entry filtering (minLevel/maxLevel)
  - Luck/magic find modifiers affecting drop rates and selection weights
  - Guaranteed drops bypassing probability calculations
  - Duplicate prevention (configurable per-table)
  - Drop count ranges (minDrops/maxDrops)
  - Implementations: `LootEntry`, `LootTable`, `LootGenerator`, `LootTableRegistry`
  - Events: `LootGeneratedEvent`, `LootDroppedEvent`
  - 23 comprehensive NUnit tests (100% pass rate: 23/23)
  - .NET 8.0 test harness + static boundary validator (12 files scanned, zero violations)
  - GitHub Actions workflow: `rpg-framework-loot.yml`
  - Implementation plan: `EPIC-11-Phase3-Implementation-Plan.md` (944 lines)
  - Stats: 20 files (+2176 insertions)
  - Validation: .NET tests ✅ (23/23 PASSED), Static boundary ✅ (12 files), GitHub Actions GREEN ✅
  - Zero Unity dependencies (noEngineReferences: true)
  - Zero MythHunter changes (additive only)
  - Zero Framework dependencies (first fully standalone module, uses string ItemIds)
  - Integration with Phase 2 (Items): Loot generates item IDs for lookup

**EPIC 11 Completed Phases:**
- Phase 1: Inventory System ✅ COMPLETE (24 tests, 43 files)
- Phase 2: Item System ✅ COMPLETE (27 tests, 27 files)
- Phase 3: Loot System ✅ COMPLETE (23 tests, 20 files)

**EPIC 11 Planned Phases:**
- Phase 4: Ability System (pending - ability definitions, cooldowns, resource costs)
- Phase 5: Progression System (pending - XP, leveling, stat growth, unlocks)
- Phase 6: Integration (pending - MythHunter adapters, end-to-end validation)

**EPIC 11 Progress:**
- **Completed:** 3 of 6 phases
- **Total Files:** 90 files (+8929 insertions)
- **Total Tests:** 24 + 27 + 23 = **74 comprehensive tests**
- **Test Success Rate:** 100% (74/74 tests pass)
- **Assemblies Created:** `RPGFramework.Inventory.Runtime`, `RPGFramework.Items.Runtime`, `RPGFramework.Loot.Runtime`
- **Framework Quality:** Zero Unity dependencies, zero MythHunter coupling, pure .NET 8.0 C#

### Earlier milestones
- PR #50 merged as `0af6ab74f8236b5de7455349c795377c883b5295`: `PreloadDebugTool.RenderGUI` GUI calls guarded by `#if UNITY_EDITOR`.
- PR #48 merged as `0d97060d45d978cafbe376cd43f00eb8fdf9f175`: Incremental membership reconciliation, rotating refresh batches, OnDemand live reads and ComponentCache characterization tests.
- **Architecture Enforcement Phase 1**: PR #57 merged as `6637cca` — Foundation layer with architecture tests.
- **JobHandle IL2CPP fix**: PR #56 merged as `d36b271` — Fixes IL2CPP compilation error in `JobHandleExtensions.IsValid()`.
- PR #52 merged documentation reconciliation at `54a35204dab5ced9a825bb4e7946ab8efdb31ce3`.
- PR #53 merged as `6bef9b210ee35b40e4c28e74e5550c3f1c58d762`: MovementSystem authoritative write-through.

## Active Work

**EPIC 11 Phase 3 complete!** 🎉 Loot System extraction finished.

**Next Focus:** EPIC 11 Phase 4 — Ability System (ability definitions, cooldowns, resource costs)

**Planned for Phase 4:**
- `IAbility`, `IAbilityDefinition`, `IAbilityInstance` interfaces
- Cooldown management and tracking
- Resource cost calculations (HP/MP/Stamina)
- Casting time and channeling support
- Target validation and range checks
- Ability effects and modifiers
- 20-25 comprehensive tests

## Dormant GitHub PRs — not active roadmap work
These historical PRs predate the current architecture effort and must not be merged as-is.
- [PR #1 — CombatSystem V2](https://github.com/Aizekhan/MythHunter/pull/1): residual diff only folder .meta files.
- [PR #3 — RuneSystemV1](https://github.com/Aizekhan/MythHunter/pull/3): 1 commit ahead and 244 behind dev.
- [PR #5 — Feature/phases](https://github.com/Aizekhan/MythHunter/pull/5): 9 commits ahead and 224 behind dev.
- [PR #4 — UnifiedMythHunterWizard](https://github.com/Aizekhan/MythHunter/pull/4): long-open dev → main integration PR.
