# RPG Framework Module Guide

Comprehensive guide to all 21 Framework modules.

---

## Core Infrastructure Modules

### 1. ECS (Entity Component System)

**Purpose:** Foundation for entity and component management

**Key Interfaces:**
- `IEntityManager` - Create/destroy entities, add/remove/get components
- `IComponent` - Marker interface for all components

**Usage:**
```csharp
var entityManager = new EntityManager();
int entityId = entityManager.CreateEntity();
entityManager.AddComponent(entityId, new HealthComponent { CurrentHealth = 100 });
var health = entityManager.GetComponent<HealthComponent>(entityId);
```

**Dependencies:** None

---

### 2. Logging

**Purpose:** Flexible logging system with severity levels

**Key Interfaces:**
- `ILogger` - Log messages with different severity levels

**Severity Levels:**
- `Debug` - Detailed diagnostic information
- `Info` - General information
- `Warning` - Warning messages
- `Error` - Error conditions
- `Fatal` - Fatal errors

**Usage:**
```csharp
logger.Log("Player spawned", LogSeverity.Info);
logger.Log("Invalid item ID", LogSeverity.Error);
```

**Dependencies:** None

---

### 3. Serialization

**Purpose:** Versioned serialization with migration support

**Key Interfaces:**
- `ISerializer` - Serialize/deserialize objects
- `IVersionedData` - Mark data with version numbers
- `IMigrator` - Migrate data between versions

**Usage:**
```csharp
[SerializableType("PlayerData", 1)]
public class PlayerData : IVersionedData
{
    public int Version => 1;
    public string Name;
    public int Level;
}

var serializer = new VersionedSerializer();
string json = serializer.Serialize(playerData);
```

**Dependencies:** None

---

### 4. DeltaCompression

**Purpose:** Efficient state synchronization for networking

**Key Interfaces:**
- `IDeltaSerializer` - Serialize only changed data

**Usage:**
```csharp
var deltaSerializer = new DeltaSerializer();
byte[] delta = deltaSerializer.SerializeDelta(oldState, newState);
```

**Dependencies:** Serialization

---

### 5. Pooling

**Purpose:** Object pooling for performance optimization

**Key Interfaces:**
- `IObjectPool<T>` - Get/return pooled objects

**Usage:**
```csharp
var pool = new ObjectPool<Bullet>(() => new Bullet(), 100);
var bullet = pool.Get();
// Use bullet...
pool.Return(bullet);
```

**Dependencies:** None

---

### 6. Core (Framework Composition)

**Purpose:** Module management and dependency injection

**Key Interfaces:**
- `IGameCompositionBuilder` - Build game composition
- `IFeatureModule` - Base for all modules

**Usage:**
```csharp
var builder = new GameCompositionBuilder();
builder.AddModule(new FrameworkCoreModule());
builder.AddModule(new CombatModule());
var composition = builder.Build();
```

**Dependencies:** ECS, Logging

---

## Combat & Stats Modules

### 7. Combat

**Purpose:** Damage calculation, resources (HP/MP), life states

**Key Types:**
- `ResourceComponent` - HP, MP, Stamina
- `IDamageCalculator` - Calculate damage with types and resistances
- `ILifeStateManager` - Manage alive/dead/unconscious states

**Features:**
- Multiple damage types (Physical, Magical, Fire, Ice, etc.)
- Critical hits and dodge mechanics
- Death and respawn handling
- Resource regeneration

**Usage:**
```csharp
var calculator = new DamageCalculator();
var result = calculator.CalculateDamage(
    baseDamage: 50f,
    damageType: DamageType.Physical,
    attackerLevel: 10,
    defenderLevel: 8,
    defenderArmor: 20f
);
Debug.Log($"Damage: {result.FinalDamage}, Critical: {result.IsCritical}");
```

**Dependencies:** ECS, Stats

---

### 8. Stats

**Purpose:** Character stats with modifiers

**Key Interfaces:**
- `IStatContainer` - Store and manage stats
- `IStatModifier` - Modify stats (flat, percentage, override)

**Modifier Types:**
- `Flat` - +10 damage
- `Percentage` - +20% damage
- `Override` - Set damage to 50

