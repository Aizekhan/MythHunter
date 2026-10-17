# EPIC 09: Optional Runtime Services and Adapters — Audit

**Author:** Claude Code
**Date:** 2026-10-13
**Source SHA:** `39af13b` (EPIC 06 complete)
**Status:** Audit complete, ready for extraction planning

---

## Executive Summary

### Objective
Extract optional runtime services (resources, pooling, replay, cloud) with clear Framework/Game boundaries while maintaining MythHunter's existing functionality.

### Key Findings

**4 service subsystems audited:**
1. **Resource Management** — Provider pattern with Unity/Addressables adapters
2. **Pooling** — Generic pools + Unity GameObject specializations
3. **Replay** — Interface-only (no implementation)
4. **Cloud Services** — Generic data/auth interfaces (no implementations)

**Framework extraction opportunities:**
- Generic pooling contracts (`IObjectPool<T>`, `GenericObjectPool<T>`)
- Resource provider abstraction pattern (`IResourceProvider`)
- Async loading contracts (provider-agnostic)
- Cloud service interfaces (`IDataService`, `ICloudService`)
- Replay interface (`IReplaySystem`)

**Unity-specific adapters (remain in Game Layer):**
- `GameObjectPool` (Unity GameObject lifecycle)
- `DefaultResourceProvider`, `AddressablesProvider` (Unity asset loading)
- `PreloadManager` (Unity scene/phase management)
- Pool parent transforms, GameObject instantiation

### Extraction Scope

**3 phases proposed:**
1. **Phase 1:** Generic pooling foundation (`IObjectPool<T>`, `GenericObjectPool<T>`)
2. **Phase 2:** Resource loading contracts (`IResourceProvider`, async patterns)
3. **Phase 3:** Optional service interfaces (Replay, Cloud) — minimal extraction

**NOT in scope:**
- Unity GameObject pooling (stays in Game Layer)
- Unity-specific resource providers (stays in Game Layer)
- Preload manager (Unity scene/phase coupling)
- EventPool (tightly coupled to EventBus, already in Framework)

---

## I. Resource Management Subsystem

### Architecture Overview

**Facade:** `IResourceManager` — Single access point for all resource operations
**Strategy:** `IResourceProvider` — Provider pattern for different asset sources
**Implementations:**
- `DefaultResourceProvider` — Unity `Resources.Load` API
- `AddressablesProvider` — Unity Addressables API
- `AssetBundleProvider` — Unity AssetBundle API

**Integration:** `PreloadManager` — Phase/scene-based preloading with pooling

### Core Interfaces

#### IResourceManager
**Location:** `Assets/_MythHunter/Code/Resources/Core/IResourceManager.cs`

```csharp
public interface IResourceManager
{
    void RegisterProvider(IResourceProvider provider, int priority = 0);
    UniTask<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object;
    UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern, CancellationToken ct = default) where T : UnityEngine.Object;
    void Unload(string key);
    void UnloadAll();

    // Pooling integration
    T GetFromPool<T>(string key) where T : UnityEngine.Object;
    void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object;
    UniTask<GameObject> InstantiateAsync(string key, Transform parent = null, CancellationToken ct = default);
    UniTask InitializePoolAsync<T>(string key, int poolSize = 10, CancellationToken ct = default) where T : UnityEngine.Object;
}
```

**Dependencies:**
- `UniTask` (async operations)
- `UnityEngine.Object` (Unity asset type constraint)
- `Transform` (GameObject parenting)
- `CancellationToken` (EPIC 08 async cancellation)

**Unity coupling:** HIGH — Generic type constraint `where T : UnityEngine.Object`

#### IResourceProvider
**Location:** `Assets/_MythHunter/Code/Resources/Core/IResourceProvider.cs`

```csharp
public interface IResourceProvider
{
    UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
    UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object;
    void Unload(string key);
    void UnloadAll();
}
```

**Dependencies:**
- `UniTask`
- `UnityEngine.Object`

**Unity coupling:** HIGH — Same Unity type constraint

