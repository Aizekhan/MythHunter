# EPIC 10 Phase 3: Damage Calculation System — Implementation Plan

> **Date:** 2026-10-13
> **Author:** Claude Code
> **Status:** Planning
> **Dependencies:** EPIC 10 Phase 1 (Resources), Phase 2 (Stats)

---

## Executive Summary

Extract damage calculation system from MythHunter into reusable Framework module. Provide generic damage calculation formulas, damage types framework, and resistance mechanics that work for any RPG game.

**Principle:** Framework provides generic damage calculator and resistance formulas; MythHunter provides game-specific damage types and combat flow.

---

## 1. MythHunter Damage System Audit

### 1.1 Current Implementation

**Location:** `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs`

**Damage Calculation Method:** `ProcessAttack()` (line 483-602)

#### Damage Formula (line 595):
```csharp
float finalDamage = (baseDamage * damageMultiplier) * (100f / (100f + defenderStats.Defense));
```

**Formula Breakdown:**
1. **Base Damage:** `attackerStats.AttackPower`
2. **Multipliers:**
   - **Critical:** `attackerStats.CriticalMultiplier` (if crit)
   - **Block:** `0.5x` (50% reduction if blocked)
3. **Defense Formula:** `damage * (100 / (100 + defense))`
   - 0 defense = 100% damage
   - 100 defense = 50% damage
   - 200 defense = 33% damage

### 1.2 Damage Types

**File:** `Assets/_MythHunter/Code/Events/Domain/Gameplay/CombatEvents.cs` (line 80-85)

```csharp
public enum DamageType
{
    Physical,  // Physical attacks (melee, ranged)
    Magical,   // Magical spells
    Pure       // True damage (ignores resistances)
}
```

**Usage:** Currently only Physical damage used in CombatSystem (line 601)

### 1.3 Combat Stats Used

**File:** `Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs`

**Damage-Related Stats:**
- `AttackPower` — Base damage value
- `Defense` — Damage reduction stat
- `CriticalChance` — Probability of critical hit (0.0-1.0)
- `CriticalMultiplier` — Damage multiplier on crit (e.g., 2.0 = 200%)
- `Accuracy` — Hit chance (0.0-1.0)
- `DodgeChance` — Probability to dodge (0.0-1.0)
- `BlockChance` — Probability to block (0.0-1.0)
- `CounterAttackChance` — Probability to counter (unused)

### 1.4 Combat Flow

**Attack Resolution Order (CombatSystem.cs:498-602):**
1. **Accuracy Check** (line 499) → Miss if failed
2. **Dodge Check** (line 510) → No damage if dodged (costs Concentration)
3. **Block Check** (line 537) → 50% damage if blocked (costs Concentration)
4. **Critical Check** (line 560) → Apply critical multiplier
5. **Damage Calculation** (line 595) → Apply defense formula
6. **Apply Damage** (line 601) → Reduce health

### 1.5 Missing Features

**No Resistance System:**
- DamageType exists (Physical, Magical, Pure) but not used in calculations
- No per-type resistances (e.g., PhysicalResistance, MagicalResistance)
- Pure damage doesn't actually bypass defenses

**No Armor Penetration:**
- No stat to reduce defender's defense
- No percentage-based armor penetration

**No Damage Variance:**
- Damage is deterministic (no min-max range)
- No random damage rolls

**No Element System:**
- No elemental damage types (Fire, Ice, Lightning, etc.)
- No elemental resistances or weaknesses

---

## 2. Framework vs Game Boundary Analysis

### 2.1 What's Framework-Generic?

✅ **Generic Damage Calculation:**
- Base damage formulas
- Critical hit calculations
- Defense/resistance formulas
- Damage type framework

✅ **Generic Interfaces:**
- `IDamageCalculator` — Calculate damage with modifiers
- `IResistanceCalculator` — Calculate resistances
- Damage type enums

✅ **Generic Mechanics:**
- Multiplicative damage modifiers
- Additive defense/resistance
- Critical hit multipliers
- Armor penetration calculations

### 2.2 What's MythHunter-Specific?

