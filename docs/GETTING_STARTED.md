# Getting Started with RPG Framework

This guide will walk you through setting up and using the RPG Framework in your project.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [First Steps](#first-steps)
4. [Basic Examples](#basic-examples)
5. [Next Steps](#next-steps)

---

## Prerequisites

Before you begin, ensure you have:

- **Unity 6000.0.45f1** or later (for Unity projects)
- **.NET 8.0 SDK** (for standalone .NET projects)
- **Basic C# knowledge**
- **Understanding of Entity Component System (ECS)** concepts (helpful but not required)

---

## Installation

### Option 1: Unity Package (Recommended)

1. Open your Unity project
2. Copy the `Assets/_Framework` directory to your project's `Assets` folder
3. Unity will automatically import and compile the framework
4. Verify installation by checking for compilation errors (should be zero)

### Option 2: Standalone .NET Project

1. Clone the repository
2. Copy the `Assets/_Framework` source files to your project
3. Add references to the required assemblies
4. Build your project

---

## First Steps

### 1. Understanding the Framework Structure

The framework is organized into modules:

```
Assets/_Framework/
├── Core/              # Framework composition and modules
├── ECS/               # Entity Component System
├── Combat/            # Combat and damage systems
├── Inventory/         # Inventory management
├── Items/             # Item definitions
├── Quests/            # Quest system
├── Dialogue/          # Dialogue system
├── Crafting/          # Crafting system
├── Trading/           # Trading system
└── ... (more modules)
```

Each module contains:
- `Runtime/` - Production code
- `Tests/` - Unit tests
- `.asmdef` files - Assembly definitions

### 2. Create Your First Entity

```csharp
using RPGFramework.ECS;
using RPGFramework.Combat;

public class GameBootstrap : MonoBehaviour
{
    private IEntityManager _entityManager;

    void Start()
    {
        // Initialize the entity manager
        _entityManager = new EntityManager();

        // Create a player entity
        int playerId = _entityManager.CreateEntity();

        // Add health component
        var healthComponent = new ResourceComponent
        {
            ResourceType = ResourceType.Health,
            CurrentValue = 100f,
            MaxValue = 100f
        };
        _entityManager.AddComponent(playerId, healthComponent);

        // Add mana component
        var manaComponent = new ResourceComponent
        {
            ResourceType = ResourceType.Mana,
            CurrentValue = 50f,
            MaxValue = 50f
        };
        _entityManager.AddComponent(playerId, manaComponent);

        Debug.Log($"Player created with ID: {playerId}");
    }
}
```

### 3. Working with Inventory

```csharp
using RPGFramework.Inventory;
using RPGFramework.Items;

public class InventoryExample
{
    public void SetupInventory()
    {
        // Create an inventory container
        var inventory = new InventoryContainer(maxSlots: 20);

        // Create an item stack
        var sword = new ItemStack(itemId: "sword_iron", quantity: 1);

        // Add item to inventory
        bool added = inventory.AddItem(sword);
        if (added)
        {
            Debug.Log("Sword added to inventory!");
        }

        // Find item in inventory
        var foundItem = inventory.FindItem("sword_iron");
        if (foundItem != null)
        {
            Debug.Log($"Found {foundItem.Quantity}x {foundItem.ItemId}");
        }

        // Remove item
        bool removed = inventory.RemoveItem("sword_iron", 1);
        if (removed)
        {
            Debug.Log("Sword removed from inventory");
        }
    }
}
```

### 4. Creating Quests

```csharp
using RPGFramework.Quests;

public class QuestExample
{
    public void CreateQuest()
    {
        // Create a quest
        var quest = Quest.Create(
            questId: "save_village",
            title: "Save the Village",
            description: "Defend the village from wolf attacks"
        );

        // Add objectives
        var objective1 = new QuestObjective
        {
            ObjectiveId = "kill_wolves",
            Description = "Kill 5 wolves",
            RequiredProgress = 5,
            CurrentProgress = 0
        };
        quest.AddObjective(objective1);

        var objective2 = new QuestObjective
        {
            ObjectiveId = "talk_elder",
            Description = "Report to the village elder",
            RequiredProgress = 1,
            CurrentProgress = 0
        };
        quest.AddObjective(objective2);

        // Set rewards
        quest.SetRewards(
            experienceReward: 500,
            goldReward: 100,
            itemRewards: new[] { "potion_health", "sword_steel" }
        );

        Debug.Log($"Quest created: {quest.Title}");
    }

    public void ProgressQuest(IQuest quest)
    {
        // Update objective progress
        quest.UpdateObjectiveProgress("kill_wolves", 1);

        // Check if quest is complete
        if (quest.State == QuestState.Completed)
        {
            Debug.Log("Quest completed!");
        }
    }
}
```

### 5. Setting Up Dialogue

```csharp
using RPGFramework.Dialogue;

public class DialogueExample
{
    public void CreateDialogue()
    {
        // Create a dialogue tree
        var dialogue = Dialogue.Create(
            dialogueId: "elder_greeting",
            title: "Village Elder"
        );

        // Create dialogue nodes
        var greeting = DialogueNode.Create(
            nodeId: "greeting",
            speakerId: "elder",
            text: "Welcome, traveler! The village needs your help."
        );

        var acceptQuest = DialogueNode.Create(
            nodeId: "accept",
            speakerId: "player",
            text: "I'll help defend the village."
        );

        var declineQuest = DialogueNode.Create(
            nodeId: "decline",
            speakerId: "player",
            text: "I'm sorry, I can't help right now."
        );

        // Link nodes
        greeting.AddResponse(new DialogueResponse
        {
            ResponseId = "accept_response",
            ResponseText = "I'll help.",
            NextNodeId = "accept"
        });

        greeting.AddResponse(new DialogueResponse
        {
            ResponseId = "decline_response",
            ResponseText = "Not now.",
            NextNodeId = "decline"
        });

        // Add nodes to dialogue
        dialogue.AddNode(greeting);
        dialogue.AddNode(acceptQuest);
        dialogue.AddNode(declineQuest);
        dialogue.SetRootNode("greeting");

        Debug.Log($"Dialogue created: {dialogue.Title}");
    }
}
```

---

## Basic Examples

### Example 1: Combat System

```csharp
using RPGFramework.Combat;

public class CombatExample
{
    public void SimulateCombat()
    {
        var damageCalculator = new DamageCalculator();

        // Calculate damage
        var result = damageCalculator.CalculateDamage(
            baseDamage: 50f,
            damageType: DamageType.Physical,
            attackerLevel: 10,
            defenderLevel: 8,
            defenderArmor: 20f
        );

        Debug.Log($"Damage dealt: {result.FinalDamage}");
        Debug.Log($"Was critical: {result.IsCritical}");

        // Apply damage to entity
        var entityManager = new EntityManager();
        int enemyId = entityManager.CreateEntity();
        var healthComponent = new ResourceComponent
        {
            ResourceType = ResourceType.Health,
            CurrentValue = 100f,
            MaxValue = 100f
        };
        entityManager.AddComponent(enemyId, healthComponent);

        // Get and modify health
        var health = entityManager.GetComponent<ResourceComponent>(enemyId);
        health.CurrentValue -= result.FinalDamage;
        entityManager.UpdateComponent(enemyId, health);

        Debug.Log($"Enemy health: {health.CurrentValue}/{health.MaxValue}");
    }
}
```

### Example 2: Crafting System

```csharp
using RPGFramework.Crafting;

public class CraftingExample
{
    public void SetupCrafting()
    {
        var registry = new CraftingRecipeRegistry();

        // Create a recipe
        var recipe = CraftingRecipe.Create(
            recipeId: "craft_iron_sword",
            recipeName: "Iron Sword",
            resultItemId: "sword_iron",
            resultQuantity: 1
        );

        // Add ingredients
        recipe.AddIngredient(new CraftingIngredient
        {
            ItemId = "iron_ingot",
            RequiredQuantity = 3
        });

        recipe.AddIngredient(new CraftingIngredient
        {
            ItemId = "wood_plank",
            RequiredQuantity = 1
        });

        // Set requirements
        recipe.RequiredSkillLevel = 5;
        recipe.CraftingTime = 10f;

        // Register recipe
        registry.RegisterRecipe(recipe);

        Debug.Log($"Recipe registered: {recipe.RecipeName}");
    }

    public void CraftItem(ICraftingRecipe recipe, IInventoryContainer inventory)
    {
        // Check if player has ingredients
        bool hasIngredients = true;
        foreach (var ingredient in recipe.Ingredients)
        {
            var item = inventory.FindItem(ingredient.ItemId);
            if (item == null || item.Quantity < ingredient.RequiredQuantity)
            {
                hasIngredients = false;
                break;
            }
        }

        if (!hasIngredients)
        {
            Debug.Log("Missing ingredients!");
            return;
        }

        // Consume ingredients
        foreach (var ingredient in recipe.Ingredients)
        {
            inventory.RemoveItem(ingredient.ItemId, ingredient.RequiredQuantity);
        }

        // Add crafted item
        var craftedItem = new ItemStack(recipe.ResultItemId, recipe.ResultQuantity);
        inventory.AddItem(craftedItem);

        Debug.Log($"Crafted: {recipe.RecipeName}");
    }
}
```

### Example 3: Trading System

```csharp
using RPGFramework.Trading;

public class TradingExample
{
    public void SetupTrading()
    {
        var registry = new TraderRegistry();
        var manager = new TradingManager(registry);

        // Create a merchant
        var merchant = Trader.Create(
            traderId: "merchant_general",
            name: "General Merchant",
            traderType: TraderType.Merchant
        );

        // Create a trade offer
        var offer = TradeOffer.Create(
            offerId: "buy_health_potion",
            traderId: "merchant_general"
        );

        // Configure offer (player gets potion, pays gold)
        (offer as TradeOffer)?.AddOfferedItem(new TradeItem
        {
            ItemId = "potion_health",
            Quantity = 1,
            Quality = 0
        });
        (offer as TradeOffer)?.SetGoldAmount(-50); // Negative = player pays

        merchant.AddOffer(offer);
        registry.RegisterTrader(merchant);

        Debug.Log($"Trader registered: {merchant.Name}");
    }

    public void ExecuteTrade(ITradingManager manager, int playerId, string offerId)
    {
        // Initialize player gold
        manager.InitializePlayerGold(playerId, goldAmount: 500);

        // Propose trade
        bool proposed = manager.ProposeTradeOffer(playerId, offerId);
        if (!proposed)
        {
            Debug.Log("Trade proposal failed!");
            return;
        }

        // Accept trade
        var result = manager.AcceptTrade(playerId, offerId);
        if (result.Success)
        {
            Debug.Log($"Trade successful! Gold change: {result.GoldChange}");
        }
        else
        {
            Debug.Log($"Trade failed: {result.FailureReason}");
        }
    }
}
```

---

## Next Steps

Now that you've learned the basics, explore:

1. **[Module Guide](MODULE_GUIDE.md)** - Deep dive into each module
2. **[API Reference](API_REFERENCE.md)** - Complete API documentation
3. **[Best Practices](BEST_PRACTICES.md)** - Coding standards and patterns
4. **[Integration Guide](INTEGRATION_GUIDE.md)** - Integrate with existing projects
5. **Sample Game** - Check out the MythHunter sample in `Assets/_MythHunter/SampleGame/`

---

## Common Pitfalls

### 1. Forgetting to Initialize Services

```csharp
// WRONG - EntityManager not initialized
var component = entityManager.GetComponent<ResourceComponent>(entityId); // NullReferenceException

// CORRECT - Initialize first
var entityManager = new EntityManager();
int entityId = entityManager.CreateEntity();
// Now you can use it
```

### 2. Mixing Framework and Game Logic

```csharp
// WRONG - Unity-specific code in framework
public class ItemComponent : MonoBehaviour { } // Don't use MonoBehaviour in framework

// CORRECT - Keep framework pure .NET
public struct ItemComponent : IComponent
{
    public string ItemId;
    public int Quantity;
}
```

### 3. Not Checking Return Values

```csharp
// WRONG - Assuming success
inventory.AddItem(itemStack);
inventory.RemoveItem("sword", 1);

// CORRECT - Check results
if (inventory.AddItem(itemStack))
{
    Debug.Log("Item added successfully");
}
else
{
    Debug.Log("Inventory full!");
}
```

---

## Getting Help

- **Documentation:** Read the full documentation in the `Docs/` folder
- **Examples:** Check the sample game in `Assets/_MythHunter/SampleGame/`
- **Tests:** Review unit tests in each module's `Tests/` folder
- **Community:** Join discussions on GitHub

---

## Quick Reference

### Common Interfaces

- `IEntityManager` - Entity and component management
- `IInventoryContainer` - Inventory operations
- `IQuest` - Quest tracking
- `IDialogue` - Dialogue trees
- `ICraftingRecipe` - Crafting recipes
- `ITrader` - Trading with NPCs
- `IStatContainer` - Stat management

### Common Data Types

- `ResourceComponent` - HP, MP, Stamina resources
- `ItemStack` - Item with quantity
- `QuestObjective` - Quest goal tracking
- `DialogueNode` - Conversation node
- `StatusEffect` - Buffs and debuffs
- `TradeItem` - Item in trade offer

---

**Happy game development!**
