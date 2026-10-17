# EPIC 11 Phase 1: Inventory System — Implementation Plan

**Date:** 2026-10-14
**Author:** Claude Code
**Status:** Ready for implementation
**Audit:** Documentation/EPIC-11-Gameplay-Domain-Audit.md

---

## 1. Executive Summary

**Goal:** Extract MythHunter's inventory management into a universal, reusable RPG Framework module.

**Scope:**
- Generic item containers (bags, banks, vaults)
- Equipment slot management (weapons, armor, accessories)
- Item stacking logic (stackable vs non-stackable)
- Capacity constraints and overflow handling
- Add/Remove/Transfer operations

**Out of Scope (Phase 2):**
- Item definitions (IItem, IEquipment, IConsumable) — Phase 2
- Stat modifiers from equipment — Phase 2
- Item rarity/quality system — Phase 2
- Item instance management — Phase 2

**Deliverables:**
- 12-15 Runtime C# files (interfaces, components, implementations)
- 1 Test C# file (~20-25 tests)
- .NET 8.0 test harness integration
- Static boundary validation
- GitHub Actions workflow
- Zero Unity dependencies
- Zero MythHunter coupling

**Estimated Effort:** 6-8 hours

---

## 2. Audit Findings Recap

From EPIC-11-Gameplay-Domain-Audit.md:

### Current MythHunter Implementation

**InventoryComponent.cs:**
```csharp
public struct InventoryComponent
{
    public List<string> Items;           // Generic items (IDs only)
    public EquippedItems EquippedItems;  // Armor + Weapons + Artifact
    public Bag Bag;                      // Consumables with slots
}
```

**Observations:**
- ✅ Container concept exists (Items list, Bag)
- ✅ Equipment slots concept exists (Armor, Weapons, Artifact)
- ✅ Capacity constraints (Bag.Slots)
- ❌ Items are just string IDs (no item instance objects)
- ❌ Fixed slot types (Boots, Gloves, Helmet, etc.)
- ❌ No stacking logic (UI has Quantity but not used)
- ❌ Single bag only (no multi-container support)

**Universal Patterns Identified:**
1. **Container with capacity** (Bag has Slots limit)
2. **Equipment slots by type** (Armor vs Weapons vs Artifact)
3. **Item references** (string IDs can become generic TItem)
4. **Add/Remove operations** (implied by List<string>)

**Game-Specific Hardcoding:**
1. Specific slot names (Boots, Gloves, Chest, Helmet)
2. CurrentWeapon tracking (weapon switching logic)
3. "Elixir" category
4. Fixed 10-item UI limit

---

## 3. Framework Design

### 3.1 Core Interfaces

#### IInventoryContainer<TItem>

