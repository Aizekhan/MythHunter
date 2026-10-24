# EPIC 10 Phase 1: Resource System — Implementation Plan

> **Status:** Design phase
> **Date:** 2026-10-13
> **Dependencies:** EPIC 06 (Serialization), EPIC 09 (Pooling - optional)
> **Target:** Extract generic resource system from HealthComponent

---

## Objective

Extract `ResourceComponent<TResourceType>` from MythHunter's `HealthComponent` to create a universal RPG resource pool system (HP, MP, Stamina, Energy, etc.).

**Principle:** Framework provides generic resource management; MythHunter adapts it for game-specific Health.

---

## Current State Analysis

### HealthComponent (MythHunter Game Layer)

**File:** `Assets/_MythHunter/Code/Components/Combat/HealthComponent.cs:1-57`

```csharp
public struct HealthComponent : IComponent, ISerializableComponent
{
    // Core resource fields
    public float CurrentHealth;
    public float MaxHealth;
    public float RegenRate;
    public float LastRegenTime;

    // Combat tracking
    public float LastDamageTime;

    // Status flags
    public bool IsInvulnerable;
    public bool IsDead;

    // Manual serialization
    public byte[] Serialize() { /* BinaryWriter */ }
    public void Deserialize(byte[] data) { /* BinaryReader */ }
}
```

**Usage Pattern:**
```csharp
// CombatSystem.cs:282-284
if (_entityManager.HasComponent<HealthComponent>(entityId))
{
    HealthComponent health = _entityManager.GetComponent<HealthComponent>(entityId);
    if (!health.IsDead)
    {
        // Process logic
    }
}

// CombatSystem.cs:680-691 (ApplyDamage)
HealthComponent health = _entityManager.GetComponent<HealthComponent>(targetEntityId);
if (health.IsDead || health.IsInvulnerable) { return 0f; }
```

**Dependencies:**
- `RPGFramework.ECS.IComponent`
- `MythHunter.Core.ECS.ISerializableComponent`

---

## Framework Design

### ResourceComponent<TResourceType>

**Assembly:** `RPGFramework.Combat.Runtime`
**Namespace:** `RPGFramework.Combat`
**File:** `Assets/_Framework/Combat/Runtime/ResourceComponent.cs`

```csharp
using RPGFramework.ECS;
using RPGFramework.Serialization;

namespace RPGFramework.Combat
{
    /// <summary>
    /// Generic resource pool component (HP, MP, Stamina, Energy, etc.)
    /// </summary>
    /// <typeparam name="TResourceType">Resource identifier (enum or struct)</typeparam>
    [SerializableType("RPGFramework.Combat.ResourceComponent")]
    public struct ResourceComponent<TResourceType> : IComponent, ISerializable
        where TResourceType : struct
    {
        /// <summary>Resource type identifier</summary>
        public TResourceType ResourceType;

        /// <summary>Current resource value</summary>
        public float Current;

        /// <summary>Maximum resource capacity</summary>
        public float Max;

        /// <summary>Resource regeneration rate per second</summary>
        public float RegenRate;

        /// <summary>Last time regeneration occurred (game time)</summary>
        public float LastRegenTime;

        /// <summary>
        /// Serialization using EPIC 06 foundation
        /// </summary>
        public byte[] Serialize(ITypeRegistry typeRegistry)
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // Version for migration support
                writer.Write((ushort)1);

                // Resource type (as int for generic support)
                writer.Write(System.Runtime.CompilerServices.Unsafe.As<TResourceType, int>(ref ResourceType));

                writer.Write(Current);
                writer.Write(Max);
                writer.Write(RegenRate);
                writer.Write(LastRegenTime);

                return ms.ToArray();
            }
        }

        public void Deserialize(byte[] data, ITypeRegistry typeRegistry)
        {
            using (var ms = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                ushort version = reader.ReadUInt16();

                if (version == 1)
                {
                    int resourceTypeInt = reader.ReadInt32();
                    ResourceType = System.Runtime.CompilerServices.Unsafe.As<int, TResourceType>(ref resourceTypeInt);

                    Current = reader.ReadSingle();
                    Max = reader.ReadSingle();
                    RegenRate = reader.ReadSingle();
                    LastRegenTime = reader.ReadSingle();
                }
                else
                {
                    throw new System.InvalidOperationException($"Unsupported ResourceComponent version: {version}");
                }
            }
        }
    }
}
```

