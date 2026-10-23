# Best Practices

Coding standards, patterns, and performance tips for RPG Framework.

---

## Code Organization

### 1. Follow Framework/Game Layer Separation

**DO:**
```csharp
// Framework - Pure .NET
namespace RPGFramework.Combat
{
    public class DamageCalculator : IDamageCalculator { }
}

// Game Layer - Unity integration
namespace MythHunter.Combat
{
    public class CombatAdapter : MonoBehaviour { }
}
```

**DON'T:**
```csharp
// Framework with Unity dependency ❌
namespace RPGFramework.Combat
{
    public class DamageCalculator : MonoBehaviour { } // ❌
}
```

### 2. Use Interfaces for Dependencies

**DO:**
```csharp
public class QuestManager
{
    private readonly IEntityManager _entityManager;
    private readonly IItemRegistry _itemRegistry;

    public QuestManager(IEntityManager entityManager, IItemRegistry itemRegistry)
    {
        _entityManager = entityManager;
        _itemRegistry = itemRegistry;
    }
}
```

**DON'T:**
```csharp
public class QuestManager
{
    private EntityManager _entityManager; // Concrete type ❌
    private ItemRegistry _itemRegistry; // Concrete type ❌
}
```

### 3. Prefer Composition Over Inheritance

**DO:**
```csharp
int warriorId = entityManager.CreateEntity();
entityManager.AddComponent(warriorId, new HealthComponent());
entityManager.AddComponent(warriorId, new AttackComponent());
entityManager.AddComponent(warriorId, new InventoryComponent());
```

**DON'T:**
```csharp
public class Character { }
public class Warrior : Character { }
public class MeleeWarrior : Warrior { } // ❌ Deep hierarchy
```

---

## Performance Best Practices

### 1. Cache Component Queries

**DO:**
```csharp
private List<int> _healthEntities;

void CacheEntities()
{
    _healthEntities = entityManager
        .GetEntitiesWithComponent<HealthComponent>()
        .ToList();
}
```

**DON'T:**
```csharp
void Update()
{
    foreach (var id in entityManager.GetEntitiesWithComponent<HealthComponent>()) // ❌ Query every frame
    {
        // ...
    }
}
```

### 2. Use Structs for Components

**DO:**
```csharp
public struct HealthComponent : IComponent // Struct for simple data
{
    public float CurrentHealth;
    public float MaxHealth;
}
```

**DON'T:**
```csharp
public class HealthComponent : IComponent // ❌ Class for simple data
{
    public float CurrentHealth;
    public float MaxHealth;
}
```

### 3. Avoid Allocations in Hot Paths

**DO:**
```csharp
public IReadOnlyList<IItem> GetItems()
{
    return _items; // No allocation
}
```

**DON'T:**
```csharp
public List<IItem> GetItems()
{
    return new List<IItem>(_items); // ❌ Allocates every call
}
```

### 4. Use Object Pooling

**DO:**
```csharp
private IObjectPool<Bullet> _bulletPool;

public Bullet SpawnBullet()
{
    return _bulletPool.Get(); // From pool
}

public void DestroyBullet(Bullet bullet)
{
    _bulletPool.Return(bullet); // Return to pool
}
```

---

## Testing Best Practices

### 1. Write Unit Tests for Framework Code

```csharp
[Test]
public void AddItem_ValidItem_ReturnsTrue()
{
    // Arrange
    var inventory = new InventoryContainer(10);
    var item = new ItemStack("sword", 1);

    // Act
    bool result = inventory.AddItem(item);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(1, inventory.GetItemCount());
}
```

### 2. Use Mocks for Dependencies

```csharp
[Test]
public void CompleteQuest_AwardsItems()
{
    // Arrange
    var mockInventory = new Mock<IInventoryContainer>();
    var questManager = new QuestManager(mockInventory.Object);

    // Act
    questManager.CompleteQuest("quest_001");

    // Assert
    mockInventory.Verify(x => x.AddItem(It.IsAny<IItemStack>()), Times.Once);
}
```

### 3. Test Edge Cases

```csharp
[Test]
public void AddItem_FullInventory_ReturnsFalse()
{
    var inventory = new InventoryContainer(maxSlots: 1);
    inventory.AddItem(new ItemStack("item1", 1));

    bool result = inventory.AddItem(new ItemStack("item2", 1));

    Assert.IsFalse(result);
}
```

---

## API Design Best Practices

### 1. Return Clear Success/Failure

**DO:**
```csharp
public bool AddItem(IItemStack item)
{
    if (IsFull) return false;
    _items.Add(item);
    return true;
}
```

