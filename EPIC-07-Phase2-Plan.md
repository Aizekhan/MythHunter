# EPIC 07 Phase 2 - Installer Migration Plan

> Created: 2026-10-12
> Status: DRAFT - Waiting for PR #58 merge
> Base: EPIC 07 Phase 1 (PR #58, HEAD: 1fa6115)

## Objective

Complete the modular composition migration by moving 4 remaining installers from GameBootstrapper into the appropriate feature modules.

## Current State (Phase 1 Complete)

**Modules Implemented:**
- ✅ FrameworkCoreModule - Universal ECS/DI/Logger
- ✅ GameLayerModule - Game flow, state machine, scenes
- ✅ InfrastructureModule - Resources, pooling, serialization, UI
- ✅ GameplayModule - Movement, visibility, hero systems
- ✅ NetworkingModule - Multiplayer infrastructure (conditional)
- ✅ LobbyModule - Pre-game lobby (conditional)
- ✅ DebugToolsModule - Development tools (conditional)

**Remaining Direct Installer Calls** (GameBootstrapper.cs:110-120):
1. LazyDependencyInjectorInstaller
2. EventsInstaller
3. ProfileInstaller
4. ParallelSystemsInstaller

## Installer Analysis

### 1. ParallelSystemsInstaller
**Lines:** 29
**Registers:**
- `ISystemJobScheduler → SystemJobScheduler`
- `ISystemRegistry → ParallelSystemRegistry`

**Critical:** Replaces basic ISystemRegistry with parallel execution version.

**Migration Target:** **FrameworkCoreModule**

**Reason:** System execution strategy is Framework Core responsibility, not game-specific.

**Concerns:**
- Must verify if CoreInstaller already registers ISystemRegistry
- If yes, need to either:
  a) Make ParallelSystemRegistry the default in CoreInstaller
  b) Make CoreInstaller conditional (check container for existing registration)
- Check dependency order: Many systems depend on ISystemRegistry

---

### 2. EventsInstaller
**Lines:** 57
**Registers:**
- `IEventThrottler → EventThrottler`
- `IEventThrottlerUpdateSystem → EventThrottlerUpdateSystem`
- `IEventBatcher → EventBatcher`
- `IEventStore → EventStore`
- `IEventBus` - **CONDITIONAL LOGIC:**
  - If `EventNetworkMode.Offline` → `EventBus`
  - Otherwise → `NetworkEventBus` (and alias to IEventBus)
- `ITimerSystem → TimerSystem` (registered in ISystemRegistry)

**Dependencies:**
- Reads `EventNetworkMode` from container
- Resolves `ISystemRegistry` to register TimerSystem

**Migration Target:** **GameLayerModule**

**Reason:**
- Events are core game layer functionality
- Network-aware logic (offline vs multiplayer) belongs in game layer
- Already has access to EventNetworkMode through container

**Concerns:**
- Must run AFTER ParallelSystemsInstaller (needs ISystemRegistry)
- Network mode must be registered before this module configures

---

### 3. LazyDependencyInjectorInstaller
**Lines:** 36
**Registers:**
- Creates GameObject "LazyDependencyInjector" with DontDestroyOnLoad
- Adds LazyDependencyInjector component
- Initializes with container and logger
- Registers as `IDependencyInjector`

**Dependencies:**
- Needs IMythLogger from container

**Migration Target:** **InfrastructureModule**

**Reason:**
- DI infrastructure concern (scene injection)
- Creates Unity GameObject - infrastructure responsibility
- Not Framework Core (game-specific scene injection mechanism)

**Concerns:**
- Creates GameObject - ensure module initialization happens in Unity context
- DontDestroyOnLoad - ensure proper lifecycle

---

### 4. ProfileInstaller
**Lines:** 22
**Registers:**
- `IProfilePresenter → ProfilePresenter`

**Dependencies:** None

**Migration Target:** **InfrastructureModule**

**Reason:**
- UI presenter - already fits with UI services in Infrastructure
- Game-specific UI component
- Simple, no special concerns

**Concerns:** None - straightforward migration

## Migration Strategy

### Phase 2.1: Framework Core Update
**Target:** ParallelSystemsInstaller → FrameworkCoreModule

1. **Investigate CoreInstaller.cs**
   ```bash
   # Check what CoreInstaller currently registers
   grep -n "ISystemRegistry" Assets/_MythHunter/Code/Core/Installers/CoreServices/CoreInstaller.cs
   ```