### ResourceType Enum (Example)

**File:** `Assets/_Framework/Combat/Runtime/ResourceType.cs`

```csharp
namespace RPGFramework.Combat
{
    /// <summary>
    /// Standard RPG resource types
    /// </summary>
    public enum ResourceType
    {
        Health = 0,
        Mana = 1,
        Stamina = 2,
        Energy = 3,
        Rage = 4,
        Focus = 5
    }
}
```

### IResourceManager (Optional - Phase 1.5)

**File:** `Assets/_Framework/Combat/Runtime/IResourceManager.cs`

```csharp
namespace RPGFramework.Combat
{
    /// <summary>
    /// Lifecycle manager for resource components
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>Apply damage/cost to resource</summary>
        float Consume<TResourceType>(int entityId, TResourceType resourceType, float amount) where TResourceType : struct;

        /// <summary>Restore resource value</summary>
        float Restore<TResourceType>(int entityId, TResourceType resourceType, float amount) where TResourceType : struct;

        /// <summary>Process regeneration for all entities</summary>
        void ProcessRegeneration(float deltaTime);

        /// <summary>Check if entity has sufficient resource</summary>
        bool HasResource<TResourceType>(int entityId, TResourceType resourceType, float amount) where TResourceType : struct;
    }
}
```

---

## MythHunter Adapter

### Option A: Keep HealthComponent, add ResourceComponent internally

**File:** `Assets/_MythHunter/Code/Components/Combat/HealthComponent.cs` (Modified)

```csharp
using RPGFramework.Combat;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// MythHunter health component - adapter for Framework ResourceComponent
    /// </summary>
    public struct HealthComponent : IComponent, ISerializableComponent
    {
        // Framework resource component
        private ResourceComponent<ResourceType> _resource;

        // MythHunter-specific fields
        public float LastDamageTime;
        public bool IsInvulnerable;
        public bool IsDead;

        // Public properties for backward compatibility
        public float CurrentHealth
        {
            get => _resource.Current;
            set => _resource.Current = value;
        }

        public float MaxHealth
        {
            get => _resource.Max;
            set => _resource.Max = value;
        }

        public float RegenRate
        {
            get => _resource.RegenRate;
            set => _resource.RegenRate = value;
        }

        public float LastRegenTime
        {
            get => _resource.LastRegenTime;
            set => _resource.LastRegenTime = value;
        }

        public byte[] Serialize()
        {
            // Delegate to Framework serialization + MythHunter fields
            // ... implementation
        }

        public void Deserialize(byte[] data)
        {
            // ... implementation
        }
    }
}
```

### Option B: Migrate to ResourceComponent<ResourceType> directly

Replace all `HealthComponent` usage with `ResourceComponent<ResourceType>` in systems:

```csharp
// Before
HealthComponent health = _entityManager.GetComponent<HealthComponent>(entityId);
if (health.IsDead) return;

// After
ResourceComponent<ResourceType> health = _entityManager.GetComponent<ResourceComponent<ResourceType>>(entityId);
// IsDead moved to separate DeathStateComponent or status effect
```

**Recommendation:** Option A for Phase 1 (preserve existing API), Option B for Phase 2 migration.

---

## Assembly Definition

**File:** `Assets/_Framework/Combat/Runtime/RPGFramework.Combat.Runtime.asmdef`

```json
{
    "name": "RPGFramework.Combat.Runtime",
    "rootNamespace": "RPGFramework.Combat",
    "references": [
        "RPGFramework.ECS.Runtime",
        "RPGFramework.Logging.Runtime",
        "RPGFramework.Serialization.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": true,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

**Note:** `allowUnsafeCode: true` for `Unsafe.As` in generic serialization.

---

## Test Suite

**File:** `Assets/_Framework/Combat/Tests/ResourceComponentTests.cs`

```csharp
using NUnit.Framework;
using RPGFramework.Combat;
using RPGFramework.ECS;

