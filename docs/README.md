# RPG Framework

**A Universal, Modular RPG Game Development Framework**

Version: 1.0.0
Platform: .NET 8.0 (Unity-compatible)
License: MIT

---

## Overview

RPG Framework is a comprehensive, production-ready framework for building role-playing games. Designed with modularity, performance, and reusability in mind, it provides 21 specialized modules covering all aspects of RPG game development.

### Key Features

- **21 Specialized Modules** - Combat, Inventory, Quests, Dialogue, Crafting, Trading, and more
- **Pure .NET 8.0** - Framework core has zero Unity dependencies
- **ECS-Compatible** - Built on Entity Component System architecture
- **312+ Source Files** - Comprehensive implementation with 23 test suites
- **Production-Ready** - Battle-tested patterns and extensive unit testing
- **Modular Design** - Use only what you need, when you need it
- **Framework/Game Separation** - Clear architectural boundaries
- **Interface-Driven** - Extensible and testable by design

---

## Framework Modules

### Core Systems
1. **ECS (Entity Component System)** - Foundation for game entities and components
2. **Logging** - Flexible logging system with severity levels
3. **Serialization** - Versioned serialization with migration support
4. **DeltaCompression** - Efficient state synchronization
5. **Pooling** - Object pooling for performance optimization
6. **Core** - Framework composition and module management

### Combat & Stats
7. **Combat** - Damage calculation, resources (HP/MP), life states
8. **Stats** - Stat containers with modifiers (flat, percentage, override)
9. **StatusEffects** - Timed and recurring status effects with stacking
10. **Abilities** - Ability system with cooldowns and resource costs

### Items & Economy
11. **Items** - Item definitions with quality, rarity, and types
12. **Inventory** - Container-based inventory with equipment slots
13. **Loot** - Loot tables with weighted random generation
14. **Trading** - NPC trading with conditions and reputation
15. **Crafting** - Recipe-based crafting with skill requirements

### Progression & Social
16. **Progression** - Level-up system with experience and rewards
17. **Skills** - Skill trees with prerequisites and unlocking
18. **Quests** - Quest system with objectives and branching
19. **Dialogue** - Conversation trees with conditional responses
20. **Factions** - Faction reputation and relationships

---

## Quick Start

### Installation

1. Clone or download the repository
2. Open in Unity 6000.0.45f1 or later
3. Import the `Assets/_Framework` directory into your project

### Basic Setup

```csharp
using RPGFramework.Core.Composition;
using RPGFramework.ECS;
using RPGFramework.Combat;

// Create a game composition
var builder = new GameCompositionBuilder();
builder.AddModule(new FrameworkCoreModule());
builder.AddModule(new CombatModule());
var composition = builder.Build();

// Create an entity with components
var entityManager = composition.GetService<IEntityManager>();
var entityId = entityManager.CreateEntity();
entityManager.AddComponent(entityId, new ResourceComponent
{
    ResourceType = ResourceType.Health,
    CurrentValue = 100,
    MaxValue = 100
});
```

### Using Individual Modules

```csharp
// Inventory Example
var container = new InventoryContainer(maxSlots: 20);
var itemStack = new ItemStack(itemId: "sword_001", quantity: 1);
container.AddItem(itemStack);

// Quest Example
var quest = Quest.Create("quest_001", "Save the Village");
quest.AddObjective(new QuestObjective
{
    ObjectiveId = "kill_wolves",
    Description = "Kill 5 wolves",
    RequiredProgress = 5
});
```

---

## Architecture

The framework follows a strict layered architecture:

```
┌─────────────────────────────────────┐
│     Game Layer (MythHunter)         │  ← Game-specific logic, UI, adapters
├─────────────────────────────────────┤
│     Framework Layer (RPGFramework)  │  ← Reusable, pure .NET systems
├─────────────────────────────────────┤
│     .NET 8.0 Runtime                │  ← Standard library
└─────────────────────────────────────┘
```

**Key Principles:**
- Framework does NOT depend on Game Layer
- Framework does NOT depend on Unity Engine
- Game Layer adapters depend on Framework
- Clear interface contracts between layers

---

## Documentation

- [Getting Started Guide](GETTING_STARTED.md) - Step-by-step tutorial
- [Architecture Overview](ARCHITECTURE.md) - Design decisions and patterns
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Module Guide](MODULE_GUIDE.md) - Detailed module documentation
- [Integration Guide](INTEGRATION_GUIDE.md) - Integrate into existing projects
- [Best Practices](BEST_PRACTICES.md) - Coding standards and patterns
- [FAQ](FAQ.md) - Frequently asked questions
- [Changelog](CHANGELOG.md) - Version history

---

## Code Statistics

- **Total Modules:** 21
- **Total Files:** 312+ C# files
- **Test Files:** 23 test suites
- **Lines of Code:** ~25,000+ LOC
- **Test Coverage:** Comprehensive unit testing
- **Dependencies:** Pure .NET 8.0 (Moq for testing)

---

## Example Projects

### MythHunter (Included)
A complete RPG game demonstrating all framework features:
- Player character with stats, inventory, and equipment
- Quest system with multiple quest chains
- Dialogue with NPCs
- Combat with enemies
- Crafting and trading
- Faction reputation system

---

## Requirements

- **Unity:** 6000.0.45f1 or later (for Unity projects)
- **.NET:** 8.0 or later
- **OS:** Windows, macOS, Linux
- **C#:** 11.0 language features

---

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Clone the repository
2. Open in Unity or your preferred IDE
3. Run tests: `unity test --mode EditMode`
4. Make your changes
5. Submit a pull request

---

## Support

- **Documentation:** [Full documentation](https://github.com/Aizekhan/RPG-Framework-Roadmap)
- **Issues:** [GitHub Issues](https://github.com/Aizekhan/RPG-Framework-Roadmap/issues)
- **Discussions:** [GitHub Discussions](https://github.com/Aizekhan/RPG-Framework-Roadmap/discussions)

---

## License

MIT License - See [LICENSE.txt](../LICENSE.txt) for details

---

## Credits

Developed as part of the MythHunter RPG project.

**Framework Design:** Entity Component System architecture
**Testing:** NUnit framework with Moq
**Build System:** Unity 6000.0.45f1

---

## Roadmap

See [ROADMAP.md](ROADMAP.md) for planned features and upcoming releases.

### Upcoming Features
- Weather and environment system
- Guild/clan management
- Companion AI system
- Housing and furniture placement
- Dynamic world events
- Networking and multiplayer support

---

**Built with care for the RPG game development community.**
