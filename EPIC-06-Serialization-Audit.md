# EPIC 06 — Serialization Foundation Audit

> Audit date: 2026-10-13
> MythHunter dev HEAD: `ff9351d`
> Prerequisites: EPIC 07 ✅, EPIC 08 ✅

## Executive Summary

MythHunter has **four distinct serialization subsystems** with different purposes, wire formats, and coupling levels:

1. **Component Serialization** — Binary serialization for ECS components (persistence/network)
2. **Network Serialization** — Optimized binary for multiplayer messages + delta compression
3. **Generic Data Serialization** — Versioned binary wrapper for schema evolution
4. **Persistence Layer** — JSON-based save/load using Unity's `JsonUtility`

**Key findings:**
- ✅ Clear separation of concerns (components, network, persistence)
- ✅ Version management exists (`VersionedSerializer`)
- ⚠️ Type identification uses `Type.FullName` strings (fragile across assemblies/refactoring)
- ⚠️ No schema migration strategy for breaking changes
- ⚠️ Delta serialization tightly coupled to networking
- ⚠️ Component serializer registry hardcodes game-specific components
- ⚠️ Multiple `ISerializable` patterns (byte[] vs BinaryWriter)

**Framework extraction opportunity:** Generic serialization contracts + versioning strategy + schema registry patterns.

---

## 1. Component Serialization (ECS Binary)

### Location
`Assets/_MythHunter/Code/Data/Serialization/`

### Contracts
```csharp
// Generic component serializer
public interface IComponentSerializer<T> where T : struct, RPGFramework.ECS.IComponent
{
    byte[] Serialize(T component);
    T Deserialize(byte[] data);
}

// Registry for type-to-serializer mapping
public interface IComponentSerializerRegistry
{
    void RegisterSerializer<T, TSerializer>(TSerializer serializer)
        where T : struct, IComponent
        where TSerializer : IComponentSerializer<T>;

    IComponentSerializer<T> GetSerializer<T>() where T : struct, IComponent;
    byte[] Serialize<T>(T component) where T : struct, IComponent;
    T Deserialize<T>(byte[] data) where T : struct, IComponent;
}

// Marker interface (currently unused for runtime checks)
public interface ISerializableComponent : IComponent { }
```

### Implementation

**ComponentSerializerBase<T>**
Location: `Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerBase.cs:10`

```csharp
public abstract class ComponentSerializerBase<T> : IComponentSerializer<T>
    where T : struct, IComponent
{
    public byte[] Serialize(T component)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            SerializeInternal(writer, component);
            return stream.ToArray();
        }
    }

    protected abstract void SerializeInternal(BinaryWriter writer, T component);
    protected abstract T DeserializeInternal(BinaryReader reader);
}
```

**ComponentSerializerRegistry**
Location: `Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerRegistry.cs:13`

- Singleton registry with `Dictionary<Type, object>` for serializer lookup
- Constructor calls `RegisterDefaultSerializers()` with **hardcoded game components**:
  - `IdComponent`, `NameComponent`, `DescriptionComponent`, `ValueComponent`
- Registered via DI in `SerializationInstaller.cs:15`

**CoreComponentSerializers**
Location: `Assets/_MythHunter/Code/Data/Serialization/CoreComponentSerializers.cs:6`

Concrete serializers for 4 core components (Id, Name, Description, Value).

### Analysis

**Framework candidates:**
- ✅ `IComponentSerializer<T>` — generic, tied to `IComponent`
- ✅ `ComponentSerializerBase<T>` — reusable template
- ✅ `IComponentSerializerRegistry` interface — abstraction is reusable

**Game-specific:**
- ❌ `ComponentSerializerRegistry` implementation — calls `RegisterDefaultSerializers()` with MythHunter components
- ❌ `CoreComponentSerializers` — domain-specific (MythHunter.Components.Core)

**Issues:**
1. **No schema versioning** — components can change structure; no migration path
2. **No type identifier** — relies on caller knowing component type at deserialization
3. **Registry is eager** — constructs all serializers at startup even if unused
4. **Marker interface unused** — `ISerializableComponent` exists but no runtime validation