**DON'T:**
```csharp
public void AddItem(IItemStack item)
{
    _items.Add(item); // No indication of success ❌
}
```

### 2. Use Result Objects for Complex Operations

```csharp
public class TradeResultData
{
    public bool Success { get; init; }
    public string FailureReason { get; init; }
    public int GoldChange { get; init; }
    public IEnumerable<TradeItem> ItemsReceived { get; init; }
}

public TradeResultData AcceptTrade(int playerId, string offerId)
{
    // Return detailed result
}
```

### 3. Validate Input

```csharp
public void AddComponent<T>(int entityId, T component) where T : IComponent
{
    if (!EntityExists(entityId))
        throw new ArgumentException($"Entity {entityId} does not exist");

    if (component == null)
        throw new ArgumentNullException(nameof(component));

    // Add component...
}
```

---

## Naming Conventions

### Interfaces
```csharp
public interface IEntityManager { }
public interface IInventoryContainer { }
```

### Classes
```csharp
public class EntityManager : IEntityManager { }
public class InventoryContainer : IInventoryContainer { }
```

### Events
```csharp
public class ItemAddedEvent { }
public class QuestCompletedEvent { }
```

### Components
```csharp
public struct HealthComponent : IComponent { }
public struct InventoryComponent : IComponent { }
```

### Methods
```csharp
public void AddItem(IItemStack item) { } // Verb + noun
public bool HasItem(string itemId) { } // Question format for booleans
public IItem GetItem(string itemId) { } // Get for retrieval
```

---

## Documentation Best Practices

### 1. XML Documentation

```csharp
/// <summary>
/// Adds an item to the inventory.
/// </summary>
/// <param name="item">The item stack to add</param>
/// <returns>True if item was added successfully, false if inventory is full</returns>
public bool AddItem(IItemStack item)
{
    // Implementation...
}
```

### 2. Code Comments

```csharp
// Only add comments for non-obvious logic
public float CalculateDamage(float baseDamage, float armor)
{
    // Armor reduces damage by a percentage: 100 armor = 50% reduction
    // Formula: damage * (1 - armor / (armor + 100))
    return baseDamage * (1 - armor / (armor + 100));
}
```

---

## Error Handling

### 1. Use Exceptions for Exceptional Cases

```csharp
public void DestroyEntity(int entityId)
{
    if (!EntityExists(entityId))
        throw new InvalidOperationException($"Entity {entityId} does not exist");

    _entities.Remove(entityId);
}
```

### 2. Return Failure for Expected Cases

```csharp
public bool AddItem(IItemStack item)
{
    if (IsFull) return false; // Expected scenario
    _items.Add(item);
    return true;
}
```

### 3. Log Warnings

```csharp
public void UpdateObjective(string questId, string objectiveId, int progress)
{
    if (!_quests.ContainsKey(questId))
    {
        _logger.Log($"Quest {questId} not found", LogSeverity.Warning);
        return;
    }
    // Update...
}
```

---

## Versioning and Migration

### 1. Version Data Structures

```csharp
[SerializableType("PlayerData", 2)]
public class PlayerData : IVersionedData
{
    public int Version => 2;
    public string Name;
    public int Level;
    public int Experience; // Added in version 2
}
```

### 2. Provide Migrators

```csharp
public class PlayerDataMigrator : IMigrator
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public object Migrate(object oldData)
    {
        var v1 = (PlayerDataV1)oldData;
        return new PlayerData
        {
            Name = v1.Name,
            Level = v1.Level,
            Experience = 0 // Default for new field
        };
    }
}
```

---

## Common Patterns

### Repository Pattern

```csharp
public interface IItemRegistry
{
    void RegisterItem(IItem item);
    IItem GetItem(string itemId);
    bool TryGetItem(string itemId, out IItem item);
}
```

### Factory Pattern

```csharp
public static class Quest
{
    public static IQuest Create(string questId, string title)
    {
        return new QuestImplementation(questId, title);
    }
}
```

### Builder Pattern

```csharp
var quest = Quest.Create("quest_001", "Title");
quest.AddObjective(objective1);
quest.AddObjective(objective2);
quest.SetRewards(500, 100, new[] { "sword" });
```

---

## Checklist for New Modules

- [ ] Pure .NET 8.0 (no Unity dependencies in Runtime)
- [ ] All public types have interfaces
- [ ] All interfaces documented with XML comments
- [ ] Unit tests for all public methods
- [ ] Assembly definition (.asmdef) configured
- [ ] Dependencies declared explicitly
- [ ] Follows naming conventions
- [ ] Performance considerations addressed
- [ ] Error handling implemented
- [ ] Example usage provided

---

**Version:** 1.0.0
