# ⚠️ EPIC 12 Phase 1 — BLOCKER DISCOVERED

**Date:** 2026-10-16
**Status:** ❌ BLOCKED - API Misunderstanding
**Severity:** HIGH - Requires Architecture Refactoring

---

## Critical Issue Discovered

During Unity compilation validation, discovered **fundamental misunderstanding** of RPG Framework API.

### What Went Wrong

**Assumption (Incorrect):**
```csharp
// I thought ResourceComponent was a CONTAINER for multiple resources
public struct MythHunterResourceComponent {
    public ResourceComponent<MythHunterResourceType> Resources; // ❌ WRONG
}

// And had methods like:
Resources.GetCurrent(ResourceType.Health);  // ❌ Does not exist
Resources.SetCurrent(ResourceType.Health, 100f); // ❌ Does not exist
```

**Reality (Framework API):**
```csharp
// ResourceComponent<T> is a SINGLE resource
public struct ResourceComponent<TResourceType> {
    public TResourceType ResourceType; // Which resource this is
    public float Current;               // Current value
    public float Max;                   // Max value
    public float RegenRate;            // Regen per second
    public float LastRegenTime;        // Last regen timestamp
}

// NO METHODS - just fields!
// ONE component per resource type
```

### Additional API Issues

**IEntityManager:**
```csharp
// ❌ SetComponent does NOT exist
_entityManager.SetComponent(entityId, component);

// ✅ Use AddComponent (adds or replaces)
_entityManager.AddComponent(entityId, component);
```

---

## Impact Assessment

**Files Affected:** 14 files (ALL Phase 1 code)
**Lines to Refactor:** ~1900 lines
**Tests Affected:** 43 tests

**Compilation Errors:** 30+ errors across:
- `MythHunterResourceComponent.cs` (incorrect API usage)
- `CombatSystemIntegration.cs` (SetComponent → AddComponent)
- `MythHunterDamageAdapter.cs` (resource access)
- `CombatComponentMigration.cs` (resource migration)
- All 3 test files (API calls)

---

## Architecture Decision Required

### Option 1: Multiple ResourceComponent Instances (Framework Pattern)

```csharp
// Entity has 5 separate components
_entityManager.AddComponent(entityId, new ResourceComponent<MythHunterResourceType> {
    ResourceType = MythHunterResourceType.Health,
    Current = 100f,
    Max = 100f,
    RegenRate = 0f
});

_entityManager.AddComponent(entityId, new ResourceComponent<MythHunterResourceType> {
    ResourceType = MythHunterResourceType.Mana,
    Current = 50f,
    Max = 50f,
    RegenRate = 5f
});
// ... repeat for Stamina, Rage, Concentration
```

**Pros:**
- Uses Framework as designed
- Each resource independently tracked
- Clean ECS pattern

**Cons:**
- 5 components per entity (more queries)
- Less convenient API
- More boilerplate

### Option 2: Resource Container Wrapper (Custom)

```csharp
public struct MythHunterResourceComponent : IComponent {
    public ResourceData Health;
    public ResourceData Mana;
    public ResourceData Stamina;
    public ResourceData Rage;
    public ResourceData Concentration;
}

public struct ResourceData {
    public float Current;
    public float Max;
    public float RegenRate;
}
```

**Pros:**
- Single component per entity
- Convenient API
- Easier migration

**Cons:**
- NOT using Framework ResourceComponent
- Defeats purpose of Framework integration
- Need custom logic

### Option 3: Hybrid Approach

Use Framework `ResourceComponent<T>` internally but provide helper methods:

```csharp
public static class ResourceHelpers {
    public static float GetHealth(IEntityManager em, int entityId) {
        var resources = em.GetComponent<ResourceComponent<MythHunterResourceType>[]>(entityId);
        return resources.First(r => r.ResourceType == MythHunterResourceType.Health).Current;
    }
}
```

**Pros:**
- Uses Framework components
- Convenient helper API

**Cons:**
- Array storage complexity
- Performance overhead (array search)

---

## Recommended Path Forward

**Recommendation: Option 2 (Resource Container Wrapper)**

**Reasoning:**
1. Phase 1 goal: **Game-layer integration**, not pure Framework usage
2. MythHunter needs **convenient resource access** (health, mana together)
3. Adapter pattern allows **future** migration to pure Framework if needed
4. Simpler implementation = faster delivery

**Implementation:**
```csharp
// MythHunterResourceComponent - custom wrapper
public struct MythHunterResourceComponent : IComponent {
    // Direct fields (no Framework ResourceComponent)
    public float HealthCurrent;
    public float HealthMax;
    public float ManaCurrent;
    public float ManaMax;
    public float StaminaCurrent;
    public float StaminaMax;
    public float RageCurrent;
    public float RageMax;
    public float ConcentrationCurrent;
    public float ConcentrationMax;

    public bool IsAlive() => HealthCurrent > 0f;
    public float GetHealthPercentage() => HealthMax > 0 ? HealthCurrent / HealthMax : 0f;
}
```

**Future:** Phase 2 can integrate Framework `ResourceComponent<T>` properly if multi-instance pattern works better.

---

## Refactoring Checklist

- [ ] Update `MythHunterResourceComponent` to use direct fields
- [ ] Fix all `SetComponent` → `AddComponent` calls
- [ ] Update `MythHunterDamageAdapter` resource access
- [ ] Update `CombatSystemIntegration` resource helpers
- [ ] Update `CombatComponentMigration` resource migration
- [ ] Fix all 3 test files
- [ ] Remove incorrect `using RPGFramework.Combat` from resource component
- [ ] Update documentation to reflect custom wrapper approach

---

## Time Estimate

**Refactoring:** 2-3 hours
- Update 14 files
- Fix 43 tests
- Validate compilation

**Alternative:** Start Phase 1 fresh with correct architecture (4-5 hours total)

---

## Next Session Instructions

```bash
/start

# Decision needed:
# 1. Refactor existing code to Option 2 (custom wrapper)
# 2. Start Phase 1 fresh with correct understanding

# If refactoring:
# - Update MythHunterResourceComponent to direct fields
# - Global find/replace: SetComponent → AddComponent
# - Update all resource access to use fields
# - Run Unity validation

# If starting fresh:
# - Delete Phase 1 branch
# - Create new branch with correct API
# - Reference EPIC-12-Phase1-Implementation-Plan.md
# - Use correct Framework API from start
```

---

## Lessons Learned

✅ **Always validate Framework API** before implementation
✅ **Compile early** - discovered issues late (after 7 commits)
✅ **Unit tests don't catch integration issues** - need Unity compilation
✅ **Document assumptions** explicitly
✅ **Quick API verification** saves hours of rework

**Root Cause:** Assumed Framework had helper methods without checking actual API.

---

## Current PR Status

**PR #76:** https://github.com/Aizekhan/MythHunter/pull/76
**Status:** ❌ DOES NOT COMPILE
**Commits:** 7 (all need refactoring)
**Recommendation:** Close and restart OR force-push refactored code

---

**Phase 1 BLOCKED pending architecture decision.**

Next session: Decide approach → Refactor → Validate → Merge
