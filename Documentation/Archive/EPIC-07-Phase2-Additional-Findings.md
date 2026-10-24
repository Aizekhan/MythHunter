# EPIC 07 Phase 2 - Additional Research Findings

> Date: 2026-10-12 (continued investigation)
> Related: EPIC-07-Phase2-Research-Findings.md

## 🔴 Additional Discovery: Duplicate LazyDependencyInjector

### Problem

**TWO different installers create LazyDependencyInjector GameObject:**

#### 1. DIExtensionsInstaller (called from CoreInstaller)
```csharp
// Line 28-35 in DIExtensionsInstaller.cs
if (Object.FindFirstObjectByType<LazyDependencyInjector>() == null)
{
    var injectorObject = new GameObject("MythHunter_LazyDependencyInjector");
    var injector = injectorObject.AddComponent<LazyDependencyInjector>();
    Object.DontDestroyOnLoad(injectorObject);
    logger.LogInfo("Created LazyDependencyInjector", "Installer");
}
```

**Key Details:**
- ✅ Has check: `if (FindFirstObjectByType == null)`
- 📛 GameObject name: `"MythHunter_LazyDependencyInjector"`
- ❌ Does NOT register in DI container as `IDependencyInjector`

#### 2. LazyDependencyInjectorInstaller (called directly from GameBootstrapper)
```csharp
// Lines 20-30 in LazyDependencyInjectorInstaller.cs
var lazyDIGameObject = new GameObject("LazyDependencyInjector");
Object.DontDestroyOnLoad(lazyDIGameObject);

var lazyDI = lazyDIGameObject.AddComponent<LazyDependencyInjector>();
lazyDI.Initialize(container, logger);

container.RegisterInstance<IDependencyInjector>(lazyDI);
```

**Key Details:**
- ❌ NO check - always creates
- 📛 GameObject name: `"LazyDependencyInjector"` (different!)
- ✅ Registers in DI container as `IDependencyInjector`

### Current Behavior (By Accident)

```
Execution Order:
1. GameLayerModule.Configure()
   └── CoreInstaller
       └── DIExtensionsInstaller
           └── Creates "MythHunter_LazyDependencyInjector" ← UNUSED!

2. LazyDependencyInjectorInstaller (after modules)
   └── Creates "LazyDependencyInjector" ← THIS ONE IS USED
   └── Registers as IDependencyInjector
```

**Result**: Two GameObject instances with LazyDependencyInjector component!
- First one is created but never used (no DI registration)
- Second one is created AND registered in DI

### Why This Is a Problem

1. **Resource Waste**: Creates unnecessary GameObject that's never used
2. **Confusion**: Two objects with similar names doing the same thing
3. **Fragile**: DIExtensionsInstaller has FindFirstObjectByType check, but runs BEFORE LazyDependencyInjectorInstaller
4. **Hidden Duplication**: No error, no warning - just silent waste

### Root Cause

**DIExtensionsInstaller is trying to do too much:**
- Creates global DI scope
- Creates dependency validator
- **Tries to create LazyDependencyInjector** (infrastructure concern)

**LazyDependencyInjectorInstaller duplicates the work:**
- Creates LazyDependencyInjector again
- Registers it in DI container (critical step that DIExtensionsInstaller missed)

### Impact on Phase 2 Strategy

This discovery **validates** our Phase 2 approach:

#### Original DIExtensionsInstaller Responsibilities
1. ✅ DependencyValidator registration → **Keep in GameCoreInstaller**
2. ✅ GlobalScope creation → **Keep in GameCoreInstaller** (or FrameworkCoreInstaller?)
3. ❌ LazyDependencyInjector creation → **REMOVE** (LazyDependencyInjectorInstaller handles this)

#### Revised Phase 2 Plan Update