Generic item container with capacity constraints.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Generic item container with capacity management.
    /// </summary>
    /// <typeparam name="TItem">Type of items stored (use struct or int for IDs)</typeparam>
    public interface IInventoryContainer<TItem>
    {
        /// <summary>
        /// Maximum number of items this container can hold.
        /// -1 for unlimited capacity.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Current number of items in the container.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Whether the container is at maximum capacity.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Whether the container is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds an item to the container.
        /// </summary>
        /// <returns>True if item was added, false if container is full</returns>
        bool Add(TItem item);

        /// <summary>
        /// Removes an item from the container.
        /// </summary>
        /// <returns>True if item was removed, false if item not found</returns>
        bool Remove(TItem item);

        /// <summary>
        /// Removes item at specific index.
        /// </summary>
        /// <returns>True if item was removed, false if index invalid</returns>
        bool RemoveAt(int index);

        /// <summary>
        /// Checks if container contains an item.
        /// </summary>
        bool Contains(TItem item);

        /// <summary>
        /// Gets item at specific index.
        /// </summary>
        /// <returns>Item at index, or default(TItem) if invalid</returns>
        TItem GetAt(int index);

        /// <summary>
        /// Clears all items from the container.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets all items in the container.
        /// </summary>
        /// <returns>Read-only collection of items</returns>
        System.Collections.Generic.IReadOnlyList<TItem> GetAllItems();
    }
}
```

**Design Rationale:**
- Generic `TItem` allows string IDs (current MythHunter) or item instances (Phase 2)
- Capacity of -1 enables unlimited containers (quest items, key items)
- Index-based access for UI rendering (slot 0, slot 1, etc.)
- IReadOnlyList prevents external modification

#### IEquipmentSlot<TSlotType, TItem>

Generic equipment slot with type constraints.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Generic equipment slot for equippable items.
    /// </summary>
    /// <typeparam name="TSlotType">Enum type for slot categories</typeparam>
    /// <typeparam name="TItem">Type of items that can be equipped</typeparam>
    public interface IEquipmentSlot<TSlotType, TItem> where TSlotType : System.Enum
    {
        /// <summary>
        /// The type/category of this equipment slot.
        /// </summary>
        TSlotType SlotType { get; }

        /// <summary>
        /// Currently equipped item (default(TItem) if empty).
        /// </summary>
        TItem EquippedItem { get; }

        /// <summary>
        /// Whether this slot has an item equipped.
        /// </summary>
        bool IsOccupied { get; }

        /// <summary>
        /// Equips an item to this slot.
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <returns>Previously equipped item (default if slot was empty)</returns>
        TItem Equip(TItem item);

        /// <summary>
        /// Unequips the current item.
        /// </summary>
        /// <returns>The unequipped item (default if slot was empty)</returns>
        TItem Unequip();

        /// <summary>
        /// Checks if an item can be equipped to this slot.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if item can be equipped</returns>
        bool CanEquip(TItem item);
    }
}
```

**Design Rationale:**
- `TSlotType : Enum` constraint enables game-specific enums (ArmorSlot, WeaponSlot, etc.)
- `TItem` generic allows string IDs (Phase 1) or item objects (Phase 2)
- `CanEquip` validation hook for Phase 2 (stat requirements, item type restrictions)
- Equip returns previous item (for auto-swap logic)

#### IItemStack

Item stacking contract for stackable items.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Represents a stack of identical items.
    /// </summary>
    public interface IItemStack
    {
        /// <summary>
        /// The item being stacked (ID, instance, or definition).
        /// </summary>
        object Item { get; }

        /// <summary>
        /// Current stack quantity.
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// Maximum stack size. -1 for unlimited.
        /// </summary>
        int MaxStackSize { get; }

        /// <summary>
        /// Whether this stack is at maximum capacity.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Adds quantity to this stack.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <returns>Amount successfully added (may be less if stack is full)</returns>
        int AddToStack(int amount);

        /// <summary>
        /// Removes quantity from this stack.
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount successfully removed (may be less if insufficient)</returns>
        int RemoveFromStack(int amount);

        /// <summary>
        /// Splits this stack into two stacks.
        /// </summary>
        /// <param name="splitAmount">Quantity to split off</param>
        /// <returns>New stack with split amount, or null if invalid</returns>
        IItemStack Split(int splitAmount);

        /// <summary>
        /// Merges another stack into this one.
        /// </summary>
        /// <param name="other">Stack to merge</param>
        /// <returns>Remaining quantity that couldn't be merged (0 if all merged)</returns>
        int Merge(IItemStack other);
    }
}
```

**Design Rationale:**
- `object Item` for maximum flexibility (Phase 1: string, Phase 2: ItemInstance<T>)
- Split/Merge operations for inventory management UIs
- Returns values indicate success/overflow (important for UI feedback)

### 3.2 Component Design

#### InventoryContainer<TItem>

Default implementation of IInventoryContainer<TItem>.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Default implementation of IInventoryContainer.
    /// Uses a List for storage.
    /// </summary>
    public class InventoryContainer<TItem> : IInventoryContainer<TItem>
    {
        private readonly List<TItem> _items;
        private readonly int _capacity;

        public InventoryContainer(int capacity = -1)
        {
            _capacity = capacity;
            _items = capacity > 0 ? new List<TItem>(capacity) : new List<TItem>();
        }

        public int Capacity => _capacity;
        public int Count => _items.Count;
        public bool IsFull => _capacity > 0 && _items.Count >= _capacity;
        public bool IsEmpty => _items.Count == 0;

        public bool Add(TItem item)
        {
            if (IsFull) return false;
            _items.Add(item);
            return true;
        }

        public bool Remove(TItem item)
        {
            return _items.Remove(item);
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return false;
            _items.RemoveAt(index);
            return true;
        }

        public bool Contains(TItem item)
        {
            return _items.Contains(item);
        }

        public TItem GetAt(int index)
        {
            if (index < 0 || index >= _items.Count) return default(TItem);
            return _items[index];
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IReadOnlyList<TItem> GetAllItems()
        {
            return _items.AsReadOnly();
        }
    }
}
```

