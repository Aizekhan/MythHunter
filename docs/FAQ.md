# Frequently Asked Questions (FAQ)

---

## General Questions

### Q: What is RPG Framework?

**A:** RPG Framework is a universal, modular framework for building RPG games. It provides 21 specialized modules covering combat, inventory, quests, dialogue, crafting, and more.

### Q: What platforms does it support?

**A:** RPG Framework is built on .NET 8.0 and is compatible with:
- Unity 6000.0.45f1 or later
- Standalone .NET 8.0 applications
- Windows, macOS, Linux

### Q: Is it free?

**A:** Yes, RPG Framework is open-source under the MIT license.

### Q: Can I use it in commercial projects?

**A:** Yes, the MIT license allows commercial use.

---

## Technical Questions

### Q: Does RPG Framework depend on Unity?

**A:** No. The Framework core (`Assets/_Framework`) is pure .NET 8.0 with zero Unity dependencies. Only the game layer (adapters) uses Unity-specific code.

### Q: What is ECS?

**A:** Entity Component System (ECS) is an architectural pattern where:
- **Entities** are unique IDs
- **Components** are pure data
- **Systems** contain logic that operates on components

This pattern provides better performance and modularity than traditional object-oriented hierarchies.

### Q: Can I use RPG Framework without Unity?

**A:** Yes. The Framework core is pure .NET and can be used in any .NET 8.0 application. You'll need to create your own adapters for your specific engine/platform.

### Q: How do I add Unity-specific features?

**A:** Create adapter classes in your game layer that bridge Framework logic to Unity:

```csharp
public class PlayerAdapter : MonoBehaviour
{
    private IEntityManager _entityManager;
    private int _entityId;

    // Unity-specific code here
}
```

### Q: What testing framework does it use?

**A:** NUnit for unit testing, with Moq for mocking interfaces.

---

## Usage Questions

### Q: How do I get started?

**A:** See [Getting Started Guide](GETTING_STARTED.md) for step-by-step instructions.

### Q: Which modules should I use?

**A:** Start with Core, ECS, and the modules relevant to your game (e.g., Combat, Inventory). You can add more modules as needed.

### Q: How do I create an entity?

**A:**
```csharp
var entityManager = new EntityManager();
int entityId = entityManager.CreateEntity();
entityManager.AddComponent(entityId, new HealthComponent { CurrentHealth = 100 });
```

### Q: How do I query entities?

**A:**
```csharp
foreach (var entityId in entityManager.GetEntitiesWithComponent<HealthComponent>())
{
    var health = entityManager.GetComponent<HealthComponent>(entityId);
    // Process entity...
}
```

### Q: How do I handle events?

**A:** Framework systems fire events that you can subscribe to. You'll need to implement an event bus or use Unity's event system.

### Q: Can I modify Framework code?

**A:** Yes, but consider creating extensions or adapters instead to make upgrades easier.

---

## Integration Questions

### Q: How do I integrate with my existing Unity project?

**A:** See [Integration Guide](INTEGRATION_GUIDE.md). In short:
1. Copy `Assets/_Framework` to your project
2. Create adapters for Framework services
3. Initialize services in your game bootstrap

### Q: How do I save/load game state?

**A:** Use the Serialization module:

```csharp
var serializer = new VersionedSerializer();
string json = serializer.Serialize(playerData);
// Save json to file...

// Load
var loadedData = serializer.Deserialize<PlayerData>(json);
```

### Q: How do I create custom components?

**A:**
```csharp
public struct MyCustomComponent : IComponent
{
    public string CustomData;
    public int CustomValue;
}

entityManager.AddComponent(entityId, new MyCustomComponent { ... });
```

### Q: How do I create custom systems?

**A:**
```csharp
public class MyCustomSystem
{
    public void Update(IEntityManager entityManager, float deltaTime)
    {
        foreach (var entityId in entityManager.GetEntitiesWithComponent<MyCustomComponent>())
        {
            var component = entityManager.GetComponent<MyCustomComponent>(entityId);
            // Custom logic...
            entityManager.UpdateComponent(entityId, component);
        }
    }
}
```

---

## Performance Questions

### Q: Is ECS faster than traditional OOP?

**A:** Yes, ECS provides better cache locality and enables data-oriented design, which can be significantly faster for large numbers of entities.

### Q: How many entities can it handle?

