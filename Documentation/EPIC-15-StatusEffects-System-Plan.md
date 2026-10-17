# EPIC 15: Status Effects System (Greenfield)

**Status**: 🔄 Planning
**Start Date**: 2026-10-17
**Target**: Create universal Status Effects (Buffs/Debuffs) System for RPGFramework

---

## Mission

Build a **universal Status Effects System** from scratch for RPGFramework. Enables buffs, debuffs, DoT (Damage over Time), HoT (Heal over Time), stat modifications, and other temporary effects that are core to RPG combat and gameplay.

---

## Context

### What Exists Today

**In MythHunter:**
- ❌ No Status Effects system
- ✅ Stats System exists (can be modified by effects)
- ✅ Combat System exists (can apply effects)
- ✅ Abilities System exists (can trigger effects)

**In Framework:**
- ❌ No `RPGFramework.StatusEffects.Runtime` assembly
- ✅ Can integrate with `RPGFramework.Stats.Runtime` (stat modifications)
- ✅ Can integrate with `RPGFramework.Combat.Runtime` (damage/healing effects)
- ✅ Can integrate with `RPGFramework.Abilities.Runtime` (ability-triggered effects)

### Why Status Effects System?

**Universal RPG Feature:**
- **Buffs**: Temporary positive effects (Speed Boost, Damage Buff, Shield)
- **Debuffs**: Temporary negative effects (Poison, Slow, Weaken, Stun)
- **DoT/HoT**: Damage/Healing over Time
- **Stat Modifiers**: Temporary stat changes (+20% attack, -50% defense)
- **Status Conditions**: Stunned, Frozen, Silenced, etc.

**Examples from RPGs:**
- World of Warcraft: Buffs/debuffs with durations, stacking
- Diablo: Poison damage, frozen status, curse effects
- Final Fantasy: Status ailments (poison, blind, sleep, haste)
- Path of Exile: Auras, curses, ailments

**Integration Points:**
- **Stats System**: Effects modify stats temporarily
- **Combat System**: Effects deal damage/healing over time
- **Abilities System**: Abilities apply effects
- **Skills System**: Skills can grant effect resistance or potency

---

## Five Framework Filters

1. **Needed by ANY RPG?** ✅ YES - Status effects are universal across RPG genres
2. **Depends on game-specific logic?** ❌ NO - Effect mechanics are generic
3. **Reusable across RPG games?** ✅ YES - Any RPG can use buffs/debuffs
4. **Framework Core or Game Layer?**
   - **Framework**: Effect definitions, application, expiration, stacking
   - **Game Layer**: Specific effect IDs, visual representations, MythHunter metadata
5. **Proof of problem?** ✅ YES - Every RPG needs status effects (buffs, debuffs, DoT, etc.)

---

## Architecture Design

### Framework Module: `RPGFramework.StatusEffects.Runtime`

**Core Contracts:**
```csharp
public interface IStatusEffect
{
    string Id { get; }
    string Name { get; }
    EffectType Type { get; } // Buff, Debuff, Dot, Hot, StatModifier, etc.
    float Duration { get; } // -1 for permanent, 0 for instant, >0 for timed
    bool IsStackable { get; }
    int MaxStacks { get; }
    IReadOnlyList<IEffectModifier> Modifiers { get; }
}

public interface IEffectModifier
{
    ModifierType Type { get; } // Stat, Damage, Healing, etc.
    string TargetId { get; } // StatId, ResourceId, etc.
    float Value { get; }
    ModifierOperation Operation { get; } // Add, Multiply, Override
}

public interface IStatusEffectInstance
{
    string EffectId { get; }
    int EntityId { get; }
    float StartTime { get; }
    float RemainingDuration { get; }
    int CurrentStacks { get; }
    string SourceId { get; } // Who applied this effect
    bool IsExpired { get; }
}

public interface IStatusEffectManager
{
    bool ApplyEffect(int entityId, string effectId, string sourceId, float currentTime);
    bool RemoveEffect(int entityId, string effectId);
    void RemoveAllEffects(int entityId);
    void UpdateEffects(int entityId, float deltaTime);
    IReadOnlyList<IStatusEffectInstance> GetActiveEffects(int entityId);
    bool HasEffect(int entityId, string effectId);
}
```

