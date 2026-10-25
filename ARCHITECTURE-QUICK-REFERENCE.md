# RPG Framework — Architecture Quick Reference

> **One-page guide for developers** — Keep this open while coding!

---

## 🚦 Core Principles (NEVER VIOLATE)

```
✅ Framework → Framework (OK)
✅ Game → Framework (OK)
❌ Framework → Game (FORBIDDEN!)
```

**Rule #1**: Framework code NEVER depends on game-specific code.
**Rule #2**: Systems use interfaces, not concrete types.
**Rule #3**: Components are data-only structs.

---

## 📁 Folder Structure

```
Assets/
├─ _Framework/              ← UNIVERSAL (any RPG game)
│   ├─ ECS/                 ← Core (Entity, Component, System)
│   ├─ Systems/             ← Reusable systems
│   │   ├─ Movement/
│   │   ├─ Combat/
│   │   └─ Inventory/
│   └─ Editor/              ← Framework tools
│
└─ _MythHunter/             ← GAME-SPECIFIC
    ├─ Code/                ← MythHunter logic
    ├─ Content/             ← Assets, prefabs
    └─ Adapters/            ← Connects Framework to Game
```

**Where do I put my code?**
- **Reusable for ANY RPG?** → `_Framework/`
- **Specific to MythHunter?** → `_MythHunter/`

---

## 🏗️ Creating a New System

### Option 1: Use Wizard (Recommended)
```
Assets → Create → RPG Framework → New System
```
Fills in all the boilerplate correctly!

### Option 2: Manual (Use template)
```csharp
using RPGFramework.ECS;
using Cysharp.Threading.Tasks;

namespace RPGFramework.ECS.Systems.Movement  // ← Framework namespace
{
    [RPGSystem("Movement", "Grid-Based")]
    public class GridMovementSystem : IMovementSystem  // ← Must implement interface
    {
        private readonly IEntityManager _entityManager;
        private readonly ComponentCache<MovementComponent> _cache;

        public GridMovementSystem(IEntityManager entityManager)
        {
            _entityManager = entityManager;
            _cache = new ComponentCache<MovementComponent>(entityManager);
        }

        public void Initialize()
        {
            _cache.Update();
        }

        public void Update(float deltaTime)
        {
            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var (entityId, component) = enumerator.Current;
                // Process entity
            }
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
```

**Checklist**:
- [ ] In `Assets/_Framework/ECS/Systems/`
- [ ] Namespace starts with `RPGFramework`
- [ ] Has `[RPGSystem]` attribute
- [ ] Implements `I*System` interface
- [ ] No references to `MythHunter.*` namespaces

---

## 📦 Creating a Component

```csharp
using RPGFramework.ECS;

namespace RPGFramework.ECS.Components
{
    /// <summary>
    /// Movement data for an entity
    /// </summary>
    public struct MovementComponent : IComponent  // ← STRUCT, not class!
    {
        public float Speed;           // ← Data only
        public Vector3 Velocity;      // ← Data only
        public MovementState State;   // ← Data only

        // ❌ NO METHODS!
        // ❌ NO LOGIC!
        // ❌ NO UnityEngine.Object references (no GameObject, Transform, etc)
    }

    public enum MovementState
    {
        Idle,
        Moving,
        Blocked
    }
}
```

**Checklist**:
- [ ] `struct`, not `class`
- [ ] Implements `IComponent`
- [ ] Only public fields (data)
- [ ] No methods (logic goes in System)
- [ ] No `GameObject`, `Transform`, or other `UnityEngine.Object` refs

---

## 🧪 Testing Architecture Rules

```csharp
using NUnit.Framework;

[TestFixture]
public class MySystemArchitectureTests
{
    [Test]
    public void MySystem_IsInFrameworkNamespace()
    {
        var type = typeof(MySystem);
        Assert.IsTrue(
            type.Namespace.StartsWith("RPGFramework"),
            $"{type.Name} must be in RPGFramework namespace"
        );
    }

    [Test]
    public void MyComponent_IsStruct()
    {
        var type = typeof(MyComponent);
        Assert.IsTrue(
            type.IsValueType,
            $"{type.Name} must be a struct, not a class"
        );
    }
}
```

---

## ⚠️ Common Mistakes

### ❌ Mistake #1: Framework references Game
```csharp
// ❌ BAD: Framework code uses MythHunter type
namespace RPGFramework.ECS.Systems
{
    public class MovementSystem
    {
        private MythHunterPlayer _player;  // VIOLATION!
    }
}

// ✅ GOOD: Use interface
namespace RPGFramework.ECS.Systems
{
    public class MovementSystem
    {
        private IMovable _movable;  // Framework interface
    }
}
```

### ❌ Mistake #2: Component with Logic
```csharp
// ❌ BAD: Component has methods
public struct HealthComponent : IComponent
{
    public int Health;

    public void TakeDamage(int amount) { }  // VIOLATION!
}

// ✅ GOOD: Logic in System
public struct HealthComponent : IComponent
{
    public int CurrentHealth;  // Data only
    public int MaxHealth;      // Data only
}

public class HealthSystem : ISystem
{
    public void ApplyDamage(int entityId, int amount) { }  // Logic here
}
```

### ❌ Mistake #3: Component is Class
```csharp
// ❌ BAD: Component is class
public class PositionComponent : IComponent { }  // VIOLATION!

// ✅ GOOD: Component is struct
public struct PositionComponent : IComponent { }
```

### ❌ Mistake #4: Direct Unity Access
```csharp
// ⚠️ WARNING: Direct Unity dependencies
public class MovementSystem : IMovementSystem
{
    public void Update()
    {
        var player = GameObject.Find("Player");  // Tight coupling!
    }
}

// ✅ GOOD: Work with component data
public class MovementSystem : IMovementSystem
{
    private ComponentCache<PositionComponent> _positions;

    public void Update()
    {
        foreach (var (entityId, pos) in _positions)
        {
            // Work with data, not GameObjects
        }
    }
}
```

---

## 🔍 Validation Tools

### Check Health in Editor
```
Window → RPG Framework → Architecture Health Check
```
Shows all violations in real-time!

### Run Tests
```bash
unity test --mode EditMode --category Architecture
```

### Before Commit
```bash
# Check for violations
dotnet build  # Roslyn analyzers will warn
unity test --architecture-only
```

---

## 📚 Error Codes

| Code | Severity | Description |
|------|----------|-------------|
| **RPGFW001** | ERROR | Framework references Game code |
| **RPGFW002** | ERROR | System doesn't implement interface |
| **RPGFW003** | WARNING | Direct UnityEngine usage |
| **RPGFW004** | ERROR | Component is class, not struct |
| **RPGFW005** | WARNING | Component has methods |

**See**: `EPIC-ARCHITECTURE-ENFORCEMENT.md` for detailed examples

---

## 🆘 Need Help?

1. **Check Architecture Health Window**: Real-time violations
2. **Run Architecture Tests**: `unity test --architecture-only`
3. **Read Full EPIC**: `EPIC-ARCHITECTURE-ENFORCEMENT.md`
4. **Ask Team**: Architecture channel on Discord

---

## ✅ Pre-Commit Checklist

Before committing Framework code:
- [ ] No `using MythHunter.*` in `_Framework/` files
- [ ] All systems have `[RPGSystem]` attribute
- [ ] All systems implement `I*System` interface
- [ ] All components are `struct`, not `class`
- [ ] Components have NO methods
- [ ] Architecture tests pass
- [ ] No compiler warnings from analyzers

---

**Remember**: If it's easier to violate architecture than follow it, **the tooling is wrong, not you**. File an issue!

Last updated: 2026-10-12