❌ **Game-Specific Flow:**
- Accuracy/Dodge/Block checks (before damage calculation)
- Rage/Concentration costs
- Combat state management
- Attack timing (AttackSpeed intervals)

❌ **Game-Specific Stats:**
- CombatStatsComponent (11 specific stats)
- Rage/Concentration mechanics
- Block reduces damage by exactly 50%

❌ **Game-Specific Events:**
- DamageAppliedEvent (with game-specific fields)
- CombatStartedEvent, CombatEndedEvent

### 2.3 Extraction Boundary

**Framework Scope (Phase 3):**
- Generic damage calculation formulas
- Damage type framework (extensible enum)
- Resistance calculations
- Critical hit mechanics
- Armor penetration formulas

**MythHunter Adapter (Phase 3.5):**
- CombatSystem continues to use existing flow
- Delegates damage calculation to Framework
- Maps CombatStatsComponent → Framework calculator
- Preserves existing events and mechanics

---

## 3. Proposed Framework Design

### 3.1 Assembly Structure

**Assembly:** `RPGFramework.Combat.Runtime` (extends existing assembly from Phase 1)

**Namespace:** `RPGFramework.Combat`

**Files to Add:**
```
Assets/_Framework/Combat/Runtime/
  - IDamageCalculator.cs        (damage calculation interface)
  - DamageCalculator.cs          (default implementation)
  - DamageType.cs                (generic damage type enum)
  - DamageResult.cs              (calculation result struct)
  - IResistanceCalculator.cs     (resistance calculation interface)
  - ResistanceCalculator.cs      (default implementation)
```

### 3.2 Core Interfaces

#### IDamageCalculator

```csharp
namespace RPGFramework.Combat
{
    /// <summary>
    /// Generic damage calculator interface
    /// </summary>
    public interface IDamageCalculator
    {
        /// <summary>
        /// Calculate final damage amount
        /// </summary>
        /// <param name="baseDamage">Base damage before modifiers</param>
        /// <param name="defenseValue">Defender's defense stat</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <param name="criticalMultiplier">Critical damage multiplier (e.g., 2.0)</param>
        /// <param name="damageMultipliers">Additional multipliers (buffs, debuffs)</param>
        /// <param name="armorPenetration">Armor penetration value (reduces defense)</param>
        /// <returns>Final damage amount</returns>
        float CalculateDamage(
            float baseDamage,
            float defenseValue,
            bool isCritical = false,
            float criticalMultiplier = 1.0f,
            float[] damageMultipliers = null,
            float armorPenetration = 0f);

        /// <summary>
        /// Calculate damage with full result (damage, is crit, etc.)
        /// </summary>
        DamageResult CalculateDamageWithResult(
            float baseDamage,
            float defenseValue,
            float criticalChance,
            float criticalMultiplier,
            float[] damageMultipliers = null,
            float armorPenetration = 0f);
    }
}
```

#### DamageResult

```csharp
namespace RPGFramework.Combat
{
    /// <summary>
    /// Result of damage calculation
    /// </summary>
    public struct DamageResult
    {
        public float FinalDamage;
        public float BaseDamage;
        public float EffectiveDefense;
        public bool IsCritical;
        public float CriticalMultiplier;
        public float TotalMultiplier;
    }
}
```

#### IDamageType (Generic)

```csharp
namespace RPGFramework.Combat
{
    /// <summary>
    /// Generic damage type enum (games can extend)
    /// </summary>
    public enum DamageType
    {
        Physical = 0,
        Magical = 1,
        Pure = 2,        // True damage (ignores resistances)

        // Extensible: games can cast to int and add custom types
        // e.g., Fire = 100, Ice = 101, Lightning = 102
    }
}
```

### 3.3 Damage Formulas

**Default Formula (matches MythHunter):**
```csharp
finalDamage = (baseDamage * totalMultiplier) * (100 / (100 + effectiveDefense))

where:
  totalMultiplier = critMultiplier * additionalMultipliers
  effectiveDefense = max(0, defense - armorPenetration)
```