**Design Notes:**
- Simple List<TItem> backing store
- Pre-allocates capacity if known (performance)
- Validates index bounds

#### EquipmentSlot<TSlotType, TItem>

Default implementation of IEquipmentSlot<TSlotType, TItem>.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Default implementation of IEquipmentSlot.
    /// </summary>
    public class EquipmentSlot<TSlotType, TItem> : IEquipmentSlot<TSlotType, TItem>
        where TSlotType : System.Enum
    {
        private TItem _equippedItem;
        private readonly TSlotType _slotType;
        private readonly Func<TItem, bool> _canEquipValidator;

        public EquipmentSlot(TSlotType slotType, Func<TItem, bool> canEquipValidator = null)
        {
            _slotType = slotType;
            _equippedItem = default(TItem);
            _canEquipValidator = canEquipValidator;
        }

        public TSlotType SlotType => _slotType;
        public TItem EquippedItem => _equippedItem;
        public bool IsOccupied => !EqualityComparer<TItem>.Default.Equals(_equippedItem, default(TItem));

        public TItem Equip(TItem item)
        {
            TItem previous = _equippedItem;
            _equippedItem = item;
            return previous;
        }

        public TItem Unequip()
        {
            TItem previous = _equippedItem;
            _equippedItem = default(TItem);
            return previous;
        }

        public bool CanEquip(TItem item)
        {
            return _canEquipValidator?.Invoke(item) ?? true;
        }
    }
}
```

**Design Notes:**
- Optional validation delegate (for Phase 2: stat requirements, item type checks)
- Uses EqualityComparer for default(TItem) comparison
- Simple field storage

#### ItemStack

Default implementation of IItemStack.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Default implementation of IItemStack.
    /// </summary>
    public class ItemStack : IItemStack
    {
        private object _item;
        private int _quantity;
        private readonly int _maxStackSize;

        public ItemStack(object item, int quantity = 1, int maxStackSize = 99)
        {
            _item = item;
            _quantity = quantity > 0 ? quantity : 1;
            _maxStackSize = maxStackSize;
        }

        public object Item => _item;
        public int Quantity => _quantity;
        public int MaxStackSize => _maxStackSize;
        public bool IsFull => _maxStackSize > 0 && _quantity >= _maxStackSize;

        public int AddToStack(int amount)
        {
            if (amount <= 0) return 0;

            int spaceAvailable = _maxStackSize > 0 ? _maxStackSize - _quantity : int.MaxValue;
            int amountToAdd = System.Math.Min(amount, spaceAvailable);

            _quantity += amountToAdd;
            return amountToAdd;
        }

        public int RemoveFromStack(int amount)
        {
            if (amount <= 0) return 0;

            int amountToRemove = System.Math.Min(amount, _quantity);
            _quantity -= amountToRemove;
            return amountToRemove;
        }

        public IItemStack Split(int splitAmount)
        {
            if (splitAmount <= 0 || splitAmount >= _quantity) return null;

            int removed = RemoveFromStack(splitAmount);
            if (removed == 0) return null;

            return new ItemStack(_item, removed, _maxStackSize);
        }

        public int Merge(IItemStack other)
        {
            if (other == null || !_item.Equals(other.Item)) return other?.Quantity ?? 0;

            int amountAdded = AddToStack(other.Quantity);
            int remaining = other.Quantity - amountAdded;

            return remaining;
        }
    }
}
```

**Design Notes:**
- MaxStackSize of -1 allows unlimited stacking
- Split creates new ItemStack (caller responsible for cleanup)
- Merge returns overflow (caller handles remaining quantity)