namespace RPGFramework.Combat.Tests
{
    [TestFixture]
    public class ResourceComponentTests
    {
        [Test]
        public void ResourceComponent_InitialValues_AreZero()
        {
            var resource = new ResourceComponent<ResourceType>
            {
                ResourceType = ResourceType.Health,
                Current = 0f,
                Max = 0f,
                RegenRate = 0f,
                LastRegenTime = 0f
            };

            Assert.AreEqual(ResourceType.Health, resource.ResourceType);
            Assert.AreEqual(0f, resource.Current);
            Assert.AreEqual(0f, resource.Max);
        }

        [Test]
        public void ResourceComponent_SetValues_PreservesData()
        {
            var resource = new ResourceComponent<ResourceType>
            {
                ResourceType = ResourceType.Mana,
                Current = 50f,
                Max = 100f,
                RegenRate = 5f,
                LastRegenTime = 10f
            };

            Assert.AreEqual(ResourceType.Mana, resource.ResourceType);
            Assert.AreEqual(50f, resource.Current);
            Assert.AreEqual(100f, resource.Max);
            Assert.AreEqual(5f, resource.RegenRate);
            Assert.AreEqual(10f, resource.LastRegenTime);
        }

        [Test]
        public void ResourceComponent_Serialization_RoundTrip()
        {
            var original = new ResourceComponent<ResourceType>
            {
                ResourceType = ResourceType.Stamina,
                Current = 75f,
                Max = 150f,
                RegenRate = 10f,
                LastRegenTime = 20f
            };

            var typeRegistry = new RPGFramework.Serialization.TypeRegistry();
            byte[] data = original.Serialize(typeRegistry);

            var deserialized = new ResourceComponent<ResourceType>();
            deserialized.Deserialize(data, typeRegistry);

            Assert.AreEqual(original.ResourceType, deserialized.ResourceType);
            Assert.AreEqual(original.Current, deserialized.Current);
            Assert.AreEqual(original.Max, deserialized.Max);
            Assert.AreEqual(original.RegenRate, deserialized.RegenRate);
            Assert.AreEqual(original.LastRegenTime, deserialized.LastRegenTime);
        }

        [Test]
        public void ResourceComponent_MultipleResourceTypes_DoNotConflict()
        {
            var health = new ResourceComponent<ResourceType> { ResourceType = ResourceType.Health, Current = 100f };
            var mana = new ResourceComponent<ResourceType> { ResourceType = ResourceType.Mana, Current = 50f };

            Assert.AreEqual(ResourceType.Health, health.ResourceType);
            Assert.AreEqual(ResourceType.Mana, mana.ResourceType);
            Assert.AreNotEqual(health.Current, mana.Current);
        }

        [Test]
        public void ResourceComponent_CurrentExceedsMax_IsAllowed()
        {
            // Framework does not enforce max cap - that's game layer policy
            var resource = new ResourceComponent<ResourceType>
            {
                ResourceType = ResourceType.Health,
                Current = 150f,
                Max = 100f
            };

            Assert.Greater(resource.Current, resource.Max);
        }

        [Test]
        public void ResourceComponent_NegativeCurrent_IsAllowed()
        {
            // Framework allows negative (e.g., overheal debt) - game layer decides
            var resource = new ResourceComponent<ResourceType>
            {
                ResourceType = ResourceType.Health,
                Current = -10f,
                Max = 100f
            };

            Assert.Less(resource.Current, 0f);
        }
    }
}
```

**Assembly:** `Assets/_Framework/Combat/Tests/RPGFramework.Combat.Tests.asmdef`

---

## Implementation Steps

### 1. Create Directory Structure
```
Assets/_Framework/Combat/
├── Runtime/
│   ├── RPGFramework.Combat.Runtime.asmdef
│   ├── ResourceComponent.cs
│   └── ResourceType.cs
└── Tests/
    ├── RPGFramework.Combat.Tests.asmdef
    └── ResourceComponentTests.cs