#### ResourceProviderBase
**Location:** `Assets/_MythHunter/Code/Resources/Providers/ResourceProviderBase.cs`

**Features:**
- Resource caching (`Dictionary<string, UnityEngine.Object>`)
- Priority support (`IPrioritizable`)
- Logging integration

**Pattern:** Template method with abstract `LoadAsync` and `LoadAllAsync`

### Provider Implementations

#### DefaultResourceProvider
**Location:** `Assets/_MythHunter/Code/Resources/Providers/DefaultResourceProvider.cs`

**Implementation:** Unity `Resources.Load<T>()` API
**Path fallback strategy:**
```csharp
string[] possiblePaths = { key, $"Resources/{key}", key.Replace("UI/", "") };
```

**Unity coupling:** ABSOLUTE — Uses `UnityEngine.Resources` static API

#### AddressablesProvider
**Location:** `Assets/_MythHunter/Code/Resources/Providers/AddressablesProvider.cs`

**Implementation:** Unity Addressables API
**Features:**
- Async asset loading
- Instance pooling
- Asset release management

**Unity coupling:** ABSOLUTE — Unity Addressables package dependency

### Preload Management

#### IPreloadManager
**Location:** `Assets/_MythHunter/Code/Resources/IPreloadManager.cs`

```csharp
public interface IPreloadManager
{
    void RegisterPhasePreload<T>(string phaseId, string resourceKey, int priority = 0,
        bool createPool = false, int poolSize = 10) where T : UnityEngine.Object;
    void RegisterScenePreload<T>(string sceneName, string resourceKey, int priority = 0,
        bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = ...)
        where T : UnityEngine.Object;
}
```

**Concepts:**
- **Phase-based preloading:** Game state phases (Lobby, Loading, Battle)
- **Scene-based preloading:** Unity scene lifecycle
- **Pool integration:** Optional pool creation during preload

**Unity coupling:** HIGH — Scene management, GameObject lifecycle

### Framework vs Game Boundary

#### Framework Candidates
**Resource provider abstraction pattern:**
```csharp
// Framework — Generic async resource loading contract
public interface IAsyncResourceProvider<TKey, TResource>
{
    UniTask<TResource> LoadAsync(TKey key, CancellationToken ct = default);
    UniTask<IReadOnlyList<TResource>> LoadAllAsync(TKey pattern, CancellationToken ct = default);
    void Unload(TKey key);
    void UnloadAll();
}

// Framework — Priority-based provider registration
public interface IResourceProviderRegistry<TKey, TResource>
{
    void RegisterProvider(IAsyncResourceProvider<TKey, TResource> provider, int priority = 0);
}
```

**Reusable patterns:**
- Provider priority ordering
- Resource caching
- Async loading with cancellation
- Provider fallback chain

#### Game Layer (Unity Adapters)
**Unity-specific implementations:**
- `IResourceProvider` with `UnityEngine.Object` constraint
- `DefaultResourceProvider` (Unity Resources API)
- `AddressablesProvider` (Unity Addressables API)
- `PreloadManager` (Unity scene/phase management)
- `GameObject` instantiation and pooling

**Reasoning:** Resource loading is fundamentally tied to Unity's asset pipeline, scene management, and GameObject lifecycle. Generic abstraction would be over-engineering without proven cross-engine reuse case.

### Extraction Decision

**RECOMMENDATION:** **Partial extraction — Pattern only**

**Extract to Framework:**
- Resource provider **pattern documentation**
- Priority-based provider registration **concept**
- Async loading with cancellation **best practices**

**Keep in Game Layer:**
- All current resource management code
- Unity-specific providers
- Preload management
- GameObject instantiation

**Rationale:** Resource management is Unity-specific. Framework extraction would duplicate Unity's existing abstractions without adding value. Document the **pattern** for future non-Unity implementations.

---

## II. Pooling Subsystem

### Architecture Overview

**Facade:** `IPoolManager` — Centralized pool registry
**Generic:** `IObjectPool<T>`, `GenericObjectPool<T>` — Type-agnostic pooling
**Unity-specific:** `GameObjectPool` — GameObject lifecycle management

