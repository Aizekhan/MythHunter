# EPIC 06 & 08 - Implementation Plan

**Created:** 2026-10-13
**Commit:** 92c9abe
**Total Estimated Effort:** 11-15 weeks

---

## Overview

This document outlines the implementation plan for completing EPIC 06 (Serialization System) and EPIC 08 (Dependency Debt and Lifecycle Stabilization) in parallel where possible.

**Parallel Execution Strategy:**
- Some tasks can be done in parallel (serialization + async fixes)
- Other tasks have dependencies (entity serialization requires component serializers)
- Priority: Address critical performance and stability issues first

---

## Phase 1: Foundation & Critical Fixes (Weeks 1-3)

### EPIC 08 Phase 1 - Critical Stabilization

**Priority: CRITICAL - Do First**

#### 1.1 Add Cancellation Tokens to Async Methods (Week 1)

**Files to Modify:**
1. `EventBus.cs`
   - Add `CancellationToken token = default` parameter to `StartProcessingAsync()`
   - Pass token to `UniTask.Delay()` calls

2. `GameBootstrapper.cs`
   - Add `CancellationToken` to `InitializeAsync()`
   - Create `_initCancellation` CancellationTokenSource
   - Cancel on `OnDestroy()`

3. `ResourceManager.cs`
   - Add `CancellationToken token = default` to:
     - `LoadAsync<T>()`
     - `LoadAllAsync<T>()`
     - `InstantiateAsync()`

4. **All State classes** (7 files):
   - `MainMenuState.cs`
   - `LobbyState.cs`
   - `GameplayState.cs`
   - `LoadingState.cs`
   - `CombatState.cs`
   - `ResultState.cs`
   - `PauseState.cs`
   - Add `CancellationToken token = default` to `InitializeAsync()`/`EnterAsync()` methods

5. `NavigationService.cs`
   - Add token to `NavigateToAsync()`, `GoBackAsync()`, `ShowModalAsync()`

**Acceptance Criteria:**
- [ ] All 50+ async methods have CancellationToken parameter
- [ ] Tests verify cancellation works
- [ ] No breaking changes to existing callers (default parameter)

**Estimated Effort:** 3-4 days

#### 1.2 Fix Reflection Performance in EventBus (Week 1)

**Changes:**

1. **Cache MethodInfo objects** (`EventBus.cs`):
   ```csharp
   private readonly Dictionary<Type, MethodInfo> _returnMethods = new();

   private void ReturnToPool(Type eventType, object eventData)
   {
       if (!_returnMethods.TryGetValue(eventType, out var method))
       {
           method = typeof(IEventPool).GetMethod("Return")
               .MakeGenericMethod(eventType);
           _returnMethods[eventType] = method;
       }
       method.Invoke(_eventPool, new object[] { eventData });
   }
   ```

2. **Eliminate DynamicInvoke** - Use compiled delegates:
   ```csharp
   private delegate void EventHandlerDelegate(object handler, object evt);
   private readonly Dictionary<(Type, Type), EventHandlerDelegate> _compiledHandlers = new();
   ```

3. **EventBatcher.cs** - Cache `Publish` method info

**Acceptance Criteria:**
- [ ] MethodInfo cached per event type
- [ ] Performance tests show 10-20x improvement
- [ ] No memory leaks from unbounded cache growth

**Estimated Effort:** 2-3 days

#### 1.3 Implement Task Tracking and Cancellation (Week 2)

**New Files:**
- `AsyncTaskTracker.cs` - Tracks fire-and-forget tasks

**Implementation:**
```csharp
public class AsyncTaskTracker : IDisposable
{
    private readonly List<UniTask> _tracked Tasks = new();
    private CancellationTokenSource _cts = new();

    public void Track(UniTask task)
    {
        _trackedTasks.Add(task);
        task.ContinueWith(() => _trackedTasks.Remove(task));
    }

    public async UniTask CancelAllAsync()
    {
        _cts.Cancel();
        await UniTask.WhenAll(_trackedTasks);
    }

    public void Dispose()
    {
        CancelAllAsync().Forget();
        _cts?.Dispose();
    }
}
```

**Replace .Forget() calls:**
- GameBootstrapper: Track initialization
- All States: Track async enter/exit
- UI Presenters: Track async operations

**Acceptance Criteria:**
- [ ] All 38 .Forget() calls replaced
- [ ] Tasks cancelled on dispose
- [ ] No "accessing disposed object" errors

**Estimated Effort:** 3-4 days

#### 1.4 Add Initialization/Shutdown Timeouts (Week 2)

