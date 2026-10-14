# EPIC 11 Phase 4: Ability System — Implementation Plan

## Overview

Extract ability/skill system from MythHunter into universal RPG Framework module. Provides ability definitions, cooldown management, resource costs, casting mechanics, and target validation — all without Unity dependencies.

**Target:** `RPGFramework.Abilities.Runtime` assembly (.NET 8.0, zero dependencies)

## Existing MythHunter Code Analysis

### Current Implementation
- **CombatAbilityComponent:** Active/Passive/Reactive abilities, auto-activation
- **CombatAbilitySystem:** Handler-based execution, cooldown tracking
- **Concrete Abilities:** shield_block, backstab, rage_burst, teleport, poison_attack, etc.

### Dependencies to Remove
- Unity: `Time.time`, `Vector3`, `UnityEngine`
- Async: `UniTask` (replace with callbacks)
- ECS: Specific to MythHunter ECS implementation

### Core Concepts to Extract
1. Ability definitions and metadata
2. Cooldown tracking with time-based expiry
3. Resource cost validation (HP/MP/Stamina)
4. Casting time and channeling
5. Target validation and range checks
6. Ability types (Active, Passive, Triggered)
7. Ability registry for lookup

## Architecture

### Module Structure
```
Assets/_Framework/Abilities/
├── Runtime/
│   ├── Contracts/
│   │   ├── IAbility.cs
│   │   ├── IAbilityDefinition.cs
│   │   ├── IAbilityInstance.cs
│   │   ├── IAbilityExecutor.cs
│   │   ├── IAbilityRegistry.cs
│   │   ├── ICooldownTracker.cs
│   │   └── ITargetValidator.cs
│   ├── Data/
│   │   ├── AbilityType.cs
│   │   ├── TargetType.cs
│   │   ├── ResourceCost.cs
│   │   ├── CastInfo.cs
│   │   ├── CooldownState.cs
│   │   ├── AbilityExecutionResult.cs
│   │   └── AbilityMetadata.cs
│   ├── Implementations/
│   │   ├── Ability.cs
│   │   ├── AbilityDefinition.cs
│   │   ├── AbilityInstance.cs
│   │   ├── AbilityExecutor.cs
│   │   ├── AbilityRegistry.cs
│   │   ├── CooldownTracker.cs
│   │   └── TargetValidator.cs
│   ├── Events/
│   │   ├── AbilityExecutedEvent.cs
│   │   ├── AbilityCanceledEvent.cs
│   │   ├── CooldownStartedEvent.cs
│   │   └── CooldownExpiredEvent.cs
│   └── RPGFramework.Abilities.Runtime.asmdef
└── Tests/
    ├── AbilitySystemTests.cs
    └── RPGFramework.Abilities.Tests.asmdef
```

### Build Infrastructure
```
Build/
├── RPGFramework.Abilities.Runtime/
│   └── RPGFramework.Abilities.Runtime.csproj
├── RPGFramework.Abilities.Tests/
│   └── RPGFramework.Abilities.Tests.csproj
└── validate_framework_abilities_boundary.py
```

## Data Structures

### AbilityType Enumeration
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Types of abilities in the RPG system.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>Active ability requiring manual activation</summary>
        Active = 0,

        /// <summary>Passive ability always active</summary>
        Passive = 1,

        /// <summary>Triggered ability activated by conditions</summary>
        Triggered = 2,

        /// <summary>Toggle ability that can be turned on/off</summary>
        Toggle = 3,

        /// <summary>Channeled ability requiring continuous casting</summary>
        Channeled = 4
    }
}
```

### TargetType Enumeration
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Defines valid targets for abilities.
    /// </summary>
    public enum TargetType
    {
        /// <summary>No target required (self-cast)</summary>
        None = 0,

        /// <summary>Targets self only</summary>
        Self = 1,

        /// <summary>Targets single enemy</summary>
        Enemy = 2,

        /// <summary>Targets single ally</summary>
        Ally = 3,

        /// <summary>Targets area (AoE)</summary>
        Area = 4,

        /// <summary>Targets ground location</summary>
        Ground = 5,

        /// <summary>Targets any entity</summary>
        Any = 6
    }
}
```