**GameCoreInstaller should:**
```csharp
public override void InstallBindings(IDIContainer container)
{
    // ... existing Game services ...

    // DI Extensions (WITHOUT LazyDependencyInjector creation)
    container.Bind<DependencyValidator, DependencyValidator>();
    var defaultScope = container.CreateScope("GlobalScope");
    container.SetCurrentScope(defaultScope);

    // Validation in editor
    #if UNITY_EDITOR
    var validator = container.Resolve<DependencyValidator>();
    var issues = validator.ValidateAllDependencies();
    if (issues.Count > 0)
    {
        var logger = container.Resolve<IMythLogger>();
        logger.LogWarning($"Found {issues.Count} dependency issues", "Installer");
    }
    #endif

    // Event System (network-aware)
    RegisterEventSystem(container);
}
```

**InfrastructureModule should:**
```csharp
protected override void OnConfigure()
{
    // ... existing infrastructure services ...

    // LazyDependencyInjector (scene injection infrastructure)
    var logger = _container.Resolve<IMythLogger>();

    // Check if already exists (defensive)
    if (Object.FindFirstObjectByType<LazyDependencyInjector>() == null)
    {
        var lazyDIGameObject = new GameObject("LazyDependencyInjector");
        Object.DontDestroyOnLoad(lazyDIGameObject);

        var lazyDI = lazyDIGameObject.AddComponent<LazyDependencyInjector>();
        lazyDI.Initialize(_container, logger);

        _container.RegisterInstance<IDependencyInjector>(lazyDI);

        logger.LogInfo("LazyDependencyInjector created and registered", "Infrastructure");
    }

    // Profile Presenter
    _container.BindSingleton<IProfilePresenter, ProfilePresenter>();
}
```

## 📊 Updated Installer Responsibility Map

### DIExtensionsInstaller (Current - To be eliminated)
- ❌ DependencyValidator → Move to GameCoreInstaller
- ❌ GlobalScope creation → Move to GameCoreInstaller
- ❌ LazyDependencyInjector → **DELETE** (duplicate!)
- **Status**: Should be ELIMINATED or heavily reduced

### LazyDependencyInjectorInstaller (Current - To be integrated)
- ✅ LazyDependencyInjector creation → Move to InfrastructureModule
- ✅ IDependencyInjector registration → Move to InfrastructureModule
- **Status**: Logic moves to InfrastructureModule, file can be deleted

### Proposed: GameCoreInstaller (New)
- ✅ Game services (StateMachine, SceneLoader, etc.)
- ✅ DependencyValidator
- ✅ GlobalScope creation
- ✅ Event System (network-aware)
- **Status**: Consolidates Game Layer concerns

### Proposed: InfrastructureModule (Updated)
- ✅ Resources, Pooling, Serialization, UI (existing)
- ✅ LazyDependencyInjector (NEW - properly!)
- ✅ ProfilePresenter (NEW)
- **Status**: Handles infrastructure including scene injection

## 🔑 Key Learnings

1. **Hidden Duplication is Insidious**
   - Different GameObject names hide the duplication
   - FindFirstObjectByType check runs too early to help
   - Only the second instance actually gets used (via DI registration)

2. **Installers Can Overstep Boundaries**
   - DIExtensionsInstaller tries to create infrastructure (LazyDI)
   - Should only handle DI framework extensions (validator, scopes)
   - Infrastructure creation belongs in InfrastructureModule

3. **Execution Order Masks Problems**
   - Works now because of lucky ordering
   - Phase 2 must be careful to maintain correct order
   - Or better: eliminate the duplication entirely

4. **DI Registration is Critical**
   - Creating a GameObject isn't enough
   - Must register in DI container to be useful
   - DIExtensionsInstaller forgot this step!

## 📝 Revised Phase 2 Checklist

### Step 1: Create FrameworkCoreInstaller
- [x] Already planned in previous findings
- No changes needed

### Step 2: Create GameCoreInstaller (UPDATED)
- [ ] Register Game services (StateMachine, SceneLoader, etc.)
- [ ] Register DependencyValidator (from DIExtensionsInstaller)
- [ ] Create GlobalScope (from DIExtensionsInstaller)
- [ ] Register Event System (network-aware, from EventsInstaller)
- [ ] **SKIP** LazyDependencyInjector creation (handled by Infrastructure)

### Step 3: Update FrameworkCoreModule
- [x] Already planned
- No changes needed