**Usage:**
```csharp
var stats = new StatContainer();
stats.SetBaseStat("Strength", 10);
stats.AddModifier("Strength", new StatModifier
{
    Value = 5,
    Type = StatModifierType.Flat,
    Source = "Potion"
});
float finalStrength = stats.GetFinalStat("Strength"); // 15
```

**Dependencies:** ECS

---

### 9. StatusEffects

**Purpose:** Buffs, debuffs, and status effects

**Key Interfaces:**
- `IStatusEffect` - Base status effect
- `IStatusEffectManager` - Apply/remove effects
- `IStatusEffectTarget` - Entity that can receive effects

**Effect Types:**
- `TimedStatusEffect` - Duration-based (e.g., 10 seconds)
- `RecurringStatusEffect` - Periodic tick (e.g., poison damage every 2s)

**Stacking:**
- `None` - Only one instance
- `Stack` - Multiple instances allowed
- `Refresh` - Reset duration on reapplication

**Usage:**
```csharp
var effect = new TimedStatusEffect
{
    EffectId = "poison",
    Duration = 10f,
    StackBehavior = StackBehavior.Stack
};

var manager = new StatusEffectManager();
manager.ApplyEffect(entityId, effect);
```

**Dependencies:** ECS, Stats

---

### 10. Abilities

**Purpose:** Active and passive abilities with cooldowns

**Key Interfaces:**
- `IAbility` - Ability definition
- `IAbilityManager` - Activate/cooldown management

**Features:**
- Cooldown tracking
- Resource costs (mana, stamina)
- Cast time
- Targeting (self, single, area)

**Usage:**
```csharp
var ability = Ability.Create("fireball", "Fireball");
ability.SetCooldown(5f);
ability.SetManaCost(30);
ability.SetCastTime(2f);

bool canCast = abilityManager.CanActivateAbility(playerId, "fireball");
if (canCast)
{
    abilityManager.ActivateAbility(playerId, "fireball", targetId);
}
```

**Dependencies:** ECS, Combat

---

## Items & Economy Modules

### 11. Items

**Purpose:** Item definitions with quality, rarity, and types

**Key Interfaces:**
- `IItem` - Item definition
- `IItemInstance` - Specific item instance
- `IItemRegistry` - Item database

**Item Types:**
- Weapon, Armor, Consumable, QuestItem, Material, Currency

**Quality Levels:**
- Poor, Common, Uncommon, Rare, Epic, Legendary

**Usage:**
```csharp
var item = Item.Create("sword_iron", "Iron Sword");
item.SetType(ItemType.Weapon);
item.SetQuality(ItemQuality.Common);
item.SetRarity(ItemRarity.Common);

var registry = new ItemRegistry();
registry.RegisterItem(item);
```

**Dependencies:** ECS

---

### 12. Inventory

**Purpose:** Container-based inventory with equipment slots

**Key Interfaces:**
- `IInventoryContainer` - Inventory storage
- `IItemStack` - Item with quantity
- `IEquipmentSlot` - Equippable item slots

**Features:**
- Slot-based storage
- Stack management
- Equipment slots (head, chest, weapon, etc.)
- Item events (added, removed, equipped)

**Usage:**
```csharp
var inventory = new InventoryContainer(maxSlots: 20);
var itemStack = new ItemStack("potion_health", quantity: 5);

bool added = inventory.AddItem(itemStack);
bool removed = inventory.RemoveItem("potion_health", 1);
var found = inventory.FindItem("potion_health"); // 4 remaining
```

**Dependencies:** ECS, Items

---

### 13. Loot

**Purpose:** Loot tables with weighted random generation

**Key Interfaces:**
- `ILootTable` - Loot table definition
- `ILootGenerator` - Generate loot drops
- `ILootEntry` - Single loot entry with weight

**Usage:**
```csharp
var table = LootTable.Create("wolf_loot");
table.AddEntry(new LootEntry
{
    ItemId = "wolf_pelt",
    Weight = 50,
    MinQuantity = 1,
    MaxQuantity = 3
});
table.AddEntry(new LootEntry
{
    ItemId = "gold",
    Weight = 100,
    MinQuantity = 5,
    MaxQuantity = 15
});

var generator = new LootGenerator();
var drops = generator.GenerateLoot(table);
```

**Dependencies:** Items