### ResourceCost Structure
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Represents resource costs for ability execution.
    /// Supports HP, MP, Stamina, or custom resources.
    /// </summary>
    public struct ResourceCost
    {
        /// <summary>Resource type identifier (e.g., "HP", "MP", "Stamina")</summary>
        public string ResourceType;

        /// <summary>Amount of resource required</summary>
        public float Amount;

        /// <summary>Whether cost is percentage of max resource (true) or flat amount (false)</summary>
        public bool IsPercentage;

        public ResourceCost(string resourceType, float amount, bool isPercentage = false)
        {
            ResourceType = resourceType;
            Amount = amount;
            IsPercentage = isPercentage;
        }
    }
}
```

### CastInfo Structure
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Information about ability casting mechanics.
    /// </summary>
    public struct CastInfo
    {
        /// <summary>Time required to cast ability (in seconds)</summary>
        public float CastTime;

        /// <summary>Duration of channeling (in seconds, 0 for non-channeled)</summary>
        public float ChannelDuration;

        /// <summary>Whether casting can be interrupted</summary>
        public bool IsInterruptible;

        /// <summary>Whether caster can move while casting</summary>
        public bool CanMoveWhileCasting;

        public CastInfo(float castTime, float channelDuration = 0f, bool isInterruptible = true, bool canMoveWhileCasting = false)
        {
            CastTime = castTime;
            ChannelDuration = channelDuration;
            IsInterruptible = isInterruptible;
            CanMoveWhileCasting = canMoveWhileCasting;
        }

        /// <summary>Instant cast (no cast time)</summary>
        public static CastInfo Instant => new CastInfo(0f);
    }
}
```

### CooldownState Structure
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Tracks cooldown state for an ability.
    /// </summary>
    public struct CooldownState
    {
        /// <summary>Ability ID</summary>
        public string AbilityId;

        /// <summary>Total cooldown duration (in seconds)</summary>
        public float CooldownDuration;

        /// <summary>Timestamp when cooldown started (game time)</summary>
        public float StartTime;

        /// <summary>Whether ability is currently on cooldown</summary>
        public bool IsOnCooldown => RemainingTime > 0f;

        /// <summary>Time remaining on cooldown (requires current game time)</summary>
        public float GetRemainingTime(float currentTime) =>
            Math.Max(0f, (StartTime + CooldownDuration) - currentTime);

        public CooldownState(string abilityId, float cooldownDuration, float startTime)
        {
            AbilityId = abilityId;
            CooldownDuration = cooldownDuration;
            StartTime = startTime;
        }
    }
}
```

### AbilityMetadata Structure
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Metadata and configuration for an ability.
    /// </summary>
    public struct AbilityMetadata
    {
        /// <summary>Unique identifier</summary>
        public string AbilityId;

        /// <summary>Display name</summary>
        public string Name;

        /// <summary>Description text</summary>
        public string Description;

        /// <summary>Icon identifier (for UI)</summary>
        public string IconId;

        /// <summary>Ability type</summary>
        public AbilityType Type;

        /// <summary>Cooldown duration in seconds</summary>
        public float Cooldown;

        /// <summary>Maximum range (0 for melee/self)</summary>
        public float Range;

        /// <summary>Area of effect radius (0 for single target)</summary>
        public float AoeRadius;

        /// <summary>Minimum level required to use</summary>
        public int MinLevel;

        /// <summary>Maximum number of charges (0 for unlimited)</summary>
        public int MaxCharges;

        public AbilityMetadata(
            string abilityId,
            string name,
            AbilityType type = AbilityType.Active,
            float cooldown = 0f,
            float range = 0f,
            float aoeRadius = 0f,
            int minLevel = 0,
            int maxCharges = 0)
        {
            AbilityId = abilityId;
            Name = name;
            Description = string.Empty;
            IconId = string.Empty;
            Type = type;
            Cooldown = cooldown;
            Range = range;
            AoeRadius = aoeRadius;
            MinLevel = minLevel;
            MaxCharges = maxCharges;
        }
    }
}
```

