# EPIC 07 Phase 2 - Complete Architecture Analysis

> Date: 2026-10-12
> Status: Research Complete - Ready for Implementation
> Related Documents:
> - EPIC-07-Phase2-Plan.md (initial plan)
> - EPIC-07-Phase2-Research-Findings.md (registration conflicts)
> - EPIC-07-Phase2-Additional-Findings.md (duplication issues)

## 📊 Executive Summary

**Phase 1 implemented modular composition, but left 4 installers outside the module system.**

Deep research revealed **the current architecture has critical flaws** that work only by accident:
1. **Hidden Overwrites**: Services registered twice, second wins
2. **Resource Duplication**: LazyDependencyInjector created twice
3. **Boundary Violations**: Installers stepping outside their responsibilities
4. **Dead Code**: InstallerRegistry no longer used but still present

**Phase 2 will fix all issues by properly integrating installers into modules.**

---

## 🔴 Problems Discovered

### Category 1: Hidden Registration Overwrites

**Two critical services get registered TWICE with different implementations:**

#### ISystemRegistry Overwrite
```
1. GameLayerModule → CoreInstaller
   └── ISystemRegistry → SystemRegistry

2. Direct Call (after modules)
   └── ParallelSystemsInstaller
       └── ISystemRegistry → ParallelSystemRegistry ← WINS
```

#### IEventBus Overwrite
```
1. GameLayerModule → CoreInstaller
   └── IEventBus → SimpleEventBus

2. Direct Call (after modules)
   └── EventsInstaller
       └── IEventBus → EventBus/NetworkEventBus ← WINS
```

**Impact**: Works by accident (execution order), fragile architecture.

---

### Category 2: Resource Duplication

**LazyDependencyInjector GameObject created TWICE:**

#### First Creation (Unused)
```
GameLayerModule → CoreInstaller → DIExtensionsInstaller
└── Creates GameObject "MythHunter_LazyDependencyInjector"
└── HAS check: if (FindFirstObjectByType == null)
└── DOES NOT register in DI container ❌
```

#### Second Creation (Actually Used)
```
Direct Call → LazyDependencyInjectorInstaller
└── Creates GameObject "LazyDependencyInjector"
└── NO check - always creates
└── REGISTERS in DI as IDependencyInjector ✅
```

**Impact**: Resource waste, confusion, two objects with similar names.

---

### Category 3: God Classes & Boundary Violations

#### CoreInstaller: The God Class

**Mixes Framework + Game concerns:**

| Service | Layer | Used? |
|---------|-------|-------|
| IEntityManager | Framework ✅ | ✅ |
| IEventPool | Framework ✅ | ✅ |
| **IEventBus** | Framework | ❌ Overwritten by EventsInstaller |
| **ISystemRegistry** | Framework | ❌ Overwritten by ParallelSystemsInstaller |
| IEcsWorld | Framework ✅ | ✅ |
| IDILifecycleManager | Framework ✅ | ✅ |
| IGameStateMachine | Game 🎮 | ✅ |
| IPhaseProvider | Game 🎮 | ✅ |
| SceneLoader | Game 🎮 | ✅ |
| ISceneDispatcher | Game 🎮 | ✅ |
| IGameFlowManager | Game 🎮 | ✅ |
| IGameSettingsService | Game 🎮 | ✅ |
| AutoPreloadConfigurator | Game 🎮 | ✅ |

**Plus calls**: DIExtensionsInstaller

**Problems**:
- 13 services in one installer
- Mixes Framework and Game layers
- 2 registrations get overwritten
- Violates Single Responsibility Principle

#### DIExtensionsInstaller: Overstepping Boundaries

**Should handle**: DI framework extensions (validator, scopes)
**Actually does**: Creates infrastructure (LazyDependencyInjector GameObject)

**Problem**: Infrastructure creation belongs in InfrastructureModule, not DI installer.

---

### Category 4: Dead Code

#### InstallerRegistry (UNUSED)

```csharp
// InstallerRegistry.cs - NOT CALLED ANYWHERE
public static void RegisterInstallers(IDIContainer container)
{
    var installers = new DIInstaller[]
    {
        new LazyDependencyInjectorInstaller(),
        new CoreInstaller(),
        new NetworkingInstaller(),
        // ... 16 installers total
    };
    // ...
}
```

**Finding**: `InstallerRegistry.RegisterInstallers()` has **ZERO usages** in codebase.

**Status**: Dead code - replaced by modular composition in Phase 1.

**Action**: Can be safely deleted in Phase 2.

---

## ✅ Solution: Revised Phase 2 Architecture

### Proposed Structure