**Reusability:** 7/10
Contracts are reusable; implementation needs game-agnostic registry + lazy registration.

---

## 2. Network Serialization (Binary + Delta Compression)

### Location
`Assets/_MythHunter/Code/Networking/Serialization/`

### Contracts

**INetworkMessage**
Location: `Assets/_MythHunter/Code/Networking/Messages/INetworkMessage.cs:8`

```csharp
public interface INetworkMessage : ISerializable
{
    string GetMessageId();
}
```

**INetworkSerializer**
Location: `Assets/_MythHunter/Code/Networking/Serialization/INetworkSerializer.cs:7`

```csharp
public interface INetworkSerializer
{
    byte[] Serialize<T>(T message) where T : INetworkMessage;
    T Deserialize<T>(byte[] data) where T : INetworkMessage, new();
    INetworkMessage Deserialize(byte[] data, System.Type messageType);
}
```

### Implementation

**BinaryNetworkSerializer**
Location: `Assets/_MythHunter/Code/Networking/Serialization/BinaryNetworkSerializer.cs:12`

- Wraps messages with `Type.FullName` header + payload
- Reflection-based **message type registry** (scans all assemblies for `INetworkMessage` implementations)
- **Constructor registers all types eagerly** — expensive startup cost
- Uses `Activator.CreateInstance` for deserialization

Wire format:
```
[Type.FullName (string)] [PayloadLength (int32)] [Payload (byte[])]
```

**DeltaSerializer**
Location: `Assets/_MythHunter/Code/Networking/Serialization/DeltaSerializer.cs:12`

- **Stateful delta compression** for entity snapshots
- Maintains `Dictionary<int, byte[]>` of last known state per entity
- Compares byte-by-byte; sends only changed offsets
- Falls back to full state if delta > 80% of original

Wire format (delta):
```
[IsFullState (bool)] [ChangeCount (int32)] [Offset (int32), Value (byte)]...
```

Wire format (full):
```
[IsFullState=true (bool)] [Length (int32)] [FullState (byte[])]
```

**NetworkEventMessage**
Location: `Assets/_MythHunter/Code/Networking/Messages/NetworkEventMessage.cs:13`

Concrete message for event propagation. Uses reflection (`GetMethod("Deserialize")`) to deserialize event payloads.

### Analysis

**Framework candidates:**
- ⚠️ `INetworkMessage` — depends on `ISerializable`, could be generic
- ⚠️ `INetworkSerializer` — contract is reusable but assumes byte[] pattern
- ✅ Delta compression algorithm — reusable pattern (not Network-specific)

**Game-specific:**
- ❌ `BinaryNetworkSerializer` — assembly scanning, hardcoded type discovery
- ❌ `NetworkEventMessage` — uses reflection + game event types

**Issues:**
1. **Type identification fragility** — `Type.FullName` breaks across assemblies, obfuscation, refactoring
2. **No versioning** — message schema changes cause silent deserialization failures
3. **Assembly scanning cost** — startup reflection is expensive + brittle
4. **Delta serializer is stateful** — cannot be shared across contexts; no cleanup policy
5. **No endianness handling** — BinaryWriter/Reader assumes platform endianness

**Reusability:** 4/10
Delta compression logic is valuable; type discovery + wire format need redesign.

---

## 3. Generic Data Serialization (Versioned Binary)

### Location
`Assets/_MythHunter/Code/Data/Serialization/`

### Contracts

**ISerializable**
Location: `Assets/_MythHunter/Code/Data/Serialization/ISerializable.cs:8`

```csharp
public interface ISerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}
```

### Implementation

**VersionedSerializer**
Location: `Assets/_MythHunter/Code/Data/Serialization/VersionedSerializer.cs:11`

Wrapper that adds version + type metadata to `ISerializable` payloads.

Wire format:
```
[Version (ushort)] [Type.FullName (string)] [PayloadLength (int32)] [Payload (byte[])]
```

