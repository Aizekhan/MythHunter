# EPIC 10 Phase 2: Stat System — Implementation Plan

> **Date:** 2026-10-13
> **Phase:** EPIC 10 Phase 2 (Stat System extraction)
> **Dependencies:** EPIC 06 (Serialization), EPIC 10 Phase 1 (Resource System)
> **Status:** Planning

---

## Objective

Extract generic stat/attribute system from MythHunter into reusable Framework module supporting:
- Generic stat storage (any stat type, any value type)
- Stat modifiers (additive, multiplicative, overrides)
- Modifier calculations (base + additives → multiply)
- MythHunter adapter for seamless migration

**Framework Principle:** Framework provides generic stat contracts; MythHunter provides game-specific stat types and definitions.

---

## Current MythHunter Stat System Analysis

### 1. StatType Enum

**Location:** `Assets/_MythHunter/Code/Components/Character/StatType.cs:10-106`

**Structure:**
- **91 stat types** across **6 categories**:
  - **Basic** (12): Level, HP, Stamina, Regen, Vision
  - **Combat** (45): Damage, Attack Speed, Crit, Block, Armor, etc.
  - **Magic** (2): MagicChance, MagicPower
  - **Status** (6): Poison, Bleeding, Plague
  - **Economy** (9): Gold, Item drop chances
  - **Inventory** (1): BagSlots

**Example Stats:**
```csharp
public enum StatType
{
    Level,
    HP,
    HpRegen,
    Stamina,
    StaminaRegen,
    Damage,
    CritChance,
    CritPower,
    // ... 83 more stats
}
```

**Analysis:**
- ✅ Game-specific: MythHunter-specific RPG stats
- ❌ Not Framework-generic: Other RPG games have different stat types
- 🎯 **Boundary:** StatType enum stays in MythHunter; Framework uses generic `TStatType` parameter

### 2. StatsComponent

**Location:** `Assets/_MythHunter/Code/Components/Character/StatsComponent.cs:11-83`

**Structure:**
```csharp
public struct StatsComponent : ISerializableComponent
{
    public Dictionary<StatType, float> Values;

    public float GetStat(StatType type, float defaultValue = 0f);
    public void SetStat(StatType type, float value);

    // Convenience properties
    public float Level => GetStat(StatType.Level, 1);
    public float HP => GetStat(StatType.HP);
    // ... more properties

    public byte[] Serialize();
    public void Deserialize(byte[] data);
}
```

**Features:**
- ✅ Simple dictionary storage (StatType → float)
- ✅ Convenience accessors for common stats
- ✅ Custom serialization (writes count + pairs)
- ❌ No modifiers (buffs/debuffs)
- ❌ No base vs current value distinction
- ❌ No stat calculation formulas

**Analysis:**
- 🔄 **Extractable:** Dictionary storage pattern is generic
- 🔄 **Extractable:** GetStat/SetStat methods are generic
- ❌ **Game-specific:** Convenience properties (HP, Damage, etc.)
- 🎯 **Framework:** `IStatContainer<TStatType, TValue>` with generic storage

### 3. StatRegistry

**Location:** `Assets/_MythHunter/Code/Data/StatRegistry.cs:13-188`

**Structure:**
```csharp
public static class StatRegistry
{
    private static readonly Dictionary<StatType, StatDefinition> _definitions;

    static StatRegistry() { /* Registers 91 stats */ }

    public static StatDefinition GetDefinition(StatType type);
    public static IEnumerable<StatDefinition> GetAllDefinitions();
    public static IEnumerable<StatDefinition> GetDefinitionsByCategory(StatCategory category);
}

public class StatDefinition
{
    public StatType Type { get; set; }
    public string Name { get; set; }           // Localized name
    public string Description { get; set; }    // Localized description
    public float MinValue { get; set; }        // Min allowed value
    public float MaxValue { get; set; }        // Max allowed value
    public StatCategory Category { get; set; } // UI category
    public Color Color { get; set; }           // UI color
}
```

**Features:**
- ✅ Static registry of all stat definitions
- ✅ Metadata: name, description, min/max, category, color
- ✅ Category-based queries
- ❌ UnityEngine.Color dependency (not Framework-safe)

**Analysis:**
- ❌ **Game-specific:** All 91 stat definitions are MythHunter-specific
- ❌ **Game-specific:** UI-related (Color, localized names)
- ❌ **Unity dependency:** Uses `UnityEngine.Color`
- 🎯 **Boundary:** StatRegistry stays in MythHunter; Framework doesn't need metadata