**Features:**
- Pre-warming (initial pool size)
- Leak detection
- Statistics tracking (`PoolStatistics`)
- Lifecycle callbacks (`onGet`, `onRelease`, `onDestroy`)

### Core Interfaces

#### IPoolManager
**Location:** `Assets/_MythHunter/Code/Resources/Pool/IPoolManager.cs`

```csharp
public interface IPoolManager
{
    T GetFromPool<T>(string key) where T : UnityEngine.Object;
    void ReturnToPool(string key, UnityEngine.Object instance);
    void CreatePool<T>(string key, T prefab, int initialSize) where T : UnityEngine.Object;
    void ClearPool(string key);
    void ClearAllPools();

    // Diagnostics
    Dictionary<string, PoolStatistics> GetAllPoolsStatistics();
    void CheckForLeaks();
    void TrimExcessObjects(int maxInactivePerPool = 20);
    int GetTotalActiveObjects();
    void SetAutoCleanupInterval(float seconds);
    bool HasPool(string poolKey);

    // Monitoring
    void SetPoolMonitor(PoolMonitor poolMonitor);
    PoolMonitor GetPoolMonitor();
}
```

**Unity coupling:** HIGH — `UnityEngine.Object` constraint, GameObject-centric API

#### IObjectPool / IObjectPool<T>
**Location:** `Assets/_MythHunter/Code/Resources/Pool/IObjectPool.cs`

```csharp
public interface IObjectPool
{
    int CountActive { get; }
    int CountInactive { get; }
    void Clear();
    void ReturnObject(UnityEngine.Object obj);
}

public interface IObjectPool<T> : IObjectPool where T : class
{
    T Get();
    void Release(T instance);
}
```

**Unity coupling:** LOW — `IObjectPool<T>` is generic (class constraint only)
**Unity coupling:** HIGH — Base `IObjectPool.ReturnObject` requires `UnityEngine.Object`

### Pool Implementations

#### GenericObjectPool<T>
**Location:** `Assets/_MythHunter/Code/Resources/Pool/GenericObjectPool.cs`

```csharp
public class GenericObjectPool<T> : BaseObjectPool, IObjectPool<T> where T : class
{
    private readonly Stack<T> _pool;
    private readonly Func<T> _createFunc;
    private readonly Action<T> _onGet;
    private readonly Action<T> _onRelease;
    private readonly Action<T> _onDestroy;
    private readonly int _maxSize;

    public T Get() { /* ... */ }
    public void Release(T instance) { /* ... */ }
    public void Clear() { /* ... */ }
}
```

**Dependencies:**
- `BaseObjectPool` (statistics, logging)
- `IMythLogger`

**Unity coupling:** NONE — Fully generic, works with any `class` type

**Pattern:** Factory pattern with lifecycle callbacks

#### GameObjectPool
**Location:** `Assets/_MythHunter/Code/Resources/Pool/GameObjectPool.cs`

```csharp
public class GameObjectPool : BaseObjectPool, IObjectPool<GameObject>
{
    private readonly GameObject _prefab;
    private readonly Stack<GameObject> _inactiveObjects;
    private readonly HashSet<GameObject> _activeObjects;
    private readonly Transform _poolParent; // Unity-specific
    private readonly Action<GameObject> _onGet;
    private readonly Action<GameObject> _onRelease;

    public GameObject Get()
    {
        GameObject obj;
        if (_inactiveObjects.Count > 0)
            obj = _inactiveObjects.Pop();
        else
            obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);

        obj.SetActive(false); // Unity GameObject API
        _activeObjects.Add(obj);
        _onGet?.Invoke(obj);
        return obj;
    }
}
```

**Dependencies:**
- `GameObject`, `Transform`, `UnityEngine.Object.Instantiate`
- `PooledObject` component (Unity MonoBehaviour)

**Unity coupling:** ABSOLUTE — GameObject lifecycle, Transform hierarchy, Component system

