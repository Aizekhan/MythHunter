# EPIC 12: Framework Integration — Audit

**Date:** 2026-10-16
**Status:** Planning
**Objective:** Integrate all extracted Framework modules (EPIC 10 + EPIC 11) with MythHunter game layer

---

## Context

### What We Have Extracted (EPIC 10 + EPIC 11)

**10 Framework Runtime Assemblies:**

**EPIC 10 — Combat Foundation:**
1. `RPGFramework.Combat.Runtime` — Resources (HP/MP/Stamina), Damage, Status Effects, Death/Life
2. `RPGFramework.Stats.Runtime` — Stat System with Modifiers

**EPIC 11 — Gameplay Modules:**
3. `RPGFramework.Inventory.Runtime` — Containers, Equipment Slots, Stacking
4. `RPGFramework.Items.Runtime` — Items, Equipment, Consumables, Registry
5. `RPGFramework.Loot.Runtime` — Loot Tables, Drop Generation
6. `RPGFramework.Abilities.Runtime` — Abilities, Cooldowns, Resource Costs
7. `RPGFramework.Progression.Runtime` — XP, Leveling, Stat Growth, Unlocks

**Infrastructure:**
8. `RPGFramework.ECS.Runtime` — Entity Component System
9. `RPGFramework.Pooling.Runtime` — Object Pooling
10. `RPGFramework.Serialization.Runtime` — Serialization Foundation

### Current MythHunter State

**Existing Systems:**
- `CombatSystem` — Old combat logic
- `CombatStatsComponent` — Duplicates Framework Stats
- `CombatAbilityComponent` — Duplicates Framework Abilities
- `PhaseSystem` — Turn-based gameplay (Rune, Planning, Active, Freeze)
- `EntitySpawnSystem` — Entity creation
- `SimpleLobbyAI` — Basic AI for lobby

**Existing Adapters:**
- `ObjectPoolAdapter` — Only adapter currently exists (for EPIC 09 Pooling)

**Problem:**
- MythHunter does NOT use any of the 10 Framework modules
- Old components duplicate Framework functionality
- No integration = Framework is not validated in real game

---

## Integration Opportunities

### 1. Combat Stats Integration

**Current State:**
```csharp
// MythHunter OLD
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
    public float Rage;
    public float MaxRage;
    public float Concentration;
    // ... etc
}
```

**Framework Equivalent:**
```csharp
// RPGFramework.Stats.Runtime
StatContainer<CombatStatType, float>
- Supports any stat type via enum
- Modifier system (Additive, Multiplicative, Override)
- Performance optimization with dirty flags
- 18 tests passing
```

**Integration Plan:**
- Create `CombatStatType` enum in MythHunter
- Replace `CombatStatsComponent` with `StatContainer<CombatStatType, float>`
- Create adapter to migrate existing data
- Update `CombatSystem` to use new stat system

---

### 2. Combat Abilities Integration

**Current State:**
```csharp
// MythHunter OLD
public struct CombatAbilityComponent
{
    public string ActiveAbilityId;
    public bool IsActiveAbilityUsed;
    public float LastActiveUseTime;
    public string[] PassiveAbilityIds;
}
```

**Framework Equivalent:**
```csharp
// RPGFramework.Abilities.Runtime
IAbility, ICooldownManager, IAbilityExecutor
- Resource cost integration (HP, MP, Stamina)
- Cooldown tracking
- Ability events (Executed, Canceled, CooldownStarted, CooldownExpired)
```

**Integration Plan:**
- Migrate ability definitions to Framework `IAbility`
- Replace cooldown tracking with `ICooldownManager`
- Create MythHunter adapter for ability execution
- Hook ability resource costs to Resource System

---

### 3. Resource System Integration (HP/MP/Stamina)

**Current State:**
- Likely embedded in `CombatStatsComponent` or character components
- No explicit resource management

**Framework Equivalent:**
```csharp
// RPGFramework.Combat.Runtime (Phase 1)
ResourceComponent<TResourceType>
- Generic resource pool (HP, MP, Stamina, Rage, Energy, Focus)
- Min/Max/Current tracking
- Regeneration support
- Events: ResourceChanged, ResourceDepleted, ResourceRestored
```