**Files to Modify:**
1. `GameBootstrapper.cs`:
   ```csharp
   private const int INIT_TIMEOUT_MS = 30000; // 30 seconds

   public async UniTask InitializeAsync(CancellationToken token)
   {
       using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
       cts.CancelAfter(INIT_TIMEOUT_MS);

       try
       {
           await InitializeServicesAsync(cts.Token);
       }
       catch (OperationCanceledException)
       {
           _logger.LogError("Initialization timed out!");
           throw;
       }
   }
   ```

2. `SystemRegistry.cs`:
   - Add timeout to `InitializeSystemsByCategory()`

**Acceptance Criteria:**
- [ ] Initialization times out after 30 seconds
- [ ] Shutdown times out after 10 seconds
- [ ] Clear error messages on timeout

**Estimated Effort:** 2 days

---

### EPIC 06 Phase 1 - Component Serialization (Week 3)

**Priority: HIGH - After EPIC 08 Phase 1**

#### 1.5 Complete Stub Component Serializers (Week 3)

**Files to Complete (12 files):**

1. **Movement Components** (4):
   - `PositionComponent.cs`
   - `MovementComponent.cs`
   - `PathComponent.cs`
   - `VisibilityComponent.cs`

2. **Lobby Components** (2):
   - `HeroSelectionComponent.cs`
   - `LobbyStateComponent.cs`

3. **Combat Components** (6):
   - `TeamComponent.cs`
   - `CombatStyleComponent.cs`
   - `CombatStatsComponent.cs`
   - `CombatAbilityComponent.cs`
   - Additional combat components

**Implementation Pattern** (PositionComponent example):
```csharp
public byte[] Serialize()
{
    using (var ms = new System.IO.MemoryStream())
    using (var writer = new System.IO.BinaryWriter(ms))
    {
        // Vector3 Position
        writer.Write(Position.x);
        writer.Write(Position.y);
        writer.Write(Position.z);

        // Quaternion Rotation
        writer.Write(Rotation.x);
        writer.Write(Rotation.y);
        writer.Write(Rotation.z);
        writer.Write(Rotation.w);

        // Vector3 Scale
        writer.Write(Scale.x);
        writer.Write(Scale.y);
        writer.Write(Scale.z);

        return ms.ToArray();
    }
}

public void Deserialize(byte[] data)
{
    using (var ms = new System.IO.MemoryStream(data))
    using (var reader = new System.IO.BinaryReader(ms))
    {
        Position = new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );

        Rotation = new Quaternion(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );

        Scale = new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }
}
```

**Testing:**
- Create `ComponentSerializationTests.cs`
- Round-trip test for each component

**Acceptance Criteria:**
- [ ] All 12 components serialize/deserialize correctly
- [ ] Round-trip tests pass
- [ ] No data loss

**Estimated Effort:** 4-5 days

---

## Phase 2: Entity Serialization & System Refactoring (Weeks 4-6)

### EPIC 06 Phase 2 - Entity-Level Serialization

#### 2.1 Create EntitySerializer (Week 4)

**New Files:**
- `EntitySerializer.cs`

**Implementation:**
```csharp
public class EntitySerializer
{
    private readonly IEntityManager _entityManager;
    private readonly ComponentSerializerRegistry _registry;

    public byte[] SerializeEntity(int entityId)
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write entity ID
            writer.Write(entityId);

            // Get all components for entity
            var components = _entityManager.GetAllComponents(entityId);
            writer.Write(components.Count);

            foreach (var component in components)
            {
                var type = component.GetType();
                writer.Write(type.FullName);

                if (component is ISerializable serializable)
                {
                    var data = serializable.Serialize();
                    writer.Write(data.Length);
                    writer.Write(data);
                }
            }

            return ms.ToArray();
        }
    }

    public int DeserializeEntity(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        using (var reader = new BinaryReader(ms))
        {
            int entityId = reader.ReadInt32();
            int componentCount = reader.ReadInt32();

            // Create entity
            int newEntityId = _entityManager.CreateEntity();

            for (int i = 0; i < componentCount; i++)
            {
                string typeName = reader.ReadString();
                int dataLength = reader.ReadInt32();
                byte[] componentData = reader.ReadBytes(dataLength);

                // Deserialize and add component
                var type = Type.GetType(typeName);
                var component = Activator.CreateInstance(type) as ISerializable;
                component.Deserialize(componentData);

                _entityManager.AddComponent(newEntityId, (IComponent)component);
            }

            return newEntityId;
        }
    }
}
```

**Acceptance Criteria:**
- [ ] Can serialize entity with all components
- [ ] Can deserialize and recreate entity
- [ ] Tests verify component data preserved

**Estimated Effort:** 3-4 days

#### 2.2 Schema Migration System (Week 5)