2. **Decision Point:**
   - If CoreInstaller registers ISystemRegistry:
     - **Option A:** Replace with ParallelSystemRegistry in CoreInstaller directly
     - **Option B:** Make CoreInstaller check if already registered
   - If CoreInstaller doesn't register ISystemRegistry:
     - Add ParallelSystemsInstaller logic to FrameworkCoreModule.OnConfigure()

3. **Update FrameworkCoreModule.cs**
   ```csharp
   protected override void OnConfigure()
   {
       // Existing: Already has CoreInstaller logic

       // ADD: Parallel Systems
       _container.BindSingleton<ISystemJobScheduler, SystemJobScheduler>();
       _container.BindSingleton<ISystemRegistry, ParallelSystemRegistry>();
   }
   ```

4. **Test:**
   - Verify ISystemRegistry resolves to ParallelSystemRegistry
   - Verify no duplicate registration errors
   - Run EditMode tests

### Phase 2.2: Game Layer Update
**Target:** EventsInstaller → GameLayerModule

1. **Update GameLayerModule.cs**
   ```csharp
   protected override void OnConfigure()
   {
       // Existing: State machine, scene management, game flow

       // ADD: Event System
       _container.BindSingleton<IEventThrottler, EventThrottler>();
       _container.BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>();
       _container.BindSingleton<IEventBatcher, EventBatcher>();
       _container.BindSingleton<IEventStore, EventStore>();

       // Network-aware Event Bus
       var networkMode = _container.Resolve<EventNetworkMode>();
       if (networkMode == EventNetworkMode.Offline)
       {
           _container.BindSingleton<IEventBus, EventBus>();
       }
       else
       {
           _container.BindSingleton<INetworkEventBus, NetworkEventBus>();
           _container.BindInstance<IEventBus>(_container.Resolve<INetworkEventBus>());
       }

       // Timer System (registered in SystemRegistry)
       var systemRegistry = _container.Resolve<ISystemRegistry>();
       _container.BindSingleton<ITimerSystem, TimerSystem>();
       systemRegistry.RegisterSystem(_container.Resolve<ITimerSystem>());

       _container.Bind<EventLogger, EventLogger>();
   }
   ```

2. **Concerns:**
   - Ensure ParallelSystemsInstaller runs BEFORE this (dependency order)
   - Verify EventNetworkMode is in container before module.Configure()

3. **Test:**
   - Verify offline mode uses EventBus
   - Verify multiplayer mode uses NetworkEventBus
   - Verify TimerSystem registered in SystemRegistry

### Phase 2.3: Infrastructure Update
**Target:** LazyDependencyInjectorInstaller + ProfileInstaller → InfrastructureModule

1. **Update InfrastructureModule.cs**
   ```csharp
   protected override void OnConfigure()
   {
       // Existing: Resources, pooling, serialization, UI

       // ADD: Lazy Dependency Injector
       var logger = _container.Resolve<IMythLogger>();
       var lazyDIGameObject = new GameObject("LazyDependencyInjector");
       Object.DontDestroyOnLoad(lazyDIGameObject);
       var lazyDI = lazyDIGameObject.AddComponent<LazyDependencyInjector>();
       lazyDI.Initialize(_container, logger);
       _container.RegisterInstance<IDependencyInjector>(lazyDI);

       // ADD: Profile Presenter
       _container.BindSingleton<IProfilePresenter, ProfilePresenter>();
   }
   ```

2. **Concerns:**
   - GameObject creation in Configure() - ensure Unity context is active
   - DontDestroyOnLoad - verify lifecycle

3. **Test:**
   - Verify IDependencyInjector resolves
   - Verify LazyDependencyInjector GameObject exists
   - Verify scene injection works

### Phase 2.4: GameBootstrapper Cleanup

1. **Remove direct installer calls** (lines 110-120)
   ```csharp
   // REMOVE these lines:
   var lazyInjectorInstaller = new MythHunter.Core.Installers.LazyDependencyInjectorInstaller();
   lazyInjectorInstaller.InstallBindings(_container);

   var eventsInstaller = new MythHunter.Core.Installers.EventsInstaller();
   eventsInstaller.InstallBindings(_container);

   var profileInstaller = new MythHunter.Core.Installers.ProfileInstaller();
   profileInstaller.InstallBindings(_container);

   var parallelSystemsInstaller = new MythHunter.Core.Installers.ParallelSystemsInstaller();
   parallelSystemsInstaller.InstallBindings(_container);
   ```

