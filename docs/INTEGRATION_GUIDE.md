# Integration Guide

Guide for integrating RPG Framework into existing Unity projects.

---

## Integration Steps

### 1. Import Framework

Copy the `Assets/_Framework` folder to your Unity project:

```
YourProject/Assets/
├── _Framework/           # ← Copy this folder
│   ├── ECS/
│   ├── Combat/
│   ├── Inventory/
│   └── ... (all modules)
└── YourGameCode/         # Your existing code
```

### 2. Verify Compilation

Open Unity and wait for compilation. Check Console for errors (should be zero).

### 3. Create Game Composition

```csharp
using RPGFramework.Core.Composition;
using RPGFramework.ECS;

public class GameBootstrap : MonoBehaviour
{
    private IGameComposition _composition;
    private IEntityManager _entityManager;

    void Start()
    {
        // Build game composition
        var builder = new GameCompositionBuilder();
        builder.AddModule(new FrameworkCoreModule());
        builder.AddModule(new CombatModule());
        _composition = builder.Build();

        // Get services
        _entityManager = _composition.GetService<IEntityManager>();
    }
}
```

### 4. Create Adapters

Create adapter classes to bridge Framework and Unity:

```csharp
using RPGFramework.Combat;
using UnityEngine;

public class PlayerCombatAdapter : MonoBehaviour
{
    [SerializeField] private int _entityId;
    private IEntityManager _entityManager;
    private IDamageCalculator _damageCalculator;

    public void Initialize(IEntityManager entityManager, IDamageCalculator damageCalculator)
    {
        _entityManager = entityManager;
        _damageCalculator = damageCalculator;

        // Create entity
        _entityId = _entityManager.CreateEntity();
        _entityManager.AddComponent(_entityId, new ResourceComponent
        {
            ResourceType = ResourceType.Health,
            CurrentValue = 100,
            MaxValue = 100
        });
    }

    public void TakeDamage(float damage, DamageType type)
    {
        var result = _damageCalculator.CalculateDamage(damage, type, 10, 10, 0);
        var health = _entityManager.GetComponent<ResourceComponent>(_entityId);
        health.CurrentValue -= result.FinalDamage;
        _entityManager.UpdateComponent(_entityId, health);

        if (health.CurrentValue <= 0)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log("Player died!");
    }
}
```

---

## Integration Patterns

### Pattern 1: MonoBehaviour + Framework Entity

Link Unity GameObjects to Framework entities:

```csharp
public class GameEntity : MonoBehaviour
{
    public int EntityId { get; private set; }
    private IEntityManager _entityManager;

    public void Initialize(IEntityManager entityManager)
    {
        _entityManager = entityManager;
        EntityId = _entityManager.CreateEntity();
    }

    void OnDestroy()
    {
        if (_entityManager != null && _entityManager.EntityExists(EntityId))
        {
            _entityManager.DestroyEntity(EntityId);
        }
    }
}
```

### Pattern 2: ScriptableObject Data Assets

Use ScriptableObjects for game data:

```csharp
[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemDataAsset : ScriptableObject
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public ItemQuality quality;

    public IItem ToFrameworkItem()
    {
        var item = Item.Create(itemId, itemName);
        item.SetType(itemType);
        item.SetQuality(quality);
        return item;
    }
}
```

### Pattern 3: Unity Events + Framework Events

Bridge Framework events to Unity events:

```csharp
using RPGFramework.Inventory;
using UnityEngine.Events;

public class InventoryEventBridge : MonoBehaviour
{
    public UnityEvent<string, int> OnItemAdded;

    public void Initialize(IInventoryContainer inventory)
    {
        // Subscribe to framework events
        // (Framework would need event system integrated)
    }

    private void HandleItemAdded(ItemAddedEvent evt)
    {
        OnItemAdded?.Invoke(evt.ItemId, evt.Quantity);
    }
}
```

---

## Module-Specific Integration

### Combat System

```csharp
public class CombatSystemAdapter : MonoBehaviour
{
    private IEntityManager _entityManager;
    private IDamageCalculator _damageCalculator;
    private ILifeStateManager _lifeStateManager;

    public void Initialize()
    {
        _entityManager = new EntityManager();
        _damageCalculator = new DamageCalculator();
        _lifeStateManager = new LifeStateManager(_entityManager);
    }

    public void SpawnEnemy(Vector3 position)
    {
        int enemyId = _entityManager.CreateEntity();
        _entityManager.AddComponent(enemyId, new ResourceComponent
        {
            ResourceType = ResourceType.Health,
            CurrentValue = 50,
            MaxValue = 50
        });
        _entityManager.AddComponent(enemyId, new LifeStateComponent
        {
            CurrentState = LifeState.Alive
        });

        // Create Unity GameObject for rendering
        var enemyGO = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemyGO.GetComponent<GameEntity>().Initialize(_entityManager);
    }
}
```

### Inventory System

```csharp
public class InventoryUIAdapter : MonoBehaviour
{
    private IInventoryContainer _inventory;
    [SerializeField] private InventorySlotUI[] _slotUIs;

    public void Initialize(IInventoryContainer inventory)
    {
        _inventory = inventory;
        RefreshUI();
    }

    public void AddItem(string itemId, int quantity)
    {
        var itemStack = new ItemStack(itemId, quantity);
        bool added = _inventory.AddItem(itemStack);
        if (added)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        var items = _inventory.GetAllItems().ToList();
        for (int i = 0; i < _slotUIs.Length; i++)
        {
            if (i < items.Count)
            {
                _slotUIs[i].SetItem(items[i].ItemId, items[i].Quantity);
            }
            else
            {
                _slotUIs[i].Clear();
            }
        }
    }
}
```

