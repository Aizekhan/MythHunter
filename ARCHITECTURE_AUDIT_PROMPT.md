# COMPREHENSIVE ARCHITECTURE AUDIT PROMPT

## Context
You are auditing the **MythHunter RPG Framework** project to ensure architectural consistency, proper separation of concerns, and compilation readiness. The project follows a strict **Framework-Game separation** architecture.

---

## Architecture Principles (MUST ENFORCE)

### 1. Framework Core Independence
**Rule:** Framework Core (`Assets/_Framework`) MUST NOT depend on game-specific code (`Assets/_MythHunter`).

**Verification:**
- [ ] Search all Framework files for `using MythHunter` - MUST find ZERO matches
- [ ] Check Framework .asmdef files - MUST NOT reference MythHunter assemblies
- [ ] Verify Framework interfaces/implementations use only Framework namespaces

**Command:**
```bash
grep -r "using MythHunter" Assets/_Framework/
# Expected: No matches found
```

---

### 2. System Architecture Patterns

**Framework systems follow TWO patterns:**

#### Pattern A: Manager-Based Systems
These systems have dedicated Manager classes that coordinate operations:
- ✅ Crafting → `ICraftingManager`, `CraftingManager`
- ✅ Dialogue → `IDialogueManager`, `DialogueManager`
- ✅ Quest → `IQuestManager`, `QuestManager`
- ✅ StatusEffect → `IStatusEffectManager`, `StatusEffectManager`
- ✅ Trading → `ITradingManager`, `TradingManager`
- ✅ Faction → `IFactionManager`, `FactionManager`
- ✅ Companion → `ICompanionManager`, `CompanionManager`
- ✅ Guild → `IGuildManager`, `GuildManager`
- ✅ Achievement → `IAchievementManager`, `AchievementManager`

#### Pattern B: ECS Component-Based Systems
These systems work through direct ECS component access (NO Manager):
- ✅ Inventory → Uses `IInventory` interface directly
- ✅ Abilities → Uses `IAbilitySystem` interface directly
- ✅ Stats → Uses `IStatsSystem` interface directly
- ✅ Progression → Uses `IProgressionSystem` interface directly
- ✅ Loot → Uses `ILootSystem` interface directly

**Verification:**
- [ ] Confirm Pattern A systems have Manager implementations
- [ ] Confirm Pattern B systems do NOT have Manager layer
- [ ] UI files use correct interfaces (NOT Facades)

**Anti-Pattern (FORBIDDEN):**
```csharp
// ❌ DO NOT CREATE - These do not exist:
IInventoryManager
IAbilityManager
IStatsManager
IProgressionManager
ILootManager
```

---

### 3. Namespace Organization

**Framework Namespace Structure:**
```
RPGFramework.{SystemName}                    // Root namespace for system
RPGFramework.{SystemName}.Runtime            // Runtime data/enums
RPGFramework.{SystemName}.Runtime.Contracts  // Interfaces
RPGFramework.{SystemName}.Runtime.Data       // Data structures
RPGFramework.{SystemName}.Runtime.Events     // Event definitions
RPGFramework.{SystemName}.Runtime.Implementations // Concrete classes
RPGFramework.{SystemName}.Tests              // Unit tests
```

**Game Namespace Structure:**
```
MythHunter.GameLogic.{System}                // Game-specific logic
MythHunter.GameLogic.{System}.Adapters       // Framework integration
MythHunter.GameLogic.{System}.Components     // Game components
MythHunter.GameLogic.{System}.Data           // Game metadata
MythHunter.UI.Panels.{PanelName}             // UI systems
```

**Verification:**
- [ ] All Framework files use `RPGFramework.*` namespace
- [ ] All MythHunter files use `MythHunter.*` namespace
- [ ] Integration adapters are in MythHunter layer (NOT Framework)

---

### 4. UI System Integration

**Correct Pattern:**
```csharp
// ✅ CORRECT - UI uses Framework interfaces directly
using RPGFramework.Crafting;
using RPGFramework.Dialogue;
using RPGFramework.Inventory;

public class CraftingUI : MonoBehaviour
{
    private ICraftingSystem craftingSystem;  // Framework interface

    public void Initialize(ICraftingSystem system)
    {
        craftingSystem = system;
    }
}
```

**Incorrect Pattern (DO NOT USE):**
```csharp
// ❌ WRONG - Facades are NOT needed
using MythHunter.Systems.Facades;

public class CraftingUI : MonoBehaviour
{
    private ICraftingSystemFacade facade;  // ❌ No Facades!
}
```

**Verification:**
- [ ] Check all UI files in `Assets/_MythHunter/Code/UI/Panels/`
- [ ] Ensure they import Framework namespaces (e.g., `RPGFramework.Crafting`)
- [ ] Ensure they use Framework interfaces (e.g., `ICraftingSystem`, `IDialogueSystem`)
- [ ] Ensure NO Facade classes exist in codebase

