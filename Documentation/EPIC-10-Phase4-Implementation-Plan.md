# EPIC 10 Phase 4: Status Effect System - Implementation Plan

**Date:** 2026-10-13
**Author:** Claude Code
**Epic:** EPIC 10 — RPG Foundation Domain Modules
**Phase:** 4 of 5
**Status:** In Progress

## Executive Summary

Phase 4 creates status effect system (buffs, debuffs, damage over time, healing over time) for generic RPG Framework. This builds on Phase 2 (Stats with modifiers) and Phase 3 (Damage Calculator) to provide temporal stat modifications and recurring effects.

**Key Finding:** MythHunter has **no existing status effect implementation**. This allows clean generic design without legacy constraints.

---

## 1. MythHunter Status Effect Audit

### 1.1 Search Results

**File Search:**
- `**/*Buff*.cs`: No matches
- `**/*Effect*.cs`: No matches
- `**/*Status*.cs`: No matches

**Code Search:**
- No `BuffSystem`, `DebuffSystem`, or `StatusEffectManager` classes
- No interfaces for status effects
- No mentions of buffs/debuffs in CombatSystem

### 1.2 Conclusion

MythHunter **does not have** an implemented status effect system. This creates opportunity to design pure generic system based on RPG best practices, similar to Phase 1 (Resources) and Phase 2 (Stats).

---

## 2. Proposed Framework Design

### 2.1 Core Components

**Status Effect System provides:**

1. **StatusEffectType enum** - Generic effect classifications (Buff, Debuff, DOT, HOT, Stun, etc.)
2. **StackBehavior enum** - How multiple effects stack (Stack, Replace, Refresh, Ignore, StackCount)
3. **IStatusEffect** - Core effect interface with lifecycle hooks
4. **IStatusEffectTarget** - Interface for entities that receive effects
5. **IStatusEffectManager** - Manager for applying/removing/updating effects
6. **StatusEffect base class** - Common implementation
7. **TimedStatusEffect** - Effects with expiration
8. **RecurringStatusEffect** - Effects that tick (DOT/HOT)

### 2.2 Key Features

- ✅ **Duration tracking** - Permanent (-1) or timed effects
- ✅ **Stacking behaviors** - Multiple application handling
- ✅ **Lifecycle hooks** - OnApplied, OnUpdate, OnRemoved
- ✅ **Recurring effects** - Tick-based DOT/HOT
- ✅ **Stat integration** - Works with Phase 2 StatContainer
- ✅ **Generic design** - No Unity, no game-specific logic

---

## 3. Implementation Files

### 3.1 Enums (2 files)

**StatusEffectType.cs:**
```csharp
public enum StatusEffectType
{
    Buff = 0,           // Positive stat boost
    HealOverTime = 1,   // HOT
    Shield = 2,         // Damage absorption

    Debuff = 100,       // Negative stat reduction
    DamageOverTime = 101, // DOT
    Stun = 102,         // Action prevention
    Slow = 103,         // Speed reduction

    Transform = 200,    // Shape-shift
    Channeling = 201,   // Ongoing channel

    Custom = 1000       // Game-specific extensions
}
```

**StackBehavior.cs:**
```csharp
public enum StackBehavior
{
    Stack = 0,        // Allow multiple instances
    Replace = 1,      // Replace old with new
    Refresh = 2,      // Reset duration of existing
    Ignore = 3,       // Ignore new if exists
    StackCount = 4    // Increment counter
}
```

### 3.2 Interfaces (3 files)

**IStatusEffect.cs** - Core effect contract with:
- Properties: EffectId, Name, EffectType, StackBehavior, Duration, RemainingDuration, IsActive, StackCount, MaxStacks
- Methods: OnApplied, OnUpdate, OnRemoved, UpdateDuration, RefreshDuration, AddStack, RemoveStack

**IStatusEffectTarget.cs** - Entity that receives effects:
- TargetId, CanReceiveEffect, GetStat, ApplyStatModifier, RemoveStatModifier, ApplyDamage, ApplyHealing

**IStatusEffectManager.cs** - Effect manager:
- ApplyEffect, RemoveEffect, RemoveAllEffects, RemoveEffectsOfType, GetActiveEffects, HasEffect, UpdateEffects

### 3.3 Implementation Classes (4 files)

**StatusEffect.cs** - Abstract base class with common implementation

**TimedStatusEffect.cs** - Effects that expire after duration