### Step 4: Update GameLayerModule
- [ ] Use GameCoreInstaller
- [ ] **DO NOT** call DIExtensionsInstaller separately

### Step 5: Update InfrastructureModule (UPDATED)
- [ ] Keep existing: Resources, Pooling, Serialization, UI
- [ ] **ADD**: LazyDependencyInjector creation + registration
- [ ] **ADD**: ProfilePresenter registration
- [ ] **ADD**: Defensive check for existing LazyDependencyInjector

### Step 6: Remove Direct Installer Calls
- [ ] Remove LazyDependencyInjectorInstaller call
- [ ] Remove EventsInstaller call (logic moved to GameCoreInstaller)
- [ ] Remove ProfileInstaller call
- [ ] Remove ParallelSystemsInstaller call (logic moved to FrameworkCoreInstaller)

### Step 7: Mark Obsolete
- [ ] Mark CoreInstaller as [Obsolete]
- [ ] Mark DIExtensionsInstaller as [Obsolete] (or delete if no other usage)
- [ ] Mark LazyDependencyInjectorInstaller as [Obsolete]
- [ ] Mark EventsInstaller as [Obsolete]
- [ ] Mark ParallelSystemsInstaller as [Obsolete]
- [ ] Mark ProfileInstaller as [Obsolete]

## 🧪 Testing Strategy

### Unit Tests
- [ ] Test: FrameworkCoreInstaller registers correct services
- [ ] Test: GameCoreInstaller registers correct services
- [ ] Test: GlobalScope is created
- [ ] Test: DependencyValidator is available

### Integration Tests
- [ ] Test: Only ONE LazyDependencyInjector GameObject exists
- [ ] Test: LazyDependencyInjector is registered as IDependencyInjector
- [ ] Test: Scene injection works
- [ ] Test: Offline mode uses EventBus
- [ ] Test: Multiplayer mode uses NetworkEventBus
- [ ] Test: ISystemRegistry resolves to ParallelSystemRegistry

### Manual Validation
- [ ] Check Hierarchy: Should see only "LazyDependencyInjector" (not "MythHunter_LazyDependencyInjector")
- [ ] Check DI container: Verify IDependencyInjector is registered
- [ ] Test scene injection in play mode
- [ ] Test network mode switching

## 📄 Files to Investigate

### Already Analyzed
- ✅ CoreInstaller.cs - Mixes Framework + Game, registers services that get overwritten
- ✅ DIExtensionsInstaller.cs - Creates duplicate LazyDependencyInjector
- ✅ LazyDependencyInjectorInstaller.cs - Creates LazyDependencyInjector (the one actually used)
- ✅ EventsInstaller.cs - Overwrites IEventBus from CoreInstaller
- ✅ ParallelSystemsInstaller.cs - Overwrites ISystemRegistry from CoreInstaller
- ✅ ProfileInstaller.cs - Simple, just registers ProfilePresenter

### May Need Review
- [ ] InstallerRegistry.cs - How is it used? Can it be eliminated?
- [ ] Other system installers referenced by modules (ResourceInstaller, etc.)

## 🎯 Summary

**Three Categories of Problems Found:**

1. **Overwrites** (from previous findings)
   - ISystemRegistry: SystemRegistry → ParallelSystemRegistry
   - IEventBus: SimpleEventBus → EventBus/NetworkEventBus

2. **Duplication** (new finding)
   - LazyDependencyInjector created TWICE by different installers
   - First one unused, second one registered in DI

3. **Boundary Violations** (new finding)
   - DIExtensionsInstaller creates infrastructure (LazyDI)
   - Should only handle DI framework concerns
   - Infrastructure belongs in InfrastructureModule

**All three will be resolved by Phase 2 plan.**

## 📚 References

- Previous findings: `EPIC-07-Phase2-Research-Findings.md`
- DIExtensionsInstaller: `Assets/_MythHunter/Code/Core/Installers/CoreServices/DIExtensionsInstaller.cs:28-35`
- LazyDependencyInjectorInstaller: `Assets/_MythHunter/Code/Core/Installers/CoreServices/LazyDependencyInjectorInstaller.cs:20-30`
