# EPIC: Architecture Enforcement System

**Status**: PLANNED
**Priority**: HIGH (Foundation for all future development)
**Dependencies**: EPIC 01 (ECS Core), EPIC 05 (System Lifecycle)

---

## 🎯 Mission

Create a **multi-layered architecture enforcement system** that ensures all Framework code (present and future) strictly adheres to architectural principles:

1. **Framework Core** must NOT depend on game-specific code
2. **Systems** must implement proper interfaces and follow conventions
3. **Components** must be data-only structs
4. **Assembly boundaries** must be respected
5. **Namespaces** must follow structure conventions

---

## 🏗️ Architecture Layers

### Layer 1: Compile-Time Enforcement (STRONGEST)
**Goal**: Make it **impossible** to compile code that violates architecture

#### 1.1 Assembly Definitions
- [ ] Create `RPGFramework.ECS.asmdef` (Core)
- [ ] Create `RPGFramework.ECS.Systems.*.asmdef` (per system type)
- [ ] Create `MythHunter.GameLogic.asmdef` (Game Layer)
- [ ] Configure **allowed references only**
- [ ] Block Framework → Game dependencies
- [ ] Allow Game → Framework dependencies

**Location**: `Assets/_Framework/**/asmdef`

**Success Criteria**:
- Unity refuses to compile if Framework references Game code
- Clear error message points to violation

#### 1.2 Roslyn Code Analyzers
- [ ] Create `RPGFramework.Analyzers` project
- [ ] Implement **RPGFW001**: Framework-Game dependency check
- [ ] Implement **RPGFW002**: System interface validation
- [ ] Implement **RPGFW003**: Unity type usage check
- [ ] Implement **RPGFW004**: Component struct validation
- [ ] Implement **RPGFW005**: Component method check (data-only)
- [ ] Package as NuGet for IDE integration

**Location**: `Assets/_Framework/Analyzers/`

**Success Criteria**:
- IDE (Rider/Visual Studio) shows errors in real-time
- Build fails on CI if violations detected
- Clear fix suggestions provided

---

### Layer 2: Runtime Validation
**Goal**: Catch violations during system registration and initialization

#### 2.1 System Registry Validation
- [ ] Create `SystemArchitectureValidator`
- [ ] Check [RPGSystem] attribute presence
- [ ] Validate interface implementation
- [ ] Check namespace conventions
- [ ] Detect circular dependencies
- [ ] Block registration if invalid

**Location**: `Assets/_Framework/ECS/SystemRegistry.cs`

**Success Criteria**:
- Invalid systems cannot be registered
- Exception thrown with detailed violation report
- Editor shows Architecture Violation Window

#### 2.2 Component Validation
- [ ] Validate components are structs (not classes)
- [ ] Check components have no methods
- [ ] Verify IComponent implementation
- [ ] Validate field types (no UnityEngine.Object references)

**Location**: `Assets/_Framework/ECS/ComponentValidator.cs`

---

### Layer 3: Automated Testing
**Goal**: Continuous architecture validation in CI/CD

#### 3.1 Architecture Tests
- [ ] `FrameworkCore_DoesNotReference_GameLayer`
- [ ] `AllSystems_ImplementISystemInterface`
- [ ] `AllComponents_AreStructs`
- [ ] `FrameworkSystems_OnlyInFrameworkNamespace`
- [ ] `NoDependencyCycles`
- [ ] `ComponentsAreDataOnly`

**Location**: `Assets/_Framework/Tests/ArchitectureTests.cs`

#### 3.2 CI Integration
- [ ] GitHub Actions workflow: `architecture-validation.yml`
- [ ] Run architecture tests on every PR
- [ ] Block merge if violations found
- [ ] Generate violation report

**Location**: `.github/workflows/architecture-validation.yml`

---

### Layer 4: Editor Tooling
**Goal**: Make it **easier to do right** than to do wrong

