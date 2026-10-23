# RPG Framework API Reference

Complete API documentation for all public interfaces and classes.

---

## Core Interfaces

### IEntityManager

Entity and component management.

```csharp
public interface IEntityManager
{
    // Entity Management
    int CreateEntity();
    void DestroyEntity(int entityId);
    bool EntityExists(int entityId);
    IEnumerable<int> GetAllEntities();

    // Component Management
    void AddComponent<T>(int entityId, T component) where T : IComponent;
    void RemoveComponent<T>(int entityId) where T : IComponent;
    T GetComponent<T>(int entityId) where T : IComponent;
    bool HasComponent<T>(int entityId) where T : IComponent;
    void UpdateComponent<T>(int entityId, T component) where T : IComponent;

    // Queries
    IEnumerable<int> GetEntitiesWithComponent<T>() where T : IComponent;
    IEnumerable<int> GetEntitiesWithComponents(params Type[] componentTypes);
}
```

**Example:**
```csharp
var manager = new EntityManager();
int playerId = manager.CreateEntity();
manager.AddComponent(playerId, new HealthComponent { CurrentHealth = 100 });
var health = manager.GetComponent<HealthComponent>(playerId);
```

---

### IInventoryContainer

Inventory management operations.

```csharp
public interface IInventoryContainer
{
    int MaxSlots { get; }
    int UsedSlots { get; }
    bool IsFull { get; }

    bool AddItem(IItemStack item);
    bool RemoveItem(string itemId, int quantity);
    IItemStack FindItem(string itemId);
    bool HasItem(string itemId, int quantity);
    IEnumerable<IItemStack> GetAllItems();
    void Clear();
    int GetItemCount();
}
```

**Example:**
```csharp
var inventory = new InventoryContainer(maxSlots: 20);
var sword = new ItemStack("sword_iron", 1);
bool added = inventory.AddItem(sword);
```

---

### IQuest

Quest tracking and progression.

```csharp
public interface IQuest
{
    string QuestId { get; }
    string Title { get; }
    string Description { get; }
    QuestState State { get; }

    void AddObjective(QuestObjective objective);
    void UpdateObjectiveProgress(string objectiveId, int progress);
    bool IsObjectiveComplete(string objectiveId);
    bool AreAllObjectivesComplete();

    void SetRewards(int experienceReward, int goldReward, string[] itemRewards);
    void StartQuest();
    void CompleteQuest();
    void FailQuest();
    void AbandonQuest();
}
```

**Example:**
```csharp
var quest = Quest.Create("quest_001", "Save Village");
quest.AddObjective(new QuestObjective
{
    ObjectiveId = "kill_wolves",
    RequiredProgress = 5
});
quest.StartQuest();
quest.UpdateObjectiveProgress("kill_wolves", 1);
```

---

### IDialogue

Dialogue tree management.

```csharp
public interface IDialogue
{
    string DialogueId { get; }
    string Title { get; }

    void AddNode(IDialogueNode node);
    IDialogueNode GetNode(string nodeId);
    void SetRootNode(string nodeId);
    IDialogueNode GetRootNode();
    IEnumerable<IDialogueResponse> GetAvailableResponses(string nodeId);
}
```

---

### ICraftingRecipe

Crafting recipe definition.

```csharp
public interface ICraftingRecipe
{
    string RecipeId { get; }
    string RecipeName { get; }
    string ResultItemId { get; }
    int ResultQuantity { get; }

    IReadOnlyList<ICraftingIngredient> Ingredients { get; }
    int RequiredSkillLevel { get; }
    float CraftingTime { get; }

    void AddIngredient(ICraftingIngredient ingredient);
    bool CanCraft(IInventoryContainer inventory, int playerSkillLevel);
}
```

---

### ITradingManager

Trading operations.

```csharp
public interface ITradingManager
{
    void InitializePlayerGold(int playerEntityId, int goldAmount);
    int GetPlayerGold(int playerEntityId);

    bool ProposeTradeOffer(int playerEntityId, string offerId);
    TradeResultData AcceptTrade(int playerEntityId, string offerId);
    void DeclineTrade(int playerEntityId, string offerId);

    int GetPlayerReputation(int playerEntityId, string traderId);
    void AddReputation(int playerEntityId, string traderId, int amount);
}
```

---

### IStatContainer

Stat management with modifiers.

```csharp
public interface IStatContainer
{
    void SetBaseStat(string statName, float value);
    float GetBaseStat(string statName);
    float GetFinalStat(string statName);

    void AddModifier(string statName, IStatModifier modifier);
    void RemoveModifier(string statName, string modifierId);
    void ClearModifiers(string statName);
    IEnumerable<IStatModifier> GetModifiers(string statName);
}
```

