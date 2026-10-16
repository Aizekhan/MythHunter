# EPIC 12 Phase 1: Core Combat Integration — Implementation Plan

**Date:** 2026-10-16
**Target:** Integrate Stats, Resources, and Damage systems into MythHunter combat
**Estimated Duration:** 3-5 days

---

## Overview

Replace MythHunter's old `CombatStatsComponent` with Framework modules:
- `RPGFramework.Stats.Runtime` → Stat system with modifiers
- `RPGFramework.Combat.Runtime` → Resources (HP/MP/etc.) + Damage calculation

**Goal:** MythHunter combat runs on Framework modules, zero duplicate functionality.

---

## Current State Analysis

### What Needs Replacement

**File:** `Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs`
```csharp
public struct CombatStatsComponent : IComponent
{
    public float AttackPower;
    public float Defense;
    public float AttackSpeed;
    public float CriticalChance;
    public float CriticalMultiplier;
    public float Accuracy;
    public float DodgeChance;
    public float BlockChance;
    public float CounterAttackChance;
    public float Rage;
    public float MaxRage;
    public float Concentration;
    public float MaxConcentration;
    public bool IsInCombat;
    public int CombatTargetEntityId;
    public float LastAttackTime;
}
```

**Problems:**
- Hardcoded stat fields (not extensible)
- No modifier system (can't apply buffs/equipment bonuses)
- Mixes stats with resources (Rage, Concentration)
- Mixes stats with combat state (IsInCombat, CombatTargetEntityId)

---

## Architecture

### New Component Structure

**1. MythHunter Stat Component**
```csharp
// Assets/_MythHunter/Code/Components/Combat/MythHunterStatsComponent.cs
using RPGFramework.Stats;

public struct MythHunterStatsComponent : IComponent
{
    public StatContainer<CombatStatType, float> Stats;

    public static MythHunterStatsComponent Create()
    {
        var component = new MythHunterStatsComponent();
        component.Stats = new StatContainer<CombatStatType, float>();

        // Initialize default stats
        component.Stats.SetBaseStat(CombatStatType.AttackPower, 10f);
        component.Stats.SetBaseStat(CombatStatType.Defense, 5f);
        component.Stats.SetBaseStat(CombatStatType.AttackSpeed, 1.0f);
        component.Stats.SetBaseStat(CombatStatType.CriticalChance, 0.05f);
        component.Stats.SetBaseStat(CombatStatType.CriticalMultiplier, 1.5f);
        component.Stats.SetBaseStat(CombatStatType.Accuracy, 0.95f);
        component.Stats.SetBaseStat(CombatStatType.DodgeChance, 0.05f);
        component.Stats.SetBaseStat(CombatStatType.BlockChance, 0.1f);
        component.Stats.SetBaseStat(CombatStatType.CounterAttackChance, 0.05f);

        return component;
    }
}
```

**2. Combat Stat Type Enum**
```csharp
// Assets/_MythHunter/Code/GameLogic/Combat/CombatStatType.cs
namespace MythHunter.GameLogic.Combat
{
    public enum CombatStatType
    {
        AttackPower,
        Defense,
        AttackSpeed,
        CriticalChance,
        CriticalMultiplier,
        Accuracy,
        DodgeChance,
        BlockChance,
        CounterAttackChance
    }
}
```

**3. MythHunter Resource Component**
```csharp
// Assets/_MythHunter/Code/Components/Combat/MythHunterResourceComponent.cs
using RPGFramework.Combat;

public struct MythHunterResourceComponent : IComponent
{
    public ResourceComponent<MythHunterResourceType> Resources;

    public static MythHunterResourceComponent Create()
    {
        var component = new MythHunterResourceComponent();
        component.Resources = new ResourceComponent<MythHunterResourceType>();

        // Initialize resources
        component.Resources.SetResource(MythHunterResourceType.Health, 100f, 0f, 100f);
        component.Resources.SetResource(MythHunterResourceType.Mana, 50f, 0f, 50f);
        component.Resources.SetResource(MythHunterResourceType.Stamina, 100f, 0f, 100f);
        component.Resources.SetResource(MythHunterResourceType.Rage, 0f, 0f, 100f);
        component.Resources.SetResource(MythHunterResourceType.Concentration, 0f, 0f, 100f);

        return component;
    }
}
```

**4. MythHunter Resource Type Enum**
```csharp
// Assets/_MythHunter/Code/GameLogic/Combat/MythHunterResourceType.cs
namespace MythHunter.GameLogic.Combat
{
    public enum MythHunterResourceType
    {
        Health,
        Mana,
        Stamina,
        Rage,
        Concentration
    }
}
```

**5. Combat State Component (separate concerns)**
```csharp
// Assets/_MythHunter/Code/Components/Combat/CombatStateComponent.cs
public struct CombatStateComponent : IComponent
{
    public bool IsInCombat;
    public int CombatTargetEntityId;
    public float LastAttackTime;

    public static CombatStateComponent Create()
    {
        return new CombatStateComponent
        {
            IsInCombat = false,
            CombatTargetEntityId = -1,
            LastAttackTime = 0f
        };
    }
}
```

---

## Adapters

### 1. MythHunter Damage Adapter

```csharp
// Assets/_MythHunter/Code/GameLogic/Combat/Adapters/MythHunterDamageAdapter.cs
using RPGFramework.Combat;

namespace MythHunter.GameLogic.Combat.Adapters
{
    /// <summary>
    /// Adapter for calculating damage using Framework DamageCalculator
    /// </summary>
    public class MythHunterDamageAdapter
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IEntityManager _entityManager;

        public MythHunterDamageAdapter(IDamageCalculator damageCalculator, IEntityManager entityManager)
        {
            _damageCalculator = damageCalculator;
            _entityManager = entityManager;
        }

        /// <summary>
        /// Calculate and apply damage from attacker to target
        /// </summary>
        public DamageResult ApplyDamage(int attackerEntityId, int targetEntityId, DamageType damageType)
        {
            // Get attacker stats
            if (!_entityManager.TryGetComponent<MythHunterStatsComponent>(attackerEntityId, out var attackerStats))
                return default;

            // Get target stats and resources
            if (!_entityManager.TryGetComponent<MythHunterStatsComponent>(targetEntityId, out var targetStats))
                return default;

            if (!_entityManager.TryGetComponent<MythHunterResourceComponent>(targetEntityId, out var targetResources))
                return default;

            // Extract combat stats
            float baseDamage = attackerStats.Stats.GetFinalStat(CombatStatType.AttackPower);
            float critChance = attackerStats.Stats.GetFinalStat(CombatStatType.CriticalChance);
            float critMultiplier = attackerStats.Stats.GetFinalStat(CombatStatType.CriticalMultiplier);
            float defense = targetStats.Stats.GetFinalStat(CombatStatType.Defense);

            // Calculate damage using Framework
            DamageResult result = _damageCalculator.CalculateDamage(
                baseDamage,
                defense,
                damageType,
                1.0f, // damage multiplier
                0f,   // armor penetration
                critChance,
                critMultiplier
            );

            // Apply damage to target health
            float currentHealth = targetResources.Resources.GetCurrent(MythHunterResourceType.Health);
            float newHealth = currentHealth - result.FinalDamage;
            targetResources.Resources.SetCurrent(MythHunterResourceType.Health, newHealth);

            // Update component
            _entityManager.SetComponent(targetEntityId, targetResources);

            return result;
        }
    }
}
```

---

## Implementation Steps

### Step 1: Create Enums (Day 1, 30 min)

**Files to create:**
1. `Assets/_MythHunter/Code/GameLogic/Combat/CombatStatType.cs`
2. `Assets/_MythHunter/Code/GameLogic/Combat/MythHunterResourceType.cs`

**Create directory if needed:**
```bash
mkdir -p "Assets/_MythHunter/Code/GameLogic/Combat"
```

---

### Step 2: Create New Components (Day 1, 1 hour)

**Files to create:**
1. `Assets/_MythHunter/Code/Components/Combat/MythHunterStatsComponent.cs`
2. `Assets/_MythHunter/Code/Components/Combat/MythHunterResourceComponent.cs`
3. `Assets/_MythHunter/Code/Components/Combat/CombatStateComponent.cs`

**Add assembly references:**
- `RPGFramework.Stats.Runtime`
- `RPGFramework.Combat.Runtime`

---

### Step 3: Create Adapters (Day 1-2, 2 hours)

**Files to create:**
1. `Assets/_MythHunter/Code/GameLogic/Combat/Adapters/MythHunterDamageAdapter.cs`
2. `Assets/_MythHunter/Code/GameLogic/Combat/Adapters/MythHunterStatsAdapter.cs` (if needed for migrations)

**Create directory:**
```bash
mkdir -p "Assets/_MythHunter/Code/GameLogic/Combat/Adapters"
```

---

### Step 4: Update CombatSystem (Day 2-3, 3-4 hours)

**File to modify:**
`Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs`

**Changes:**
1. Inject `MythHunterDamageAdapter`
2. Replace direct stat access with `MythHunterStatsComponent.Stats.GetFinalStat(...)`
3. Replace health access with `MythHunterResourceComponent.Resources.GetCurrent(...)`
4. Update damage calculations to use `ApplyDamage(...)`

**Example before:**
```csharp
var stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
float damage = stats.AttackPower - targetStats.Defense;
targetStats.Health -= damage;
```

**Example after:**
```csharp
DamageResult result = _damageAdapter.ApplyDamage(
    attackerEntityId,
    targetEntityId,
    DamageType.Physical
);
_logger.LogInfo($"Dealt {result.FinalDamage} damage (critical: {result.IsCritical})");
```

---

### Step 5: Migration Script (Day 3, 2 hours)

**File to create:**
`Assets/_MythHunter/Code/GameLogic/Combat/Migration/CombatComponentMigration.cs`

**Purpose:**
- Convert existing entities with `CombatStatsComponent` to new components
- Run once on game start or via editor tool

**Pseudocode:**
```csharp
public class CombatComponentMigration
{
    public void MigrateAllEntities(IEntityManager entityManager)
    {
        var entities = entityManager.GetEntitiesWithComponent<CombatStatsComponent>();

        foreach (var entityId in entities)
        {
            var oldStats = entityManager.GetComponent<CombatStatsComponent>(entityId);

            // Create new stats component
            var newStats = MythHunterStatsComponent.Create();
            newStats.Stats.SetBaseStat(CombatStatType.AttackPower, oldStats.AttackPower);
            newStats.Stats.SetBaseStat(CombatStatType.Defense, oldStats.Defense);
            // ... copy all stats

            // Create new resources component
            var newResources = MythHunterResourceComponent.Create();
            newResources.Resources.SetCurrent(MythHunterResourceType.Rage, oldStats.Rage);
            newResources.Resources.SetCurrent(MythHunterResourceType.Concentration, oldStats.Concentration);

            // Create combat state component
            var combatState = CombatStateComponent.Create();
            combatState.IsInCombat = oldStats.IsInCombat;
            combatState.CombatTargetEntityId = oldStats.CombatTargetEntityId;
            combatState.LastAttackTime = oldStats.LastAttackTime;

            // Add new components
            entityManager.AddComponent(entityId, newStats);
            entityManager.AddComponent(entityId, newResources);
            entityManager.AddComponent(entityId, combatState);

            // Remove old component
            entityManager.RemoveComponent<CombatStatsComponent>(entityId);
        }
    }
}
```

---

### Step 6: Testing (Day 4, 2-3 hours)

**Unit Tests:**
```csharp
// Assets/_MythHunter/Tests/Combat/MythHunterDamageAdapterTests.cs
[Test]
public void ApplyDamage_PhysicalDamage_ReducesHealth()
{
    // Arrange
    var attacker = CreateTestEntity(attackPower: 20f);
    var target = CreateTestEntity(defense: 5f, health: 100f);

    // Act
    var result = _damageAdapter.ApplyDamage(attacker, target, DamageType.Physical);

    // Assert
    Assert.That(result.FinalDamage, Is.GreaterThan(0));
    var targetResources = _entityManager.GetComponent<MythHunterResourceComponent>(target);
    Assert.That(targetResources.Resources.GetCurrent(MythHunterResourceType.Health),
                Is.LessThan(100f));
}

[Test]
public void ApplyDamage_CriticalHit_DealsBonusDamage()
{
    // Arrange
    var attacker = CreateTestEntity(attackPower: 20f, critChance: 1.0f, critMultiplier: 2.0f);
    var target = CreateTestEntity(defense: 0f, health: 100f);

    // Act
    var result = _damageAdapter.ApplyDamage(attacker, target, DamageType.Physical);

    // Assert
    Assert.That(result.IsCritical, Is.True);
    Assert.That(result.FinalDamage, Is.GreaterThan(20f)); // More than base damage
}
```

**Integration Tests:**
- Create test scene with 2 entities (attacker, target)
- Execute attack
- Verify health reduction
- Verify damage events fired

---

### Step 7: Cleanup (Day 5, 1 hour)

**Remove old code:**
1. Delete `Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs`
2. Update all references to use new components
3. Clean up unused imports

**Documentation:**
1. Update `COMBAT_SYSTEM.md` with new architecture
2. Add migration guide for developers
3. Update API documentation

---

## Success Criteria

**Code:**
- ✅ `CombatStatsComponent` deleted
- ✅ `MythHunterStatsComponent` + `MythHunterResourceComponent` + `CombatStateComponent` in use
- ✅ `MythHunterDamageAdapter` handles all damage calculations
- ✅ `CombatSystem` refactored to use new components
- ✅ Zero compilation errors/warnings

**Testing:**
- ✅ 5+ unit tests for `MythHunterDamageAdapter`
- ✅ Integration test for full combat flow
- ✅ Manual test in game scene (attack, damage, death)

**Quality:**
- ✅ Zero duplicate stat functionality
- ✅ Clean separation: Stats / Resources / CombatState
- ✅ Damage formula matches Framework (verified with tests)

---

## Files Summary

**New Files (13 files):**
1. `GameLogic/Combat/CombatStatType.cs` (enum)
2. `GameLogic/Combat/MythHunterResourceType.cs` (enum)
3. `Components/Combat/MythHunterStatsComponent.cs`
4. `Components/Combat/MythHunterResourceComponent.cs`
5. `Components/Combat/CombatStateComponent.cs`
6. `GameLogic/Combat/Adapters/MythHunterDamageAdapter.cs`
7. `GameLogic/Combat/Migration/CombatComponentMigration.cs`
8. `Tests/Combat/MythHunterDamageAdapterTests.cs`
9. `.meta` files (6 files)

**Modified Files:**
1. `Systems/Gameplay/Combat/CombatSystem.cs`
2. `MythHunter.GameLogic.asmdef` (add Framework references)

**Deleted Files:**
1. `Components/Combat/CombatStatsComponent.cs`
2. `Components/Combat/CombatStatsComponent.cs.meta`

---

## Next Phase Preview

**Phase 2: Advanced Combat Integration**
- Status Effects (buffs/debuffs)
- Death/Life State (death detection, respawn)
- Abilities (replace `CombatAbilityComponent`)

---

**Ready to implement Phase 1!** 🚀
