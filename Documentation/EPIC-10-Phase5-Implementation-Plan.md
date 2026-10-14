# EPIC 10 Phase 5: Death/Life State System - Implementation Plan

**Date:** 2026-10-14
**Author:** Claude Code
**Epic:** EPIC 10 — RPG Foundation Domain Modules
**Phase:** 5 of 5
**Status:** In Progress

## Executive Summary

Phase 5 creates death/life state system for generic RPG Framework. This completes the EPIC 10 combat domain extraction sequence: Resources → Stats → Damage → Status Effects → **Death**.

**Key Finding:** MythHunter has **basic death detection** (`IsDead` flag, `EntityDeathEvent`) but **no resurrection/respawn system**. This allows clean generic design for life state contracts.

---

## 1. MythHunter Death System Audit

### 1.1 Existing Implementation

**HealthComponent.cs:14**
```csharp
public struct HealthComponent : IComponent
{
    public float CurrentHealth;
    public float MaxHealth;
    public bool IsDead;          // ← Death state flag
    // ...
}
```

**CombatSystem.cs:702-713** (Death Detection)
```csharp
if (health.CurrentHealth <= 0f)
{
    health.CurrentHealth = 0f;
    health.IsDead = true;

    // Publish death event
    Publish(new EntityDeathEvent
    {
        EntityId = targetEntityId,
        KillerEntityId = sourceEntityId,
        Timestamp = DateTime.UtcNow
    });
}
```

**CombatSystem.cs:184-194** (Death Event Handler)
```csharp
private void OnEntityDeath(EntityDeathEvent evt)
{
    // End all combats involving dead entity
    foreach (var combatEntry in _activeCombats)
    {
        if (combatEntry.Value.attacker == evt.EntityId ||
            combatEntry.Value.defender == evt.EntityId)
        {
            EndCombat(combatEntry.Key, CombatEndReason.Death);
        }
    }
}
```

### 1.2 What's Missing

**No resurrection system:**
- No `Resurrect()` method
- No way to reset `IsDead = false`
- No respawn logic
- No death consequences framework

**Conclusion:** Clean slate to design generic death/life state Framework contracts.

---

## 2. Framework vs Game Boundary Analysis

From EPIC-10-RPG-Domain-Audit.md:

| Element | Classification | Reason |
|---------|---------------|--------|
| `IsDead` flag | **Framework** | Universal life state |
| `EntityDeathEvent` | **Framework** | Universal death notification |
| Death detection (HP ≤ 0) | **Framework** | Common RPG pattern |
| `CombatEndReason.Death` | **Game** | MythHunter-specific combat logic |
| Resurrection/Respawn | **Framework** | Universal contracts (when needed) |
| Death consequences (loot, XP) | **Game** | MythHunter-specific rewards |

---

## 3. Proposed Framework Design

### 3.1 Core Components

**Death/Life State System provides:**

1. **LifeState enum** - Generic life states (Alive, Dead, Dying, Resurrecting, Invulnerable)
2. **ILifeStateHandler** - Interface for entities that have life state
3. **IDeathCondition** - Interface for custom death conditions (HP ≤ 0, instant death, etc.)
4. **IRespawnHandler** - Interface for respawn logic contracts
5. **LifeStateManager** - Manager for tracking and transitioning life states
6. **DeathEvent** - Framework death event
7. **ResurrectionEvent** - Framework resurrection event

### 3.2 Key Features

- ✅ **Life state tracking** - Alive, Dead, Dying (transition), Resurrecting
- ✅ **Death conditions** - Generic contracts (resource depletion, instant death, etc.)
- ✅ **Death callbacks** - OnDeath, OnResurrect lifecycle hooks
- ✅ **Respawn contracts** - Reusable respawn logic (time-based, checkpoint, etc.)
- ✅ **Invulnerability** - Temporary immunity to death
- ✅ **Integration** - Works with Phase 1 (Resources), Phase 3 (Damage), Phase 4 (Status Effects)
- ✅ **Generic design** - No Unity, no game-specific logic

---

## 4. Implementation Files

### 4.1 Enums (1 file)

**LifeState.cs:**
```csharp
public enum LifeState
{
    Alive = 0,         // Normal living state
    Dying = 1,         // Transitioning to death (allows animations, effects)
    Dead = 2,          // Dead state
    Resurrecting = 3,  // Transitioning from dead to alive
    Invulnerable = 4   // Cannot die (temporary state)
}
```

### 4.2 Interfaces (4 files)

**ILifeStateHandler.cs** - Entity that has life state:
- Properties: CurrentState, CanDie, CanResurrect
- Methods: OnDeath, OnResurrect, OnLifeStateChanged
- Queries: IsAlive, IsDead, IsInvulnerable

**IDeathCondition.cs** - Custom death condition:
- Methods: ShouldDie(target), GetDeathReason()
- Examples: ResourceDepletionCondition (HP ≤ 0), InstantDeathCondition, TimeoutCondition