**RecurringStatusEffect.cs** - Effects that tick at intervals (DOT/HOT base)

**StatusEffectManager.cs** - Default manager implementation with stacking logic

---

## 4. Test Coverage Plan

### 4.1 Test Categories (30+ tests)

**Core Functionality (10 tests):**
- Apply effect to target
- Remove effect from target
- Remove all effects
- Remove by type
- Get active effects
- Has effect check
- Multiple effects on target
- Effect lifecycle (Applied → Active → Removed)
- Null validation
- Target validation

**Duration & Timing (6 tests):**
- Permanent effects (duration = -1)
- Timed effect expiration
- Duration update
- Duration refresh
- Remaining duration calculation
- Zero duration validation

**Stacking Behaviors (10 tests):**
- Stack: Multiple instances allowed
- Replace: New replaces old
- Refresh: Duration reset
- Ignore: New application ignored
- StackCount: Counter increment
- StackCount: Max stacks limit
- StackCount: Remove stack
- Stack with different effect IDs
- Stack limit enforcement

**Recurring Effects (4 tests):**
- DOT tick interval
- Tick damage calculation
- Stack count affects tick damage
- Multiple ticks over duration

---

## 5. Integration with Previous Phases

### 5.1 Phase 2 (Stats) Integration

Status effects apply stat modifiers via StatContainer:

```csharp
public class StrengthBuffEffect : TimedStatusEffect
{
    public override void OnApplied(IStatusEffectTarget target)
    {
        // Apply +20 Strength via Phase 2 StatContainer
        target.ApplyStatModifier("Strength", 20f, StatModifierType.Additive);
    }

    public override void OnRemoved(IStatusEffectTarget target)
    {
        target.RemoveStatModifier("Strength", this);
    }
}
```

### 5.2 Phase 3 (Damage) Integration

DOT effects use Phase 3 damage calculator:

```csharp
public class PoisonEffect : RecurringStatusEffect
{
    protected override void ExecuteTick(IStatusEffectTarget target)
    {
        target.ApplyDamage(TickDamage, DamageType.Pure);
    }
}
```

---

## 6. Acceptance Criteria

### 6.1 Framework Quality

- ✅ Zero Unity dependencies (noEngineReferences: true)
- ✅ Zero MythHunter dependencies
- ✅ Pure .NET 8.0 C# code
- ✅ 30+ unit tests (100% pass rate)
- ✅ Static boundary validation passes
- ✅ GitHub Actions GREEN
- ✅ Test execution < 100ms

### 6.2 API Design

- ✅ Interface-based design
- ✅ Extensible (custom effect types)
- ✅ Framework vs Game boundary clear
- ✅ XML documentation complete
- ✅ No breaking changes to Phases 1-3

---

## 7. File Structure

```
Assets/_Framework/Combat/
├── Runtime/
│   ├── StatusEffectType.cs (NEW)
│   ├── StackBehavior.cs (NEW)
│   ├── IStatusEffect.cs (NEW)
│   ├── IStatusEffectTarget.cs (NEW)
│   ├── IStatusEffectManager.cs (NEW)
│   ├── StatusEffect.cs (NEW)
│   ├── TimedStatusEffect.cs (NEW)
│   ├── RecurringStatusEffect.cs (NEW)
│   └── StatusEffectManager.cs (NEW)
└── Tests/
    └── StatusEffectTests.cs (NEW - 30+ tests)

Build/RPGFramework.Combat.Tests/
└── RPGFramework.Combat.Tests.csproj (UPDATED - add StatusEffectTests)
```

---

## 8. Timeline

**Total Estimated Time:** 4-6 hours

1. **Enums & Interfaces** (1h) - Create 2 enums + 3 interfaces
2. **Base Implementation** (2h) - 4 implementation classes
3. **Test Suite** (2h) - 30+ comprehensive tests
4. **Documentation & PR** (1h) - XML docs, summary, PR creation

---

## 9. Next Steps

Starting implementation now:
1. Create StatusEffectType.cs
2. Create StackBehavior.cs
3. Create IStatusEffect.cs
4. Create IStatusEffectTarget.cs
5. Create IStatusEffectManager.cs
6. Implement StatusEffect.cs
7. Implement TimedStatusEffect.cs
8. Implement RecurringStatusEffect.cs
9. Implement StatusEffectManager.cs
10. Create StatusEffectTests.cs (30+ tests)
11. Update .csproj
12. Create PR #69

---

**End of Implementation Plan**
