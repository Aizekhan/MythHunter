# RPG Framework Architecture

This document describes the architectural design, patterns, and principles of the RPG Framework.

---

## Table of Contents

1. [Overview](#overview)
2. [Core Principles](#core-principles)
3. [Architectural Layers](#architectural-layers)
4. [Design Patterns](#design-patterns)
5. [Module Structure](#module-structure)
6. [Data Flow](#data-flow)
7. [Performance Considerations](#performance-considerations)

---

## Overview

The RPG Framework is built on a modular, layered architecture that separates concerns and promotes reusability. The framework is designed to be:

- **Engine-Agnostic**: Core framework has zero Unity dependencies
- **Modular**: Each system is independent and can be used standalone
- **Extensible**: Interface-driven design allows easy customization
- **Testable**: Pure .NET code enables comprehensive unit testing
- **Performant**: ECS-based architecture for optimal performance

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    Game Layer                            │
│  (Unity MonoBehaviours, UI, Input, Adapters)            │
│  - MythHunter game-specific logic                       │
│  - Unity-specific implementations                        │
│  - UI controllers and presentation                       │
└────────────────────┬────────────────────────────────────┘
                     │ Depends on
                     ↓
┌─────────────────────────────────────────────────────────┐
│                Framework Layer (Pure .NET 8.0)           │
├─────────────────────────────────────────────────────────┤
│  Domain Modules:                                         │
│  - Combat, Inventory, Quests, Dialogue                  │
│  - Crafting, Trading, Skills, Progression               │
│  - Factions, StatusEffects, Abilities, Loot             │
├─────────────────────────────────────────────────────────┤
│  Core Systems:                                           │
│  - ECS (Entity Component System)                        │
│  - Serialization & DeltaCompression                     │
│  - Logging & Pooling                                    │
└────────────────────┬────────────────────────────────────┘
                     │ Built on
                     ↓
┌─────────────────────────────────────────────────────────┐
│              .NET 8.0 Runtime                            │
│  (Standard Library, Collections, LINQ)                  │
└─────────────────────────────────────────────────────────┘
```

---

## Core Principles

### 1. Entity Component System (ECS)

The framework is built on ECS architecture:

**Entity:**
- Unique identifier (int)
- No behavior, just an ID
- Lightweight and fast to create/destroy

**Component:**
- Pure data structures (structs or classes)
- No methods (except getters/setters)
- Implements `IComponent` marker interface

**System:**
- Contains all game logic
- Operates on entities with specific component combinations
- Stateless when possible

```csharp
// Entity
int entityId = entityManager.CreateEntity();

// Component (data only)
public struct HealthComponent : IComponent
{
    public float CurrentHealth;
    public float MaxHealth;
}

// System (logic)
public class HealthRegenerationSystem
{
    public void Update(IEntityManager entityManager, float deltaTime)
    {
        foreach (var entityId in entitiesWithHealth)
        {
            var health = entityManager.GetComponent<HealthComponent>(entityId);
            health.CurrentHealth = Math.Min(health.CurrentHealth + 1f * deltaTime, health.MaxHealth);
            entityManager.UpdateComponent(entityId, health);
        }
    }
}
```

### 2. Interface-Driven Design

All major systems expose interfaces:

- Enables dependency injection
- Facilitates unit testing with mocks
- Allows swapping implementations
- Defines clear contracts

```csharp
// Interface defines contract
public interface IInventoryContainer
{
    bool AddItem(IItemStack item);
    IItemStack FindItem(string itemId);
    bool RemoveItem(string itemId, int quantity);
}

// Implementation can vary
public class InventoryContainer : IInventoryContainer
{
    // Implementation details...
}

// Easy to mock for testing
var mockInventory = new Mock<IInventoryContainer>();
mockInventory.Setup(x => x.AddItem(It.IsAny<IItemStack>())).Returns(true);
```

### 3. Framework/Game Separation

**Framework Layer:**
- Pure .NET 8.0 code
- No Unity dependencies (no `UnityEngine` imports)
- No game-specific logic
- Reusable across any RPG game

**Game Layer:**
- Unity-specific code (`MonoBehaviour`, `ScriptableObject`)
- Game-specific rules and policies
- UI and presentation logic
- Adapters that connect Framework to Unity

**Rule:** Framework NEVER depends on Game Layer. Game Layer MAY depend on Framework.

```csharp
// GOOD - Framework code
namespace RPGFramework.Combat
{
    public class DamageCalculator : IDamageCalculator
    {
        public DamageResult CalculateDamage(float baseDamage, DamageType type)
        {
            // Pure calculation logic
        }
    }
}

// BAD - Don't do this in Framework
namespace RPGFramework.Combat
{
    public class DamageCalculator : MonoBehaviour // ❌ No UnityEngine in Framework
    {
        // ...
    }
}

// GOOD - Game Layer adapter
namespace MythHunter.Combat
{
    public class CombatAdapter : MonoBehaviour
    {
        private IDamageCalculator _damageCalculator; // Uses Framework interface
        // ...
    }
}
```

### 4. Modular Design

Each module is self-contained:

- Has its own assembly definition (`.asmdef`)
- Declares dependencies explicitly
- Can be used independently
- Has its own test suite

**Module Dependencies (Examples):**
```
Combat → Stats, ECS
Inventory → Items, ECS
Quests → Dialogue, ECS
Crafting → Items, Inventory, Skills
Trading → Items, Factions
```

### 5. Data-Oriented Design

Prefer data structures over complex class hierarchies:

```csharp
// GOOD - Simple data structure
public struct QuestObjective
{
    public string ObjectiveId;
    public string Description;
    public int CurrentProgress;
    public int RequiredProgress;
}

// AVOID - Over-engineered hierarchy
public abstract class BaseObjective
{
    public abstract class KillObjective : BaseObjective { }
    public abstract class CollectObjective : BaseObjective { }
    // ...
}
```

---

## Architectural Layers

### Layer 1: Core Infrastructure

**Modules:** ECS, Logging, Serialization, Pooling, DeltaCompression

**Purpose:** Foundational systems that all other modules depend on

**Key Components:**
- `IEntityManager` - Entity and component management
- `ILogger` - Flexible logging with severity levels
- `ISerializer` - Versioned serialization
- `IObjectPool<T>` - Object pooling for performance
- `IDeltaSerializer` - State synchronization

### Layer 2: Game Systems

**Modules:** Combat, Stats, Items, Inventory, Loot

**Purpose:** Core RPG gameplay mechanics

**Key Components:**
- `IDamageCalculator` - Damage calculations
- `IStatContainer` - Character stats with modifiers
- `IItemRegistry` - Item definitions database
- `IInventoryContainer` - Inventory management
- `ILootGenerator` - Loot drop generation

### Layer 3: Progression & Content

**Modules:** Progression, Skills, Abilities, StatusEffects

**Purpose:** Character advancement and powers

**Key Components:**
- `IProgressionManager` - Level-up and XP
- `ISkillTree` - Skill progression
- `IAbility` - Active and passive abilities
- `IStatusEffectManager` - Buffs and debuffs

### Layer 4: Social & Economy

**Modules:** Quests, Dialogue, Trading, Crafting, Factions

**Purpose:** Player interaction and economy

**Key Components:**
- `IQuestManager` - Quest tracking
- `IDialogue` - Conversation trees
- `ITradingManager` - NPC trading
- `ICraftingManager` - Crafting system
- `IFactionManager` - Reputation system

### Layer 5: Game Layer (MythHunter)

**Purpose:** Game-specific implementations and Unity integration

**Components:**
- Unity MonoBehaviours
- UI controllers
- Input handlers
- Adapters and bridges
- ScriptableObject data assets

---

## Design Patterns

### 1. Repository Pattern

Used for registries and databases:

```csharp
public interface IItemRegistry
{
    void RegisterItem(IItem item);
    IItem GetItem(string itemId);
    bool TryGetItem(string itemId, out IItem item);
    IEnumerable<IItem> GetAllItems();
}
```

### 2. Factory Pattern

Used for creating complex objects:

```csharp
public static class Quest
{
    public static IQuest Create(string questId, string title, string description = "")
    {
        return new QuestImplementation(questId, title, description);
    }
}

var quest = Quest.Create("quest_001", "Save the Village");
```

### 3. Builder Pattern

Used for configuring objects:

```csharp
var offer = TradeOffer.Create("offer_001", "trader_001");
(offer as TradeOffer)?.AddOfferedItem(new TradeItem("sword", 1));
(offer as TradeOffer)?.SetGoldAmount(-100);
(offer as TradeOffer)?.SetMaxUses(5);
```

### 4. Observer Pattern (Events)

Used for decoupled communication:

```csharp
public class ItemAddedEvent
{
    public int EntityId { get; init; }
    public string ItemId { get; init; }
    public int Quantity { get; init; }
}

// Publish
eventBus.Publish(new ItemAddedEvent { EntityId = 1, ItemId = "sword", Quantity = 1 });

// Subscribe
eventBus.Subscribe<ItemAddedEvent>(OnItemAdded);
```

### 5. Strategy Pattern

Used for swappable algorithms:

```csharp
public interface ILootStrategy
{
    IEnumerable<ILootDrop> GenerateLoot(ILootTable table);
}

public class WeightedRandomLootStrategy : ILootStrategy { }
public class GuaranteedLootStrategy : ILootStrategy { }
```

### 6. Composition Over Inheritance

Prefer component composition:

```csharp
// GOOD - Composition
int warriorId = entityManager.CreateEntity();
entityManager.AddComponent(warriorId, new HealthComponent { ... });
entityManager.AddComponent(warriorId, new AttackComponent { ... });
entityManager.AddComponent(warriorId, new InventoryComponent { ... });

// AVOID - Deep inheritance
public class Character { }
public class Warrior : Character { }
public class MeleeWarrior : Warrior { }
```

---

## Module Structure

Standard module layout:

```
ModuleName/
├── Runtime/
│   ├── Contracts/          # Interfaces (IFoo, IBar)
│   ├── Data/               # Data structures and DTOs
│   ├── Enums/              # Enumerations
│   ├── Events/             # Event types
│   ├── Implementations/    # Concrete classes
│   ├── Components/         # ECS components (if applicable)
│   └── ModuleName.Runtime.asmdef
├── Tests/
│   ├── ModuleNameTests.cs
│   └── ModuleName.Tests.asmdef
└── README.md (optional)
```

### Assembly Definitions

Each module has two assembly definitions:

**Runtime Assembly:**
```json
{
    "name": "RPGFramework.Combat.Runtime",
    "references": [
        "RPGFramework.ECS.Runtime",
        "RPGFramework.Stats.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

**Test Assembly:**
```json
{
    "name": "RPGFramework.Combat.Tests",
    "references": [
        "RPGFramework.Combat.Runtime",
        "RPGFramework.ECS.Runtime",
        "UnityEngine.TestRunner",
        "Moq"
    ],
    "includePlatforms": ["Editor"],
    "optionalUnityReferences": ["TestAssemblies"]
}
```

---

## Data Flow

### 1. Entity Creation Flow

```
User Code
    → IEntityManager.CreateEntity()
    → EntityManager generates new ID
    → Returns entity ID to user
```

### 2. Component Addition Flow

```
User Code
    → IEntityManager.AddComponent(entityId, component)
    → EntityManager stores component
    → EntityManager indexes by type
    → Fires ComponentAddedEvent (if event system enabled)
```

### 3. Quest Completion Flow

```
Game Logic
    → IQuest.UpdateObjectiveProgress()
    → Quest checks if objective complete
    → If all objectives complete, quest state = Completed
    → Fires QuestCompletedEvent
    → Event handler awards rewards
    → Updates player inventory/XP
```

### 4. Trading Flow

```
Player initiates trade
    → ITradingManager.ProposeTradeOffer()
    → Manager validates conditions
    → If valid, fires TradeProposedEvent
    → Player accepts
    → ITradingManager.AcceptTrade()
    → Manager transfers items and gold
    → Updates reputation
    → Fires TradeCompletedEvent
```

---

## Performance Considerations

### 1. Component Storage

Current implementation:
- `Dictionary<int, Dictionary<Type, IComponent>>` for entity→components
- `Dictionary<Type, HashSet<int>>` for component→entities

Future optimization:
- Archetype-based storage (entities with same components stored together)
- Cache-friendly memory layout
- SIMD-friendly data structures

### 2. Query Performance

```csharp
// SLOW - Iterating all entities
foreach (int entityId in entityManager.GetAllEntities())
{
    if (entityManager.HasComponent<HealthComponent>(entityId))
    {
        // Process...
    }
}

// FAST - Query specific component type
foreach (int entityId in entityManager.GetEntitiesWithComponent<HealthComponent>())
{
    // Process...
}
```

### 3. Object Pooling

Use pooling for frequently created/destroyed objects:

```csharp
public class BulletPool
{
    private IObjectPool<Bullet> _pool;

    public Bullet SpawnBullet()
    {
        return _pool.Get();
    }

    public void ReturnBullet(Bullet bullet)
    {
        _pool.Return(bullet);
    }
}
```

### 4. Struct vs Class

**Use structs for:**
- Small data (< 16 bytes)
- Immutable data
- Components with simple data

**Use classes for:**
- Large data
- Mutable state
- Objects with identity
- Polymorphic behavior

### 5. Avoid Allocations in Hot Paths

```csharp
// BAD - Creates garbage
public List<IItem> GetItems()
{
    return new List<IItem>(_items); // Allocates every call
}

// GOOD - Returns existing collection
public IReadOnlyList<IItem> GetItems()
{
    return _items; // No allocation
}
```

---

## Testing Strategy

### 1. Unit Tests

Each module has comprehensive unit tests:

```csharp
[Test]
public void AddItem_ValidItem_ReturnsTrue()
{
    // Arrange
    var inventory = new InventoryContainer(maxSlots: 10);
    var item = new ItemStack("sword", 1);

    // Act
    bool result = inventory.AddItem(item);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(1, inventory.GetItemCount());
}
```

### 2. Integration Tests

Test interactions between modules:

```csharp
[Test]
public void CompleteQuest_AwardsItemsToInventory()
{
    // Arrange
    var questManager = new QuestManager();
    var inventory = new InventoryContainer(20);
    var quest = CreateQuestWithItemReward("sword");

    // Act
    quest.CompleteQuest();
    inventory.AddItem(new ItemStack("sword", 1));

    // Assert
    Assert.IsTrue(inventory.HasItem("sword"));
}
```

### 3. Mocking

Use Moq for interface mocking:

```csharp
var mockRegistry = new Mock<IItemRegistry>();
mockRegistry.Setup(x => x.GetItem("sword")).Returns(new Item("sword", "Iron Sword"));

var manager = new InventoryManager(mockRegistry.Object);
```

---

## Extension Points

### 1. Custom Components

Add new component types:

```csharp
public struct CustomComponent : IComponent
{
    public string CustomData;
    public int CustomValue;
}

entityManager.AddComponent(entityId, new CustomComponent { ... });
```

### 2. Custom Systems

Create new systems:

```csharp
public class CustomSystem
{
    public void Update(IEntityManager entityManager, float deltaTime)
    {
        foreach (var entityId in entityManager.GetEntitiesWithComponent<CustomComponent>())
        {
            var component = entityManager.GetComponent<CustomComponent>(entityId);
            // Custom logic...
        }
    }
}
```

### 3. Custom Modules

Create new framework modules following the standard structure.

---

## Future Enhancements

### Planned Improvements

1. **Archetype Storage** - Optimize component storage for cache efficiency
2. **Job System Integration** - Parallelize system updates
3. **Event Sourcing** - Record all state changes for replay/debugging
4. **Network Synchronization** - Built-in multiplayer support
5. **Visual Scripting** - Node-based quest/dialogue editor
6. **Hot Reloading** - Change code without restarting game

---

## References

- [ECS Architecture](https://en.wikipedia.org/wiki/Entity_component_system)
- [Data-Oriented Design](https://www.dataorienteddesign.com/dodbook/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Version:** 1.0.0
**Last Updated:** October 2026