---

### IStatusEffectManager

Status effect application and management.

```csharp
public interface IStatusEffectManager
{
    void ApplyEffect(int targetEntityId, IStatusEffect effect);
    void RemoveEffect(int targetEntityId, string effectId);
    void RemoveAllEffects(int targetEntityId);

    bool HasEffect(int targetEntityId, string effectId);
    IEnumerable<IStatusEffect> GetActiveEffects(int targetEntityId);

    void Update(float deltaTime);
}
```

---

### IFactionManager

Faction reputation management.

```csharp
public interface IFactionManager
{
    void RegisterFaction(IFaction faction);
    IFaction GetFaction(string factionId);

    void InitializeReputation(int playerEntityId, string factionId, int startingReputation);
    int GetReputation(int playerEntityId, string factionId);
    void AddReputation(int playerEntityId, string factionId, int amount);

    FactionStanding GetFactionStanding(int playerEntityId, string factionId);
    void SetFactionRelationship(string factionId1, string factionId2, FactionRelationType relationType);
}
```

---

## Data Structures

### ResourceComponent

```csharp
public struct ResourceComponent : IComponent
{
    public ResourceType ResourceType; // Health, Mana, Stamina, etc.
    public float CurrentValue;
    public float MaxValue;
    public float RegenerationRate; // Per second
}
```

### ItemStack

```csharp
public class ItemStack : IItemStack
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public int Quality { get; set; } // 0-100
}
```

### QuestObjective

```csharp
public struct QuestObjective
{
    public string ObjectiveId;
    public string Description;
    public int CurrentProgress;
    public int RequiredProgress;
    public bool IsComplete => CurrentProgress >= RequiredProgress;
}
```

### DialogueNode

```csharp
public class DialogueNode : IDialogueNode
{
    public string NodeId { get; }
    public string SpeakerId { get; }
    public string Text { get; }
    public List<IDialogueResponse> Responses { get; }
}
```

### TradeItem

```csharp
public struct TradeItem
{
    public string ItemId;
    public int Quantity;
    public int Quality; // 0-100
}
```

### StatModifier

```csharp
public class StatModifier : IStatModifier
{
    public string ModifierId { get; set; }
    public float Value { get; set; }
    public StatModifierType Type { get; set; } // Flat, Percentage, Override
    public string Source { get; set; }
}
```

---

## Enumerations

### QuestState
```csharp
public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed,
    Abandoned
}
```

### DamageType
```csharp
public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Holy,
    Shadow,
    True // Ignores resistances
}
```

### ItemType
```csharp
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    QuestItem,
    Material,
    Currency,
    Miscellaneous
}
```

### ItemQuality
```csharp
public enum ItemQuality
{
    Poor = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5
}
```

### TraderType
```csharp
public enum TraderType
{
    Merchant,
    Blacksmith,
    Alchemist,
    FenceDealer,
    QuestGiver
}
```

### FactionStanding
```csharp
public enum FactionStanding
{
    Hated = 0,      // < -6000
    Hostile = 1,    // -6000 to -3000
    Unfriendly = 2, // -3000 to 0
    Neutral = 3,    // 0 to 3000
    Friendly = 4,   // 3000 to 6000
    Honored = 5,    // 6000 to 12000
    Revered = 6,    // 12000 to 21000
    Exalted = 7     // > 21000
}
```

---

## Event Types

### Combat Events

```csharp
public class DamageDealtEvent
{
    public int AttackerEntityId { get; init; }
    public int TargetEntityId { get; init; }
    public float Damage { get; init; }
    public DamageType DamageType { get; init; }
    public bool IsCritical { get; init; }
}

public class DeathEvent
{
    public int EntityId { get; init; }
    public int KillerEntityId { get; init; }
}
```

### Inventory Events

```csharp
public class ItemAddedEvent
{
    public int EntityId { get; init; }
    public string ItemId { get; init; }
    public int Quantity { get; init; }
}

public class ItemRemovedEvent
{
    public int EntityId { get; init; }
    public string ItemId { get; init; }
    public int Quantity { get; init; }
}

public class ItemEquippedEvent
{
    public int EntityId { get; init; }
    public string ItemId { get; init; }
    public string SlotType { get; init; }
}
```

### Quest Events