### Quest System

```csharp
public class QuestUIAdapter : MonoBehaviour
{
    private IQuestManager _questManager;
    [SerializeField] private Text _questTitleText;
    [SerializeField] private Text _objectivesText;

    public void Initialize(IQuestManager questManager)
    {
        _questManager = questManager;
    }

    public void DisplayQuest(string questId)
    {
        var quest = _questManager.GetQuest(questId);
        _questTitleText.text = quest.Title;

        string objectives = "";
        foreach (var objective in quest.Objectives)
        {
            objectives += $"[{(objective.IsComplete ? "X" : " ")}] {objective.Description} ({objective.CurrentProgress}/{objective.RequiredProgress})\n";
        }
        _objectivesText.text = objectives;
    }
}
```

---

## Best Practices

### 1. Centralize Framework Services

```csharp
public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance;

    public IEntityManager EntityManager { get; private set; }
    public IInventoryContainer PlayerInventory { get; private set; }
    public IQuestManager QuestManager { get; private set; }

    void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;

        EntityManager = new EntityManager();
        PlayerInventory = new InventoryContainer(20);
        QuestManager = new QuestManager();
    }
}
```

### 2. Use Dependency Injection

```csharp
public class PlayerController : MonoBehaviour
{
    private IEntityManager _entityManager;
    private IInventoryContainer _inventory;

    [Inject]
    public void Initialize(IEntityManager entityManager, IInventoryContainer inventory)
    {
        _entityManager = entityManager;
        _inventory = inventory;
    }
}
```

### 3. Separation of Concerns

- **Framework:** Pure game logic
- **Adapters:** Bridge Framework to Unity
- **Unity Components:** Rendering, input, physics

### 4. Testing Strategy

Test Framework code independently:

```csharp
[Test]
public void AddItem_ValidItem_ReturnsTrue()
{
    // No Unity dependencies required
    var inventory = new InventoryContainer(10);
    var item = new ItemStack("sword", 1);
    bool result = inventory.AddItem(item);
    Assert.IsTrue(result);
}
```

---

## Common Issues

### Issue 1: Assembly Definition Errors

**Problem:** Unity can't find Framework types

**Solution:** Add assembly reference to your game's .asmdef:

```json
{
    "name": "MyGame.Runtime",
    "references": [
        "RPGFramework.ECS.Runtime",
        "RPGFramework.Combat.Runtime",
        "RPGFramework.Inventory.Runtime"
    ]
}
```

### Issue 2: Null Reference Exceptions

**Problem:** Services not initialized

**Solution:** Initialize in correct order:

```csharp
void Start()
{
    // 1. Create entity manager first
    _entityManager = new EntityManager();

    // 2. Create systems that depend on it
    _combatSystem = new CombatSystem(_entityManager);

    // 3. Initialize entities
    CreatePlayer();
}
```

### Issue 3: Performance Issues

**Problem:** Calling GetComponent every frame

**Solution:** Cache components:

```csharp
private ResourceComponent _cachedHealth;

void Start()
{
    _cachedHealth = _entityManager.GetComponent<ResourceComponent>(_entityId);
}

void Update()
{
    // Use cached component
    _cachedHealth.CurrentValue += regenRate * Time.deltaTime;
    _entityManager.UpdateComponent(_entityId, _cachedHealth);
}
```

---

## Example: Full Integration

Complete example integrating multiple systems:

```csharp
public class GameManager : MonoBehaviour
{
    // Framework services
    private IEntityManager _entityManager;
    private IInventoryContainer _playerInventory;
    private IQuestManager _questManager;
    private ITradingManager _tradingManager;

    // Player entity
    private int _playerId;

    void Start()
    {
        InitializeFramework();
        CreatePlayer();
        LoadGameData();
    }

    private void InitializeFramework()
    {
        _entityManager = new EntityManager();
        _playerInventory = new InventoryContainer(20);
        _questManager = new QuestManager();
        _tradingManager = new TradingManager(new TraderRegistry());
    }

    private void CreatePlayer()
    {
        _playerId = _entityManager.CreateEntity();
        _entityManager.AddComponent(_playerId, new ResourceComponent
        {
            ResourceType = ResourceType.Health,
            CurrentValue = 100,
            MaxValue = 100
        });
        _entityManager.AddComponent(_playerId, new InventoryComponent());

        // Initialize trading
        _tradingManager.InitializePlayerGold(_playerId, 1000);
    }

    private void LoadGameData()
    {
        // Load items
        var itemRegistry = new ItemRegistry();
        foreach (var itemAsset in Resources.LoadAll<ItemDataAsset>("Items"))
        {
            itemRegistry.RegisterItem(itemAsset.ToFrameworkItem());
        }

        // Load quests
        foreach (var questAsset in Resources.LoadAll<QuestDataAsset>("Quests"))
        {
            var quest = questAsset.ToFrameworkQuest();
            _questManager.RegisterQuest(quest);
        }
    }

    public void OnKillEnemy(string enemyType)
    {
        _questManager.UpdateObjective(_playerId, "kill_wolves", 1);
    }

    public void OnTalkToNPC(string npcId)
    {
        var dialogue = GetDialogue(npcId);
        // Display dialogue...
    }
}
```

---

**Version:** 1.0.0