**Alternative Formulas (for other games):**
1. **Linear Defense:** `finalDamage = max(1, baseDamage - defense)`
2. **Percentage Reduction:** `finalDamage = baseDamage * (1 - (defense / 100))`
3. **Diminishing Returns:** `finalDamage = baseDamage * (100 / (100 + defense * 0.7))`

### 3.4 Resistance System (Future Extension)

**IResistanceCalculator:**
```csharp
public interface IResistanceCalculator
{
    /// <summary>
    /// Apply resistance to damage
    /// </summary>
    float ApplyResistance(float damage, DamageType damageType, float resistanceValue);
}
```

**Usage:**
```csharp
// Physical damage with Physical resistance
float damage = 100f;
float physicalResistance = 50f; // 50% reduction
float finalDamage = resistanceCalculator.ApplyResistance(damage, DamageType.Physical, physicalResistance);
// finalDamage = 50f
```

---

## 4. Implementation Plan

### Phase 3.1: Framework Damage Calculator

**Goal:** Extract damage formulas into Framework

**Tasks:**
1. ✅ Audit MythHunter damage system (DONE)
2. Create `IDamageCalculator.cs` interface
3. Create `DamageCalculator.cs` implementation
4. Create `DamageResult.cs` struct
5. Create `DamageType.cs` enum (generic)
6. Write 15+ comprehensive tests

**Deliverables:**
- Generic damage calculator (reusable)
- Defense formula implementation
- Critical hit mechanics
- Armor penetration support
- Damage multiplier stacking

**Test Coverage:**
- Base damage calculation
- Defense reduction formula
- Critical hit multiplier
- Multiple damage multipliers (stack correctly)
- Armor penetration (reduces defense)
- Edge cases (0 defense, negative values, Pure damage)

### Phase 3.2: Build Infrastructure

**Goal:** .NET test harness + CI validation

**Tasks:**
1. Extend `Build/RPGFramework.Combat.Tests/RPGFramework.Combat.Tests.csproj`
2. Update `Build/validate_framework_combat_boundary.py`
3. Update `.github/workflows/rpg-framework-combat.yml`

**Validation:**
- .NET 8.0 compilation
- All tests pass
- Zero Unity dependencies
- Zero MythHunter dependencies

### Phase 3.3: Documentation

**Tasks:**
1. ✅ Create `EPIC-10-Phase3-Implementation-Plan.md` (this document)
2. Create `EPIC-10-Phase3-Summary.md` (after implementation)

### Phase 3.4: MythHunter Adapter (Optional Phase 3.5)

**Goal:** Update CombatSystem to use Framework calculator

**Tasks:**
1. Inject `IDamageCalculator` into CombatSystem
2. Replace inline formula with Framework call
3. Preserve existing behavior (backward compatible)
4. Smoke test combat

**Example Adapter:**
```csharp
// Before (inline formula):
float finalDamage = (baseDamage * damageMultiplier) * (100f / (100f + defenderStats.Defense));

// After (Framework):
float finalDamage = _damageCalculator.CalculateDamage(
    baseDamage,
    defenderStats.Defense,
    isCritical,
    attackerStats.CriticalMultiplier,
    new[] { blockMultiplier }  // 0.5 if blocked
);
```

---

## 5. Technical Decisions

### 5.1 Generic Type Parameters?

**Decision:** NO generic type parameters for Phase 3

**Rationale:**
- Damage calculations use `float` values (standard in RPGs)
- Defense is `float` (not stat-specific)
- Unlike Phase 2 (StatContainer<TStatType, TValue>), damage formulas are value-based
- Simpler API: `CalculateDamage(float, float)` vs `CalculateDamage<T>(T, T)`

**Trade-off:**
- Less flexible (only float values)
- Acceptable: 99% of RPGs use float for damage/defense

### 5.2 Defense Formula Choice

**Decision:** Use MythHunter's formula: `damage * (100 / (100 + defense))`

**Rationale:**
- Standard in RPGs (League of Legends, Dota 2 use similar)
- Diminishing returns (200 defense ≠ 100% reduction)
- Easy to understand (100 defense = 50% reduction)
- Matches MythHunter's existing formula (preserves balance)