**Command:**
```bash
grep -r "Facade" Assets/_MythHunter/Code/
# Expected: No matches (Facades removed)
```

---

### 5. ECS Integration

**Framework ECS:**
```csharp
namespace RPGFramework.ECS
{
    public interface IEntityManager
    {
        int CreateEntity();
        void AddComponent<T>(int entityId, T component) where T : IComponent;
        T GetComponent<T>(int entityId) where T : IComponent;
        bool HasComponent<T>(int entityId) where T : IComponent;
    }

    public interface IComponent { }  // Marker interface
}
```

**Usage Pattern:**
- Components are **structs** implementing `IComponent`
- Managers receive `IEntityManager` as constructor dependency
- NO `IComponentContainer` interface exists

**Verification:**
- [ ] All ECS components implement `IComponent`
- [ ] Components are declared as `struct` (not class)
- [ ] Managers use `IEntityManager` (NOT `IComponentContainer`)

**Examples:**
```csharp
// ✅ CORRECT
public struct GuildMemberData : IComponent { ... }

public class GuildManager
{
    private readonly IEntityManager _entityManager;

    public GuildManager(IEntityManager entityManager, ...) { }
}
```

---

### 6. Compilation Verification

**Steps to verify compilation:**

1. **Check Unity Editor.log for errors:**
```bash
tail -500 "C:\Users\Admin\AppData\Local\Unity\Editor\Editor.log" | grep "error CS"
```

2. **Check for specific error patterns:**
```bash
# Missing using statements
grep "error CS0246" Editor.log

# Namespace conflicts
grep "error CS0234" Editor.log

# Interface implementation mismatches
grep "error CS0738" Editor.log
```

3. **Verify assembly references:**
- [ ] Framework assemblies do NOT reference MythHunter assemblies
- [ ] MythHunter assemblies CAN reference Framework assemblies
- [ ] Editor assemblies properly reference RPGFramework.Editor

4. **Check for circular dependencies:**
```bash
find Assets/_Framework -name "*.asmdef" -exec cat {} \;
# Verify no circular references
```

---

### 7. Common Issues and Fixes

#### Issue 1: Missing using statements
**Symptom:** `error CS0246: The type or namespace name 'X' could not be found`

**Fix:** Add appropriate using directive:
```csharp
using RPGFramework.Achievements.Runtime;           // For data types
using RPGFramework.Achievements.Runtime.Contracts; // For interfaces
```

#### Issue 2: Struct vs Class confusion
**Symptom:** `error CS0019: Operator '==' cannot be applied to operands of type 'X' and '<null>'`

**Fix:** Structs cannot be null - use proper equality:
```csharp
// ❌ WRONG for structs
if (questData == null) { }

// ✅ CORRECT for structs
if (string.IsNullOrEmpty(questData.Id)) { }
```

#### Issue 3: ScriptableObject vs Runtime Data
**Symptom:** Editor windows trying to load structs as ScriptableObjects

**Fix:** Disable buggy Editor windows with `#if FALSE` until refactored:
```csharp
#if FALSE
public class BuggyEditorWindow : EditorWindow { ... }
#endif
```

#### Issue 4: Wrong Manager constructor signature
**Symptom:** `error CS1503: Argument cannot convert from 'X' to 'Y'`

**Fix:** Verify Manager constructor matches Integration usage:
```csharp
// Check how Integration creates Manager
var manager = new GuildManager(entityManager, factionRegistry, guildRegistry);

// Ensure Manager constructor matches
public GuildManager(IEntityManager entityManager,
                   IFactionRegistry factionRegistry,
                   IGuildRegistry guildRegistry) { }
```

---

### 8. Audit Checklist

#### Framework Layer (`Assets/_Framework`)
- [ ] No references to `MythHunter` namespace
- [ ] All systems follow Pattern A (Manager) or Pattern B (ECS-direct)
- [ ] Proper namespace organization (Runtime, Contracts, Implementations, etc.)
- [ ] All interfaces defined in Contracts folders
- [ ] All implementations in Implementations folders
- [ ] Unit tests in Tests folders
- [ ] .asmdef files properly configured

#### Game Layer (`Assets/_MythHunter`)
- [ ] Integration adapters in `GameLogic/{System}/Adapters`
- [ ] Game-specific components in `GameLogic/{System}/Components`
- [ ] Metadata in `GameLogic/{System}/Data`
- [ ] UI panels use Framework interfaces directly
- [ ] NO Facade classes exist
- [ ] Proper using statements for Framework namespaces