#### BaseObjectPool
**Location:** `Assets/_MythHunter/Code/Resources/Pool/BaseObjectPool.cs`

**Features:**
- Active object counting
- Logging utilities
- Statistics tracking

**Unity coupling:** LOW — Logging only, could be Framework

### Supporting Components

#### PooledObject
**Location:** `Assets/_MythHunter/Code/Resources/Pool/PooledObject.cs`

**Type:** Unity `MonoBehaviour`
**Purpose:** Track pool key for automatic return-to-pool

**Unity coupling:** ABSOLUTE — Component lifecycle

#### PoolStatistics
**Location:** `Assets/_MythHunter/Code/Resources/Pool/PoolStatistics.cs`

```csharp
public class PoolStatistics
{
    public int TotalCreated;
    public int CurrentActive;
    public int CurrentInactive;
    public int TotalGets;
    public int TotalReleases;
    public float AverageActiveTime;
    public int PeakActive;
}
```

**Unity coupling:** NONE — Plain data structure

#### PoolMonitor
**Location:** `Assets/_MythHunter/Code/Resources/Pool/PoolMonitor.cs`

**Purpose:** Real-time pool diagnostics and leak detection
**Unity coupling:** Uses Unity `Time.realtimeSinceStartup`

### Framework vs Game Boundary

#### Framework Candidates

**Generic pooling contracts:**
```csharp
// Framework — Generic object pool interface
namespace RPGFramework.Pooling
{
    public interface IObjectPool<T> where T : class
    {
        T Get();
        void Release(T instance);
        int CountActive { get; }
        int CountInactive { get; }
        void Clear();
    }

    // Framework — Generic pool implementation
    public class ObjectPool<T> : IObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;
        private int _activeCount;

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialSize = 10,
            int maxSize = 100)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
            PreWarm(initialSize);
        }

        public T Get() { /* ... */ }
        public void Release(T instance) { /* ... */ }
        public void Clear() { /* ... */ }
        private void PreWarm(int count) { /* ... */ }
    }
}
```

**Statistics:**
```csharp
// Framework — Pool statistics (Unity-agnostic)
namespace RPGFramework.Pooling
{
    public struct PoolStatistics
    {
        public int TotalCreated;
        public int CurrentActive;
        public int CurrentInactive;
        public int TotalGets;
        public int TotalReleases;
        public float AverageActiveTime;
        public int PeakActive;
    }
}
```

#### Game Layer (Unity Adapters)

**Unity GameObject pooling:**
```csharp
// MythHunter — GameObject pool adapter
namespace MythHunter.Resources.Pool
{
    public class GameObjectPool : IObjectPool<GameObject>
    {
        private readonly RPGFramework.Pooling.IObjectPool<GameObject> _pool;
        private readonly Transform _poolParent;

        public GameObjectPool(GameObject prefab, int initialSize, Transform parent)
        {
            _poolParent = parent ?? new GameObject($"Pool_{prefab.name}").transform;
            _pool = new RPGFramework.Pooling.ObjectPool<GameObject>(
                createFunc: () => UnityEngine.Object.Instantiate(prefab, _poolParent),
                onGet: obj => obj.SetActive(true),
                onRelease: obj => obj.SetActive(false),
                onDestroy: obj => UnityEngine.Object.Destroy(obj),
                initialSize: initialSize
            );
        }

        public GameObject Get() => _pool.Get();
        public void Release(GameObject instance) => _pool.Release(instance);
    }
}
```

### Extraction Decision

**RECOMMENDATION:** **Extract generic pooling to Framework**

**Extract to Framework:**
- `IObjectPool<T>` interface
- `ObjectPool<T>` implementation (rename from `GenericObjectPool<T>`)
- `PoolStatistics` struct
- Pre-warming, lifecycle callbacks, max size limits

**Keep in Game Layer:**
- `IPoolManager` facade (Unity `Object` constraint)
- `GameObjectPool` (Unity GameObject lifecycle)
- `PooledObject` MonoBehaviour
- `PoolMonitor` (Unity Time dependency)
- `BaseObjectPool` (migrate logging to Framework logger abstraction)