- Current version: `1` (hardcoded constant)
- Warns if deserializing newer version but **does not reject**
- Warns on type mismatch but **proceeds anyway**
- Requires `new()` constraint (must have parameterless constructor)

### Analysis

**Framework candidates:**
- ✅ `ISerializable` — fully generic, no dependencies
- ⚠️ `VersionedSerializer` — good pattern but weak validation

**Issues:**
1. **No schema migration strategy** — version check is advisory only
2. **Type name fragility** — same as network serialization
3. **No compatibility matrix** — can't declare "version 1 → 2 migrator"
4. **Constructor constraint** — `new()` excludes struct-based serialization
5. **Silent failures** — logs warnings but deserializes anyway (can corrupt data)

**Reusability:** 6/10
Solid foundation; needs migration hooks + stronger validation.

---

## 4. Persistence Layer (JSON, Unity-specific)

### Location
`Assets/_MythHunter/Code/Services/Heroes/LocalHeroDataService.cs:17`

### Implementation

**LocalHeroDataService**
Uses Unity's `JsonUtility.ToJson` / `JsonUtility.FromJson<T>` for hero data persistence.

- File-based storage in `Application.persistentDataPath/HeroData/`
- In-memory cache (`Dictionary<string, HeroDataModel>`)
- Async file I/O via `UniTask` + `StreamWriter`
- No versioning, no schema validation

### Analysis

**Framework candidates:**
- ❌ Completely Unity + MythHunter domain-specific

**Issues:**
1. **No schema versioning** — breaking changes to `HeroDataModel` break save files
2. **Unity JsonUtility limitations** — no polymorphism, limited types, no custom converters
3. **No corruption handling** — single file per hero; corruption = data loss
4. **No backup/recovery** — overwrites in place
5. **No encryption** — hero data is plaintext JSON

**Reusability:** 1/10
Domain-specific service; only the **persistence policy pattern** (cache + file storage) is reusable.

---

## Cross-Cutting Concerns

### Type Identification
**Current approach:** `Type.FullName` strings

**Problems:**
- Breaks across assembly renames
- Breaks during refactoring/namespace changes
- Fragile to obfuscation
- No explicit type registry

**Framework opportunity:** Stable type IDs (e.g., `[SerializableType("hero.stats.v1")]` attributes)

### Schema Versioning
**Current state:**
- `VersionedSerializer` has version field but no migration
- Component/network serializers have no versioning at all
- Persistence has no schema tracking

**Framework opportunity:**
- Version metadata in all wire formats
- Migration hook registry (`IMigrator<TFrom, TTo>`)
- Compatibility matrix (which versions can interoperate)

### Wire Format Diversity
**Current formats:**
1. Binary (BinaryWriter/Reader) — Component, Network, Versioned
2. JSON (JsonUtility) — Persistence

**Issues:**
- No common abstraction
- Cannot swap formats without rewriting serializers
- No format negotiation for network (client/server must match exactly)

**Framework opportunity:**
- Abstraction: `ISerializer` / `IDeserializer` + `IWireFormat`
- Pluggable formats (binary, JSON, protobuf, MessagePack)

### Boundary Violations
**Observed:**
1. `ComponentSerializerRegistry` hardcodes game components in Framework-layer code
2. Network serializers use reflection to discover game types
3. `ISerializable` pattern is duplicated across layers

**Required separation:**
- **Framework:** contracts, base classes, versioning, migration hooks
- **Game Layer:** concrete serializers, component/message registration
- **Networking Module:** wire protocols, delta compression
- **Persistence Module:** storage policy, backup/recovery

---

## Framework Extraction Plan

### Phase 1: Core Contracts (Reusable, No Dependencies)
**Target:** `Assets/_Framework/Serialization/Runtime/`