**IRespawnHandler.cs** - Respawn logic contract:
- Methods: CanRespawn(target), CalculateRespawnDelay(target), OnRespawn(target)
- Properties: RespawnLocation, RespawnResources

**ILifeStateManager.cs** - Life state manager:
- Methods: Kill(entityId, killerId, reason), Resurrect(entityId), SetInvulnerable(entityId, duration)
- Queries: GetLifeState(entityId), IsAlive(entityId), IsDead(entityId)
- Events: OnDeathEvent, OnResurrectEvent

### 4.3 Events (2 files)

**DeathEvent.cs:**
```csharp
public struct DeathEvent
{
    public int EntityId;
    public int KillerEntityId;  // -1 if environmental/self
    public string DeathReason;   // "resource_depletion", "instant_death", etc.
    public float TimeOfDeath;
}
```

**ResurrectionEvent.cs:**
```csharp
public struct ResurrectionEvent
{
    public int EntityId;
    public int ResurrectorEntityId;  // -1 if self-resurrection
    public string ResurrectionType;  // "respawn", "player_revive", etc.
    public float TimeOfResurrection;
}
```

### 4.4 Implementation Classes (3 files)

**LifeStateComponent.cs** - Component for tracking life state:
```csharp
public struct LifeStateComponent
{
    public LifeState CurrentState;
    public bool CanDie;
    public bool CanResurrect;
    public float InvulnerabilityEndTime;  // -1 if not invulnerable
    public float DeathTime;               // -1 if alive
}
```

**ResourceDepletionDeathCondition.cs** - Common death condition (HP ≤ 0):
```csharp
public class ResourceDepletionDeathCondition : IDeathCondition
{
    private readonly string _resourceType;  // "Health"

    public bool ShouldDie(ILifeStateHandler target)
    {
        // Check if target's resource (HP) is depleted
        return target.GetResource(_resourceType) <= 0f;
    }
}
```

**LifeStateManager.cs** - Default manager implementation:
```csharp
public class LifeStateManager : ILifeStateManager
{
    public void Kill(int entityId, int killerId, string reason)
    {
        // Set LifeState.Dead
        // Publish DeathEvent
        // Trigger OnDeath callbacks
    }

    public void Resurrect(int entityId)
    {
        // Set LifeState.Alive
        // Publish ResurrectionEvent
        // Trigger OnResurrect callbacks
    }
}
```

---

## 5. Test Coverage Plan

### 5.1 Test Categories (30+ tests)

**Life State Transitions (8 tests):**
- Alive → Dead transition
- Alive → Dying → Dead transition
- Dead → Resurrecting → Alive transition
- Invulnerable state prevents death
- Invalid transitions rejected
- OnLifeStateChanged callbacks triggered
- Multiple transitions in sequence
- Concurrent transition handling

**Death Conditions (6 tests):**
- Resource depletion triggers death
- Instant death condition
- Multiple death conditions (priority)
- Custom death condition
- Death condition evaluation order
- Conditional immunity (undying buff)

**Death Detection (6 tests):**
- HP reaches 0 triggers death
- HP below 0 triggers death
- Invulnerable entity survives 0 HP
- Death event published with correct data
- Killer ID recorded correctly
- Death reason captured

**Resurrection (6 tests):**
- Basic resurrection (Dead → Alive)
- Resurrection with resource restoration
- Resurrection cooldown
- Cannot resurrect non-dead entity
- Resurrection event published
- Multiple resurrections

**Invulnerability (4 tests):**
- Temporary invulnerability prevents death
- Invulnerability expires after duration
- Permanent invulnerability
- Invulnerability does not prevent damage

---

## 6. Integration with Previous Phases

### 6.1 Phase 1 (Resources) Integration

Death triggered by resource depletion:

```csharp
public class HealthDeathCondition : IDeathCondition
{
    public bool ShouldDie(ILifeStateHandler target)
    {
        // Use Phase 1 ResourceComponent
        var health = target.GetResource<ResourceType>(ResourceType.Health);
        return health.Current <= 0f;
    }
}
```

### 6.2 Phase 3 (Damage) Integration

Damage can trigger death:

```csharp
public void ApplyDamage(int targetId, DamageResult damage)
{
    // Apply damage via Phase 3 calculator
    var finalDamage = _damageCalculator.Calculate(damage);

    // Reduce HP via Phase 1 resources
    _resourceManager.ModifyResource(targetId, ResourceType.Health, -finalDamage.FinalDamage);

    // Check death condition via Phase 5
    if (_lifeStateManager.ShouldDie(targetId, new HealthDeathCondition()))
    {
        _lifeStateManager.Kill(targetId, damage.SourceEntityId, "resource_depletion");
    }
}
```

### 6.3 Phase 4 (Status Effects) Integration

Status effects can prevent/trigger death:

```csharp
// Undying buff (status effect) prevents death
public class UndyingBuffEffect : StatusEffect
{
    public override void OnApplied(IStatusEffectTarget target)
    {
        // Phase 5: Set temporary invulnerability
        _lifeStateManager.SetInvulnerable(target.TargetId, Duration);
    }
}

// DOT can trigger death when HP depletes
public class PoisonEffect : RecurringStatusEffect
{
    protected override void ExecuteTick(IStatusEffectTarget target)
    {
        // Phase 3: Apply damage
        target.ApplyDamage(TickDamage, DamageType.Pure);

        // Phase 5: Check death (handled automatically by manager)
    }
}
```

---

## 7. Acceptance Criteria

### 7.1 Framework Quality

- ✅ Zero Unity dependencies (noEngineReferences: true)
- ✅ Zero MythHunter dependencies
- ✅ Pure .NET 8.0 C# code
- ✅ 30+ unit tests (100% pass rate)
- ✅ Static boundary validation passes
- ✅ GitHub Actions GREEN
- ✅ Test execution < 100ms

### 7.2 API Design

- ✅ Interface-based design
- ✅ Extensible (custom death conditions, respawn handlers)
- ✅ Framework vs Game boundary clear
- ✅ XML documentation complete
- ✅ No breaking changes to Phases 1-4
- ✅ Integration points documented

---

## 8. File Structure

```
Assets/_Framework/Combat/
├── Runtime/
│   ├── LifeState.cs (NEW)
│   ├── ILifeStateHandler.cs (NEW)
│   ├── IDeathCondition.cs (NEW)
│   ├── IRespawnHandler.cs (NEW)
│   ├── ILifeStateManager.cs (NEW)
│   ├── DeathEvent.cs (NEW)
│   ├── ResurrectionEvent.cs (NEW)
│   ├── LifeStateComponent.cs (NEW)
│   ├── ResourceDepletionDeathCondition.cs (NEW)
│   └── LifeStateManager.cs (NEW)
└── Tests/
    └── LifeStateTests.cs (NEW - 30+ tests)

Build/RPGFramework.Combat.Tests/
└── RPGFramework.Combat.Tests.csproj (UPDATED - add LifeStateTests)
```

---

## 9. MythHunter Adapter Strategy

### 9.1 Existing Code Impact

**HealthComponent.cs** - Add LifeStateComponent:
```csharp
// OLD:
public bool IsDead;

// NEW (after Framework extraction):
// Use LifeStateComponent instead
// IsDead becomes: lifeState.CurrentState == LifeState.Dead
```

**CombatSystem.cs** - Delegate to LifeStateManager:
```csharp
// OLD:
if (health.CurrentHealth <= 0f)
{
    health.IsDead = true;
    Publish(new EntityDeathEvent { ... });
}

// NEW (after Framework extraction):
if (_lifeStateManager.ShouldDie(targetId, new HealthDeathCondition()))
{
    _lifeStateManager.Kill(targetId, sourceId, "combat");
}
```

### 9.2 Adapter Pattern (Optional Phase 5.5)

Create MythHunter adapter to preserve existing API:

```csharp
// MythHunter adapter (game layer)
public class MythHunterLifeStateAdapter
{
    private readonly ILifeStateManager _frameworkManager;

    public void OnEntityDeath(EntityDeathEvent evt)
    {
        // Forward to Framework
        _frameworkManager.Kill(evt.EntityId, evt.KillerEntityId, "game_event");

        // MythHunter-specific logic
        HandleCombatEnd(evt.EntityId);
        HandleLootDrop(evt.EntityId);
        HandleExperienceGain(evt.KillerEntityId);
    }
}
```

---

## 10. Rollback Plan

If Phase 5 causes issues:

1. **Revert:** Git revert Phase 5 PR commit
2. **MythHunter still works:** No changes to existing code (additive only)
3. **Tests still pass:** Phase 5 tests are isolated
4. **CI still GREEN:** Boundary validation prevents dependencies

---

## 11. Next Steps

Starting implementation now:
1. Create LifeState.cs enum
2. Create ILifeStateHandler.cs interface
3. Create IDeathCondition.cs interface
4. Create IRespawnHandler.cs interface
5. Create ILifeStateManager.cs interface
6. Create DeathEvent.cs
7. Create ResurrectionEvent.cs
8. Implement LifeStateComponent.cs
9. Implement ResourceDepletionDeathCondition.cs
10. Implement LifeStateManager.cs
11. Create LifeStateTests.cs (30+ tests)
12. Update .csproj
13. Create PR #70

---

## 12. Open Questions

1. **Dying state duration:** Should Framework define transition timing, or leave to game?
   - **Recommendation:** Leave to game (allows custom death animations)

2. **Respawn location:** Framework provides contract, game implements logic?
   - **Recommendation:** IRespawnHandler interface (game implements)

3. **Death persistence:** Should death state serialize for save/load?
   - **Recommendation:** Yes, LifeStateComponent is serializable

4. **Multi-life systems:** Support for lives counter (arcade-style)?
   - **Recommendation:** Defer to game layer (MythHunter doesn't need it)

---

**End of Implementation Plan**