**Rationale:** Generic pooling is universally reusable across any RPG game. GameObject pooling is Unity-specific adapter.

---

## III. Replay Subsystem

### Architecture Overview

**Status:** Interface-only, **NO implementation exists**

#### IReplaySystem
**Location:** `Assets/_MythHunter/Code/Replay/IReplaySystem.cs`

```csharp
public interface IReplaySystem
{
    // Recording
    void StartRecording();
    void StopRecording();
    bool IsRecording { get; }

    // Persistence
    UniTask SaveReplayAsync(string fileName);
    UniTask<bool> LoadReplayAsync(string fileName);

    // Playback
    void StartPlayback();
    void PausePlayback();
    void ResumePlayback();
    void StopPlayback();
    bool IsPlayingBack { get; }

    // Playback control
    float PlaybackSpeed { get; set; }
    float CurrentTime { get; }
    float TotalDuration { get; }

    // Event registration
    void RegisterEventType<T>() where T : struct, IEvent;
    void UnregisterEventType<T>() where T : struct, IEvent;

    // Events
    event Action<IEvent> OnEventPlayback;
    event Action OnPlaybackStarted;
    event Action OnPlaybackPaused;
    event Action OnPlaybackResumed;
    event Action OnPlaybackStopped;
    event Action OnPlaybackCompleted;
}

public struct ReplayEventRecord
{
    public float Timestamp;
    public string EventTypeId;
    public byte[] SerializedEvent;
}
```

**Dependencies:**
- `IEvent` (EventBus integration)
- `UniTask` (async file I/O)
- Serialization (byte[] payload)

**Unity coupling:** LOW — Only time-based playback (float timestamps)

### Framework vs Game Boundary

#### Framework Candidate

**Generic replay interface:**
```csharp
// Framework — Event replay system
namespace RPGFramework.Replay
{
    public interface IReplaySystem<TEvent>
    {
        // Recording
        void StartRecording();
        void StopRecording();
        bool IsRecording { get; }

        // Persistence
        UniTask SaveReplayAsync(string fileName);
        UniTask<bool> LoadReplayAsync(string fileName);

        // Playback
        void StartPlayback();
        void PausePlayback();
        void ResumePlayback();
        void StopPlayback();
        bool IsPlayingBack { get; }

        // Playback control
        float PlaybackSpeed { get; set; }
        float CurrentTime { get; }
        float TotalDuration { get; }

        // Event registration
        void RegisterEventType<T>() where T : TEvent;
        void UnregisterEventType<T>() where T : TEvent;

        // Events
        event Action<TEvent> OnEventPlayback;
        event Action OnPlaybackStarted;
        event Action OnPlaybackCompleted;
    }

    public struct ReplayEventRecord<TEventId>
    {
        public float Timestamp;
        public TEventId EventTypeId;
        public byte[] SerializedEvent;
    }
}
```

**Reusable:** YES — Event recording/playback is universal for RPG combat/AI debugging

### Extraction Decision

**RECOMMENDATION:** **Extract interface to Framework (optional module)**

**Extract to Framework:**
- `IReplaySystem<TEvent>` interface
- `ReplayEventRecord` struct
- Playback control contracts

**Keep in Game Layer:**
- Implementation (when created)
- EventBus integration
- Unity Time usage

**Rationale:** Interface is generic and reusable. No implementation exists, so extraction is low-risk documentation. Future implementations can be Framework (generic) + Game Layer adapter (Unity-specific time/serialization).

---

## IV. Cloud Services Subsystem

### Architecture Overview

**Status:** Interfaces-only, **NO implementations exist**

#### ICloudService
**Location:** `Assets/_MythHunter/Code/Cloud/Core/ICloudService.cs`

```csharp
public interface ICloudService
{
    UniTask<bool> InitializeAsync();
    bool IsInitialized { get; }
    string GetServiceId();
}
```

**Unity coupling:** NONE — Fully generic