### 3.3 Component Integration (ECS)

#### InventoryComponent<TItem, TSlotType>

RPG Framework ECS component for inventory management.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Generic inventory component for entities.
    /// Stores containers and equipment slots.
    /// </summary>
    public struct InventoryComponent<TItem, TSlotType> : RPGFramework.ECS.IComponent
        where TSlotType : System.Enum
    {
        /// <summary>
        /// Primary container (backpack, bag, etc.)
        /// </summary>
        public IInventoryContainer<TItem> PrimaryContainer;

        /// <summary>
        /// Equipment slots keyed by slot type.
        /// </summary>
        public System.Collections.Generic.Dictionary<TSlotType, IEquipmentSlot<TSlotType, TItem>> EquipmentSlots;

        /// <summary>
        /// Additional named containers (bank, quest items, etc.)
        /// Optional for most entities.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, IInventoryContainer<TItem>> AdditionalContainers;

        /// <summary>
        /// Factory method to create an entity with basic inventory.
        /// </summary>
        public static InventoryComponent<TItem, TSlotType> Create(int primaryCapacity, params TSlotType[] equipmentSlotTypes)
        {
            var component = new InventoryComponent<TItem, TSlotType>
            {
                PrimaryContainer = new InventoryContainer<TItem>(primaryCapacity),
                EquipmentSlots = new System.Collections.Generic.Dictionary<TSlotType, IEquipmentSlot<TSlotType, TItem>>(),
                AdditionalContainers = new System.Collections.Generic.Dictionary<string, IInventoryContainer<TItem>>()
            };

            foreach (var slotType in equipmentSlotTypes)
            {
                component.EquipmentSlots[slotType] = new EquipmentSlot<TSlotType, TItem>(slotType);
            }

            return component;
        }
    }
}
```

**Design Notes:**
- Primary container for general inventory (bag, backpack)
- Equipment slots as dictionary (flexible slot types)
- Additional containers for banks, quest items, etc.
- Factory method for easy construction

### 3.4 Event Definitions

#### ItemAddedEvent<TItem>

Raised when an item is added to a container.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    /// <summary>
    /// Event raised when an item is added to a container.
    /// </summary>
    public struct ItemAddedEvent<TItem> : RPGFramework.Core.IEvent
    {
        public int EntityId;
        public string ContainerName;
        public TItem Item;
        public int Quantity;
        public float Timestamp;
    }
}
```

#### ItemRemovedEvent<TItem>

Raised when an item is removed from a container.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    public struct ItemRemovedEvent<TItem> : RPGFramework.Core.IEvent
    {
        public int EntityId;
        public string ContainerName;
        public TItem Item;
        public int Quantity;
        public float Timestamp;
    }
}
```

#### ItemEquippedEvent<TSlotType, TItem>

Raised when an item is equipped.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    public struct ItemEquippedEvent<TSlotType, TItem> : RPGFramework.Core.IEvent
        where TSlotType : System.Enum
    {
        public int EntityId;
        public TSlotType SlotType;
        public TItem EquippedItem;
        public TItem PreviousItem;
        public float Timestamp;
    }
}
```

#### ItemUnequippedEvent<TSlotType, TItem>

Raised when an item is unequipped.

```csharp
namespace RPGFramework.Inventory.Runtime
{
    public struct ItemUnequippedEvent<TSlotType, TItem> : RPGFramework.Core.IEvent
        where TSlotType : System.Enum
    {
        public int EntityId;
        public TSlotType SlotType;
        public TItem UnequippedItem;
        public float Timestamp;
    }
}
```

**Design Notes:**
- Events use generic types for flexibility
- Timestamp for event ordering/logging
- ContainerName for multi-container scenarios

---

## 4. Integration with EPIC 10

### 4.1 Stat Modifiers (Phase 2 Dependency)

Equipment will provide stat bonuses in Phase 2:

```csharp
// Future Phase 2 integration example:
public struct EquipmentItem
{
    public string ItemId;
    public List<StatModifier<MyStatType, int>> StatModifiers;
}

// When equipped:
ItemEquippedEvent<ArmorSlot, EquipmentItem> evt = ...;
foreach (var mod in evt.EquippedItem.StatModifiers)
{
    statContainer.AddModifier(mod); // EPIC 10 Phase 2
}
```