### 4. CombatStatsComponent

**Location:** `Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs:11-87`

**Structure:**
```csharp
public struct CombatStatsComponent : IComponent, ISerializableComponent
{
    // Combat stats (14 fields)
    public float AttackPower;
    public float Defense;
    public float CriticalChance;
    public float Rage;
    public float MaxRage;
    public float Concentration;
    public float MaxConcentration;
    // ... more fields

    // Combat state (3 fields)
    public bool IsInCombat;
    public int CombatTargetEntityId;
    public float LastAttackTime;

    public byte[] Serialize();
    public void Deserialize(byte[] data);
}
```

**Analysis:**
- ❌ **Duplicate storage:** Stats already in `StatsComponent` (Rage, CritChance, etc.)
- ❌ **Mixed concerns:** Stats + combat state in one component
- 🔄 **Refactor opportunity:** Merge with StatsComponent, extract combat state to separate component
- 🎯 **Recommendation:** Phase out CombatStatsComponent in favor of generic stat system

### 5. Stat Modifiers (Missing)

**Current State:**
- ❌ No `IStatModifier` interface
- ❌ No buff/debuff system
- ❌ No additive/multiplicative modifier calculations
- ❌ No temporary stat boosts
- ❌ Stats are simple values (no base vs current distinction)

**Impact:**
- Game cannot implement buffs/debuffs properly
- Temporary effects (potions, equipment bonuses) not supported
- No modifier stacking logic

---

## Framework vs Game Boundary

### Framework Layer (RPGFramework.Stats.Runtime)

**What belongs in Framework:**
- ✅ `IStatContainer<TStatType, TValue>` — generic stat storage
- ✅ `IStatModifier<TStatType, TValue>` — generic modifier interface
- ✅ `StatModifierType` enum — additive, multiplicative, override
- ✅ Stat calculation formulas: `(base + additives) * multiplicatives`
- ✅ Modifier priority/ordering
- ✅ Serialization support (using EPIC 06 infrastructure)

**What does NOT belong in Framework:**
- ❌ StatType enum (game-specific stat list)
- ❌ StatRegistry (game-specific metadata, UI colors)
- ❌ Localized names/descriptions
- ❌ UnityEngine dependencies (Color, etc.)

### Game Layer (MythHunter)

**What stays in MythHunter:**
- ✅ StatType enum (91 game-specific stats)
- ✅ StatCategory enum (Basic, Combat, Magic, etc.)
- ✅ StatRegistry (metadata, min/max values, UI colors)
- ✅ StatsComponent adapter (uses Framework under the hood)
- ✅ Game-specific stat calculation logic (if any)

---

## Proposed Framework Design

### 1. IStatContainer<TStatType, TValue>

Generic stat storage interface supporting any stat type and value type.

```csharp
namespace RPGFramework.Stats
{
    /// <summary>
    /// Generic stat storage container
    /// </summary>
    /// <typeparam name="TStatType">Stat identifier type (enum or struct)</typeparam>
    /// <typeparam name="TValue">Stat value type (float, int, etc.)</typeparam>
    public interface IStatContainer<TStatType, TValue>
        where TStatType : struct, IConvertible
        where TValue : struct, IComparable<TValue>
    {
        /// <summary>
        /// Get base stat value (before modifiers)
        /// </summary>
        TValue GetBaseStat(TStatType statType);

        /// <summary>
        /// Set base stat value
        /// </summary>
        void SetBaseStat(TStatType statType, TValue value);

        /// <summary>
        /// Get final stat value (after modifiers applied)
        /// </summary>
        TValue GetFinalStat(TStatType statType);

        /// <summary>
        /// Check if stat exists
        /// </summary>
        bool HasStat(TStatType statType);

        /// <summary>
        /// Get all stat types in container
        /// </summary>
        IEnumerable<TStatType> GetAllStatTypes();
    }
}
```

### 2. IStatModifier<TStatType, TValue>

Generic stat modifier interface for buffs/debuffs.