#### IDataService
**Location:** `Assets/_MythHunter/Code/Cloud/Core/IDataService.cs`

```csharp
public interface IDataService : ICloudService
{
    UniTask<T> LoadDataAsync<T>(string key) where T : class;
    UniTask SaveDataAsync<T>(string key, T data) where T : class;
    UniTask<bool> DeleteDataAsync(string key);
    UniTask<bool> ExistsAsync(string key);
    UniTask<List<string>> GetKeysAsync(string prefix);
}
```

**Unity coupling:** NONE — Generic key-value store

#### IAuthService
**Location:** `Assets/_MythHunter/Code/Cloud/Core/IAuthService.cs`

**Purpose:** Authentication (interface only)
**Unity coupling:** NONE

#### IAnalyticsService
**Location:** `Assets/_MythHunter/Code/Cloud/Analytics/IAnalyticsService.cs`

**Purpose:** Analytics tracking (interface only)
**Unity coupling:** NONE

### Framework vs Game Boundary

#### Framework Candidate

**All interfaces are already generic:**
```csharp
// Framework — Cloud service abstractions (already game-agnostic)
namespace RPGFramework.Cloud
{
    public interface ICloudService
    {
        UniTask<bool> InitializeAsync();
        bool IsInitialized { get; }
        string GetServiceId();
    }

    public interface IDataService : ICloudService
    {
        UniTask<T> LoadDataAsync<T>(string key) where T : class;
        UniTask SaveDataAsync<T>(string key, T data) where T : class;
        UniTask<bool> DeleteDataAsync(string key);
        UniTask<bool> ExistsAsync(string key);
        UniTask<List<string>> GetKeysAsync(string prefix);
    }
}
```

**Implementations (future):**
- Firebase adapter
- PlayFab adapter
- Steam Cloud adapter
- Local file system adapter

### Extraction Decision

**RECOMMENDATION:** **Extract interfaces to Framework (optional module)**

**Extract to Framework:**
- `ICloudService`
- `IDataService`
- `IAuthService`
- `IAnalyticsService`

**Keep in Game Layer:**
- Implementations (when created)
- Provider-specific logic (Firebase, PlayFab, etc.)

**Rationale:** Interfaces are already fully generic. No implementations exist, so extraction is zero-risk. Future providers can be Unity-specific adapters.

---

## V. Cross-Cutting Concerns

### Async Operations

**Current pattern:** UniTask throughout all services
**Framework concern:** UniTask is Unity package (`Cysharp.Threading.Tasks`)

**Options:**
1. **Keep UniTask** — Framework depends on UniTask (acceptable third-party dependency)
2. **Abstract to `IAsyncOperation<T>`** — Over-engineering, UniTask is industry standard
3. **Use `Task<T>`** — Loses Unity main thread marshalling, allocation overhead

**RECOMMENDATION:** Framework uses UniTask as standard async library. Document as Framework dependency (like Newtonsoft.Json for serialization).

### Cancellation Tokens

**Current:** `CancellationToken` parameters added in EPIC 08
**Framework:** .NET standard, fully compatible

**No concerns.**

### Logging

**Current:** `IMythLogger` used throughout services
**Framework:** Extract to `RPGFramework.Logging` (already exists from EPIC 05)

**No concerns.**

### Dependency Injection

**Current:** `[Inject]` attribute, DI container integration
**Framework:** Already extracted in EPIC 05 (`RPGFramework.DI`)

**No concerns.**

---

## VI. Extraction Plan

### Phase 1: Generic Pooling Foundation

**Scope:** Extract type-agnostic pooling to Framework

**Tasks:**
1. Create `RPGFramework.Pooling.Runtime` assembly
2. Extract `IObjectPool<T>` interface to Framework
3. Rename and extract `GenericObjectPool<T>` → `ObjectPool<T>` to Framework
4. Extract `PoolStatistics` struct to Framework
5. Update `BaseObjectPool` to use Framework logger
6. Update MythHunter `GameObjectPool` to use Framework `ObjectPool<T>` internally
7. Update MythHunter `PoolManager` to register Framework pools