**New Files:**
- `IMigrationStrategy.cs`
- `MigrationRegistry.cs`
- `Migrations/V1ToV2Migration.cs` (example)

**Implementation:**
```csharp
public interface IMigrationStrategy
{
    int SourceVersion { get; }
    int TargetVersion { get; }
    byte[] Migrate(byte[] oldData);
}

public class MigrationRegistry
{
    private readonly Dictionary<(int, int), IMigrationStrategy> _migrations = new();

    public void Register(IMigrationStrategy migration)
    {
        _migrations[(migration.SourceVersion, migration.TargetVersion)] = migration;
    }

    public byte[] MigrateToLatest(int currentVersion, byte[] data, int targetVersion)
    {
        while (currentVersion < targetVersion)
        {
            if (_migrations.TryGetValue((currentVersion, currentVersion + 1), out var migration))
            {
                data = migration.Migrate(data);
                currentVersion++;
            }
            else
            {
                throw new Exception($"No migration path from {currentVersion} to {currentVersion + 1}");
            }
        }
        return data;
    }
}
```

**Acceptance Criteria:**
- [ ] Can migrate from version N to N+1
- [ ] Can chain migrations (N → N+1 → N+2)
- [ ] Tests verify old saves still load

**Estimated Effort:** 4-5 days

---

### EPIC 08 Phase 2 - System Refactoring (Week 6)

#### 2.3 Refactor System-to-System Dependencies (Week 6)

**Target Systems:**
1. **LobbySystem** (depends on IHeroSelectionSystem, IArchetypeSystem, ITimerSystem)
2. **PhaseSystem** (depends on ITimerSystem)

**Refactoring Strategy:**

1. **Extract Timer Service**:
   - Convert ITimerSystem to ITimerService (not a system)
   - Register in InfrastructureModule
   - Remove from gameplay systems

2. **Event-Driven Communication**:
   - Replace direct system calls with events
   - Example: `HeroSelectionRequestEvent` instead of `IHeroSelectionSystem.SelectHero()`

**Acceptance Criteria:**
- [ ] No system depends on another system
- [ ] Event-driven communication works
- [ ] Tests verify behavior preserved

**Estimated Effort:** 5-6 days

---

## Phase 3: Game State & Advanced Features (Weeks 7-10)

### EPIC 06 Phase 3 - World State Serialization

#### 3.1 World Snapshot System (Week 7)

**New Files:**
- `WorldSnapshot.cs`
- `SaveGameManager.cs`

**Implementation:**
```csharp
public class WorldSnapshot
{
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public List<byte[]> Entities { get; set; }
}

public class SaveGameManager
{
    public async UniTask SaveWorldAsync(int slotId, WorldSnapshot snapshot, CancellationToken token)
    {
        string path = GetSavePath(slotId);
        byte[] data = SerializeSnapshot(snapshot);

        // Compress
        byte[] compressed = await CompressAsync(data, token);

        // Write to disk
        await File.WriteAllBytesAsync(path, compressed, token);
    }

    public async UniTask<WorldSnapshot> LoadWorldAsync(int slotId, CancellationToken token)
    {
        string path = GetSavePath(slotId);
        byte[] compressed = await File.ReadAllBytesAsync(path, token);

        // Decompress
        byte[] data = await DecompressAsync(compressed, token);

        // Deserialize
        return DeserializeSnapshot(data);
    }
}
```

**Acceptance Criteria:**
- [ ] Can save all entities to file
- [ ] Can load and restore world state
- [ ] Compression reduces file size

**Estimated Effort:** 4-5 days

#### 3.2 Auto-Save & Checkpoints (Week 8)

**New Files:**
- `AutoSaveService.cs`

**Implementation:**
```csharp
public class AutoSaveService : IDisposable
{
    private readonly SaveGameManager _saveManager;
    private readonly int _intervalSeconds;
    private CancellationTokenSource _cts;

    public async UniTask StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        while (!_cts.Token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_intervalSeconds), cancellationToken: _cts.Token);
            await AutoSaveAsync(_cts.Token);
        }
    }

    private async UniTask AutoSaveAsync(CancellationToken token)
    {
        var snapshot = CaptureWorldSnapshot();
        await _saveManager.SaveWorldAsync(AUTO_SAVE_SLOT, snapshot, token);
        RotateOldSaves();
    }
}
```

**Acceptance Criteria:**
- [ ] Auto-saves every N seconds
- [ ] Keeps last 3 auto-saves
- [ ] Can restore from checkpoint

**Estimated Effort:** 3-4 days

#### 3.3 Save Validation (Week 8)