```csharp
namespace RPGFramework.Stats
{
    /// <summary>
    /// Modifier type for stat calculations
    /// </summary>
    public enum StatModifierType
    {
        Additive,       // Adds to base value: base + modifier
        Multiplicative, // Multiplies final value: (base + additives) * (1 + modifier)
        Override        // Overrides final value (highest priority)
    }

    /// <summary>
    /// Generic stat modifier (buff/debuff)
    /// </summary>
    /// <typeparam name="TStatType">Stat identifier type</typeparam>
    /// <typeparam name="TValue">Stat value type</typeparam>
    public interface IStatModifier<TStatType, TValue>
        where TStatType : struct, IConvertible
        where TValue : struct, IComparable<TValue>
    {
        /// <summary>
        /// Unique modifier ID (for removal)
        /// </summary>
        string ModifierId { get; }

        /// <summary>
        /// Stat type being modified
        /// </summary>
        TStatType StatType { get; }

        /// <summary>
        /// Modifier value
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// Modifier type (additive, multiplicative, override)
        /// </summary>
        StatModifierType ModifierType { get; }

        /// <summary>
        /// Priority for ordering (higher = applied later)
        /// </summary>
        int Priority { get; }
    }
}
```

### 3. StatContainer<TStatType, TValue> Component

Generic stat component with modifier support.

```csharp
namespace RPGFramework.Stats
{
    /// <summary>
    /// Generic stat container component with modifier support
    /// </summary>
    [SerializableType(typeId: 3)] // Stable type ID for serialization
    public struct StatContainer<TStatType, TValue> : IComponent, ISerializable, IStatContainer<TStatType, TValue>
        where TStatType : struct, IConvertible
        where TValue : struct, IComparable<TValue>
    {
        // Internal storage
        private Dictionary<TStatType, TValue> _baseStats;
        private Dictionary<TStatType, List<IStatModifier<TStatType, TValue>>> _modifiers;

        // IStatContainer implementation
        public TValue GetBaseStat(TStatType statType);
        public void SetBaseStat(TStatType statType, TValue value);
        public TValue GetFinalStat(TStatType statType); // Calculates with modifiers
        public bool HasStat(TStatType statType);
        public IEnumerable<TStatType> GetAllStatTypes();

        // Modifier management
        public void AddModifier(IStatModifier<TStatType, TValue> modifier);
        public void RemoveModifier(string modifierId);
        public void RemoveAllModifiers(TStatType statType);
        public void ClearAllModifiers();

        // ISerializable implementation
        public byte[] Serialize();
        public void Deserialize(byte[] data);
    }
}
```

### 4. Stat Calculation Formula

**Formula:**
```
FinalStat = ((BaseStat + AdditiveModifiers) * MultiplicativeModifiers) or OverrideModifiers
```

**Calculation Order:**
1. Start with BaseStat
2. Apply all Additive modifiers: `base + mod1 + mod2 + ...`
3. Apply all Multiplicative modifiers: `value * (1 + mod1) * (1 + mod2) * ...`
4. If Override modifier exists, use highest-priority override value
5. Return final calculated value

**Example:**
```csharp
BaseStat = 100
Additive modifiers: +20, +30 → 100 + 20 + 30 = 150
Multiplicative modifiers: +10%, +20% → 150 * 1.1 * 1.2 = 198
FinalStat = 198
```

---

## MythHunter Adapter Options

### Option A: Internal Adapter (Recommended for Phase 2)

**Approach:** Preserve existing `StatsComponent` API, delegate to Framework internally.

**Implementation:**
```csharp
namespace MythHunter.Components.Character
{
    public struct StatsComponent : ISerializableComponent
    {
        // Framework stat container (internal)
        private StatContainer<StatType, float> _stats;

        // Public API (backward compatible)
        public float GetStat(StatType type, float defaultValue = 0f)
        {
            return _stats.HasStat(type) ? _stats.GetFinalStat(type) : defaultValue;
        }

        public void SetStat(StatType type, float value)
        {
            _stats.SetBaseStat(type, value);
        }

        // Modifier API (new feature)
        public void AddModifier(IStatModifier<StatType, float> modifier)
        {
            _stats.AddModifier(modifier);
        }

        public void RemoveModifier(string modifierId)
        {
            _stats.RemoveModifier(modifierId);
        }

        // Convenience properties (unchanged)
        public float Level => GetStat(StatType.Level, 1);
        public float HP => GetStat(StatType.HP);
        // ... etc

        // Serialization (delegates to Framework)
        public byte[] Serialize() => _stats.Serialize();
        public void Deserialize(byte[] data) => _stats.Deserialize(data);
    }
}
```

**Benefits:**
- ✅ Zero MythHunter code changes required
- ✅ Existing systems continue to work
- ✅ New modifier feature available immediately
- ✅ Gradual migration path

**Drawbacks:**
- ❌ Indirection layer (minor performance cost)
- ❌ Duplicate API surface