**Alternative Considered:** Linear reduction `damage - defense`
- Rejected: Can reduce damage to 0 or negative (breaks game balance)

### 5.3 Critical Hit Calculation

**Decision:** Multiplicative critical (damage * critMultiplier)

**Rationale:**
- Standard in RPGs (critical = 2x, 3x damage)
- Scales with base damage and other multipliers
- Matches MythHunter's CriticalMultiplier stat

**Example:**
```
Base: 100
Multipliers: +50% (buff), 2x (crit)
Final: 100 * 1.5 * 2.0 = 300 damage
```

### 5.4 Damage Multiplier Stacking

**Decision:** Multiplicative stacking (mult1 * mult2 * mult3)

**Rationale:**
- Matches MythHunter's block multiplier (0.5x)
- Consistent with Phase 2 stat modifiers (multiplicative)
- Standard in RPGs (buffs/debuffs multiply)

**Example:**
```
Base: 100
Buffs: +20% (1.2), +30% (1.3)
Block: -50% (0.5)
Final: 100 * 1.2 * 1.3 * 0.5 = 78 damage
```

### 5.5 Armor Penetration

**Decision:** Subtract from defense before calculation

**Rationale:**
- Standard in RPGs (reduce effective defense)
- Formula: `effectiveDefense = max(0, defense - armorPenetration)`
- Cannot reduce defense below 0

**Example:**
```
Defense: 100
Armor Pen: 30
Effective Defense: 70
Damage Reduction: 100 / (100 + 70) = 58.8%
```

### 5.6 Pure Damage (True Damage)

**Decision:** Defer to Phase 3.5 (Resistance System)

**Rationale:**
- Pure damage should ignore resistances, not defense
- Phase 3 focuses on defense formulas
- Resistance system (Phase 3.5) will handle Pure damage bypass

---

## 6. Acceptance Criteria

### 6.1 Framework Module

✅ **Assembly:**
- `RPGFramework.Combat.Runtime` assembly updated
- `noEngineReferences: true` (zero Unity dependencies)
- References: ECS, Serialization, Stats (from Phase 2)

✅ **Interfaces:**
- `IDamageCalculator` — damage calculation interface
- `DamageResult` — calculation result struct
- `DamageType` enum (generic)

✅ **Implementation:**
- `DamageCalculator` — default implementation
- Defense formula: `damage * (100 / (100 + defense))`
- Critical hit support
- Damage multiplier stacking
- Armor penetration

### 6.2 Test Coverage

✅ **15+ Tests:**
1. Base damage (no modifiers)
2. Defense reduction (0, 50, 100, 200 defense)
3. Critical hit multiplier (2x, 3x)
4. Single damage multiplier
5. Multiple damage multipliers (stacking)
6. Armor penetration (reduces defense)
7. Block multiplier (0.5x)
8. Combined: crit + buff + block
9. Edge case: 0 defense
10. Edge case: negative defense (clamped to 0)
11. Edge case: 0 base damage
12. Edge case: 100% damage reduction (impossible)
13. DamageResult struct (all fields correct)
14. Armor penetration > defense (clamps to 0)
15. Multiple multipliers (order independence)

✅ **Test Execution:**
- .NET 8.0 test harness
- All tests parameterless (hermetic)
- Execution time < 500ms

### 6.3 Build Infrastructure

✅ **.NET Test Harness:**
- Extends `Build/RPGFramework.Combat.Tests/RPGFramework.Combat.Tests.csproj`
- Compiles Framework Combat + dependencies
- Runs NUnit tests without Unity

✅ **Static Boundary Validator:**
- Extends `Build/validate_framework_combat_boundary.py`
- Validates: no Unity/MythHunter deps in damage files
- GUID uniqueness

✅ **GitHub Actions:**
- `.github/workflows/rpg-framework-combat.yml` updated
- Runs tests on push/PR to dev and feature branch
- Steps: boundary validation → .NET tests

### 6.4 Documentation

✅ **Implementation Plan:**
- `EPIC-10-Phase3-Implementation-Plan.md` (this document)

