# EPIC 10: RPG Foundation Domain Modules — Source Audit

> **Status:** Active audit for EPIC 10 — RPG Foundation (Stats, Resources, Buffs, Damage)
> **Date:** 2026-10-13
> **Scope:** Audit existing MythHunter RPG domain systems to identify Framework extraction candidates
> **Dependencies:** EPIC 06 (Serialization), EPIC 07 (Modular Composition), EPIC 08 (Lifecycle), EPIC 09 Phase 1 (Pooling)

---

## Objective

Extract universal RPG domain modules from MythHunter:
- Stats and attributes
- Resources (HP, MP, Stamina)
- Statuses, buffs and debuffs
- Damage and resistances
- Death/life-state rules

**Principle:** Define interactions and test domain behavior without MythHunter-specific content.

---

## 1. Stats and Attributes

### Existing Implementation

**Assets/_MythHunter/Code/Components/Character/StatType.cs** (1-107)
- Enum with 90+ stat types:
  - **Universal:** Level, HP, HpRegen, Stamina, Damage, AttackSpeed, CritChance, ArmorResistance
  - **MythHunter-specific:** GoldPerTap, BagSlots, ManaCost, ItemChances
- StatCategory: Basic, Combat, Magic, Status, Economy, Inventory

**Assets/_MythHunter/Code/Components/Character/StatsComponent.cs** (1-84)
- Dictionary<StatType, float> Values
- Serializable component (ISerializableComponent)
- Convenience properties: Level, HP, Stamina, Damage, etc.

**Assets/_MythHunter/Code/Components/Character/StatRegistry.cs** (1-189)
- Static metadata registry
- StatDefinition: Name, Description, MinValue, MaxValue, Category, Color
- Ukrainian descriptions (game-specific)

**Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs** (1-88)
- Combat-specific: AttackPower, Defense, AttackSpeed, CriticalChance, Accuracy
- MythHunter mechanics: Rage, Concentration, IsInCombat

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| StatType enum | **Mixed** | Contains universal RPG stats + MythHunter-specific stats |
| StatsComponent storage | **Framework** | Dictionary<TStatType, float> pattern is universal |
| StatRegistry metadata | **Game** | Ukrainian descriptions, Color for UI |
| CombatStatsComponent | **Mixed** | Basic combat stats are universal; Rage/Concentration are game-specific |

### Dependencies
- MythHunter.Core.ECS.ISerializableComponent
- RPGFramework.ECS.IComponent
- UnityEngine.Color (StatRegistry only)

### Public API Surface
```csharp
// StatsComponent
public float GetStat(StatType type, float defaultValue = 0f);
public void SetStat(StatType type, float value);
public float Level { get; set; }
public float HP { get; set; }
public float Damage { get; set; }

// StatRegistry
public static StatDefinition GetDefinition(StatType type);
public static IEnumerable<StatDefinition> GetAllDefinitions();
public static IEnumerable<StatDefinition> GetDefinitionsByCategory(StatCategory category);
```

### Framework Extraction Candidates
1. **Generic Stat Storage:** `Dictionary<TStatType, TValue>` pattern → `RPGFramework.Stats.IStatContainer<TStatType, TValue>`
2. **Stat Definition Metadata:** Separate schema from presentation (Name, Min, Max) → Framework; UI (Color, Description) → Game
3. **Core RPG StatType subset:** HP, Stamina, Damage, ArmorResistance, CritChance → Framework enum
4. **MythHunter-specific stats:** GoldPerTap, BagSlots → Game layer extension

---

## 2. Resources (HP/MP/Stamina)

### Existing Implementation

**Assets/_MythHunter/Code/Components/Combat/HealthComponent.cs** (1-57)
```csharp
public struct HealthComponent : IComponent, ISerializableComponent
{
    public float CurrentHealth;
    public float MaxHealth;
    public bool IsInvulnerable;
    public bool IsDead;
    public float RegenRate;
    public float LastDamageTime;
    public float LastRegenTime;
}
```