### Option B: Direct Migration (Phase 3+)

**Approach:** Replace `StatsComponent` with Framework component directly.

**Implementation:**
```csharp
// MythHunter systems updated to use Framework component directly
if (_entityManager.HasComponent<StatContainer<StatType, float>>(entityId))
{
    var stats = _entityManager.GetComponent<StatContainer<StatType, float>>(entityId);
    float damage = stats.GetFinalStat(StatType.Damage);
}
```

**Benefits:**
- ✅ Pure Framework usage (cleaner architecture)
- ✅ No indirection
- ✅ Faster performance

**Drawbacks:**
- ❌ Requires updating all MythHunter systems
- ❌ Breaking API change
- ❌ Higher migration risk

**Recommendation:** Option B deferred to Phase 3 after Phase 2 validation.

---

## Implementation Plan

### Phase 2 Deliverables

**1. Framework Stats Runtime Module:**
- Assembly: `RPGFramework.Stats.Runtime`
- Files:
  - `IStatContainer.cs`
  - `IStatModifier.cs`
  - `StatModifierType.cs` (enum)
  - `StatContainer.cs` (component implementation)
  - `StatModifier.cs` (default modifier implementation)
- References: ECS.Runtime, Logging.Runtime, Serialization.Runtime
- `noEngineReferences: true`
- `allowUnsafeCode: true` (for generic serialization)

**2. Test Suite:**
- Assembly: `RPGFramework.Stats.Tests`
- Tests (15+ comprehensive NUnit tests):
  1. `StatContainer_InitialState_IsEmpty`
  2. `StatContainer_SetBaseStat_StoresValue`
  3. `StatContainer_GetFinalStat_NoModifiers_ReturnsBase`
  4. `StatContainer_AdditiveModifier_AppliesCorrectly`
  5. `StatContainer_MultiplicativeModifier_AppliesCorrectly`
  6. `StatContainer_OverrideModifier_OverridesValue`
  7. `StatContainer_MultipleAdditives_SumCorrectly`
  8. `StatContainer_MultipleMultiplicatives_MultiplyCorrectly`
  9. `StatContainer_MixedModifiers_CalculateCorrectOrder`
  10. `StatContainer_RemoveModifier_RecalculatesStat`
  11. `StatContainer_Serialization_RoundTrip`
  12. `StatContainer_MultipleStatTypes_IndependentModifiers`
  13. `StatContainer_ModifierPriority_OrdersCorrectly`
  14. `StatContainer_ClearAllModifiers_ResetsToBase`
  15. `StatContainer_GenericTypes_IntValues`

**3. Build Infrastructure:**
- .NET Test Harness: `Build/RPGFramework.Stats.Tests/RPGFramework.Stats.Tests.csproj`
- Static Boundary Validator: `Build/validate_framework_stats_boundary.py`
- GitHub Actions: `.github/workflows/rpg-framework-stats.yml`

**4. Documentation:**
- `EPIC-10-Phase2-Implementation-Plan.md` (this document)
- `EPIC-10-Phase2-Summary.md` (post-implementation)

**5. MythHunter Integration (Phase 2):**
- **Option A** adapter: Update `StatsComponent` to use Framework internally
- NO changes to existing MythHunter systems (backward compatible)

### Step-by-Step Implementation

**Step 1: Create Framework Stats Module (Week 1)**
1. Create `Assets/_Framework/Stats/Runtime/` directory
2. Create `RPGFramework.Stats.Runtime.asmdef`
3. Implement `IStatContainer<TStatType, TValue>`
4. Implement `IStatModifier<TStatType, TValue>`
5. Implement `StatContainer<TStatType, TValue>` component
6. Implement `StatModifier<TStatType, TValue>` default class
7. Add XML documentation (100% coverage)

**Step 2: Create Test Suite (Week 1)**
1. Create `Assets/_Framework/Stats/Tests/` directory
2. Create `RPGFramework.Stats.Tests.asmdef`
3. Write 15+ comprehensive tests
4. Create .NET test harness (compiles without Unity)
5. Verify all tests pass locally

**Step 3: Build Infrastructure (Week 1)**
1. Create static boundary validator (no Unity/MythHunter deps)
2. Create GitHub Actions workflow
3. Verify CI green on feature branch

**Step 4: MythHunter Adapter (Week 2)**
1. Update `StatsComponent.cs` (Option A: internal adapter)
2. Add modifier API methods
3. Preserve backward compatibility
4. Update serialization to delegate to Framework