```
FrameworkCoreModule
└── FrameworkCoreInstaller (NEW)
    ├── IEntityManager → EntityManager
    ├── IEventPool → EventPool
    ├── IEcsWorld → EcsWorld
    ├── IDILifecycleManager → DILifecycleManager
    ├── ISystemJobScheduler → SystemJobScheduler
    └── ISystemRegistry → ParallelSystemRegistry ✅ NO CONFLICT

GameLayerModule
└── GameCoreInstaller (NEW)
    ├── IGameStateMachine → GameStateMachine
    ├── IPhaseProvider → GamePhaseProvider
    ├── SceneLoader, ISceneDispatcher, etc.
    ├── DependencyValidator (from DIExtensionsInstaller)
    ├── GlobalScope creation (from DIExtensionsInstaller)
    └── Event System (from EventsInstaller) ✅ NO CONFLICT
        ├── IEventThrottler, IEventBatcher, IEventStore
        └── IEventBus (network-aware: EventBus or NetworkEventBus)

InfrastructureModule (UPDATED)
├── ResourceInstaller (existing)
├── SerializationInstaller (existing)
├── PoolInstaller (existing)
├── EntitiesInstaller (existing)
├── UIInstaller (existing)
├── LazyDependencyInjector creation (NEW) ✅ NO DUPLICATION
│   └── With defensive check: if (FindFirstObjectByType == null)
└── ProfilePresenter registration (NEW)

GameplayModule (NO CHANGES)
├── GameplaySystemsInstaller
└── HeroSystemsInstaller

NetworkingModule (NO CHANGES)
├── NetworkingInstaller
└── NetworkingSystemsInstaller

LobbyModule (NO CHANGES)
└── LobbySystemsInstaller

DebugToolsModule (NO CHANGES)
├── DebugToolsInstaller
└── DebugSystemsInstaller
```

---

## 📋 Complete Phase 2 Implementation Plan

### Step 1: Create FrameworkCoreInstaller

**Location**: `Assets/_Framework/Core/Installers/FrameworkCoreInstaller.cs`

```csharp
namespace RPGFramework.Core.Installers
{
    public class FrameworkCoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Framework Core", "Installer");

            // Core ECS runtime - universal across all RPG games
            BindSingleton<IEntityManager, EntityManager>(container);
            BindSingleton<IEventPool, EventPool>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);
            BindSingleton<IDILifecycleManager, DILifecycleManager>(container);

            // Parallel system execution (Framework responsibility)
            BindSingleton<ISystemJobScheduler, SystemJobScheduler>(container);
            BindSingleton<ISystemRegistry, ParallelSystemRegistry>(container);

            logger.LogInfo("Framework Core installed", "Installer");
        }
    }
}
```

---

### Step 2: Create GameCoreInstaller

**Location**: `Assets/_MythHunter/Code/Core/Installers/GameCoreInstaller.cs`

```csharp
namespace MythHunter.Core.Installers
{
    public class GameCoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing MythHunter Game Core", "Installer");

            // MythHunter game-specific core services
            BindSingleton<IGameStateMachine, GameStateMachine>(container);
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
            BindSingleton<SceneLoader, SceneLoader>(container);
            BindSingleton<ISceneDispatcher, SceneDispatcher>(container);
            BindSingleton<IGameFlowManager, GameFlowManager>(container);
            BindSingleton<IGameSettingsService, GameSettingsService>(container);
            BindSingleton<AutoPreloadConfigurator, AutoPreloadConfigurator>(container);

            // DI Extensions (validator + scopes)
            RegisterDIExtensions(container, logger);

            // Event System (network-aware)
            RegisterEventSystem(container, logger);

            logger.LogInfo("MythHunter Game Core installed", "Installer");
        }

        private void RegisterDIExtensions(IDIContainer container, IMythLogger logger)
        {
            // Dependency validator
            container.Bind<DependencyValidator, DependencyValidator>();

            // Global scope
            var defaultScope = container.CreateScope("GlobalScope");
            container.SetCurrentScope(defaultScope);

            // Validation in editor
            #if UNITY_EDITOR
            var validator = container.Resolve<DependencyValidator>();
            var issues = validator.ValidateAllDependencies();
            if (issues.Count > 0)
            {
                logger.LogWarning($"Found {issues.Count} dependency issues", "Installer");
            }
            #endif
        }

        private void RegisterEventSystem(IDIContainer container, IMythLogger logger)
        {
            // Event infrastructure
            container.BindSingleton<IEventThrottler, EventThrottler>();
            container.BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>();
            container.BindSingleton<IEventBatcher, EventBatcher>();
            container.BindSingleton<IEventStore, EventStore>();

            // Network-aware event bus selection
            var networkMode = container.Resolve<EventNetworkMode>();
            if (networkMode == EventNetworkMode.Offline)
            {
                logger.LogInfo("Offline mode: using EventBus", "Installer");
                container.BindSingleton<IEventBus, EventBus>();
            }
            else
            {
                logger.LogInfo($"Network mode {networkMode}: using NetworkEventBus", "Installer");
                container.BindSingleton<INetworkEventBus, NetworkEventBus>();
                container.BindInstance<IEventBus>(container.Resolve<INetworkEventBus>());
            }

            // Timer system
            var systemRegistry = container.Resolve<ISystemRegistry>();
            container.BindSingleton<ITimerSystem, TimerSystem>();
            systemRegistry.RegisterSystem(container.Resolve<ITimerSystem>());

            container.Bind<EventLogger, EventLogger>();
        }
    }
}
```