**Contracts to extract:**
```csharp
namespace RPGFramework.Serialization
{
    // Generic serialization
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }

    // Type-safe serialization (alternative to ISerializable)
    public interface ISerializer<T>
    {
        byte[] Serialize(T value);
        T Deserialize(byte[] data);
    }

    // Schema versioning
    public interface IVersionedData
    {
        ushort Version { get; }
    }

    // Type identification (stable IDs)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializableTypeAttribute : Attribute
    {
        public string TypeId { get; }
        public ushort Version { get; }
    }

    // Schema migration
    public interface IMigrator<TFrom, TTo>
    {
        TTo Migrate(TFrom oldVersion);
    }

    // Type registry (for polymorphic deserialization)
    public interface ITypeRegistry
    {
        void RegisterType<T>(string typeId);
        Type GetType(string typeId);
        string GetTypeId<T>();
    }
}
```

**Exit criteria:**
- Contracts compile in Framework assembly (no Unity, no game dependencies)
- .NET unit tests for type registry + migration hooks
- Documentation with usage examples

### Phase 2: Versioned Serialization
**Target:** Extract `VersionedSerializer` pattern with migration support

**Changes:**
1. Replace `Type.FullName` with `ITypeRegistry` lookup
2. Add migration hook: `IMigrator<TFrom, TTo>`
3. Reject incompatible versions instead of logging warnings
4. Support struct serialization (remove `new()` constraint)

**Exit criteria:**
- Version mismatch is detected and rejected
- Migration test: v1 → v2 struct transformation
- Unity EditMode tests for versioned component serialization

### Phase 3: Component Serialization (Framework)
**Target:** Decouple `IComponentSerializer<T>` from game-specific registry