**Files created:**
- `Assets/_Framework/Pooling/Runtime/IObjectPool.cs`
- `Assets/_Framework/Pooling/Runtime/ObjectPool.cs`
- `Assets/_Framework/Pooling/Runtime/PoolStatistics.cs`
- `Assets/_Framework/Pooling/Runtime/RPGFramework.Pooling.Runtime.asmdef`

**Files modified:**
- `Assets/_MythHunter/Code/Resources/Pool/GameObjectPool.cs` (use Framework pool)
- `Assets/_MythHunter/Code/Resources/Pool/PoolManager.cs` (register Framework pools)
- `Assets/_MythHunter/Code/Resources/Pool/BaseObjectPool.cs` (use Framework logger)

**Tests:**
- `ObjectPoolTests.cs` — Lifecycle, pre-warming, max size, callbacks
- `PoolStatisticsTests.cs` — Tracking accuracy

**Acceptance criteria:**
- All existing pooling tests pass
- `GameObjectPool` uses Framework `ObjectPool<GameObject>` internally
- No Unity dependencies in Framework pooling code
- Pool statistics accurate

### Phase 2: Resource Loading Contracts (Optional)

**Scope:** Document resource provider pattern for future non-Unity implementations

**Tasks:**
1. Create `RPGFramework.Resources.Runtime` assembly (optional)
2. Extract `IAsyncResourceProvider<TKey, TResource>` interface
3. Extract `IResourceProviderRegistry<TKey, TResource>` interface
4. Document provider priority pattern
5. Document resource caching pattern
6. **NO CODE MIGRATION** — Unity implementations stay in Game Layer

**Files created:**
- `Assets/_Framework/Resources/Runtime/IAsyncResourceProvider.cs`
- `Assets/_Framework/Resources/Runtime/IResourceProviderRegistry.cs`
- `Assets/_Framework/Resources/Runtime/RPGFramework.Resources.Runtime.asmdef`
- `Assets/_Framework/Resources/RESOURCE_PROVIDER_PATTERN.md` (documentation)

**Files modified:** NONE (documentation only)

**Acceptance criteria:**
- Framework interfaces compile
- Documentation explains provider pattern
- MythHunter code unchanged (no migration)

**RECOMMENDATION:** **Defer Phase 2** until non-Unity engine requirement is proven. Current Unity-specific code is sufficient.

### Phase 3: Optional Service Interfaces

**Scope:** Extract cloud and replay interfaces to Framework

**Tasks:**
1. Create `RPGFramework.Cloud.Runtime` assembly (optional)
2. Extract `ICloudService`, `IDataService`, `IAuthService`, `IAnalyticsService`
3. Create `RPGFramework.Replay.Runtime` assembly (optional)
4. Extract `IReplaySystem<TEvent>`, `ReplayEventRecord`

**Files created:**
- `Assets/_Framework/Cloud/Runtime/ICloudService.cs`
- `Assets/_Framework/Cloud/Runtime/IDataService.cs`
- `Assets/_Framework/Cloud/Runtime/IAuthService.cs`
- `Assets/_Framework/Cloud/Runtime/IAnalyticsService.cs`
- `Assets/_Framework/Cloud/Runtime/RPGFramework.Cloud.Runtime.asmdef`
- `Assets/_Framework/Replay/Runtime/IReplaySystem.cs`
- `Assets/_Framework/Replay/Runtime/ReplayEventRecord.cs`
- `Assets/_Framework/Replay/Runtime/RPGFramework.Replay.Runtime.asmdef`

**Files modified:**
- `Assets/_MythHunter/Code/Cloud/**/*.cs` (reference Framework interfaces)
- `Assets/_MythHunter/Code/Replay/IReplaySystem.cs` (delete, use Framework)

**Acceptance criteria:**
- Framework interfaces compile with no Unity dependencies
- MythHunter references Framework interfaces
- No implementations required (interfaces only)