### AbilityExecutionResult Structure
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Result of ability execution attempt.
    /// </summary>
    public struct AbilityExecutionResult
    {
        /// <summary>Whether execution succeeded</summary>
        public bool Success;

        /// <summary>Failure reason (if Success is false)</summary>
        public string FailureReason;

        /// <summary>Ability ID that was executed</summary>
        public string AbilityId;

        /// <summary>Caster entity ID</summary>
        public int CasterEntityId;

        /// <summary>Target entity ID (-1 for none)</summary>
        public int TargetEntityId;

        /// <summary>Timestamp of execution</summary>
        public float Timestamp;

        public static AbilityExecutionResult CreateSuccess(string abilityId, int casterEntityId, int targetEntityId, float timestamp)
        {
            return new AbilityExecutionResult
            {
                Success = true,
                FailureReason = string.Empty,
                AbilityId = abilityId,
                CasterEntityId = casterEntityId,
                TargetEntityId = targetEntityId,
                Timestamp = timestamp
            };
        }

        public static AbilityExecutionResult CreateFailure(string abilityId, int casterEntityId, string reason, float timestamp)
        {
            return new AbilityExecutionResult
            {
                Success = false,
                FailureReason = reason,
                AbilityId = abilityId,
                CasterEntityId = casterEntityId,
                TargetEntityId = -1,
                Timestamp = timestamp
            };
        }
    }
}
```

## Core Interfaces

### IAbility Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Base interface for all abilities.
    /// Abilities are reusable definitions that can be instantiated per entity.
    /// </summary>
    public interface IAbility
    {
        /// <summary>Unique ability identifier</summary>
        string AbilityId { get; }

        /// <summary>Display name</summary>
        string Name { get; }

        /// <summary>Ability metadata</summary>
        AbilityMetadata Metadata { get; }

        /// <summary>Ability type (Active, Passive, Triggered, etc.)</summary>
        AbilityType Type { get; }

        /// <summary>Cooldown duration in seconds</summary>
        float Cooldown { get; }

        /// <summary>Resource costs for execution</summary>
        IReadOnlyList<ResourceCost> ResourceCosts { get; }

        /// <summary>Casting information</summary>
        CastInfo CastInfo { get; }

        /// <summary>Target type requirement</summary>
        TargetType TargetType { get; }

        /// <summary>Maximum range (0 for melee/self)</summary>
        float Range { get; }
    }
}
```

### IAbilityInstance Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Instance of an ability for a specific entity.
    /// Tracks per-entity state (cooldowns, charges, etc.).
    /// </summary>
    public interface IAbilityInstance
    {
        /// <summary>Ability definition</summary>
        IAbility Ability { get; }

        /// <summary>Entity ID that owns this instance</summary>
        int OwnerEntityId { get; }

        /// <summary>Current cooldown state</summary>
        CooldownState CooldownState { get; }

        /// <summary>Remaining charges (for abilities with charges)</summary>
        int RemainingCharges { get; }

        /// <summary>Whether ability is currently being cast</summary>
        bool IsCasting { get; }

        /// <summary>Cast start time (for tracking cast progress)</summary>
        float CastStartTime { get; }

        /// <summary>Whether ability can be used right now</summary>
        bool CanUse(float currentTime);

        /// <summary>Start casting (for abilities with cast time)</summary>
        void StartCast(float currentTime);

        /// <summary>Cancel casting</summary>
        void CancelCast();

        /// <summary>Complete casting and start cooldown</summary>
        void CompleteCast(float currentTime);

        /// <summary>Use one charge (for charge-based abilities)</summary>
        void UseCharge();

        /// <summary>Restore one charge</summary>
        void RestoreCharge();
    }
}
```

### IAbilityRegistry Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Registry for ability definitions.
    /// Provides lookup and management of all abilities in the game.
    /// </summary>
    public interface IAbilityRegistry
    {
        /// <summary>Register new ability definition</summary>
        void RegisterAbility(IAbility ability);

        /// <summary>Unregister ability</summary>
        bool UnregisterAbility(string abilityId);

        /// <summary>Get ability by ID</summary>
        IAbility GetAbility(string abilityId);

        /// <summary>Check if ability exists</summary>
        bool HasAbility(string abilityId);

        /// <summary>Get all registered abilities</summary>
        IReadOnlyList<IAbility> GetAllAbilities();

        /// <summary>Get abilities by type</summary>
        IReadOnlyList<IAbility> GetAbilitiesByType(AbilityType type);

        /// <summary>Clear all registered abilities</summary>
        void Clear();
    }
}
```