**Stamina/Mana:**
- Tracked via StatType.Stamina, StatType.StaminaRegen in StatsComponent
- No dedicated ManaComponent or StaminaComponent

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| HealthComponent fields | **Framework** | Universal RPG resource pattern |
| IsInvulnerable flag | **Mixed** | Common mechanic but might be game-specific rule |
| IsDead flag | **Framework** | Universal life/death state |
| Regeneration tracking | **Framework** | Time-based resource recovery is universal |

### Dependencies
- MythHunter.Core.ECS.ISerializableComponent
- RPGFramework.ECS.IComponent
- **No game-specific dependencies** ✅

### Public API Surface
```csharp
public struct HealthComponent
{
    public float CurrentHealth;
    public float MaxHealth;
    public float RegenRate;
    public float LastRegenTime;
    public bool IsDead;
}
```

### Framework Extraction Candidates
1. **Generic ResourceComponent<TResourceType>** → Framework
   - CurrentValue, MaxValue, RegenRate, LastRegenTime
   - Typed for HP, MP, Stamina, Energy, etc.
2. **IResourceManager** → Framework lifecycle (regeneration, depletion)
3. **IsInvulnerable** → Consider as optional modifier/status rather than built-in field

---

## 3. Buffs/Debuffs/Status Effects

### Existing Implementation

**Assets/_MythHunter/Code/Components/Combat/CombatAbilityComponent.cs** (1-100)
- ActiveAbilityId, PassiveAbilityIds (string-based)
- AutoActivation thresholds
- No duration, stacking, or expiration tracking

**Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatAbilitySystem.cs** (1-469)
- Dictionary<string, Func<int, int, bool>> for active ability handlers
- Dictionary<string, Action<int>> for passive ability handlers
- Hardcoded abilities: shield_block, backstab, teleport, poison_attack, health_regen

**StatType enum status effects:**
- Poison, PoisonPower, Bleeding, BleedingPower, Plague, Stun

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| CombatAbilityComponent | **Game** | Hardcoded MythHunter ability IDs (string-based) |
| CombatAbilitySystem | **Game** | Concrete ability implementations (teleport, backstab) |
| Status in StatType | **Game** | Mixed into flat stat enum, no duration/stack tracking |
| **Missing:** StatusEffectComponent | **Framework** | Should exist for duration, stacking, expiration |

### Dependencies
- MythHunter.Core.ECS, MythHunter.Events
- UniTask for async operations
- Game-specific event bus

### Public API Surface
```csharp
// CombatAbilitySystem
public void RegisterActiveAbility(string abilityId, Func<int, int, bool> handler);
public void RegisterPassiveAbility(string abilityId, Action<int> handler);
public bool UseActiveAbility(int entityId, int targetEntityId = -1);
```

### Framework Extraction Candidates
1. **StatusEffectComponent** → Framework
   - EffectId, Duration, RemainingTime, StackCount, MaxStacks
   - Stat modifiers, periodic effects
2. **IStatusEffectRegistry** → Framework
   - Define effect schemas (duration, stacking rules, modifiers)
3. **StatusEffectSystem** → Framework
   - Apply, remove, tick, expire status effects
4. **MythHunter abilities** → Game layer implementations registered with Framework system

### Missing System
**No generic buff/debuff system exists.** Current implementation:
- Abilities are hardcoded string IDs
- No duration tracking
- No stack counting
- No expiration/tick system
- Status effects mixed into flat stat enum

**Recommendation:** Build new Framework status effect system from scratch.

---

## 4. Damage and Resistances

### Existing Implementation