**Integration Plan:**
- Define `MythHunterResourceType` enum (HP, MP, Stamina, Rage, Concentration)
- Add `ResourceComponent<MythHunterResourceType>` to combat entities
- Integrate with damage system
- Connect to death/life state system

---

### 4. Damage Calculation Integration

**Current State:**
- Damage likely calculated directly in `CombatSystem`
- No explicit damage calculator

**Framework Equivalent:**
```csharp
// RPGFramework.Combat.Runtime (Phase 3)
IDamageCalculator, DamageCalculator
- Damage types (Physical, Magical, Pure)
- Critical hits, armor penetration
- Formula: (baseDamage * multiplier) * (100 / (100 + defense))
- DamageResult with detailed breakdown
```

**Integration Plan:**
- Extract damage calculation to Framework `DamageCalculator`
- Use `DamageType` enum (Physical, Magical, Pure)
- Connect to stat modifiers from Equipment
- Return detailed `DamageResult` for UI/logging

---

### 5. Status Effects Integration

**Current State:**
- May exist in combat system (buffs/debuffs)
- No explicit status effect manager

**Framework Equivalent:**
```csharp
// RPGFramework.Combat.Runtime (Phase 4)
IStatusEffect, IStatusEffectManager
- 20+ status effect types (Buff, Debuff, DOT, HOT, Stun, Slow, etc.)
- Stack behaviors (Stack, Replace, Refresh, Ignore, StackCount)
- Duration tracking with auto-expiration
- Recurring effects (tick-based DOT/HOT)
```

**Integration Plan:**
- Create status effect definitions in MythHunter
- Use Framework `StatusEffectManager` for tracking
- Connect DOT/HOT to damage calculator
- Apply stat modifiers from buffs/debuffs

---

### 6. Death/Life State Integration

**Current State:**
- Death likely handled in combat system
- No explicit life state management

**Framework Equivalent:**
```csharp
// RPGFramework.Combat.Runtime (Phase 5)
ILifeStateHandler, ILifeStateManager
- Life states (Alive, Dying, Dead, Resurrecting, Invulnerable)
- Death conditions (ResourceDepletion, Instant)
- Respawn handlers (Checkpoint, PlayerRevive, Auto)
- Events: DeathEvent, ResurrectionEvent
```

**Integration Plan:**
- Add `LifeStateComponent` to combat entities
- Connect death detection to HP depletion
- Implement MythHunter-specific respawn logic
- Trigger loot drops on death

---

### 7. Inventory & Equipment Integration

**Current State:**
- Unknown if inventory system exists in MythHunter
- Equipment likely hardcoded or missing

**Framework Equivalent:**
```csharp
// RPGFramework.Inventory.Runtime (Phase 1)
IInventoryContainer, IEquipmentSlot, IItemStack
- Generic containers with capacity
- Equipment slots with type safety
- Item stacking with split/merge
- 24 tests passing
```

**Integration Plan:**
- Add inventory to hero entities
- Create equipment slot types (Weapon, Armor, Helmet, etc.)
- Implement equipment UI
- Connect equipment stat bonuses to stat system

---

### 8. Item System Integration

**Current State:**
- Unknown if items exist
- May be placeholder or hardcoded

**Framework Equivalent:**
```csharp
// RPGFramework.Items.Runtime (Phase 2)
IItem, IEquipment, IConsumable, IItemRegistry
- Item definitions and instances
- Rarity and quality tiers
- Equipment stat modifiers (integrates with Stats)
- Consumables with usage logic
```

**Integration Plan:**
- Create MythHunter item definitions
- Build item registry
- Implement consumable effects (potions, buffs)
- Design equipment with stat modifiers

---

### 9. Loot System Integration

**Current State:**
- Unknown if loot exists
- Likely needs implementation

**Framework Equivalent:**
```csharp
// RPGFramework.Loot.Runtime (Phase 3)
ILootTable, ILootGenerator
- Weighted probability drops
- Rarity-based chances
- Loot table registry
```

**Integration Plan:**
- Create loot tables for enemies
- Connect to death system (drop on kill)
- Implement loot UI
- Balance drop rates

---

### 10. Progression System Integration

**Current State:**
- Unknown if XP/leveling exists
- May be planned feature

**Framework Equivalent:**
```csharp
// RPGFramework.Progression.Runtime (Phase 5)
IProgressionTracker, ILevelTable, IStatGrowthFormula, IUnlockManager
- XP tracking and leveling
- Multiple curve types (Linear, Exponential, Polynomial)
- Stat growth on level-up
- Unlock system for abilities/items
- 27 tests passing
```