**Changes:**
1. Extract `IComponentSerializer<T>` and `ComponentSerializerBase<T>` to Framework
2. Make `ComponentSerializerRegistry` game-agnostic (no default serializers)
3. Add lazy registration (don't instantiate until first use)
4. Add serializer discovery via attributes (avoid manual registration)

**Exit criteria:**
- Framework assembly has component serialization contracts
- Game layer registers MythHunter components via installer
- No hardcoded component types in Framework code

### Phase 4: Delta Compression (Optional Module)
**Target:** Generalize `DeltaSerializer` for any state snapshots

**Changes:**
1. Separate delta algorithm from network transport
2. Make it stateless (caller manages last-known-state)
3. Add cleanup policy for stale state (TTL, max entries)
4. Support custom diff strategies (byte-by-byte, field-level, semantic)

**Exit criteria:**
- Delta compression works for non-network scenarios (e.g., save file optimization)
- Benchmark: delta vs full state for typical ECS snapshots
- Memory leak test: delta state cleanup

### Phase 5: Network Serialization (Networking Module)
**Target:** Keep in Networking Module, refactor to use Framework contracts

**Changes:**
1. Replace assembly scanning with explicit type registration
2. Use `ITypeRegistry` instead of `Type.FullName`
3. Add wire protocol versioning (handshake negotiation)
4. Separate message serialization from transport layer

**Exit criteria:**
- Client/server can negotiate wire format version
- Type refactoring does not break network protocol
- Network serialization tests independent of Unity

---

## Risks & Mitigations

### Risk 1: Breaking Existing Save Files
**Mitigation:**
- Phase 1-2 are additive (no changes to existing serializers)
- Add migration path **before** changing any wire formats
- Keep old serializers as "legacy compat layer" temporarily

### Risk 2: Network Protocol Incompatibility
**Mitigation:**
- Add protocol version handshake before extracting
- Maintain backward compat for 1 version (N-1 rule)
- Feature flag: opt-in to new serialization

### Risk 3: Performance Regression
**Mitigation:**
- Benchmark current serialization performance
- Gate: new serialization must be ≥90% of baseline
- Profile delta compression overhead

### Risk 4: Assembly Dependency Cycles
**Mitigation:**
- Framework.Serialization has zero dependencies (pure contracts)
- Game layer + modules depend on Framework (one-way)
- Static boundary validator enforces no reverse references

---

## Dependencies & Prerequisites

### Completed (Required)
- ✅ EPIC 07: Modular composition system (framework/game separation)
- ✅ EPIC 08: Lifecycle stabilization (threading, async)

### Blocked By
- None (can proceed immediately)

### Blocks
- ⚠️ EPIC 09 (Resources/Persistence) — needs serialization foundation
- ⚠️ EPIC 12 (Persistence) — needs schema versioning + migration
- ⚠️ EPIC 13 (Networking) — needs type registry + wire format

---

## Acceptance Criteria

### EPIC 06 Complete When:
1. **Framework contracts extracted:**
   - [ ] `ISerializable`, `ISerializer<T>`, `IVersionedData` in Framework assembly
   - [ ] `ITypeRegistry`, `SerializableTypeAttribute` implemented
   - [ ] Migration hook pattern (`IMigrator<TFrom, TTo>`) defined

2. **Versioning works:**
   - [ ] Version mismatch detection + rejection test
   - [ ] Migration test: v1 component → v2 component transformation
   - [ ] Type registry test: stable IDs survive refactoring

3. **Component serialization refactored:**
   - [ ] `ComponentSerializerRegistry` is game-agnostic
   - [ ] MythHunter components registered via installer (not hardcoded)
   - [ ] No Framework code knows about game components

4. **Documentation:**
   - [ ] Serialization architecture diagram (Framework vs Game vs Network)
   - [ ] Migration guide: how to add new component, how to version schema
   - [ ] Example: serializing custom RPG stat component in different game

5. **Tests:**
   - [ ] .NET unit tests for type registry + migration hooks
   - [ ] Unity EditMode: component serialization roundtrip
   - [ ] Backward compat test: deserialize old data with new code

6. **Cleanup:**
   - [ ] Remove unused `ISerializableComponent` marker or add runtime validation
   - [ ] Delta serializer moved to optional module (not mandatory Framework)
   - [ ] `VersionedSerializer` uses `ITypeRegistry` instead of `Type.FullName`

---

## Next Steps

1. **Create Phase 1 PR:**
   - Extract core contracts to `Assets/_Framework/Serialization/Runtime/`
   - Add .NET test project for type registry
   - Add static boundary validation

2. **Create Phase 2 PR:**
   - Refactor `VersionedSerializer` with migration hooks
   - Add version rejection + migration test
   - Update `ComponentSerializerRegistry` to use type registry

3. **Audit blockers for EPIC 09/12/13:**
   - Confirm serialization foundation unblocks persistence/networking
   - Update roadmap dependencies

4. **Update STATUS.md:**
   - Mark EPIC 06 Phase 1 as ACTIVE
   - Document current HEAD SHA after audit

---

## Appendix: File Inventory

### Component Serialization
- `Assets/_MythHunter/Code/Core/ECS/ISerializableComponent.cs` (marker interface)
- `Assets/_MythHunter/Code/Data/Serialization/IComponentSerializer.cs`
- `Assets/_MythHunter/Code/Entities/IComponentSerializerRegistry.cs`
- `Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerRegistry.cs`
- `Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerBase.cs`
- `Assets/_MythHunter/Code/Data/Serialization/CoreComponentSerializers.cs`

### Network Serialization
- `Assets/_MythHunter/Code/Networking/Messages/INetworkMessage.cs`
- `Assets/_MythHunter/Code/Networking/Messages/NetworkEventMessage.cs`
- `Assets/_MythHunter/Code/Networking/Serialization/INetworkSerializer.cs`
- `Assets/_MythHunter/Code/Networking/Serialization/BinaryNetworkSerializer.cs`
- `Assets/_MythHunter/Code/Networking/Serialization/DeltaSerializer.cs`

### Generic Serialization
- `Assets/_MythHunter/Code/Data/Serialization/ISerializable.cs`
- `Assets/_MythHunter/Code/Data/Serialization/VersionedSerializer.cs`

### Persistence
- `Assets/_MythHunter/Code/Services/Heroes/LocalHeroDataService.cs`

### DI Registration
- `Assets/_MythHunter/Code/Core/Installers/CoreServices/SerializationInstaller.cs`

### Total: 16 serialization-related files audited