---

### 14. Trading

**Purpose:** NPC trading with conditions and reputation

**Key Interfaces:**
- `ITrader` - NPC trader
- `ITradeOffer` - Trade offer definition
- `ITradingManager` - Execute trades

**Trader Types:**
- Merchant, Blacksmith, Alchemist, FenceDealer, QuestGiver

**Features:**
- Multi-item exchanges
- Gold transactions
- Reputation requirements
- Quest/level/faction conditions
- Limited use offers

**Usage:**
```csharp
var trader = Trader.Create("merchant_001", "General Merchant", TraderType.Merchant);
var offer = TradeOffer.Create("offer_potion", "merchant_001");
(offer as TradeOffer)?.AddOfferedItem(new TradeItem("potion_health", 1));
(offer as TradeOffer)?.SetGoldAmount(-50); // Player pays 50 gold

trader.AddOffer(offer);
var result = tradingManager.AcceptTrade(playerId, "offer_potion");
```

**Dependencies:** Items, Factions (optional)

---

### 15. Crafting

**Purpose:** Recipe-based crafting with skill requirements

**Key Interfaces:**
- `ICraftingRecipe` - Recipe definition
- `ICraftingManager` - Execute crafting
- `ICraftingRecipeRegistry` - Recipe database

**Features:**
- Multi-ingredient recipes
- Skill level requirements
- Crafting time
- Success/failure mechanics
- Result item customization

**Usage:**
```csharp
var recipe = CraftingRecipe.Create("craft_sword", "Iron Sword", "sword_iron", 1);
recipe.AddIngredient(new CraftingIngredient { ItemId = "iron_ingot", RequiredQuantity = 3 });
recipe.AddIngredient(new CraftingIngredient { ItemId = "wood_plank", RequiredQuantity = 1 });
recipe.RequiredSkillLevel = 5;
recipe.CraftingTime = 10f;

var registry = new CraftingRecipeRegistry();
registry.RegisterRecipe(recipe);

var result = craftingManager.CraftItem(playerId, "craft_sword");
```

**Dependencies:** Items, Inventory, Skills (optional)

---

## Progression & Social Modules

### 16. Progression

**Purpose:** Level-up system with experience and rewards

**Key Interfaces:**
- `IProgressionManager` - Track levels and XP
- `ILevelRewards` - Define level-up rewards

**Features:**
- Experience point tracking
- Level-up calculations
- Automatic rewards (stats, items, abilities)
- Prestige/rebirth mechanics

**Usage:**
```csharp
var manager = new ProgressionManager();
manager.InitializeProgression(playerId, startLevel: 1, startXP: 0);

manager.AddExperience(playerId, 500);
int currentLevel = manager.GetLevel(playerId);
int xpForNextLevel = manager.GetExperienceForNextLevel(playerId);
```

**Dependencies:** ECS

---

### 17. Skills

**Purpose:** Skill trees with prerequisites and unlocking

**Key Interfaces:**
- `ISkill` - Skill definition
- `ISkillTree` - Skill tree structure
- `ISkillManager` - Learn/unlock skills

**Features:**
- Skill prerequisites
- Skill points and costs
- Active and passive skills
- Multiple skill trees per character

**Usage:**
```csharp
var skill = Skill.Create("skill_fireball", "Fireball", SkillType.Active);
skill.SetMaxLevel(5);
skill.AddPrerequisite("skill_fire_mastery", minLevel: 3);

var tree = new SkillTree("mage_tree", "Mage Skills");
tree.AddSkill(skill);

bool learned = skillManager.LearnSkill(playerId, "skill_fireball");
```

**Dependencies:** ECS, Progression

---

### 18. Quests

**Purpose:** Quest system with objectives and branching

**Key Interfaces:**
- `IQuest` - Quest definition
- `IQuestManager` - Track active quests
- `IQuestObjective` - Individual quest goal

**Quest States:**
- NotStarted, Active, Completed, Failed, Abandoned

**Objective Types:**
- Kill enemies
- Collect items
- Talk to NPC
- Reach location
- Custom conditions

