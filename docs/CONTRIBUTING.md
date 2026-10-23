# Contributing to RPG Framework

Thank you for your interest in contributing! This guide will help you get started.

---

## Code of Conduct

Be respectful, constructive, and professional in all interactions.

---

## How to Contribute

### Bug Reports

Found a bug? Create an issue with:
- Clear description of the problem
- Steps to reproduce
- Expected behavior vs actual behavior
- Unity version, OS, and RPG Framework version
- Code sample (if applicable)

### Feature Requests

Have an idea? Create an issue with:
- Clear description of the feature
- Use case and benefits
- Example API or usage

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Write/update tests
5. Update documentation
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

---

## Development Setup

### Prerequisites
- Unity 6000.0.45f1 or later
- .NET 8.0 SDK
- Git
- Code editor (Visual Studio, Rider, or VS Code)

### Setup Steps

1. **Clone the repository**
```bash
git clone https://github.com/YourOrg/RPGFramework.git
cd RPGFramework
```

2. **Open in Unity**
- Launch Unity Hub
- Add project
- Open project

3. **Run tests**
```bash
unity test Assets/_Framework --mode EditMode
```

---

## Code Standards

### Framework/Game Separation

**Framework Layer:**
- Pure .NET 8.0 (no `UnityEngine` imports)
- No game-specific logic
- Interface-driven design

**Game Layer:**
- Unity-specific code allowed
- Game-specific implementations
- Adapters connecting Framework to Unity

### Naming Conventions

```csharp
// Interfaces
public interface IEntityManager { }

// Classes
public class EntityManager : IEntityManager { }

// Components (structs preferred for simple data)
public struct HealthComponent : IComponent { }

// Events
public class ItemAddedEvent { }

// Methods
public void AddItem(IItemStack item) { }
public bool HasItem(string itemId) { }
```

### Code Style

- Use C# 11 features
- 4 spaces for indentation
- Opening braces on same line for methods/properties
- XML documentation for all public APIs
- Avoid `var` for primitive types, use for complex types

Example:
```csharp
/// <summary>
/// Adds an item to the inventory.
/// </summary>
/// <param name="item">The item to add</param>
/// <returns>True if successful, false if inventory is full</returns>
public bool AddItem(IItemStack item)
{
    if (IsFull) return false;
    _items.Add(item);
    return true;
}
```

---

## Testing Guidelines

### Unit Tests Required

All new features must have unit tests:

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

### Test Coverage

- All public methods
- Edge cases (null, empty, full, etc.)
- Error conditions
- Integration scenarios

### Running Tests

```bash
# All tests
unity test Assets/_Framework --mode EditMode

# Specific module
unity test Assets/_Framework/Combat/Tests --mode EditMode
```

---

## Module Guidelines

### Creating a New Module

1. **Create directory structure**
```
Assets/_Framework/MyModule/
├── Runtime/
│   ├── Contracts/
│   ├── Data/
│   ├── Enums/
│   ├── Events/
│   ├── Implementations/
│   └── RPGFramework.MyModule.Runtime.asmdef
└── Tests/
    ├── MyModuleTests.cs
    └── RPGFramework.MyModule.Tests.asmdef
```

2. **Create Runtime assembly definition**
```json
{
    "name": "RPGFramework.MyModule.Runtime",
    "references": [
        "RPGFramework.ECS.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

3. **Create Test assembly definition**
```json
{
    "name": "RPGFramework.MyModule.Tests",
    "references": [
        "RPGFramework.MyModule.Runtime",
        "RPGFramework.ECS.Runtime",
        "UnityEngine.TestRunner",
        "Moq"
    ],
    "includePlatforms": ["Editor"],
    "optionalUnityReferences": ["TestAssemblies"]
}
```

4. **Implement interfaces**
```csharp
namespace RPGFramework.MyModule
{
    public interface IMyService
    {
        void DoSomething();
    }

    public class MyService : IMyService
    {
        public void DoSomething()
        {
            // Implementation
        }
    }
}
```

5. **Write tests**
```csharp
using NUnit.Framework;
using RPGFramework.MyModule;

public class MyModuleTests
{
    [Test]
    public void DoSomething_ValidInput_Succeeds()
    {
        // Test implementation
    }
}
```

---

## Documentation Guidelines

### Code Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Represents a quest in the game.
/// </summary>
public interface IQuest
{
    /// <summary>
    /// Gets the unique identifier for this quest.
    /// </summary>
    string QuestId { get; }

    /// <summary>
    /// Starts the quest and marks it as active.
    /// </summary>
    void StartQuest();
}
```

### README Files

Each module should have a README:

```markdown
# MyModule

Brief description of what this module does.

## Features
- Feature 1
- Feature 2

## Usage
```csharp
// Example code
```

## Dependencies
- RPGFramework.ECS.Runtime
- RPGFramework.Core.Runtime
```

---

## Pull Request Checklist

Before submitting a PR, ensure:

- [ ] Code compiles without errors
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] XML documentation for all public APIs
- [ ] README updated (if applicable)
- [ ] Changelog updated
- [ ] No Unity dependencies in Framework layer
- [ ] Follows naming conventions
- [ ] No breaking changes (or clearly documented)
- [ ] Performance considerations addressed

---

## Review Process

1. **Automated Checks**
   - CI builds project
   - CI runs all tests
   - Code quality checks

2. **Code Review**
   - Maintainer reviews code
   - Requests changes if needed
   - Approves when ready

3. **Merge**
   - PR merged to main branch
   - Included in next release

---

## Release Process

1. Update version numbers
2. Update CHANGELOG.md
3. Create git tag
4. Build packages
5. Publish release

---

## Community

### Getting Help

- **Issues:** For bugs and feature requests
- **Discussions:** For questions and general discussion
- **Discord:** (Link if available)

### Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Credited in release notes
- Appreciated by the community!

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

## Questions?

If you have questions about contributing, create an issue or ask in Discussions.

---

**Thank you for contributing to RPG Framework!**