**Integration Plan:**
- Add XP tracking to heroes
- Define level curve (1-100?)
- Configure stat growth per level
- Unlock abilities at specific levels

---

## Proposed Integration Phases

### Phase 1: Core Combat Integration (Week 1)
**Scope:**
- Integrate Stats System (`StatContainer` replaces `CombatStatsComponent`)
- Integrate Resource System (HP, MP, Stamina, Rage, Concentration)
- Integrate Damage Calculator
- Update `CombatSystem` to use Framework modules

**Deliverables:**
- `MythHunterStatsAdapter`
- `MythHunterResourceAdapter`
- `MythHunterDamageAdapter`
- Refactored `CombatSystem`
- Migration script for existing data
- Integration tests

---

### Phase 2: Advanced Combat Integration (Week 1-2)
**Scope:**
- Integrate Status Effects
- Integrate Death/Life State
- Integrate Abilities
- Remove old `CombatAbilityComponent`

**Deliverables:**
- `MythHunterStatusEffectAdapter`
- `MythHunterLifeStateAdapter`
- `MythHunterAbilityAdapter`
- Updated combat flow with death/respawn
- Ability execution in combat

---

### Phase 3: Inventory & Equipment (Week 2)
**Scope:**
- Integrate Inventory System
- Integrate Item System
- Create equipment slot UI
- Connect equipment stat bonuses to combat

**Deliverables:**
- `MythHunterInventoryAdapter`
- `MythHunterItemAdapter`
- Inventory UI
- Equipment UI
- Item definitions

---

### Phase 4: Loot & Progression (Week 3)
**Scope:**
- Integrate Loot System
- Integrate Progression System
- Connect loot to death
- XP gain and level-up

**Deliverables:**
- `MythHunterLootAdapter`
- `MythHunterProgressionAdapter`
- Loot tables
- Level curve configuration
- Stat growth formulas

---

### Phase 5: Polish & Validation (Week 3)
**Scope:**
- End-to-end gameplay testing
- Performance profiling
- Bug fixes
- Documentation
- Tutorial/demo scene

**Deliverables:**
- Full gameplay loop (spawn → combat → loot → level up)
- Performance benchmarks
- Integration documentation
- Demo scene

---

## Success Criteria

**Technical:**
- ✅ All 10 Framework modules integrated
- ✅ Zero duplicate functionality between MythHunter and Framework
- ✅ All old components migrated to Framework equivalents
- ✅ Adapters follow consistent pattern
- ✅ Zero breaking changes to existing game data

**Functional:**
- ✅ Combat works with new stat/damage/resource systems
- ✅ Items and equipment provide stat bonuses
- ✅ Loot drops on enemy death
- ✅ XP and leveling functional
- ✅ Abilities execute with resource costs

**Quality:**
- ✅ Integration tests passing (20+ tests)
- ✅ Performance >= baseline (no regressions)
- ✅ Clean Unity console (no errors/warnings)
- ✅ Documentation updated

---

## Risks & Mitigations

**Risk 1: Data Migration**
- Existing save files may break
- **Mitigation:** Version migration scripts, backup save data

**Risk 2: Performance Regression**
- Framework indirection may slow combat
- **Mitigation:** Profile before/after, optimize hot paths

**Risk 3: Scope Creep**
- Integration reveals missing features
- **Mitigation:** Strict scope per phase, defer non-critical features

**Risk 4: Adapter Complexity**
- Too many adapters = hard to maintain
- **Mitigation:** Follow consistent adapter pattern, minimize adapter logic

---

## Recommendation

**Start with Phase 1: Core Combat Integration**

**Why:**
1. **High value:** Combat is core gameplay loop
2. **Proven modules:** EPIC 10 modules battle-tested (124 tests)
3. **Clear scope:** Well-defined integration points
4. **Low risk:** Can iterate incrementally

**Next Steps:**
1. Create `EPIC-12-Phase1-Implementation-Plan.md`
2. Define `MythHunterStatType` enum
3. Build `MythHunterStatsAdapter`
4. Create feature branch: `feature/epic12-phase1-combat-integration`
5. Implement + test + PR

---

**Ready to proceed with EPIC 12!** 🚀