### 4.2 Resource Restoration (Current)

Consumables can restore resources (HP, MP) via events:

```csharp
// Phase 1: Item used event
ItemRemovedEvent<string> evt = new ItemRemovedEvent<string>
{
    EntityId = heroId,
    ContainerName = "primary",
    Item = "health_potion",
    Quantity = 1
};

// Game layer listens and applies effect:
if (evt.Item == "health_potion")
{
    resourceComponent.Restore(ResourceType.Health, 50); // EPIC 10 Phase 1
}
```

### 4.3 No Direct Dependencies

Phase 1 has **zero dependencies** on EPIC 10 modules. Integration happens via:
- Events (decoupled)
- Game layer adapters (Phase 6)

---

## 5. Test Coverage Plan

### 5.1 Test Categories

**Total: 20-25 tests**

#### Category 1: Container Tests (8 tests)

1. `AddItem_ToEmptyContainer_ShouldSucceed`
2. `AddItem_ToFullContainer_ShouldFail`
3. `RemoveItem_ExistingItem_ShouldSucceed`
4. `RemoveItem_NonExistentItem_ShouldFail`
5. `RemoveAt_ValidIndex_ShouldRemoveItem`
6. `RemoveAt_InvalidIndex_ShouldFail`
7. `Clear_PopulatedContainer_ShouldEmpty`
8. `Contains_ExistingItem_ShouldReturnTrue`

#### Category 2: Equipment Slot Tests (6 tests)

1. `Equip_ToEmptySlot_ShouldSucceed`
2. `Equip_ToOccupiedSlot_ShouldReplaceAndReturnPrevious`
3. `Unequip_OccupiedSlot_ShouldReturnItem`
4. `Unequip_EmptySlot_ShouldReturnDefault`
5. `IsOccupied_EmptySlot_ShouldBeFalse`
6. `IsOccupied_FilledSlot_ShouldBeTrue`

#### Category 3: Item Stacking Tests (6 tests)

1. `AddToStack_UnderMax_ShouldIncreaseQuantity`
2. `AddToStack_OverMax_ShouldClamp`
3. `RemoveFromStack_ValidAmount_ShouldDecreaseQuantity`
4. `RemoveFromStack_ExcessAmount_ShouldClamp`
5. `Split_ValidAmount_ShouldCreateNewStack`
6. `Merge_SameItem_ShouldCombineStacks`

#### Category 4: Component Tests (4 tests)

1. `CreateComponent_WithCapacityAndSlots_ShouldInitialize`
2. `PrimaryContainer_AddRemove_ShouldWork`
3. `EquipmentSlots_EquipUnequip_ShouldWork`
4. `AdditionalContainers_AddContainer_ShouldWork`

### 5.2 Test File Structure

**File:** `Assets/_Framework/Inventory/Tests/InventorySystemTests.cs`

```csharp
using NUnit.Framework;
using RPGFramework.Inventory.Runtime;
using System.Collections.Generic;

namespace RPGFramework.Inventory.Tests
{
    [TestFixture]
    public class InventoryContainerTests
    {
        // 8 container tests
    }

    [TestFixture]
    public class EquipmentSlotTests
    {
        // 6 equipment slot tests
    }

    [TestFixture]
    public class ItemStackTests
    {
        // 6 stacking tests
    }

    [TestFixture]
    public class InventoryComponentTests
    {
        // 4 component tests

        private enum TestSlotType { Weapon, Armor, Accessory }
    }
}
```

### 5.3 .NET Test Harness Integration

**Update:** `Build/RPGFramework.Inventory.Tests/RPGFramework.Inventory.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="../../Assets/_Framework/Inventory/Tests/InventorySystemTests.cs" Link="Tests/InventorySystemTests.cs" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../RPGFramework.Inventory.Runtime/RPGFramework.Inventory.Runtime.csproj" />
  </ItemGroup>
</Project>
```