**Assets/_MythHunter/Code/Events/Domain/Gameplay/CombatEvents.cs** (1-172)
```csharp
public struct DamageAppliedEvent : IEvent
{
    public int SourceEntityId;
    public int TargetEntityId;
    public float DamageAmount;
    public DamageType DamageType; // Physical, Magical, Pure
    public bool IsCritical;
    public bool IsBlocked;
    public bool IsDodged;
}

public struct HealingAppliedEvent : IEvent
{
    public int SourceEntityId;
    public int TargetEntityId;
    public float HealingAmount;
    public bool IsCritical;
}

public struct EntityDeathEvent : IEvent
{
    public int EntityId;
    public int KillerEntityId;
}
```

**Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs** (1-150+)
- Rage/Concentration mechanics (MythHunter-specific):
  - RAGE_GAIN_ON_HIT = 10f
  - CONCENTRATION_GAIN_ON_DAMAGE = 5f
- No visible damage formula in audit snippet

**CombatStatsComponent / StatType:**
- Defense, ArmorResistance
- CriticalChance, CriticalMultiplier
- BlockPenetrationChance, ArmorPenetrationChance

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| DamageAppliedEvent structure | **Framework** | Universal damage event schema |
| DamageType enum (Physical/Magical/Pure) | **Framework** | Common RPG damage types |
| HealingAppliedEvent structure | **Framework** | Universal healing event |
| EntityDeathEvent structure | **Framework** | Universal death event |
| Rage/Concentration mechanics | **Game** | MythHunter-specific combat resources |
| Damage calculation logic | **Game (missing)** | No exposed formula system |

### Dependencies
- MythHunter.Events.IEvent
- MythHunter.Core.ECS
- Game-specific event bus

### Public API Surface
```csharp
public enum DamageType { Physical, Magical, Pure }

public struct DamageAppliedEvent
{
    public int SourceEntityId;
    public int TargetEntityId;
    public float DamageAmount;
    public DamageType DamageType;
    public bool IsCritical;
    public bool IsBlocked;
    public bool IsDodged;
}
```

### Framework Extraction Candidates
1. **DamageAppliedEvent** → `RPGFramework.Combat.Events`
2. **DamageType enum** → `RPGFramework.Combat.DamageType`
3. **HealingAppliedEvent, EntityDeathEvent** → Framework events
4. **IDamageCalculator** → Framework interface for damage formulas
   - Input: base damage, attacker stats, defender stats, damage type
   - Output: final damage, IsCritical, IsBlocked, IsDodged
5. **MythHunter damage formula** → Game layer implementation of IDamageCalculator

### Missing System
**No exposed damage calculation engine.** Current system:
- Damage events exist (DamageAppliedEvent)
- No formula abstraction (armor reduction, resistances, penetration)
- No testable damage calculation pipeline
- CombatSystem handles damage but logic not visible in audit

**Recommendation:** Extract event structures now; design IDamageCalculator system as Phase 2.

---

## 5. Death/Life State

### Existing Implementation

**HealthComponent.IsDead field** (see Section 2)
```csharp
public bool IsDead;
```

**EntityDeathEvent** (see Section 4)
```csharp
public struct EntityDeathEvent : IEvent
{
    public int EntityId;
    public int KillerEntityId;
}
```

**Assets/_MythHunter/Code/Events/Domain/Gameplay/CombatEvents.cs** (51-57)
```csharp
public enum CombatEndReason { Death, PhaseEnded, Retreat, SystemForced }
```

**CombatSystem:**
- OnEntityDeath event handler
- EndAllActiveCombats method

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| IsDead flag | **Framework** | Universal life state |
| EntityDeathEvent | **Framework** | Universal death notification |
| CombatEndReason | **Game** | MythHunter combat phase logic |
| **Missing:** Resurrection/Respawn | **Framework?** | Not found in audit |

### Dependencies
- MythHunter.Events.IEvent
- HealthComponent

### Public API Surface
```csharp
// HealthComponent
public bool IsDead;

// Event
public struct EntityDeathEvent
{
    public int EntityId;
    public int KillerEntityId;
}
```

### Framework Extraction Candidates
1. **HealthComponent.IsDead** → Already Framework-ready
2. **EntityDeathEvent** → `RPGFramework.Combat.Events`
3. **ILifeStateManager** → Framework interface for death/resurrection
   - Kill(entityId, killerId)
   - Resurrect(entityId)
   - IsAlive(entityId)