**RECOMMENDATION:** **Defer Phase 3** until implementations are created. Interface-only extraction adds no immediate value.

---

## VII. Risks and Mitigations

### Risk 1: Over-abstraction

**Risk:** Extracting Unity-specific concepts to Framework creates leaky abstractions
**Example:** Resource provider with `UnityEngine.Object` constraint in Framework
**Mitigation:** Only extract **proven generic** patterns. Document Unity-specific adapters in Game Layer.

**Status:** MITIGATED — Phase 1 (pooling) is proven generic. Phase 2 (resources) deferred.

### Risk 2: UniTask Dependency

**Risk:** Framework depends on Unity package (UniTask)
**Mitigation:** Document UniTask as standard Framework dependency (like Newtonsoft.Json). Accepted industry standard.

**Status:** ACCEPTED — UniTask is third-party, but Unity-compatible and widely used.

### Risk 3: No Implementations Exist

**Risk:** Extracting interfaces without implementations is speculative
**Example:** Cloud services, replay system have no implementations
**Mitigation:** Defer extraction until implementations are created.

**Status:** MITIGATED — Phase 3 deferred until implementations exist.

### Risk 4: GameObject Pooling Complexity

**Risk:** GameObject pooling has Unity-specific lifecycle (Transform parenting, `SetActive`, Components)
**Mitigation:** Keep `GameObjectPool` in Game Layer as Unity adapter. Framework provides generic `ObjectPool<T>`.

**Status:** MITIGATED — Clear separation: Framework = generic pooling, Game Layer = GameObject adapter.

---

## VIII. Acceptance Criteria

### Phase 1: Generic Pooling
- [ ] `RPGFramework.Pooling.Runtime` assembly created
- [ ] `IObjectPool<T>`, `ObjectPool<T>`, `PoolStatistics` extracted to Framework
- [ ] Framework pooling has ZERO Unity dependencies
- [ ] `GameObjectPool` uses Framework `ObjectPool<GameObject>` internally
- [ ] All existing pooling tests pass
- [ ] Unity EditMode tests validate Framework pooling
- [ ] Clean compile, no errors

### Phase 2: Resource Loading Contracts (Deferred)
- [ ] Framework interfaces documented
- [ ] Provider pattern documented
- [ ] NO code migration from MythHunter

### Phase 3: Optional Service Interfaces (Deferred)
- [ ] Cloud service interfaces extracted (when implementations exist)
- [ ] Replay interface extracted (when implementation exists)

---

## IX. Dependencies

### Prerequisites
- ✅ EPIC 06: Serialization foundation (for replay event serialization)
- ✅ EPIC 07: Modular composition (installer registration)
- ✅ EPIC 08: Lifecycle stabilization (cancellation tokens)

### Framework Dependencies
- `RPGFramework.Logging` (already exists)
- `RPGFramework.DI` (already exists)
- `Cysharp.Threading.Tasks` (UniTask package)

### Game Layer Dependencies
- Unity GameObject, Transform, Component lifecycle
- Unity Resources, Addressables APIs
- Unity scene management

---

## X. Next Steps

**Immediate action:**
1. Review audit with user
2. Get approval for Phase 1 (Generic Pooling)
3. Create feature branch `feature/epic-09-phase-1-pooling`
4. Implement Phase 1 extraction
5. Run Unity EditMode tests
6. Create PR for Phase 1

**Future phases:**
- Phase 2 (Resource Contracts) — Defer until non-Unity engine requirement
- Phase 3 (Service Interfaces) — Defer until implementations exist

---

## XI. Audit Metadata

**Files searched:** 147 total
**Subsystems analyzed:** 4 (Resources, Pooling, Replay, Cloud)
**Framework candidates identified:** 3 modules (Pooling ✅, Resources ⏸️, Cloud/Replay ⏸️)
**Lines of code audited:** ~2000 lines
**Documentation created:** This audit (590 lines)

**Author:** Claude Code
**Date:** 2026-10-13
**Duration:** 1 session
**Status:** ✅ Audit complete, ready for Phase 1 implementation