**Update:** `Build/RPGFramework.Inventory.Runtime/RPGFramework.Inventory.Runtime.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="../../Assets/_Framework/Inventory/Runtime/**/*.cs" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../RPGFramework.ECS.Runtime/RPGFramework.ECS.Runtime.csproj" />
    <ProjectReference Include="../RPGFramework.Core.Runtime/RPGFramework.Core.Runtime.csproj" />
  </ItemGroup>
</Project>
```

---

## 6. Assembly Definition

**File:** `Assets/_Framework/Inventory/Runtime/RPGFramework.Inventory.Runtime.asmdef`

```json
{
    "name": "RPGFramework.Inventory.Runtime",
    "rootNamespace": "RPGFramework.Inventory.Runtime",
    "references": [
        "RPGFramework.Core.Runtime",
        "RPGFramework.ECS.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

**Key:** `"noEngineReferences": true` — Enforces zero Unity dependencies.

**File:** `Assets/_Framework/Inventory/Tests/RPGFramework.Inventory.Tests.asmdef`

```json
{
    "name": "RPGFramework.Inventory.Tests",
    "rootNamespace": "RPGFramework.Inventory.Tests",
    "references": [
        "RPGFramework.Inventory.Runtime",
        "RPGFramework.Core.Runtime",
        "RPGFramework.ECS.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

## 7. File Structure

```
Assets/_Framework/Inventory/
├── Runtime/
│   ├── RPGFramework.Inventory.Runtime.asmdef
│   ├── Contracts/
│   │   ├── IInventoryContainer.cs
│   │   ├── IEquipmentSlot.cs
│   │   └── IItemStack.cs
│   ├── Components/
│   │   └── InventoryComponent.cs
│   ├── Implementations/
│   │   ├── InventoryContainer.cs
│   │   ├── EquipmentSlot.cs
│   │   └── ItemStack.cs
│   └── Events/
│       ├── ItemAddedEvent.cs
│       ├── ItemRemovedEvent.cs
│       ├── ItemEquippedEvent.cs
│       └── ItemUnequippedEvent.cs
└── Tests/
    ├── RPGFramework.Inventory.Tests.asmdef
    └── InventorySystemTests.cs
```

**Total Runtime Files:** 12
- 3 interfaces (Contracts/)
- 1 component (Components/)
- 3 implementations (Implementations/)
- 4 events (Events/)
- 1 assembly definition

**Total Test Files:** 2
- 1 test class (24 tests)
- 1 assembly definition

---

## 8. Implementation Checklist

### Step 1: Create Directory Structure
- [ ] `Assets/_Framework/Inventory/Runtime/`
- [ ] `Assets/_Framework/Inventory/Runtime/Contracts/`
- [ ] `Assets/_Framework/Inventory/Runtime/Components/`
- [ ] `Assets/_Framework/Inventory/Runtime/Implementations/`
- [ ] `Assets/_Framework/Inventory/Runtime/Events/`
- [ ] `Assets/_Framework/Inventory/Tests/`

### Step 2: Create Contracts (Interfaces)
- [ ] `IInventoryContainer.cs`
- [ ] `IEquipmentSlot.cs`
- [ ] `IItemStack.cs`

### Step 3: Create Implementations
- [ ] `InventoryContainer.cs`
- [ ] `EquipmentSlot.cs`
- [ ] `ItemStack.cs`

### Step 4: Create Component
- [ ] `InventoryComponent.cs`

### Step 5: Create Events
- [ ] `ItemAddedEvent.cs`
- [ ] `ItemRemovedEvent.cs`
- [ ] `ItemEquippedEvent.cs`
- [ ] `ItemUnequippedEvent.cs`

### Step 6: Create Assembly Definitions
- [ ] `RPGFramework.Inventory.Runtime.asmdef`
- [ ] `RPGFramework.Inventory.Tests.asmdef`

### Step 7: Generate .meta Files
- [ ] All 12 Runtime .cs files
- [ ] 2 .asmdef files
- [ ] All directories

### Step 8: Create Tests
- [ ] `InventorySystemTests.cs` (24 tests)

### Step 9: Update .NET Build
- [ ] `Build/RPGFramework.Inventory.Runtime/RPGFramework.Inventory.Runtime.csproj`
- [ ] `Build/RPGFramework.Inventory.Tests/RPGFramework.Inventory.Tests.csproj`

### Step 10: Validation
- [ ] .NET compile: `dotnet build Build/RPGFramework.Inventory.Runtime/`
- [ ] .NET tests: `dotnet test Build/RPGFramework.Inventory.Tests/`
- [ ] Static boundary check (no Unity references)

### Step 11: Create PR
- [ ] Branch: `feature/epic11-phase1-inventory-system`
- [ ] Commit with proper message
- [ ] Push to remote
- [ ] Create PR with description
- [ ] Wait for CI (GitHub Actions)

### Step 12: Merge
- [ ] CI GREEN
- [ ] Merge to `dev`

---

## 9. Acceptance Criteria

### Functional Requirements

✅ **FR1:** IInventoryContainer<TItem> supports Add/Remove/Clear operations
✅ **FR2:** IInventoryContainer respects capacity constraints
✅ **FR3:** IEquipmentSlot<TSlotType, TItem> supports Equip/Unequip operations
✅ **FR4:** IEquipmentSlot returns previous item on Equip
✅ **FR5:** IItemStack supports stacking up to MaxStackSize
✅ **FR6:** IItemStack supports Split/Merge operations
✅ **FR7:** InventoryComponent<TItem, TSlotType> has PrimaryContainer
✅ **FR8:** InventoryComponent supports multiple EquipmentSlots
✅ **FR9:** InventoryComponent supports AdditionalContainers
✅ **FR10:** Events are raised for Add/Remove/Equip/Unequip

### Non-Functional Requirements

✅ **NFR1:** Zero Unity dependencies (noEngineReferences: true)
✅ **NFR2:** Zero MythHunter dependencies
✅ **NFR3:** .NET 8.0 compatible
✅ **NFR4:** All 24 tests pass (100% success rate)
✅ **NFR5:** Static boundary validation passes
✅ **NFR6:** GitHub Actions CI GREEN

### Documentation Requirements

✅ **DOC1:** XML documentation on all public interfaces
✅ **DOC2:** XML documentation on all public methods
✅ **DOC3:** Implementation plan (this document)
✅ **DOC4:** Audit document (EPIC-11-Gameplay-Domain-Audit.md)

---

## 10. Risks and Mitigations

### Risk 1: Generic Type Complexity

**Risk:** Developers may find `InventoryComponent<TItem, TSlotType>` syntax complex.

**Mitigation:**
- Provide clear usage examples in documentation
- MythHunter adapters will hide generics from game code
- Factory methods simplify construction

### Risk 2: Dictionary Allocations

**Risk:** EquipmentSlots as Dictionary may allocate.

**Mitigation:**
- Small dictionary (< 10 slots typically)
- Pre-allocated at component creation
- Alternative: Fixed array if slots are sequential enum

### Risk 3: Event Spam

**Risk:** ItemAddedEvent fired for every item add (100 items = 100 events).

**Mitigation:**
- Events are opt-in (subscribe only if needed)
- Batch add operations can be added in future
- Performance profiling in Phase 6

---

## 11. Future Extensions (Not Phase 1)

### Phase 2: Item Definitions
- IItem, IEquipment, IConsumable interfaces
- Item stat modifiers
- Item type restrictions on slots

### Phase 3: Loot Integration
- Loot tables generate items
- Items added to inventory via InventoryContainer.Add

### Phase 4: Ability Integration
- Item-granted abilities (equipment, consumables)
- Ability slots in inventory

### Phase 5: Progression Integration
- Level-gated equipment slots
- Inventory capacity growth

### Phase 6: UI Integration
- MythHunter UI adapters
- Drag-and-drop inventory management
- Visual feedback for events

---

## 12. Summary

Phase 1 extracts inventory container and equipment slot logic into a universal RPG Framework module. The design uses generic types to maximize reusability while maintaining zero Unity dependencies.

**Estimated Effort:** 6-8 hours
**Deliverables:** 12 Runtime files, 1 Test file (24 tests), .NET harness integration
**Dependencies:** None (Phase 2+ will integrate)
**Validation:** .NET tests, static boundary check, GitHub Actions

**Ready for implementation.** ✅