### ICooldownTracker Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Tracks cooldowns for abilities across entities.
    /// </summary>
    public interface ICooldownTracker
    {
        /// <summary>Start cooldown for ability</summary>
        void StartCooldown(int entityId, string abilityId, float duration, float currentTime);

        /// <summary>Check if ability is on cooldown</summary>
        bool IsOnCooldown(int entityId, string abilityId, float currentTime);

        /// <summary>Get remaining cooldown time</summary>
        float GetRemainingCooldown(int entityId, string abilityId, float currentTime);

        /// <summary>Clear cooldown for specific ability</summary>
        void ClearCooldown(int entityId, string abilityId);

        /// <summary>Clear all cooldowns for entity</summary>
        void ClearAllCooldowns(int entityId);

        /// <summary>Get all active cooldowns for entity</summary>
        IReadOnlyList<CooldownState> GetActiveCooldowns(int entityId, float currentTime);
    }
}
```

### ITargetValidator Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Validates targets for ability execution.
    /// Uses callback pattern to avoid Unity/game-specific dependencies.
    /// </summary>
    public interface ITargetValidator
    {
        /// <summary>
        /// Validate target for ability.
        /// Returns true if target is valid, false otherwise.
        /// </summary>
        /// <param name="ability">Ability being used</param>
        /// <param name="casterEntityId">Entity using ability</param>
        /// <param name="targetEntityId">Target entity (-1 for no target)</param>
        /// <param name="distanceCheck">Callback to check distance between entities</param>
        /// <param name="relationshipCheck">Callback to check relationship (ally/enemy)</param>
        bool ValidateTarget(
            IAbility ability,
            int casterEntityId,
            int targetEntityId,
            Func<int, int, float> distanceCheck,
            Func<int, int, bool> relationshipCheck);
    }
}
```

### IAbilityExecutor Interface
```csharp
namespace RPGFramework.Abilities
{
    /// <summary>
    /// Executes abilities with validation and cooldown management.
    /// Uses callback pattern for resource checks and effects.
    /// </summary>
    public interface IAbilityExecutor
    {
        /// <summary>
        /// Execute ability for entity.
        /// </summary>
        /// <param name="abilityId">Ability to execute</param>
        /// <param name="casterEntityId">Entity using ability</param>
        /// <param name="targetEntityId">Target entity (-1 for none)</param>
        /// <param name="currentTime">Current game time</param>
        /// <param name="resourceCheck">Callback to check if entity has required resources</param>
        /// <param name="resourceConsume">Callback to consume resources</param>
        /// <param name="applyEffect">Callback to apply ability effects</param>
        /// <returns>Execution result</returns>
        AbilityExecutionResult ExecuteAbility(
            string abilityId,
            int casterEntityId,
            int targetEntityId,
            float currentTime,
            Func<int, string, float, bool> resourceCheck,
            Action<int, string, float> resourceConsume,
            Action<int, int, string> applyEffect);

        /// <summary>Check if ability can be executed</summary>
        bool CanExecuteAbility(string abilityId, int casterEntityId, float currentTime);

        /// <summary>Cancel active cast</summary>
        void CancelCast(int entityId);
    }
}
```

## Implementation Classes

### Ability Class
```csharp
namespace RPGFramework.Abilities
{
    public class Ability : IAbility
    {
        public string AbilityId { get; }
        public string Name { get; }
        public AbilityMetadata Metadata { get; }
        public AbilityType Type => Metadata.Type;
        public float Cooldown => Metadata.Cooldown;
        public IReadOnlyList<ResourceCost> ResourceCosts { get; }
        public CastInfo CastInfo { get; }
        public TargetType TargetType { get; }
        public float Range => Metadata.Range;

        public Ability(
            AbilityMetadata metadata,
            List<ResourceCost> resourceCosts = null,
            CastInfo? castInfo = null,
            TargetType targetType = TargetType.None)
        {
            AbilityId = metadata.AbilityId;
            Name = metadata.Name;
            Metadata = metadata;
            ResourceCosts = resourceCosts ?? new List<ResourceCost>();
            CastInfo = castInfo ?? CastInfo.Instant;
            TargetType = targetType;
        }
    }
}
```