4. **MythHunter death consequences** → Game layer (loot, experience, phase transitions)

### Missing System
**No resurrection or respawn system.** Current implementation:
- IsDead flag is set
- EntityDeathEvent is published
- No reverse operation (resurrection)
- No respawn logic
- No death consequences system

**Recommendation:** Extract IsDead and EntityDeathEvent now; design ILifeStateManager as optional Phase 3.

---

## 6. Stat Modification System

### Existing Implementation

**Assets/_MythHunter/Code/Systems/Heroes/RaceClassBonusSystem.cs** (1-100+)
- Applies race/class stat bonuses
- Dictionary<StatType, float> StatBonuses
- Hardcoded bonuses (Human, Dwarf, Elf, Warrior, Mage, etc.)

**Assets/_MythHunter/Code/Systems/Heroes/RacialBonuses.cs** (1-26)
```csharp
public class RacialBonuses
{
    public Dictionary<StatType, float> StatBonuses { get; set; }
    public string PassiveAbilityId { get; set; }
}
```

**Assets/_MythHunter/Code/Components/Combat/CombatStyleComponent.cs** (1-90)
- Aggressive/Defensive stance multipliers
- AggressiveAttackMod, DefensiveDefenseMod, etc.

### Framework vs Game Boundary

| Element | Classification | Reason |
|---------|---------------|--------|
| RaceClassBonusSystem | **Game** | Hardcoded MythHunter races/classes |
| RacialBonuses / ClassBonuses | **Game** | Specific race/class definitions |
| CombatStyleComponent multipliers | **Mixed** | Stance pattern is universal; values are game-specific |
| **Missing:** Generic modifier system | **Framework** | No add/remove/recalculate abstraction |

### Dependencies
- MythHunter.Components.Character.StatType
- MythHunter.Core.ECS
- Game-specific hero definitions

### Public API Surface
```csharp
public class RacialBonuses
{
    public Dictionary<StatType, float> StatBonuses { get; set; }
    public string PassiveAbilityId { get; set; }
}
```

### Framework Extraction Candidates
1. **IStatModifier** → Framework interface
   - ModifierId, TargetStat, ModifierType (Additive/Multiplicative), Value, Priority
2. **IStatModifierRegistry** → Framework
   - AddModifier(entityId, modifier)
   - RemoveModifier(entityId, modifierId)
   - RecalculateStat(entityId, statType)
3. **Modifier stacking rules** → Framework
   - Additive before multiplicative
   - Priority-based application order
4. **RaceClassBonusSystem** → Game layer implementation using Framework modifier API

### Missing System
**No generic modifier system exists.** Current implementation:
- Race/class bonuses applied imperatively
- No "add modifier", "remove modifier" API
- No stat recalculation on modifier change
- No dirty-flag tracking
- No additive/multiplicative stacking rules

**Recommendation:** Build new Framework modifier system from scratch; migrate RaceClassBonusSystem as Phase 2 consumer.

---

## Summary: Framework Extraction Roadmap

### Phase 1: Resource System (High Priority — Nearly Complete)
**Goal:** Extract generic resource pools (HP, MP, Stamina)

**Components:**
- `RPGFramework.Combat.ResourceComponent<TResourceType>` (from HealthComponent)
- `RPGFramework.Combat.IResourceManager` (lifecycle)
- `RPGFramework.Combat.Events.ResourceChangedEvent`

**Deliverables:**
- Framework assembly: `RPGFramework.Combat.Runtime`
- Tests: Resource creation, depletion, regeneration
- MythHunter adapter: HealthComponent → ResourceComponent<Health>

**Acceptance:** Unity EditMode tests pass; MythHunter game bootstrap smoke test clean.

---

### Phase 2: Stat System (Medium Priority — Requires Refactoring)
**Goal:** Extract core stat storage and modifier framework