---

### Step 3: Update FrameworkCoreModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;
    if (container == null)
    {
        throw new InvalidOperationException("FrameworkCoreModule requires IDIContainer");
    }

    // Register Framework Core services
    var frameworkInstaller = new RPGFramework.Core.Installers.FrameworkCoreInstaller();
    frameworkInstaller.InstallBindings(container);
}
```

---

### Step 4: Update GameLayerModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;
    if (container == null)
    {
        throw new InvalidOperationException("GameLayerModule requires IDIContainer");
    }

    // Register MythHunter Game Core services
    var gameInstaller = new MythHunter.Core.Installers.GameCoreInstaller();
    gameInstaller.InstallBindings(container);
}
```

---

### Step 5: Update InfrastructureModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;
    if (container == null)
    {
        throw new InvalidOperationException("InfrastructureModule requires IDIContainer");
    }

    // Existing infrastructure services
    var resourceInstaller = new MythHunter.Core.Installers.ResourceInstaller();
    resourceInstaller.InstallBindings(container);

    var serializationInstaller = new MythHunter.Core.Installers.SerializationInstaller();
    serializationInstaller.InstallBindings(container);

    var poolInstaller = new MythHunter.Core.Installers.PoolInstaller();
    poolInstaller.InstallBindings(container);

    var entitiesInstaller = new MythHunter.Core.Installers.EntitiesInstaller();
    entitiesInstaller.InstallBindings(container);

    var uiInstaller = new MythHunter.Core.Installers.UIInstaller();
    uiInstaller.InstallBindings(container);

    // NEW: LazyDependencyInjector (scene injection infrastructure)
    RegisterLazyDependencyInjector(container);

    // NEW: Profile Presenter
    container.BindSingleton<IProfilePresenter, ProfilePresenter>();
}