**Data Structures:**
```csharp
public enum EffectType
{
    Buff,           // Positive temporary effect
    Debuff,         // Negative temporary effect
    DamageOverTime, // DoT (Poison, Bleed, Burn)
    HealOverTime,   // HoT (Regeneration, Healing Aura)
    StatModifier,   // Stat change (Speed, Damage, Defense)
    StatusCondition // Special conditions (Stun, Freeze, Silence)
}

public enum ModifierType
{
    Stat,      // Modifies a stat (e.g., "AttackPower")
    Damage,    // Deals damage per tick
    Healing,   // Heals per tick
    Resource,  // Modifies resource (e.g., Mana drain)
    Movement,  // Affects movement (e.g., Slow, Root)
    Control    // Control effects (Stun, Silence, etc.)
}

public enum ModifierOperation
{
    Add,       // Additive: value + modifier
    Multiply,  // Multiplicative: value * modifier
    Override   // Overrides value entirely
}

public struct StatusEffectData
{
    public string Id;
    public string Name;
    public string Description;
    public EffectType Type;
    public float Duration;
    public bool IsStackable;
    public int MaxStacks;
    public float TickInterval; // For DoT/HoT effects
    public List<EffectModifierData> Modifiers;

    public static StatusEffectData Create(string id, string name, EffectType type);
}

public struct EffectModifierData
{
    public ModifierType Type;
    public string TargetId;
    public float Value;
    public ModifierOperation Operation;

    public static EffectModifierData Create(ModifierType type, string targetId, float value);
}

public struct StatusEffectInstanceData
{
    public string EffectId;
    public int EntityId;
    public float StartTime;
    public float RemainingDuration;
    public int CurrentStacks;
    public string SourceId;
    public float LastTickTime; // For DoT/HoT
}

public struct ActiveEffectsData
{
    public List<StatusEffectInstanceData> ActiveInstances;

    public static ActiveEffectsData CreateEmpty();
    public void AddEffect(StatusEffectInstanceData instance);
    public void RemoveEffect(string effectId);
    public StatusEffectInstanceData? GetEffect(string effectId);
}
```

**Events:**
```csharp
public struct EffectAppliedEvent : IEvent
{
    public int EntityId;
    public string EffectId;
    public string SourceId;
    public float StartTime;
    public int Stacks;
}

public struct EffectExpiredEvent : IEvent
{
    public int EntityId;
    public string EffectId;
    public float ExpireTime;
}

public struct EffectTickEvent : IEvent
{
    public int EntityId;
    public string EffectId;
    public float Damage; // For DoT
    public float Healing; // For HoT
    public float TickTime;
}

public struct EffectRemovedEvent : IEvent
{
    public int EntityId;
    public string EffectId;
    public float RemovalTime;
    public bool WasExpired; // true if expired naturally, false if removed manually
}
```

**Implementations:**
```csharp
public class EffectModifier : IEffectModifier
{
    public ModifierType Type { get; }
    public string TargetId { get; }
    public float Value { get; }
    public ModifierOperation Operation { get; }

    public float Apply(float currentValue);
}

public class StatusEffect : IStatusEffect
{
    public string Id { get; }
    public string Name { get; }
    public EffectType Type { get; }
    public float Duration { get; }
    public bool IsStackable { get; }
    public int MaxStacks { get; }
    public float TickInterval { get; }
    public IReadOnlyList<IEffectModifier> Modifiers { get; }
}

public class StatusEffectInstance : IStatusEffectInstance
{
    public string EffectId { get; }
    public int EntityId { get; }
    public float StartTime { get; }
    public float RemainingDuration { get; private set; }
    public int CurrentStacks { get; private set; }
    public string SourceId { get; }
    public bool IsExpired => RemainingDuration <= 0f;

    public void Update(float deltaTime);
    public void AddStack();
    public void Refresh(float newDuration);
}

public class StatusEffectManager : IStatusEffectManager
{
    private readonly IEntityManager _entityManager;
    private readonly Dictionary<string, IStatusEffect> _effectRegistry;

    public bool ApplyEffect(int entityId, string effectId, string sourceId, float currentTime);
    public bool RemoveEffect(int entityId, string effectId);
    public void UpdateEffects(int entityId, float deltaTime);
    public IReadOnlyList<IStatusEffectInstance> GetActiveEffects(int entityId);
}
```