### AbilityInstance Class
```csharp
namespace RPGFramework.Abilities
{
    public class AbilityInstance : IAbilityInstance
    {
        public IAbility Ability { get; }
        public int OwnerEntityId { get; }
        public CooldownState CooldownState { get; private set; }
        public int RemainingCharges { get; private set; }
        public bool IsCasting { get; private set; }
        public float CastStartTime { get; private set; }

        public AbilityInstance(IAbility ability, int ownerEntityId)
        {
            Ability = ability;
            OwnerEntityId = ownerEntityId;
            RemainingCharges = ability.Metadata.MaxCharges > 0 ? ability.Metadata.MaxCharges : -1;
            IsCasting = false;
            CastStartTime = 0f;
        }

        public bool CanUse(float currentTime)
        {
            // Check cooldown
            if (CooldownState.GetRemainingTime(currentTime) > 0f)
                return false;

            // Check charges
            if (RemainingCharges == 0)
                return false;

            // Check if already casting
            if (IsCasting)
                return false;

            return true;
        }

        public void StartCast(float currentTime)
        {
            IsCasting = true;
            CastStartTime = currentTime;
        }

        public void CancelCast()
        {
            IsCasting = false;
            CastStartTime = 0f;
        }

        public void CompleteCast(float currentTime)
        {
            IsCasting = false;
            CastStartTime = 0f;

            // Start cooldown
            CooldownState = new CooldownState(Ability.AbilityId, Ability.Cooldown, currentTime);
        }

        public void UseCharge()
        {
            if (RemainingCharges > 0)
                RemainingCharges--;
        }

        public void RestoreCharge()
        {
            if (Ability.Metadata.MaxCharges > 0 && RemainingCharges < Ability.Metadata.MaxCharges)
                RemainingCharges++;
        }
    }
}
```

## Events

### AbilityExecutedEvent
```csharp
namespace RPGFramework.Abilities
{
    public struct AbilityExecutedEvent
    {
        public string AbilityId;
        public int CasterEntityId;
        public int TargetEntityId;
        public float Timestamp;
        public bool Success;
    }
}
```

### AbilityCanceledEvent
```csharp
namespace RPGFramework.Abilities
{
    public struct AbilityCanceledEvent
    {
        public string AbilityId;
        public int CasterEntityId;
        public float Timestamp;
        public string Reason;
    }
}
```

### CooldownStartedEvent
```csharp
namespace RPGFramework.Abilities
{
    public struct CooldownStartedEvent
    {
        public string AbilityId;
        public int EntityId;
        public float Duration;
        public float Timestamp;
    }
}
```

### CooldownExpiredEvent
```csharp
namespace RPGFramework.Abilities
{
    public struct CooldownExpiredEvent
    {
        public string AbilityId;
        public int EntityId;
        public float Timestamp;
    }
}
```

## Test Plan

### Test Categories (25+ tests)

#### Ability Definition Tests (5 tests)
1. `CreateAbility_WithMetadata_StoresCorrectly`
2. `CreateAbility_WithResourceCosts_ValidatesCorrectly`
3. `CreateAbility_WithCastInfo_ReturnsExpectedValues`
4. `CreateAbility_WithTargetType_MatchesDefinition`
5. `AbilityMetadata_DefaultValues_AreCorrect`

#### Ability Instance Tests (6 tests)
1. `CreateAbilityInstance_WithAbility_Initializes`
2. `AbilityInstance_CanUse_ReturnsTrueWhenReady`
3. `AbilityInstance_CanUse_ReturnsFalseOnCooldown`
4. `AbilityInstance_StartCast_SetsIsCasting`
5. `AbilityInstance_CancelCast_ClearsState`
6. `AbilityInstance_CompleteCast_StartsCooldown`

#### Cooldown Tests (5 tests)
1. `CooldownTracker_StartCooldown_RecordsCorrectly`
2. `CooldownTracker_IsOnCooldown_ReturnsTrueDuringCooldown`
3. `CooldownTracker_GetRemainingCooldown_ReturnsCorrectTime`
4. `CooldownTracker_ClearCooldown_RemovesCooldown`
5. `CooldownTracker_MultipleAbilities_TracksSeparately`

#### Resource Cost Tests (4 tests)
1. `ResourceCost_FlatAmount_CreatesCorrectly`
2. `ResourceCost_Percentage_CalculatesCorrectly`
3. `MultipleResourceCosts_ValidateAll`
4. `InsufficientResources_PreventsExecution`