#### Editor Layer (`Assets/_Framework/Editor`)
- [ ] Custom editors for Framework ScriptableObjects
- [ ] Editor windows disabled if incompatible with struct-based data
- [ ] Property drawers for Framework data types
- [ ] Menu items reference only working Editor windows
- [ ] No compilation errors in Editor assembly

#### Compilation
- [ ] Run `tail -500 Editor.log | grep "error CS"` → Zero errors
- [ ] All .asmdef dependencies correct
- [ ] No circular assembly references
- [ ] Framework compiles independently of MythHunter

---

## Audit Execution Process

### Step 1: Static Analysis
```bash
# Check Framework isolation
grep -r "using MythHunter" Assets/_Framework/
grep -r "MythHunter" Assets/_Framework/**/*.asmdef

# Check for Facade anti-pattern
grep -r "Facade" Assets/_MythHunter/

# Check namespace consistency
find Assets/_Framework -name "*.cs" -exec head -20 {} \; | grep "namespace" | sort | uniq

# Check for IComponentContainer (does not exist)
grep -r "IComponentContainer" Assets/_Framework/
```

### Step 2: Compilation Check
```bash
# Check Unity Editor log for errors
tail -1000 "C:\Users\Admin\AppData\Local\Unity\Editor\Editor.log" | grep "error CS"

# Expected output: Empty (no errors)
```

### Step 3: Architecture Verification
```bash
# Verify Manager pattern usage
find Assets/_Framework -name "*Manager.cs" -path "*/Implementations/*"

# Verify ECS component usage
grep -r "struct.*: IComponent" Assets/_Framework/

# Verify UI uses Framework interfaces
grep -r "using RPGFramework" Assets/_MythHunter/Code/UI/Panels/
```

### Step 4: Integration Layer Check
```bash
# Verify Integration adapters exist for each system
ls -la Assets/_MythHunter/Code/GameLogic/*/Adapters/

# Check Integration constructors match Framework
grep -A5 "new.*Manager(" Assets/_MythHunter/Code/GameLogic/*/Adapters/*.cs
```

---

## Expected Outcome

After running this audit, you should have:

1. ✅ **Zero compilation errors** in Unity Editor.log
2. ✅ **Framework independence** - no MythHunter references in Framework code
3. ✅ **Correct system patterns** - Managers where appropriate, direct ECS where appropriate
4. ✅ **Clean UI integration** - UI uses Framework interfaces, no Facades
5. ✅ **Proper namespaces** - all files in correct namespace hierarchy
6. ✅ **Valid ECS usage** - structs with IComponent, IEntityManager dependencies
7. ✅ **Working Integration** - Adapters correctly instantiate Framework systems

---

## Summary Commands

Run these commands to get a quick audit overview:

```bash
# 1. Check Framework isolation
echo "=== Framework Isolation Check ==="
grep -r "using MythHunter" Assets/_Framework/ && echo "❌ FAILED" || echo "✅ PASSED"

# 2. Check for Facades (should not exist)
echo "=== Facade Anti-Pattern Check ==="
grep -r "Facade" Assets/_MythHunter/ && echo "❌ FAILED" || echo "✅ PASSED"

# 3. Check compilation errors
echo "=== Compilation Errors Check ==="
tail -1000 "C:\Users\Admin\AppData\Local\Unity\Editor\Editor.log" | grep "error CS" && echo "❌ FAILED" || echo "✅ PASSED"

# 4. Check IComponentContainer usage (should not exist)
echo "=== IComponentContainer Check (should not exist) ==="
grep -r "IComponentContainer" Assets/_Framework/ && echo "❌ FAILED" || echo "✅ PASSED"

# 5. List all Manager implementations
echo "=== Manager Implementations ==="
find Assets/_Framework -name "*Manager.cs" -path "*/Implementations/*"

# 6. List all ECS Components
echo "=== ECS Components ==="
grep -r "struct.*: IComponent" Assets/_Framework/ | cut -d: -f1 | sort | uniq

# 7. Verify UI uses Framework interfaces
echo "=== UI Framework Integration ==="
grep -h "using RPGFramework" Assets/_MythHunter/Code/UI/Panels/**/*.cs | sort | uniq
```

---

## How to Use This Prompt

**For Claude Code:**

1. Read this entire document
2. Execute all verification commands in order
3. For each ❌ FAILED check, investigate and fix
4. Document all findings and fixes
5. Re-run audit until all checks ✅ PASS
6. Commit fixes with clear architecture-focused commit messages

**For Manual Review:**

1. Use this as a checklist when reviewing PRs
2. Verify architectural principles are maintained
3. Ensure new code follows established patterns
4. Check that Framework/Game separation is preserved

---

## Version
**Document Version:** 1.0
**Last Updated:** 2026-10-24
**Project:** MythHunter RPG Framework
**Commit Reference:** 836b8c7