✅ **Summary Document:**
- `EPIC-10-Phase3-Summary.md` (created after implementation)
- Implementation history (commits, files, stats)
- Validation results
- Technical decisions
- Metrics

---

## 7. Rollback Plan

**If Phase 3 blocks production:**

**Option A: Feature Flag**
- Add `USE_FRAMEWORK_DAMAGE_CALCULATOR` flag
- Toggle between Framework and inline formula
- Zero risk during transition

**Option B: Defer Adapter (Phase 3.5)**
- Ship Framework module (additive, no MythHunter changes)
- Defer CombatSystem refactor to Phase 3.5
- Framework validated independently

**Option C: Revert PR**
- Revert Phase 3 PR
- Framework module self-contained (no game dependencies)
- MythHunter unaffected

---

## 8. Dependencies

### 8.1 Framework Dependencies

**Direct:**
- `RPGFramework.ECS.Runtime` — IComponent interface
- `RPGFramework.Serialization.Runtime` — ISerializable interface (for future)
- `RPGFramework.Stats.Runtime` — Stat modifiers (for future integration)

### 8.2 EPIC Dependencies

**Required:**
- ✅ EPIC 06: Serialization foundation
- ✅ EPIC 07: Modular composition
- ✅ EPIC 08: Lifecycle stabilization
- ✅ EPIC 09 Phase 1: Generic pooling
- ✅ EPIC 10 Phase 1: Resource System
- ✅ EPIC 10 Phase 2: Stat System

**Future Integration:**
- Phase 2 stat modifiers can affect damage calculations
- Phase 4 status effects can add damage multipliers

---

## 9. Timeline Estimate

**Phase 3.1: Framework Damage Calculator**
- Interfaces + implementation: 2 hours
- Test suite (15+ tests): 2 hours
- **Subtotal:** 4 hours

**Phase 3.2: Build Infrastructure**
- Extend .NET test harness: 30 min
- Update boundary validator: 30 min
- Update GitHub Actions: 15 min
- **Subtotal:** 1.25 hours

**Phase 3.3: Documentation**
- ✅ Implementation Plan: DONE
- Summary document: 1 hour
- **Subtotal:** 1 hour

**Phase 3.4: MythHunter Adapter (Optional)**
- CombatSystem refactor: 1 hour
- Smoke tests: 30 min
- **Subtotal:** 1.5 hours

**Total (Phase 3.1-3.3):** ~6 hours
**Total (with Phase 3.4):** ~7.5 hours

---

## 10. Next Steps (After Approval)

1. ✅ Create feature branch: `feature/epic-10-phase-3-damage-calculator`
2. Implement `IDamageCalculator.cs`
3. Implement `DamageCalculator.cs`
4. Implement `DamageResult.cs`
5. Implement `DamageType.cs`
6. Create test suite (`DamageCalculatorTests.cs`)
7. Extend build infrastructure
8. Run local validation (boundary + .NET tests)
9. Push and create PR
10. CI validation (GitHub Actions)
11. Create summary document
12. Merge PR
13. (Optional) Phase 3.5: MythHunter adapter

---

## 11. Questions for Review

**Q1:** Should Phase 3 include resistance system, or defer to Phase 3.5?
**A:** Defer. Focus on core damage formula. Resistance adds complexity (per-type resistances, damage type mapping).

**Q2:** Should damage formulas be configurable (strategy pattern)?
**A:** Not for Phase 3. Default formula sufficient. Future: `IDefenseFormula` interface for custom formulas.

**Q3:** Should `DamageType` be extensible (games add custom types)?
**A:** Yes. Use int-based enum. Games can cast to int and add custom types (Fire = 100, Ice = 101).

**Q4:** Should armor penetration be percentage or flat value?
**A:** Both. Add `flatArmorPen` and `percentArmorPen` parameters. Formula: `effectiveDef = (defense - flat) * (1 - percent)`.

---

🤖 Generated with [Claude Code](https://claude.com/claude-code)

**Date:** 2026-10-13
**Phase:** EPIC 10 Phase 3 — Planning Complete
**Next:** Implementation (IDamageCalculator + tests)