#### Registry Tests (4 tests)
1. `AbilityRegistry_RegisterAbility_AddsToRegistry`
2. `AbilityRegistry_GetAbility_ReturnsRegistered`
3. `AbilityRegistry_GetAbilitiesByType_FiltersCorrectly`
4. `AbilityRegistry_UnregisterAbility_Removes`

#### Executor Tests (6 tests)
1. `AbilityExecutor_ExecuteAbility_CallsCallbacks`
2. `AbilityExecutor_WithCastTime_DelaysExecution`
3. `AbilityExecutor_ResourceCheck_PreventsExecutionWhenInsufficient`
4. `AbilityExecutor_CooldownCheck_PreventsExecutionOnCooldown`
5. `AbilityExecutor_CancelCast_StopsCasting`
6. `AbilityExecutor_MultipleAbilities_TracksIndependently`

## Integration Points

### With EPIC 10 (Combat Foundation)
- **Resource Costs:** Uses `ResourceType` strings that match EPIC 10 Phase 1 resources (HP, MP, Stamina)
- **Game Layer:** MythHunter adapter will connect abilities to actual resource pools

### With Future Phases
- **Phase 5 (Progression):** Abilities can have level requirements
- **Phase 6 (Integration):** MythHunter adapters for ability effects

### Standalone Design
- **No Framework Dependencies:** First module after Loot to be fully standalone
- **Callback Pattern:** Uses Func/Action delegates instead of interfaces to avoid coupling
- **Generic Time:** Uses float timestamps instead of Unity Time.time

## Implementation Steps

### Step 1: Data Structures (Day 1)
- Create enumerations (AbilityType, TargetType)
- Implement ResourceCost, CastInfo, CooldownState
- Implement AbilityMetadata, AbilityExecutionResult

### Step 2: Interfaces (Day 1)
- Define all core interfaces
- Document callback signatures
- Create interface contracts

### Step 3: Core Implementations (Day 2)
- Implement Ability, AbilityInstance
- Implement AbilityRegistry
- Implement CooldownTracker

### Step 4: Executor & Validator (Day 2)
- Implement TargetValidator
- Implement AbilityExecutor with callbacks
- Add validation logic

### Step 5: Events (Day 2)
- Create all event structures
- Document event usage

### Step 6: Tests (Day 3)
- Write 25+ comprehensive tests
- Cover all major scenarios
- Test edge cases (charges, cooldowns, resources)

### Step 7: Build Infrastructure (Day 3)
- Create .NET projects
- Create boundary validation script
- Create GitHub Actions workflow

## Boundary Validation

### Forbidden Dependencies
```python
FORBIDDEN_IMPORTS = [
    "using UnityEngine",
    "using Unity.",
    "using MythHunter",
    "using RPGFramework.ECS",
    "using RPGFramework.Stats",
    "using RPGFramework.Combat",
    "using RPGFramework.Inventory",
    "using RPGFramework.Items",
    "using RPGFramework.Loot",
]
```

### Expected Files
- 17 runtime files (Contracts: 7, Data: 7, Implementations: 7, Events: 4)
- 1 test file
- 2 .csproj files
- 1 boundary validation script
- 1 GitHub Actions workflow

## Success Criteria

- ✅ Zero Unity dependencies (noEngineReferences: true)
- ✅ Zero MythHunter coupling (additive only)
- ✅ Zero Framework dependencies (standalone module)
- ✅ 25+ tests with 100% pass rate
- ✅ .NET 8.0 test harness passes
- ✅ Static boundary validation passes
- ✅ GitHub Actions CI GREEN
- ✅ Callback pattern for game-specific logic
- ✅ Comprehensive documentation

## Timeline

- **Day 1:** Data structures + Interfaces (4-5 hours)
- **Day 2:** Core implementations + Executor (6-7 hours)
- **Day 3:** Tests + Build infrastructure + CI (5-6 hours)
- **Total:** ~16-18 hours over 3 days

## Notes

- This is the **second standalone module** (after Loot)
- Uses **callback pattern** instead of interfaces for maximum flexibility
- **No async/await** - game layer handles timing
- **Time-based cooldowns** using float timestamps
- **Charge system** for abilities with limited uses
- **Cast time support** without Unity coroutines

---

**Status:** Ready for implementation
**Target Completion:** EPIC 11 Phase 4
**Next Phase:** Phase 5 (Progression System)