private void RegisterLazyDependencyInjector(IDIContainer container)
{
    var logger = container.Resolve<IMythLogger>();

    // Defensive check (in case DIExtensionsInstaller was called elsewhere)
    if (Object.FindFirstObjectByType<LazyDependencyInjector>() == null)
    {
        var lazyDIGameObject = new GameObject("LazyDependencyInjector");
        Object.DontDestroyOnLoad(lazyDIGameObject);

        var lazyDI = lazyDIGameObject.AddComponent<LazyDependencyInjector>();
        lazyDI.Initialize(container, logger);

        container.RegisterInstance<IDependencyInjector>(lazyDI);

        logger.LogInfo("LazyDependencyInjector created and registered", "Infrastructure");
    }
    else
    {
        logger.LogWarning("LazyDependencyInjector already exists - skipping creation", "Infrastructure");
    }
}
```

---

### Step 6: Clean Up GameBootstrapper

**Remove lines 110-120:**

```csharp
// DELETE:
// var lazyInjectorInstaller = new MythHunter.Core.Installers.LazyDependencyInjectorInstaller();
// lazyInjectorInstaller.InstallBindings(_container);
//
// var eventsInstaller = new MythHunter.Core.Installers.EventsInstaller();
// eventsInstaller.InstallBindings(_container);
//
// var profileInstaller = new MythHunter.Core.Installers.ProfileInstaller();
// profileInstaller.InstallBindings(_container);
//
// var parallelSystemsInstaller = new MythHunter.Core.Installers.ParallelSystemsInstaller();
// parallelSystemsInstaller.InstallBindings(_container);
```

---

### Step 7: Mark Files Obsolete or Delete

**Mark as [Obsolete] (keep for reference during transition):**
- `CoreInstaller.cs`
- `DIExtensionsInstaller.cs`
- `LazyDependencyInjectorInstaller.cs`
- `EventsInstaller.cs`
- `ParallelSystemsInstaller.cs`
- `ProfileInstaller.cs`

**Delete immediately (dead code):**
- `InstallerRegistry.cs`

---

## 🧪 Validation Strategy

### Compilation Checks
- [ ] Full Unity project compiles with no errors
- [ ] No missing reference errors
- [ ] No duplicate registration warnings

### Unit Tests
- [ ] Test: ISystemRegistry resolves to ParallelSystemRegistry
- [ ] Test: IEventBus resolves correctly (EventBus in offline, NetworkEventBus in multiplayer)
- [ ] Test: IDependencyInjector is registered
- [ ] Test: DependencyValidator is available
- [ ] Test: GlobalScope exists

### Integration Tests
- [ ] Test: Exactly ONE LazyDependencyInjector GameObject in scene
- [ ] Test: LazyDependencyInjector is registered as IDependencyInjector
- [ ] Test: Scene injection works
- [ ] Test: Module initialization order is correct
- [ ] Test: Offline mode configuration loads correctly
- [ ] Test: Multiplayer mode configuration loads correctly

### Manual Validation
- [ ] Check Unity Hierarchy: Should see "LazyDependencyInjector" (not "MythHunter_LazyDependencyInjector")
- [ ] Verify DI container: Inspect registrations
- [ ] Test gameplay in offline mode
- [ ] Test gameplay in multiplayer mode
- [ ] Smoke test: Movement, Visibility, Hero systems

### EditMode Tests
- [ ] Run full suite: Target 58/60+ passing (maintain Phase 1 baseline)
- [ ] ModularCompositionTests: Should still pass
- [ ] No new failures introduced

---

## 📦 Deliverables

### New Files
1. `Assets/_Framework/Core/Installers/FrameworkCoreInstaller.cs`
2. `Assets/_MythHunter/Code/Core/Installers/GameCoreInstaller.cs`

### Modified Files
3. `Assets/_Framework/Core/Composition/Modules/FrameworkCoreModule.cs`
4. `Assets/_MythHunter/Code/Core/Composition/Modules/GameLayerModule.cs`
5. `Assets/_MythHunter/Code/Core/Composition/Modules/InfrastructureModule.cs`
6. `Assets/_MythHunter/Code/Core/Game/GameBootstrapper.cs`

### Obsolete Files (mark with [Obsolete])
7. `Assets/_MythHunter/Code/Core/Installers/CoreServices/CoreInstaller.cs`
8. `Assets/_MythHunter/Code/Core/Installers/CoreServices/DIExtensionsInstaller.cs`
9. `Assets/_MythHunter/Code/Core/Installers/CoreServices/LazyDependencyInjectorInstaller.cs`
10. `Assets/_MythHunter/Code/Core/Installers/CoreServices/EventsInstaller.cs`
11. `Assets/_MythHunter/Code/Core/Installers/CoreServices/ParallelSystemsInstaller.cs`
12. `Assets/_MythHunter/Code/Core/Installers/SystemsGroups/ProfileInstaller.cs`

### Deleted Files
13. `Assets/_MythHunter/Code/Core/Installers/InstallerRegistry.cs` (dead code)

---

## 🎯 Success Criteria

**Phase 2 is complete when:**

- ✅ All installers migrated to appropriate modules
- ✅ No direct installer calls in GameBootstrapper
- ✅ No registration conflicts (verified by tests)
- ✅ No resource duplication (single LazyDependencyInjector)
- ✅ Clear Framework/Game separation
- ✅ Compilation clean, no errors
- ✅ EditMode tests: 58/60+ passing
- ✅ Offline mode works
- ✅ Multiplayer mode works
- ✅ All validation tests pass

---

## 📚 Final Summary

### Problems Found (3 Categories)
1. **Hidden Overwrites**: ISystemRegistry, IEventBus registered twice
2. **Duplication**: LazyDependencyInjector created twice
3. **Boundary Violations**: CoreInstaller God Class, DIExtensionsInstaller overstepping

### Solution Designed
- Split CoreInstaller → FrameworkCoreInstaller + GameCoreInstaller
- Move LazyDI creation to InfrastructureModule (where it belongs)
- Eliminate all direct installer calls from GameBootstrapper
- Delete dead code (InstallerRegistry)

### Benefits
- ✅ Clear Framework/Game separation
- ✅ No registration conflicts
- ✅ No resource duplication
- ✅ Each module owns its responsibilities
- ✅ Testable architecture
- ✅ Maintainable and understandable

**Phase 2 is thoroughly researched and ready for execution after PR #58 merge.** 🚀