#### 4.1 Architecture Health Window
- [ ] Create `Window → RPG Framework → Architecture Health`
- [ ] Show real-time violations
- [ ] Display dependency graph
- [ ] Show metrics (coupling, cohesion)
- [ ] One-click fix suggestions

**Location**: `Assets/_Framework/Editor/ArchitectureHealthWindow.cs`

#### 4.2 System Creation Wizard
- [ ] `Assets → Create → RPG Framework → New System`
- [ ] Template-based code generation
- [ ] Automatic validation
- [ ] Proper location enforcement
- [ ] Interface implementation scaffolding

**Location**: `Assets/_Framework/Editor/SystemTemplateCreator.cs`

#### 4.3 Visual Studio Code Templates
- [ ] Snippet: `rpgsystem` → full system template
- [ ] Snippet: `rpgcomponent` → component template
- [ ] Snippet: `rpgtest` → test template

**Location**: `Assets/_Framework/Editor/Templates/`

#### 4.4 Custom Inspector Warnings
- [ ] Visual warning if system in wrong folder
- [ ] Visual warning if component is class
- [ ] "Move to correct location" button
- [ ] "Convert to struct" refactoring

**Location**: `Assets/_Framework/Editor/ArchitectureEnforcementEditor.cs`

---

### Layer 5: Documentation
**Goal**: Developers understand **why** and **how**

#### 5.1 Architecture Guidelines
- [ ] `ARCHITECTURE.md` — principles and rationale
- [ ] `SYSTEM-GUIDELINES.md` — how to create systems
- [ ] `COMPONENT-GUIDELINES.md` — component best practices
- [ ] `TESTING-GUIDELINES.md` — architecture testing

**Location**: `Docs/Architecture/`

#### 5.2 Examples
- [ ] Example: Properly structured system
- [ ] Example: Violation and fix
- [ ] Example: Custom system implementation
- [ ] Example: Testing architecture rules

**Location**: `Docs/Examples/`

---

## 📋 Validation Rules Reference

### RPGFW001: Framework-Game Dependency
**Rule**: Framework Core cannot reference Game Layer code
**Severity**: ERROR
**Fix**: Move code to Game Layer or extract to Framework interface

```csharp
// ❌ BAD: Framework references MythHunter
namespace RPGFramework.ECS.Systems.Movement
{
    public class MovementSystem
    {
        private MythHunterPlayer _player; // VIOLATION!
    }
}

// ✅ GOOD: Framework uses interface
namespace RPGFramework.ECS.Systems.Movement
{
    public class MovementSystem
    {
        private IMovable _movable; // Framework interface
    }
}
```

### RPGFW002: System Interface Implementation
**Rule**: Types with [RPGSystem] must implement ISystem
**Severity**: ERROR
**Fix**: Add `ISystem` interface or remove attribute

```csharp
// ❌ BAD: Attribute without interface
[RPGSystem("Movement", "Grid")]
public class GridMovement { } // VIOLATION!

// ✅ GOOD: Proper implementation
[RPGSystem("Movement", "Grid")]
public class GridMovement : IMovementSystem { }
```

### RPGFW003: Unity Type Usage
**Rule**: Systems should minimize direct UnityEngine usage
**Severity**: WARNING
**Fix**: Use abstractions or component data

```csharp
// ⚠️ WARNING: Direct Unity dependency
public class MovementSystem : IMovementSystem
{
    public void Update()
    {
        GameObject.Find("Player"); // WARNING: Direct Unity access
    }
}

// ✅ GOOD: Use component data
public class MovementSystem : IMovementSystem
{
    private ComponentCache<PositionComponent> _positions;

    public void Update()
    {
        // Work with component data, not GameObjects
    }
}
```

### RPGFW004: Component Struct Validation
**Rule**: Components must be structs, not classes
**Severity**: ERROR
**Fix**: Change `class` to `struct`