```csharp
public class QuestStartedEvent
{
    public int PlayerEntityId { get; init; }
    public string QuestId { get; init; }
}

public class QuestCompletedEvent
{
    public int PlayerEntityId { get; init; }
    public string QuestId { get; init; }
    public int ExperienceReward { get; init; }
    public int GoldReward { get; init; }
}

public class ObjectiveUpdatedEvent
{
    public int PlayerEntityId { get; init; }
    public string QuestId { get; init; }
    public string ObjectiveId { get; init; }
    public int Progress { get; init; }
}
```

### Trading Events

```csharp
public class TradeProposedEvent
{
    public int PlayerEntityId { get; init; }
    public string TraderId { get; init; }
    public string OfferId { get; init; }
}

public class TradeCompletedEvent
{
    public int PlayerEntityId { get; init; }
    public string TraderId { get; init; }
    public string OfferId { get; init; }
    public int GoldChange { get; init; }
    public int ReputationGain { get; init; }
}
```

---

## Factory Methods

### Creating Entities with Components

```csharp
// Create player entity
int playerId = entityManager.CreateEntity();
entityManager.AddComponent(playerId, new ResourceComponent
{
    ResourceType = ResourceType.Health,
    CurrentValue = 100,
    MaxValue = 100
});
entityManager.AddComponent(playerId, new InventoryComponent());
```

### Creating Quests

```csharp
var quest = Quest.Create("quest_id", "Quest Title", "Description");
quest.AddObjective(new QuestObjective { ObjectiveId = "obj_1", RequiredProgress = 5 });
quest.SetRewards(experienceReward: 500, goldReward: 100, itemRewards: new[] { "sword" });
```

### Creating Items

```csharp
var item = Item.Create("item_id", "Item Name");
item.SetType(ItemType.Weapon);
item.SetQuality(ItemQuality.Rare);
item.SetDescription("A powerful weapon");
```

### Creating Traders

```csharp
var trader = Trader.Create("trader_id", "Trader Name", TraderType.Merchant);
var offer = TradeOffer.Create("offer_id", "trader_id");
(offer as TradeOffer)?.AddOfferedItem(new TradeItem { ItemId = "sword", Quantity = 1 });
(offer as TradeOffer)?.SetGoldAmount(-100);
trader.AddOffer(offer);
```

---

## Common Patterns

### Entity Creation Pattern

```csharp
// 1. Create entity
int entityId = entityManager.CreateEntity();

// 2. Add components
entityManager.AddComponent(entityId, componentData);

// 3. Initialize component values
var component = entityManager.GetComponent<ComponentType>(entityId);
component.Field = value;
entityManager.UpdateComponent(entityId, component);
```

### Query Pattern

```csharp
// Query entities with specific components
foreach (var entityId in entityManager.GetEntitiesWithComponent<HealthComponent>())
{
    var health = entityManager.GetComponent<HealthComponent>(entityId);
    // Process entity...
}
```

### System Update Pattern

```csharp
public class MySystem
{
    public void Update(IEntityManager entityManager, float deltaTime)
    {
        var entities = entityManager.GetEntitiesWithComponent<MyComponent>();
        foreach (var entityId in entities)
        {
            var component = entityManager.GetComponent<MyComponent>(entityId);
            // Update logic...
            entityManager.UpdateComponent(entityId, component);
        }
    }
}
```

---

## Performance Tips

### 1. Cache Component Queries

```csharp
// SLOW - Query every frame
void Update()
{
    foreach (var id in entityManager.GetEntitiesWithComponent<HealthComponent>())
    {
        // ...
    }
}

// FAST - Cache query results
private List<int> _healthEntities = new();

void CacheEntities()
{
    _healthEntities = entityManager.GetEntitiesWithComponent<HealthComponent>().ToList();
}

void Update()
{
    foreach (var id in _healthEntities)
    {
        // ...
    }
}
```

### 2. Use Structs for Components

```csharp
// Prefer structs for small, simple data
public struct HealthComponent : IComponent
{
    public float CurrentHealth;
    public float MaxHealth;
}

// Use classes only when necessary
public class ComplexComponent : IComponent
{
    public Dictionary<string, object> Data;
    public List<string> Items;
}
```

### 3. Batch Operations

```csharp
// Process multiple entities in batch
public void DealAreaDamage(IEnumerable<int> targets, float damage)
{
    foreach (var targetId in targets)
    {
        var health = entityManager.GetComponent<ResourceComponent>(targetId);
        health.CurrentValue -= damage;
        entityManager.UpdateComponent(targetId, health);
    }
}
```

---

**Version:** 1.0.0
**Last Updated:** October 2026

For more examples, see the sample game in `Assets/_MythHunter/SampleGame/`.