**Step 5: Validation (Week 2)**
1. .NET tests: 15/15 passing
2. Static boundary: no Unity dependencies
3. Unity EditMode tests: all tests passing
4. Full project compile: clean Console
5. Smoke tests: stat systems working

**Step 6: Documentation & PR (Week 2)**
1. Write `EPIC-10-Phase2-Summary.md`
2. Create PR with detailed description
3. Merge after approval
4. Update roadmap STATUS.md

---

## Testing Strategy

### Unit Tests (.NET)

**Test Coverage:**
- ✅ Stat storage (get/set base stats)
- ✅ Modifier application (additive, multiplicative, override)
- ✅ Calculation order (base → additive → multiplicative → override)
- ✅ Modifier priority
- ✅ Modifier removal
- ✅ Serialization round-trip
- ✅ Multiple stat types (int, float)
- ✅ Edge cases (empty container, no modifiers, negative values)

**Test Framework:** NUnit 3.13+
**Execution:** .NET 8.0 test harness (no Unity)

### Integration Tests (Unity EditMode)

**Test Coverage:**
- ✅ Framework Stats tests (same as .NET)
- ✅ MythHunter adapter compatibility
- ✅ Backward compatibility (existing stat usage)
- ✅ Serialization with actual Unity environment

### Smoke Tests (Manual)

**Test Scenarios:**
1. Character stat display (HP, Damage, etc.)
2. Equipment bonus application (modifier test)
3. Buff/debuff system (temporary modifiers)
4. Save/load game (serialization test)
5. Combat calculations (final stat usage)

---

## Acceptance Criteria

**Framework Module:**
- ✅ `RPGFramework.Stats.Runtime` assembly created
- ✅ Generic stat container with modifier support
- ✅ 15+ comprehensive tests passing
- ✅ Zero Unity dependencies (noEngineReferences: true)
- ✅ Zero MythHunter dependencies
- ✅ 100% XML documentation coverage
- ✅ .NET test harness compiles and runs
- ✅ Static boundary validation passes
- ✅ GitHub Actions workflow green

**MythHunter Integration:**
- ✅ StatsComponent updated (Option A adapter)
- ✅ Backward compatible (existing systems work)
- ✅ Modifier API available (AddModifier, RemoveModifier)
- ✅ Unity compilation clean (0 errors)
- ✅ Unity EditMode tests passing
- ✅ Smoke tests passing

**Documentation:**
- ✅ Implementation plan written
- ✅ Summary document created
- ✅ PR description comprehensive
- ✅ Roadmap updated

---

## Rollback Plan

### If Framework Module Causes Issues

**Step 1: Identify Scope**
- Stats module isolated (no MythHunter changes initially)
- Only new files added in Phase 2

**Step 2: Revert Options**

**Option A: Git Revert**
```bash
git revert <merge-commit-sha>
git push origin dev
```

**Option B: Remove Stats Module**
```bash
git checkout dev~1 -- Assets/_Framework/Stats/
git checkout dev~1 -- Build/RPGFramework.Stats.Tests/
git checkout dev~1 -- Build/validate_framework_stats_boundary.py
git checkout dev~1 -- .github/workflows/rpg-framework-stats.yml
git commit -m "revert: remove Stats module"
```

**Step 3: Verify**
- Unity recompiles without Stats module
- GitHub Actions green on dev
- No orphaned references

### If MythHunter Adapter Causes Issues

**Step 1: Revert StatsComponent**
```bash
git checkout dev~1 -- Assets/_MythHunter/Code/Components/Character/StatsComponent.cs
git commit -m "revert: rollback StatsComponent adapter"
```

**Step 2: Verify**
- Unity compilation clean
- Existing systems work
- Stats display correctly

---

## Open Questions

**1. Value Type Support**
- **Q:** Should Framework support int stats (e.g., Level, BagSlots)?
- **A:** Yes, use `where TValue : struct, IComparable<TValue>` to support int, float, double

**2. Modifier Stacking**
- **Q:** How should multiple multiplicative modifiers stack?
- **A:** Compound multiplication: `value * (1 + mod1) * (1 + mod2)`
- **Alternative:** Additive multiplication: `value * (1 + mod1 + mod2)` (configurable)

**3. Negative Modifiers**
- **Q:** Should Framework allow negative stats (debt, penalties)?
- **A:** Yes, Framework allows any value; game layer enforces policy