**Components:**
- `RPGFramework.Stats.IStatContainer<TStatType, TValue>`
- `RPGFramework.Stats.IStatModifier` (additive/multiplicative)
- `RPGFramework.Stats.IStatModifierRegistry`
- `RPGFramework.Stats.Events.StatChangedEvent`

**Split StatType:**
- **Framework:** CoreStatType enum (HP, Stamina, Damage, ArmorResistance, CritChance)
- **Game:** MythHunterStatType enum extends CoreStatType (GoldPerTap, BagSlots)

**Deliverables:**
- Framework assembly: `RPGFramework.Stats.Runtime`
- Tests: Stat modification, stacking, recalculation
- MythHunter migration: StatsComponent uses IStatContainer; RaceClassBonusSystem uses IStatModifierRegistry

**Acceptance:** Unity EditMode tests pass; existing stat queries return correct values; game bootstrap clean.

---

### Phase 3: Damage Calculation System (Medium Priority — Design New)
**Goal:** Extract damage event structures and calculation framework

**Components:**
- `RPGFramework.Combat.DamageType` enum (Physical, Magical, Pure)
- `RPGFramework.Combat.Events.DamageAppliedEvent`
- `RPGFramework.Combat.Events.HealingAppliedEvent`
- `RPGFramework.Combat.Events.EntityDeathEvent`
- `RPGFramework.Combat.IDamageCalculator` interface
- `RPGFramework.Combat.DamageCalculationContext` struct

**Deliverables:**
- Framework assembly: `RPGFramework.Combat.Runtime` (extend Phase 1)
- Tests: Damage formula, critical hits, armor reduction, resistances
- MythHunter implementation: Concrete IDamageCalculator with Rage/Concentration

**Acceptance:** Unit tests for damage formulas pass; MythHunter combat smoke test clean (hit/crit/block/dodge).

---

### Phase 4: Status Effect System (High Priority — Design New)
**Goal:** Create generic buff/debuff system from scratch

**Components:**
- `RPGFramework.StatusEffects.StatusEffectComponent` (Duration, StackCount)
- `RPGFramework.StatusEffects.IStatusEffectDefinition` (schema)
- `RPGFramework.StatusEffects.IStatusEffectRegistry`
- `RPGFramework.StatusEffects.StatusEffectSystem` (apply/tick/expire)
- `RPGFramework.StatusEffects.Events.StatusEffectAppliedEvent`
- `RPGFramework.StatusEffects.Events.StatusEffectExpiredEvent`

**Deliverables:**
- Framework assembly: `RPGFramework.StatusEffects.Runtime`
- Tests: Duration, stacking, expiration, stat modifiers
- MythHunter migration: Poison, Bleeding → StatusEffectComponent instances

**Acceptance:** EditMode tests for duration/stacking pass; MythHunter poison effect works.

---

### Phase 5 (Deferred): Life State Management
**Goal:** Extract death/resurrection framework

**Components:**
- `RPGFramework.Combat.ILifeStateManager`
- `RPGFramework.Combat.Events.EntityDeathEvent` (from Phase 3)
- Resurrection/respawn contracts

**Why Deferred:**
- No resurrection system exists in MythHunter
- Current IsDead flag is sufficient
- EntityDeathEvent already extracted in Phase 3

**Reopen Condition:** MythHunter implements resurrection/respawn; multi-game need proven.

---

## Dependency Graph