---

### Game Layer: MythHunter Integration

**Component:**
```csharp
public struct MythHunterStatusEffectsComponent : IComponent
{
    public ActiveEffectsData ActiveEffects;          // Framework data
    public List<string> ImmuneToEffectTypes;        // Game-specific: immunity list
    public float EffectResistance;                   // Game-specific: 0-1, chance to resist
    public Dictionary<string, int> EffectCounters;  // Game-specific: track how many times effect applied
    public float LastEffectAppliedTime;             // Game-specific: analytics

    public static MythHunterStatusEffectsComponent CreateEmpty();
    public static MythHunterStatusEffectsComponent CreateWithImmunity(List<string> immuneTypes);
}
```

**Adapter:**
```csharp
public class StatusEffectsSystemIntegration
{
    private readonly IEntityManager _entityManager;
    private readonly IStatusEffectManager _effectManager;
    private readonly IMythLogger _logger;

    public bool ApplyEffect(
        int entityId,
        string effectId,
        string sourceId,
        float currentTime,
        Action<int, string, int> onEffectApplied = null);

    public bool RemoveEffect(int entityId, string effectId);
    public void UpdateEffects(int entityId, float deltaTime);
    public List<IStatusEffectInstance> GetActiveEffects(int entityId);
    public bool HasEffect(int entityId, string effectId);

    // Stats Integration
    public Dictionary<string, float> CalculateStatModifiers(int entityId);

    // Combat Integration
    public void ApplyTickDamage(int entityId, float deltaTime);
    public void ApplyTickHealing(int entityId, float deltaTime);
}
```

---

## Phases

### Phase 1: Framework Status Effects Module (Greenfield)
**Deliverables:**
- `RPGFramework.StatusEffects.Runtime.asmdef`
- Core contracts: `IStatusEffect`, `IEffectModifier`, `IStatusEffectInstance`, `IStatusEffectManager`
- Data structures: `StatusEffectData`, `EffectModifierData`, `ActiveEffectsData`
- Events: `EffectAppliedEvent`, `EffectExpiredEvent`, `EffectTickEvent`
- Implementations: `StatusEffect`, `EffectModifier`, `StatusEffectInstance`, `StatusEffectManager`
- .NET 8.0 test harness (25-30 tests)

**Acceptance Criteria:**
- Zero Unity dependencies (`noEngineReferences: true`)
- Zero MythHunter dependencies
- 100% test pass rate
- Supports: instant effects, timed effects, DoT/HoT, stacking, stat modifiers

### Phase 2: MythHunter Integration (Dual-Component Pattern)
**Deliverables:**
- `MythHunterStatusEffectsComponent.cs` - Game-layer component
- `StatusEffectsSystemIntegration.cs` - Adapter bridging Framework ↔ MythHunter
- Sample effect definitions (Poison, Regeneration, Speed Buff, Armor Debuff)
- Integration with Stats System (stat modifiers)
- Integration with Combat System (DoT/HoT damage/healing)

**Acceptance Criteria:**
- Dual-component pattern (Framework data + MythHunter metadata)
- Zero modifications to existing Framework code
- MythHunter compilation clean

### Phase 3: Tests & Validation
**Deliverables:**
- `StatusEffectsSystemIntegrationTests.cs` (25-30 integration tests)
- Test categories:
  - Effect application and removal
  - Effect expiration and duration
  - Effect stacking and refreshing
  - DoT/HoT ticking
  - Stat modifier calculations
  - Effect immunity and resistance
- Full project compilation validation
- Test assembly: `MythHunter.StatusEffects.Tests.asmdef`

**Acceptance Criteria:**
- 100% integration test pass rate
- Full project compiles (exit code 0)
- Zero breaking changes to existing systems

### Phase 4: Documentation & Polish
**Deliverables:**
- `EPIC-15-PHASE1-COMPLETE.md` - Framework extraction summary
- `EPIC-15-PHASE2-COMPLETE.md` - Integration summary
- `EPIC-15-COMPLETE.md` - Overall completion summary
- Sample effect definitions (JSON/ScriptableObject examples)
- Integration examples (code snippets)

**Acceptance Criteria:**
- Comprehensive documentation
- Code examples for effect creation
- Clear integration patterns with Stats/Combat systems