**Implementation:**
```csharp
public class SaveValidator
{
    public uint CalculateCRC32(byte[] data)
    {
        // CRC32 implementation
    }

    public bool ValidateSave(string path)
    {
        byte[] data = File.ReadAllBytes(path);

        // Read stored CRC
        uint storedCRC = BitConverter.ToUInt32(data, 0);

        // Calculate actual CRC
        uint actualCRC = CalculateCRC32(data, 4, data.Length - 4);

        return storedCRC == actualCRC;
    }
}
```

**Acceptance Criteria:**
- [ ] Detects corrupted saves
- [ ] Falls back to older save if current corrupted

**Estimated Effort:** 2 days

---

### EPIC 08 Phase 3 - Additional Stabilization (Weeks 9-10)

#### 3.4 Thread Safety Fixes (Week 9)

**ComponentCache Thread Safety:**
```csharp
public class ComponentCache<T> where T : struct, IComponent
{
    private readonly object _lock = new object();

    public void Add(int entityId, T component)
    {
        lock (_lock)
        {
            _components[entityId] = component;
        }
    }

    public T Get(int entityId)
    {
        lock (_lock)
        {
            return _components[entityId];
        }
    }
}
```

**Acceptance Criteria:**
- [ ] Thread-safe operations
- [ ] Concurrent tests pass
- [ ] No deadlocks

**Estimated Effort:** 3-4 days

#### 3.5 Duplicate Subscription Detection (Week 9)

**EventBus Enhancement:**
```csharp
public void Subscribe<T>(EventHandler<T> handler) where T : struct, IEvent
{
    var handlers = GetHandlers<T>();

    // Check for duplicate
    if (handlers.Any(h => h.Handler.Equals(handler)))
    {
        _logger.LogWarning($"Handler already subscribed to {typeof(T).Name}");
        return;
    }

    handlers.Add(new EventHandlerWrapper { Handler = handler });
}
```

**Acceptance Criteria:**
- [ ] Prevents duplicate subscriptions
- [ ] Logs warning on duplicate attempt

**Estimated Effort:** 1-2 days

#### 3.6 Memory Leak Fixes (Week 10)

**EventBus Pending Events Limit:**
```csharp
private const int MAX_PENDING_EVENTS = 10000;

private void AddPendingEvent<T>(T evt) where T : struct, IEvent
{
    if (_pendingEvents.Count >= MAX_PENDING_EVENTS)
    {
        _logger.LogWarning("Pending events limit reached, dropping oldest events");
        _pendingEvents.RemoveAt(0);
    }

    _pendingEvents.Add(evt);
}
```

**Acceptance Criteria:**
- [ ] Pending events capped
- [ ] Memory usage stable over time

**Estimated Effort:** 2 days

---

## Phase 4: Polish & Optimization (Weeks 11-15)

### 4.1 EPIC 06 - Advanced Serialization (Weeks 11-13)

- Cloud save integration (Firebase/PlayFab)
- Replay system implementation
- Serialization format abstraction
- Performance optimization (compression, async I/O)

**Estimated Effort:** 3 weeks

### 4.2 EPIC 08 - Final Optimization (Week 14)

- DI container reflection optimization
- Performance profiling
- Initialization rollback implementation

**Estimated Effort:** 1 week

### 4.3 Testing & Documentation (Week 15)

- Comprehensive unit tests
- Integration tests
- Performance benchmarks
- Developer documentation

**Estimated Effort:** 1 week

---

## Summary

**Total Estimated Effort:** 11-15 weeks

**Critical Path:**
1. EPIC 08 Phase 1 (Weeks 1-2) - Stabilization
2. EPIC 06 Phase 1 (Week 3) - Component serialization
3. EPIC 06 Phase 2 (Weeks 4-5) - Entity serialization
4. EPIC 08 Phase 2 (Week 6) - System refactoring
5. EPIC 06 Phase 3 (Weeks 7-8) - World state
6. EPIC 08 Phase 3 (Weeks 9-10) - Final stabilization
7. Phase 4 (Weeks 11-15) - Advanced features and polish

**Priorities:**
1. **Week 1-2:** Async cancellation + reflection fixes (CRITICAL)
2. **Week 3:** Component serialization (HIGH)
3. **Week 4-5:** Entity serialization + migrations (HIGH)
4. **Week 6:** System refactoring (MEDIUM)
5. **Week 7-10:** Game state + stabilization (MEDIUM)
6. **Week 11-15:** Advanced features + polish (LOW)

**Next Immediate Actions:**
1. Start EPIC 08 Phase 1 - Add cancellation tokens
2. Fix EventBus reflection performance
3. Implement async task tracking

---

**Created By:** Claude Code
**Status:** Ready for Implementation
**Last Updated:** 2026-10-13