**A:** Current implementation can handle thousands of entities efficiently. For tens of thousands, consider archetype-based storage (future enhancement).

### Q: Should I use structs or classes for components?

**A:** Use structs for small, simple data (<16 bytes). Use classes for large data or when you need reference semantics.

### Q: How do I optimize performance?

**A:** See [Best Practices](BEST_PRACTICES.md):
- Cache component queries
- Use structs for components
- Avoid allocations in hot paths
- Use object pooling

---

## Module-Specific Questions

### Q: How do I create a quest?

**A:**
```csharp
var quest = Quest.Create("quest_id", "Quest Title");
quest.AddObjective(new QuestObjective
{
    ObjectiveId = "kill_wolves",
    RequiredProgress = 5
});
quest.SetRewards(experienceReward: 500, goldReward: 100);
```

### Q: How does the inventory system work?

**A:** Container-based with slots:
```csharp
var inventory = new InventoryContainer(maxSlots: 20);
inventory.AddItem(new ItemStack("sword", 1));
```

### Q: How do I set up trading?

**A:**
```csharp
var trader = Trader.Create("merchant", "Merchant", TraderType.Merchant);
var offer = TradeOffer.Create("offer_001", "merchant");
(offer as TradeOffer)?.AddOfferedItem(new TradeItem("potion", 1));
(offer as TradeOffer)?.SetGoldAmount(-50);
trader.AddOffer(offer);
```

### Q: How do status effects work?

**A:**
```csharp
var effect = new TimedStatusEffect
{
    EffectId = "poison",
    Duration = 10f,
    StackBehavior = StackBehavior.Stack
};
statusEffectManager.ApplyEffect(entityId, effect);
```

---

## Troubleshooting

### Q: I'm getting "Type not found" errors

**A:** Make sure your .asmdef references the required Framework assemblies:
```json
{
    "references": [
        "RPGFramework.ECS.Runtime",
        "RPGFramework.Combat.Runtime"
    ]
}
```

### Q: Components aren't updating

**A:** Make sure to call `UpdateComponent` after modifying:
```csharp
var health = entityManager.GetComponent<HealthComponent>(entityId);
health.CurrentHealth -= damage;
entityManager.UpdateComponent(entityId, health); // ← Important!
```

### Q: Tests aren't running

**A:** Ensure test assembly definitions are configured correctly:
- `includePlatforms: ["Editor"]`
- `optionalUnityReferences: ["TestAssemblies"]`
- Reference to `UnityEngine.TestRunner`

### Q: Performance is slow

**A:** Common issues:
- Querying components every frame (cache results)
- Using classes instead of structs for components
- Allocating collections in hot paths
- Not using object pooling

---

## Contribution Questions

### Q: How can I contribute?

**A:** See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines. We welcome:
- Bug fixes
- New modules
- Documentation improvements
- Performance optimizations
- Tests

### Q: How do I report bugs?

**A:** Create an issue on GitHub with:
- Description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Unity version and OS

### Q: Can I add my own modules?

**A:** Yes! Follow the module structure:
```
MyModule/
├── Runtime/
│   ├── Contracts/
│   ├── Implementations/
│   └── MyModule.Runtime.asmdef
└── Tests/
    └── MyModule.Tests.asmdef
```

---

## Future Plans

### Q: What's on the roadmap?

**A:** See [ROADMAP.md](ROADMAP.md). Planned features:
- Weather and environment system
- Guild/clan management
- Companion AI system
- Housing system
- Dynamic world events
- Networking/multiplayer

### Q: When will multiplayer be supported?

**A:** Networking infrastructure is planned for a future release. DeltaCompression module is already in place for state synchronization.

### Q: Will there be a visual editor?

**A:** Visual scripting for quests and dialogues is planned but not yet scheduled.

---

## Support

### Q: Where can I get help?

**A:**
- **Documentation:** Full docs in `Docs/` folder
- **Examples:** Sample game in `Assets/_MythHunter/SampleGame/`
- **Issues:** GitHub Issues for bugs
- **Discussions:** GitHub Discussions for questions

### Q: Is there a Discord/community?

**A:** Check the GitHub repository for community links.

---

**Last Updated:** October 2026

For more detailed information, see:
- [Getting Started](GETTING_STARTED.md)
- [Architecture](ARCHITECTURE.md)
- [Module Guide](MODULE_GUIDE.md)
- [API Reference](API_REFERENCE.md)