---

## Integration with Existing Systems

### With Stats System (EPIC 10-14)
```csharp
// Status effects can modify stats temporarily
var statModifiers = _statusEffectsIntegration.CalculateStatModifiers(entityId);
// Returns: { "AttackPower": 1.5f (50% buff), "Defense": 0.7f (30% debuff) }

// Stats System applies these modifiers
finalAttack = baseAttack * statModifiers["AttackPower"];
```

### With Combat System (EPIC 10)
```csharp
// DoT effects deal damage per tick
_statusEffectsIntegration.ApplyTickDamage(entityId, deltaTime);
// Fires DamageEvent for each DoT tick

// HoT effects heal per tick
_statusEffectsIntegration.ApplyTickHealing(entityId, deltaTime);
// Fires HealEvent for each HoT tick
```

### With Abilities System (EPIC 13)
```csharp
// Abilities can apply status effects
_abilityIntegration.UseAbility(entityId, "ability_poison_strike", targetId,
    onAbilityUsed: (caster, target, abilityId) =>
    {
        // Apply poison effect to target
        _statusEffectsIntegration.ApplyEffect(target, "effect_poison", caster, currentTime);
    });
```

### With Skills System (EPIC 14)
```csharp
// Skills can grant effect resistance or potency
var poisonResistance = _skillIntegration.CalculateStatBonuses(entityId)["PoisonResistance"];
// Use resistance when applying poison effects
```

---

## Example Effects

### Poison (DoT Debuff)
```csharp
var poison = StatusEffectData.Create("effect_poison", "Poison", EffectType.DamageOverTime);
poison.Duration = 10f; // 10 seconds
poison.TickInterval = 1f; // Damage every 1 second
poison.IsStackable = true;
poison.MaxStacks = 5;
poison.Modifiers = new List<EffectModifierData>
{
    EffectModifierData.Create(ModifierType.Damage, "Health", 5f) // 5 damage per tick
};
```

### Speed Buff
```csharp
var speedBuff = StatusEffectData.Create("effect_haste", "Haste", EffectType.Buff);
speedBuff.Duration = 15f; // 15 seconds
speedBuff.IsStackable = false;
speedBuff.Modifiers = new List<EffectModifierData>
{
    EffectModifierData.Create(ModifierType.Stat, "MoveSpeed", 1.5f) // +50% speed
};
```

### Armor Debuff
```csharp
var armorDebuff = StatusEffectData.Create("effect_weaken", "Weaken", EffectType.Debuff);
armorDebuff.Duration = 8f; // 8 seconds
armorDebuff.Modifiers = new List<EffectModifierData>
{
    EffectModifierData.Create(ModifierType.Stat, "Defense", 0.7f) // -30% defense
};
```

### Regeneration (HoT Buff)
```csharp
var regen = StatusEffectData.Create("effect_regeneration", "Regeneration", EffectType.HealOverTime);
regen.Duration = 20f; // 20 seconds
regen.TickInterval = 2f; // Heal every 2 seconds
regen.Modifiers = new List<EffectModifierData>
{
    EffectModifierData.Create(ModifierType.Healing, "Health", 10f) // 10 HP per tick
};
```

---

## Success Criteria

1. ✅ **Universal**: Works for any RPG game
2. ✅ **Zero Unity Dependencies**: Pure .NET 8.0
3. ✅ **Tested**: 50+ tests (Framework + Integration)
4. ✅ **Integrated**: Works with Stats, Combat, Abilities, Skills
5. ✅ **Documented**: Comprehensive documentation
6. ✅ **Extensible**: Easy to add new effect types

---

## Risk Assessment

### Low Risk ✅
- Framework design follows proven EPIC 10-14 patterns
- Status effects are well-understood RPG mechanics
- No complex algorithms (just timers + modifiers)

### Medium Risk ⚠️
- Integration with multiple systems (Stats, Combat, Abilities)
- Timing/tick synchronization across systems
- Stacking logic can be complex

### Mitigation ✅
- Comprehensive tests for all edge cases
- Clear separation: Framework handles core logic, game layer handles integration
- Iterative development: Phase 1 (Framework) → Phase 2 (Integration) → Phase 3 (Tests)

---

**EPIC 15: Ready for Implementation** ✅