```csharp
// ❌ BAD: Component is class
public class HealthComponent : IComponent { } // VIOLATION!

// ✅ GOOD: Component is struct
public struct HealthComponent : IComponent { }
```

### RPGFW005: Component Method Check
**Rule**: Components should be data-only (no methods)
**Severity**: WARNING
**Fix**: Move logic to System

```csharp
// ⚠️ WARNING: Component has logic
public struct HealthComponent : IComponent
{
    public int CurrentHealth;
    public int MaxHealth;

    public void TakeDamage(int amount) { } // WARNING!
}

// ✅ GOOD: Logic in System
public struct HealthComponent : IComponent
{
    public int CurrentHealth;
    public int MaxHealth;
    // Data only
}

public class HealthSystem : ISystem
{
    public void ApplyDamage(int entityId, int amount)
    {
        // Logic here
    }
}
```

---

## 🎯 Success Metrics

### Immediate (EPIC completion)
- [ ] All 5 layers implemented
- [ ] 100% of existing code passes validation
- [ ] CI blocks invalid PRs
- [ ] Editor tools functional

### Ongoing (per sprint)
- [ ] Zero architecture violations in new code
- [ ] All systems created via wizard (not manual)
- [ ] Architecture tests pass on every commit
- [ ] No Framework → Game dependencies

### Long-term (framework maturity)
- [ ] External developers can't violate architecture
- [ ] Clear feedback on violations
- [ ] Auto-fix for common issues
- [ ] Documentation referenced regularly

---

## 📦 Deliverables

### Code
1. **Assembly Definitions** (all modules)
2. **Roslyn Analyzers** (NuGet package)
3. **Architecture Tests** (100% coverage of rules)
4. **Editor Tools** (Health Window, Wizard, Templates)
5. **Runtime Validators** (SystemRegistry, ComponentValidator)

### Documentation
1. **ARCHITECTURE.md** — principles and design
2. **System Guidelines** — how to create systems
3. **Component Guidelines** — component best practices
4. **Testing Guidelines** — architecture testing
5. **Violation Reference** — all rules with examples

### Infrastructure
1. **CI Workflow** — architecture validation on PR
2. **Pre-commit Hook** (optional) — local validation
3. **IDE Templates** — Visual Studio/Rider snippets

---

## 🚀 Implementation Plan

### Phase 1: Foundation (Week 1)
- Assembly Definitions for Framework/Game separation
- Basic Architecture Tests
- ARCHITECTURE.md documentation

### Phase 2: Compile-Time Enforcement (Week 2)
- Roslyn Analyzers (RPGFW001-005)
- NuGet packaging
- IDE integration

### Phase 3: Runtime & Editor Tools (Week 3)
- SystemRegistry validation
- Architecture Health Window
- System Creation Wizard

### Phase 4: CI/CD Integration (Week 4)
- GitHub Actions workflow
- PR blocking on violations
- Automated reporting

### Phase 5: Documentation & Polish (Week 5)
- Complete all guidelines
- Examples for each rule
- Video walkthrough
- External developer testing

---

## 🔗 Related EPICs

- **EPIC 01**: ECS Core (provides base for validation)
- **EPIC 05**: System Lifecycle (systems to validate)
- **EPIC 06**: Movement Systems (first consumer of enforcement)
- **EPIC 07**: Combat Systems (validates multi-system architecture)

---

## ✅ Definition of Done

- [ ] All 5 enforcement layers implemented
- [ ] 100% of Framework code passes all rules
- [ ] CI blocks PRs with violations
- [ ] Editor tools create valid code by default
- [ ] Documentation complete with examples
- [ ] External developer successfully creates system without violations
- [ ] Zero false positives in analyzers
- [ ] Performance impact < 1% (runtime validation)

---

**Owner**: Architecture Team
**Reviewers**: All Framework contributors
**Timeline**: 5 weeks
**Priority**: Must complete before EPIC 06 (Movement Systems)