2. **Module order should handle dependencies automatically via topological sort**

## Validation Plan

### Compilation
1. Full Unity project compile with clean Console
2. No missing dependency errors
3. No duplicate registration errors

### EditMode Tests
1. Run full test suite: target 58/60 passing (maintain Phase 1 baseline)
2. New tests for module initialization order
3. Test offline vs multiplayer mode configuration

### Runtime Validation
1. **Offline Mode:**
   - Verify EventBus (not NetworkEventBus) is used
   - Verify systems initialize correctly
   - Verify scene injection works

2. **Multiplayer Mode:**
   - Verify NetworkEventBus is used
   - Verify Networking + Lobby modules load
   - Verify network-aware events work

3. **Module Lifecycle:**
   - Verify module initialization order (Framework → Game → Infrastructure → Gameplay)
   - Verify graceful shutdown (reverse order)
   - Verify conditional modules load correctly

### Acceptance Criteria

- ✅ All 4 installers migrated to appropriate modules
- ✅ GameBootstrapper has NO direct installer instantiation
- ✅ Compilation clean, no errors
- ✅ EditMode tests: 58/60+ passing (maintain or improve Phase 1 baseline)
- ✅ Offline mode functional
- ✅ Multiplayer mode functional
- ✅ System startup/shutdown works correctly
- ✅ Scene injection works
- ✅ Architecture tests pass

## Files to Change

**New/Updated:**
- `Assets/_Framework/Core/Composition/Modules/FrameworkCoreModule.cs` (add ParallelSystems)
- `Assets/_MythHunter/Code/Core/Composition/Modules/GameLayerModule.cs` (add Events)
- `Assets/_MythHunter/Code/Core/Composition/Modules/InfrastructureModule.cs` (add LazyDI + Profile)
- `Assets/_MythHunter/Code/Core/Game/GameBootstrapper.cs` (remove 4 installer calls)

**Investigate:**
- `Assets/_MythHunter/Code/Core/Installers/CoreServices/CoreInstaller.cs` (check ISystemRegistry registration)

**Consider Removal** (after successful migration):
- `Assets/_MythHunter/Code/Core/Installers/CoreServices/LazyDependencyInjectorInstaller.cs`
- `Assets/_MythHunter/Code/Core/Installers/CoreServices/EventsInstaller.cs`
- `Assets/_MythHunter/Code/Core/Installers/SystemsGroups/ProfileInstaller.cs`
- `Assets/_MythHunter/Code/Core/Installers/CoreServices/ParallelSystemsInstaller.cs`

*(Keep installers until fully validated, then remove in separate cleanup PR)*

## Risks and Mitigation

### Risk 1: ISystemRegistry Duplicate Registration
**Mitigation:** Investigate CoreInstaller first, decide on strategy before implementation

### Risk 2: Module Initialization Order
**Mitigation:** Topological sort should handle, but verify manually and add explicit tests

### Risk 3: Network Mode Switching
**Mitigation:** Test both offline and multiplayer configurations thoroughly

### Risk 4: GameObject Creation in Module.Configure()
**Mitigation:** Ensure Configure() is called from Unity main thread, add logging

## Implementation Checklist

- [ ] PR #58 merged into dev
- [ ] Create Phase 2 branch from dev
- [ ] Investigate CoreInstaller.cs for ISystemRegistry
- [ ] Update FrameworkCoreModule (ParallelSystems)
- [ ] Update GameLayerModule (Events)
- [ ] Update InfrastructureModule (LazyDI + Profile)
- [ ] Remove installer calls from GameBootstrapper
- [ ] Full compilation check
- [ ] Run EditMode tests
- [ ] Test offline mode
- [ ] Test multiplayer mode
- [ ] Test module lifecycle
- [ ] Update architecture tests if needed
- [ ] Create PR for Phase 2
- [ ] Validate at HEAD commit
- [ ] Update STATUS.md and HANDOFF_NEXT_CHAT.md

## Notes

- This plan assumes PR #58 Phase 1 is merged and validated
- Implementation should be done in single atomic PR for Phase 2
- Consider removing old installer files in separate cleanup PR after Phase 2 validates successfully
- Phase 2 completes the "Modular Game Composition" portion of EPIC 07
- Remaining EPIC 07 work: Validate startup/shutdown paths, document game composition patterns