**Usage:**
```csharp
var quest = Quest.Create("quest_wolves", "Wolf Problem", "Kill 5 wolves threatening the village");
quest.AddObjective(new QuestObjective
{
    ObjectiveId = "kill_wolves",
    Description = "Kill wolves",
    RequiredProgress = 5,
    CurrentProgress = 0
});
quest.SetRewards(experienceReward: 500, goldReward: 100, itemRewards: new[] { "sword_steel" });

var manager = new QuestManager();
manager.StartQuest(playerId, quest);
manager.UpdateObjectiveProgress(playerId, "quest_wolves", "kill_wolves", 1);
```

**Dependencies:** ECS, Items, Dialogue (optional)

---

### 19. Dialogue

**Purpose:** Conversation trees with conditional responses

**Key Interfaces:**
- `IDialogue` - Dialogue tree
- `IDialogueNode` - Single conversation node
- `IDialogueResponse` - Player response option

**Features:**
- Branching conversations
- Conditional responses (quest, stat, item requirements)
- NPC and player dialogue
- Event triggers (quest start, shop open, etc.)

**Usage:**
```csharp
var dialogue = Dialogue.Create("elder_greeting", "Village Elder");

var greeting = DialogueNode.Create("greeting", "elder", "Welcome, traveler!");
var acceptNode = DialogueNode.Create("accept", "player", "I'll help you.");
var declineNode = DialogueNode.Create("decline", "player", "I can't right now.");

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

dialogue.AddNode(greeting);
dialogue.AddNode(acceptNode);
dialogue.AddNode(declineNode);
dialogue.SetRootNode("greeting");
```

**Dependencies:** ECS, Quests (optional)

---

### 20. Factions

**Purpose:** Faction reputation and relationships

**Key Interfaces:**
- `IFaction` - Faction definition
- `IFactionManager` - Manage reputations
- `IFactionRelationship` - Inter-faction relations

**Faction Types:**
- PlayerFaction, EnemyFaction, NeutralFaction, GuildFaction

**Standing Levels:**
- Hated, Hostile, Unfriendly, Neutral, Friendly, Honored, Revered, Exalted

**Features:**
- Reputation tracking
- Faction relationships (allied, hostile, neutral)
- Reputation rewards and penalties
- Faction-specific content unlocking

**Usage:**
```csharp
var faction = Faction.Create("kingdom_alliance", "Kingdom Alliance", FactionType.PlayerFaction);

var manager = new FactionManager();
manager.RegisterFaction(faction);
manager.InitializeReputation(playerId, "kingdom_alliance", startingReputation: 0);

manager.AddReputation(playerId, "kingdom_alliance", 100);
var standing = manager.GetFactionStanding(playerId, "kingdom_alliance");
```

**Dependencies:** ECS

---

## Module Summary Table

| Module | LOC | Tests | Dependencies | Complexity |
|--------|-----|-------|--------------|------------|
| ECS | 250 | 10 | None | Medium |
| Logging | 80 | 5 | None | Low |
| Serialization | 400 | 8 | None | High |
| DeltaCompression | 150 | 3 | Serialization | Medium |
| Pooling | 100 | 4 | None | Low |
| Core | 200 | 6 | ECS, Logging | Medium |
| Combat | 800 | 15 | ECS, Stats | High |
| Stats | 300 | 12 | ECS | Medium |
| StatusEffects | 450 | 14 | ECS, Stats | Medium |
| Abilities | 350 | 10 | ECS, Combat | Medium |
| Items | 600 | 13 | ECS | Medium |
| Inventory | 550 | 12 | ECS, Items | Medium |
| Loot | 400 | 11 | Items | Medium |
| Trading | 650 | 29 | Items, Factions | High |
| Crafting | 580 | 25 | Items, Inventory | High |
| Progression | 400 | 11 | ECS | Medium |
| Skills | 500 | 14 | ECS, Progression | Medium |
| Quests | 700 | 36 | ECS, Items | High |
| Dialogue | 600 | 30 | ECS, Quests | High |
| Factions | 450 | 18 | ECS | Medium |

**Total:** ~9,510 LOC, 286 tests

---

## Next Steps

- See [API Reference](API_REFERENCE.md) for detailed API documentation
- See [Integration Guide](INTEGRATION_GUIDE.md) for integration examples
- See [Best Practices](BEST_PRACTICES.md) for usage patterns

---

**Version:** 1.0.0