**4. Temporary Modifiers (Duration)**
- **Q:** Should modifiers have duration/expiry?
- **A:** Deferred to Phase 4 (Status Effect System) — modifiers are stateless, duration managed externally

**5. Modifier Source Tracking**
- **Q:** Should modifiers track source (equipment, buff, passive)?
- **A:** Optional extension property; not required for Phase 2

**6. CombatStatsComponent Migration**
- **Q:** Should Phase 2 merge CombatStatsComponent into StatsComponent?
- **A:** No, defer to Phase 3. Phase 2 focuses on Framework extraction only.

---

## Dependencies

### Framework Dependencies (Direct)
- ✅ `RPGFramework.ECS.Runtime` — IComponent interface
- ✅ `RPGFramework.Logging.Runtime` — ILogger interface
- ✅ `RPGFramework.Serialization.Runtime` — ISerializable interface

### EPIC Dependencies (Indirect)
- ✅ EPIC 06: Serialization foundation (ISerializable contract)
- ✅ EPIC 07: Modular composition (assembly definitions)
- ✅ EPIC 08: Lifecycle stabilization (no circular deps)
- ✅ EPIC 10 Phase 1: Resource System (pattern validation)

---

## Success Metrics

**Code Statistics:**
- Target: 20-25 files changed
- Target: +2000 insertions
- Framework Runtime: 4-5 C# files (~200 lines each)
- Framework Tests: 1-2 C# files (~250 lines total)
- Build Infrastructure: 3 files (asmdef, validator, CI workflow)

**CI Performance:**
- Static Boundary Validation: <5 seconds
- .NET Compilation + Tests: <10 seconds
- Total CI feedback time: <1 minute

**Quality Indicators:**
- ✅ Zero compiler warnings
- ✅ Zero test failures
- ✅ Zero Unity dependencies in Framework
- ✅ 100% XML documentation coverage
- ✅ Hermetic tests (no external dependencies)

---

## Lessons From Phase 1

**1. Interface Contract Verification**
- Always check Framework interface definitions before implementing
- Verify serialization signature matches `ISerializable`

**2. .NET Test Harness Value**
- Fast feedback (8 sec vs 10+ min Unity)
- Catches compilation errors immediately
- Essential for Framework modules

**3. Pre-existing Issues Don't Block**
- Document unrelated errors separately
- Don't let pre-existing issues delay new work

**4. Additive Changes Reduce Risk**
- Phase 2 adds new Framework module (zero MythHunter changes initially)
- Adapter pattern preserves backward compatibility
- Easy rollback if needed

---

## Timeline Estimate

**Week 1: Framework Module**
- Day 1: Create assembly, interfaces, enums
- Day 2: Implement StatContainer component
- Day 3: Implement StatModifier class
- Day 4: Write tests (15+ tests)
- Day 5: Build infrastructure (validator, CI)

**Week 2: Integration & Validation**
- Day 1: MythHunter adapter (StatsComponent)
- Day 2: Unity EditMode tests
- Day 3: Full validation (compile, tests, smoke)
- Day 4: Documentation (summary, PR description)
- Day 5: PR review and merge

**Total:** 2 weeks (10 working days)

---

## Next Steps

### Immediate Actions

1. ✅ **Audit complete** — Current MythHunter stat system analyzed
2. ⏳ **Design Framework interfaces** — IStatContainer, IStatModifier
3. ⏳ **Create feature branch:** `feature/epic-10-phase-2-stat-system`
4. ⏳ **Implement Framework module**
5. ⏳ **Write comprehensive tests**
6. ⏳ **Create build infrastructure**
7. ⏳ **MythHunter adapter (Option A)**
8. ⏳ **Validation and PR**

### Post-Phase 2

**EPIC 10 Phase 3: Damage Calculation System**
- Extract damage types, resistances, damage formulas
- Build on Phase 2 stat modifiers
- Create damage calculation service

**EPIC 10 Phase 4: Status Effect System**
- Extract buffs, debuffs, duration management
- Build on Phase 2 stat modifiers (add duration)
- Status effect stacking logic

**EPIC 10 Phase 5: Death/Life State System**
- Extract death detection, respawn contracts
- IDeathHandler, IRespawnable interfaces
- Death state machine

---

🤖 Generated with [Claude Code](https://claude.com/claude-code)

**Date:** 2026-10-13
**Phase:** EPIC 10 Phase 2 — Stat System Planning Complete
**Next:** Begin implementation of Framework Stats Runtime module