```
Proposed Framework Modules:
  RPGFramework.Combat.Runtime
    ├─ ResourceComponent<TResourceType>
    ├─ DamageType enum
    ├─ Events: DamageAppliedEvent, HealingAppliedEvent, EntityDeathEvent
    ├─ IDamageCalculator interface
    └─ IResourceManager

  RPGFramework.Stats.Runtime
    ├─ IStatContainer<TStatType, TValue>
    ├─ IStatModifier (additive/multiplicative)
    ├─ IStatModifierRegistry
    └─ Events: StatChangedEvent

  RPGFramework.StatusEffects.Runtime
    ├─ StatusEffectComponent
    ├─ IStatusEffectDefinition
    ├─ IStatusEffectRegistry
    ├─ StatusEffectSystem
    └─ Events: StatusEffectAppliedEvent, StatusEffectExpiredEvent

Depends on:
  RPGFramework.ECS.Runtime (IComponent, IEntityManager)
  RPGFramework.Logging.Runtime (ILogger)
  RPGFramework.Serialization.Runtime (ISerializable)

MythHunter Game Layer (Adapters):
  HealthComponent → ResourceComponent<Health>
  StatsComponent → IStatContainer<MythHunterStatType, float>
  RaceClassBonusSystem → IStatModifierRegistry consumer
  CombatSystem → IDamageCalculator implementation
  Poison/Bleeding → StatusEffectComponent instances
```

---

## Reusability Assessment

### Framework-Ready (Extract As-Is)
| Component | Reason | Phase |
|-----------|--------|-------|
| HealthComponent | No game dependencies | 1 |
| DamageAppliedEvent | Universal event structure | 3 |
| HealingAppliedEvent | Universal event structure | 3 |
| EntityDeathEvent | Universal event structure | 3 |
| DamageType enum | Common RPG types | 3 |

### Requires Refactoring (Split Framework/Game)
| Component | Framework Part | Game Part | Phase |
|-----------|---------------|-----------|-------|
| StatType enum | CoreStatType (HP, Damage) | MythHunterStatType (GoldPerTap) | 2 |
| StatsComponent | IStatContainer storage | MythHunter convenience properties | 2 |
| CombatStatsComponent | Basic combat stats | Rage/Concentration | 2 |
| StatRegistry | Metadata schema | Ukrainian descriptions, Color | 2 |

### Design New System (Missing Implementation)
| System | Reason | Phase |
|--------|--------|-------|
| Status Effect System | No duration/stack tracking exists | 4 |
| Modifier System | No add/remove/recalculate API | 2 |
| Damage Calculator | No exposed formula engine | 3 |

---

## Next Immediate Actions

1. **Update STATUS.md and HANDOFF_NEXT_CHAT.md** in roadmap repository:
   - Mark EPIC 10 as ACTIVE
   - Link this audit document
   - Set Phase 1 (Resource System) as next immediate action

2. **Create EPIC 10 Phase 1 task:**
   - Extract ResourceComponent<TResourceType> from HealthComponent
   - Create RPGFramework.Combat.Runtime assembly
   - Add resource lifecycle tests
   - Define acceptance criteria

3. **Read EPIC 10 audit findings** before implementation:
   - HealthComponent is Framework-ready
   - No game-specific dependencies
   - Serialization foundation (EPIC 06) supports component serialization
   - Pooling (EPIC 09) available if resource instances need pooling

4. **Define rollback plan:**
   - Preserve HealthComponent in MythHunter until migration complete
   - Create ResourceComponent<Health> adapter
   - Test both paths before removing legacy HealthComponent

---

## Audit Completion Checklist

- [x] Map Stats and Attributes (StatType, StatsComponent, StatRegistry)
- [x] Map Resources (HealthComponent, regeneration)
- [x] Map Buffs/Debuffs (CombatAbilityComponent, missing StatusEffectComponent)
- [x] Map Damage and Resistances (DamageAppliedEvent, CombatSystem)
- [x] Map Death/Life State (IsDead, EntityDeathEvent)
- [x] Map Stat Modification (RaceClassBonusSystem, missing modifier framework)
- [x] Classify Framework vs Game for each component
- [x] Document dependencies
- [x] Assess reusability
- [x] Record public API surface
- [x] Create phased extraction roadmap
- [x] Identify missing systems
- [x] Define acceptance criteria per phase

**Audit complete.** Ready to proceed to EPIC 10 Phase 1: Resource System extraction.

---

**End of Audit**