```

### 2. Create Feature Branch
```bash
git checkout dev
git pull origin dev
git checkout -b feature/epic-10-phase-1-resource-system
```

### 3. Implement Framework Components
- [ ] ResourceComponent<TResourceType> struct
- [ ] ResourceType enum
- [ ] SerializableTypeAttribute registration
- [ ] .asmdef files

### 4. Add Tests
- [ ] ResourceComponentTests.cs (7 tests)
- [ ] Update .NET test harness
- [ ] Update CI workflow paths

### 5. Validation
- [ ] Static boundary/GUID validation
- [ ] .NET tests pass locally
- [ ] GitHub Actions: RPGFramework Combat Runtime
- [ ] Unity EditMode tests pass

### 6. MythHunter Adapter (Phase 1 - preserve existing API)
- [ ] Keep HealthComponent.cs unchanged initially
- [ ] Add ResourceComponent<ResourceType> internally (optional)
- [ ] No migration in Phase 1

### 7. Documentation
- [ ] XML doc comments for all public APIs
- [ ] Update EPIC-10-Phase1-Implementation-Plan.md with actuals
- [ ] Create PR description with validation evidence

---

## Acceptance Criteria

### Framework Deliverables
- [x] `RPGFramework.Combat.Runtime.asmdef` created with correct references
- [ ] `ResourceComponent<TResourceType>` implements IComponent, ISerializable
- [ ] `ResourceType` enum defines 6 standard resource types
- [ ] 7 NUnit tests pass (.NET and Unity EditMode)
- [ ] No UnityEngine dependencies (`noEngineReferences: true`)
- [ ] Static boundary validation passes
- [ ] GUID uniqueness check passes

### Integration
- [ ] MythHunter compiles with no errors
- [ ] Unity Console clean (no warnings from new assembly)
- [ ] Game bootstrap smoke test passes
- [ ] No regression in HealthComponent usage
- [ ] GitHub Actions green on PR head

### Documentation
- [ ] XML doc comments on all public types/members
- [ ] PR description includes:
  - Implementation summary
  - Test results (EditMode XML, GitHub Actions links)
  - Migration plan for Phase 2

---

## Rollback Plan

If validation fails:
1. **Preserve feature branch:** Do not delete; fix issues incrementally
2. **Revert .asmdef references:** Remove `RPGFramework.Combat.Runtime` from MythHunter dependencies if added
3. **Preserve HealthComponent:** No changes to existing MythHunter code in Phase 1
4. **CI isolation:** Framework tests run independently; failure does not block MythHunter

**Key:** Phase 1 is additive only - no breaking changes to existing systems.

---

## Open Questions

### 1. Generic Serialization with Unsafe.As
**Question:** Is `Unsafe.As<TResourceType, int>` safe for all TResourceType constraints?
**Answer:** Only for `where TResourceType : struct` with size <= sizeof(int). Enums are safe. Custom structs need size validation.

**Mitigation:** Add static assertion or runtime check in Serialize method.

### 2. IsInvulnerable and IsDead Flags
**Question:** Should these be in ResourceComponent or separate?
**Answer:** Separate. These are status modifiers, not resource properties.

**Decision:** Keep them in HealthComponent (MythHunter layer) for Phase 1. Phase 4 (Status Effect System) will extract them.

### 3. LastDamageTime Tracking
**Question:** Is this resource-level or combat-level concern?
**Answer:** Combat-level. Not every resource needs damage tracking (e.g., Mana consumption).

**Decision:** Keep LastDamageTime in HealthComponent (MythHunter layer). Not universal enough for Framework.

### 4. Resource Capping Policy
**Question:** Should ResourceComponent enforce Current <= Max?
**Answer:** No. Some games allow overheal, temporary buffs exceeding max, or debt (negative values).

**Decision:** Framework stores values as-is; game layer enforces policy.

---

## Next Session Handoff

**If Phase 1 implementation is incomplete:**
1. Read this plan document
2. Check feature branch state: `feature/epic-10-phase-1-resource-system`
3. Review implementation checklist (Step 3-7)
4. Continue from last incomplete step
5. Run tests before creating PR

**Completed work will be in:**
- `Assets/_Framework/Combat/Runtime/` (Framework implementation)
- `Assets/_Framework/Combat/Tests/` (Test suite)
- `.github/workflows/` (CI configuration updates)

---

**End of Implementation Plan**
