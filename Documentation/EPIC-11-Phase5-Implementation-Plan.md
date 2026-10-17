# EPIC 11 Phase 5: Progression System — Implementation Plan

## Overview

Extract progression/leveling system from MythHunter into universal RPG Framework module. Provides XP tracking, level calculations, stat growth formulas, and unlock management — all without Unity dependencies.

**Target:** `RPGFramework.Progression.Runtime` assembly (.NET 8.0, zero dependencies)

## Core Concepts

### Progression System Features
1. **Experience Points (XP)** — Accumulation and tracking
2. **Level Calculations** — XP requirements per level
3. **Stat Growth** — Automatic stat increases on level up
4. **Unlock System** — Features/abilities unlocked at certain levels
5. **Level Tables** — Configurable XP curves (linear, exponential, custom)

### Design Principles
- **Standalone Module:** Zero dependencies on other Framework components
- **Callback Pattern:** Game layer provides stat modification, unlock activation
- **Flexible Formulas:** Support multiple XP curve types
- **Event-Driven:** Level up, XP gain, unlock events

## Architecture

### Module Structure
```
Assets/_Framework/Progression/
├── Runtime/
│   ├── Contracts/
│   │   ├── IProgressionTracker.cs
│   │   ├── ILevelTable.cs
│   │   ├── IStatGrowthFormula.cs
│   │   ├── IUnlockCondition.cs
│   │   └── IUnlockManager.cs
│   ├── Data/
│   │   ├── ExperienceData.cs
│   │   ├── LevelInfo.cs
│   │   ├── StatGrowthData.cs
│   │   ├── UnlockData.cs
│   │   ├── LevelCurveType.cs
│   │   └── ProgressionResult.cs
│   ├── Implementations/
│   │   ├── ProgressionTracker.cs
│   │   ├── LevelTable.cs
│   │   ├── LinearStatGrowth.cs
│   │   ├── ExponentialStatGrowth.cs
│   │   ├── UnlockCondition.cs
│   │   └── UnlockManager.cs
│   ├── Events/
│   │   ├── ExperienceGainedEvent.cs
│   │   ├── LevelUpEvent.cs
│   │   ├── StatIncreasedEvent.cs
│   │   └── UnlockAchievedEvent.cs
│   └── RPGFramework.Progression.Runtime.asmdef
└── Tests/
    ├── ProgressionSystemTests.cs
    └── RPGFramework.Progression.Tests.asmdef
```

## Data Structures

### ExperienceData Structure
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Represents current experience and level state.
    /// </summary>
    public struct ExperienceData
    {
        /// <summary>Current level</summary>
        public int Level;

        /// <summary>Current XP in this level</summary>
        public long CurrentXP;

        /// <summary>Total XP accumulated</summary>
        public long TotalXP;

        /// <summary>XP required for next level</summary>
        public long XPToNextLevel;

        /// <summary>Progress to next level (0.0 - 1.0)</summary>
        public float ProgressToNextLevel => XPToNextLevel > 0
            ? (float)CurrentXP / XPToNextLevel
            : 1.0f;

        public ExperienceData(int level, long currentXP, long totalXP, long xpToNextLevel)
        {
            Level = level;
            CurrentXP = currentXP;
            TotalXP = totalXP;
            XPToNextLevel = xpToNextLevel;
        }
    }
}
```

### LevelInfo Structure
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Information about a specific level.
    /// </summary>
    public struct LevelInfo
    {
        /// <summary>Level number</summary>
        public int Level;

        /// <summary>Total XP required to reach this level</summary>
        public long TotalXPRequired;

        /// <summary>XP required from previous level</summary>
        public long XPFromPrevious;

        public LevelInfo(int level, long totalXPRequired, long xpFromPrevious)
        {
            Level = level;
            TotalXPRequired = totalXPRequired;
            XPFromPrevious = xpFromPrevious;
        }
    }
}
```

### StatGrowthData Structure
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Defines how a stat grows on level up.
    /// </summary>
    public struct StatGrowthData
    {
        /// <summary>Stat identifier (e.g., "Strength", "HP")</summary>
        public string StatId;

        /// <summary>Base value at level 1</summary>
        public float BaseValue;

        /// <summary>Growth per level (flat amount)</summary>
        public float GrowthPerLevel;

        /// <summary>Growth percentage per level (multiplicative)</summary>
        public float GrowthPercentage;

        /// <summary>Growth curve type</summary>
        public GrowthCurveType CurveType;

        public StatGrowthData(string statId, float baseValue, float growthPerLevel,
            float growthPercentage = 0f, GrowthCurveType curveType = GrowthCurveType.Linear)
        {
            StatId = statId;
            BaseValue = baseValue;
            GrowthPerLevel = growthPerLevel;
            GrowthPercentage = growthPercentage;
            CurveType = curveType;
        }

        /// <summary>Calculate stat value at specific level</summary>
        public float CalculateAtLevel(int level)
        {
            if (level <= 1) return BaseValue;

            int levelsGained = level - 1;

            switch (CurveType)
            {
                case GrowthCurveType.Linear:
                    return BaseValue + (GrowthPerLevel * levelsGained);

                case GrowthCurveType.Exponential:
                    float value = BaseValue;
                    for (int i = 0; i < levelsGained; i++)
                    {
                        value += GrowthPerLevel;
                        value *= (1f + GrowthPercentage);
                    }
                    return value;

                case GrowthCurveType.Polynomial:
                    // value = base + (growth * level^2)
                    return BaseValue + (GrowthPerLevel * levelsGained * levelsGained);

                default:
                    return BaseValue;
            }
        }
    }

    public enum GrowthCurveType
    {
        Linear,
        Exponential,
        Polynomial,
        Custom
    }
}
```

### UnlockData Structure
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Represents something that can be unlocked at a certain level.
    /// </summary>
    public struct UnlockData
    {
        /// <summary>Unique unlock identifier</summary>
        public string UnlockId;

        /// <summary>Display name</summary>
        public string Name;

        /// <summary>Description</summary>
        public string Description;

        /// <summary>Level required to unlock</summary>
        public int RequiredLevel;

        /// <summary>Unlock type (Ability, Feature, Item, etc.)</summary>
        public string UnlockType;

        /// <summary>Additional data for unlock (JSON, parameters, etc.)</summary>
        public string UnlockData;

        public UnlockData(string unlockId, string name, int requiredLevel, string unlockType = "Feature")
        {
            UnlockId = unlockId;
            Name = name;
            Description = string.Empty;
            RequiredLevel = requiredLevel;
            UnlockType = unlockType;
            UnlockData = string.Empty;
        }
    }
}
```

### LevelCurveType Enumeration
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Types of XP curves for level progression.
    /// </summary>
    public enum LevelCurveType
    {
        /// <summary>Linear: XP = base * level</summary>
        Linear,

        /// <summary>Exponential: XP = base * (multiplier ^ level)</summary>
        Exponential,

        /// <summary>Polynomial: XP = base * (level ^ power)</summary>
        Polynomial,

        /// <summary>Custom table with predefined XP values</summary>
        Custom
    }
}
```

### ProgressionResult Structure
```csharp
namespace RPGFramework.Progression
{
    /// <summary>
    /// Result of adding experience.
    /// </summary>
    public struct ProgressionResult
    {
        /// <summary>Previous level</summary>
        public int OldLevel;

        /// <summary>New level after XP gain</summary>
        public int NewLevel;

        /// <summary>Number of levels gained</summary>
        public int LevelsGained => NewLevel - OldLevel;

        /// <summary>XP amount added</summary>
        public long XPAdded;

        /// <summary>New experience data</summary>
        public ExperienceData ExperienceData;

        /// <summary>Whether level up occurred</summary>
        public bool LeveledUp => LevelsGained > 0;

        public static ProgressionResult Create(int oldLevel, int newLevel, long xpAdded, ExperienceData expData)
        {
            return new ProgressionResult
            {
                OldLevel = oldLevel,
                NewLevel = newLevel,
                XPAdded = xpAdded,
                ExperienceData = expData
            };
        }
    }
}
```

## Core Interfaces

### IProgressionTracker Interface
```csharp
namespace RPGFramework.Progression
{
    public interface IProgressionTracker
    {
        /// <summary>Add experience points</summary>
        ProgressionResult AddExperience(int entityId, long xpAmount);

        /// <summary>Set level directly (for testing/admin)</summary>
        void SetLevel(int entityId, int level);

        /// <summary>Get current experience data</summary>
        ExperienceData GetExperienceData(int entityId);

        /// <summary>Get current level</summary>
        int GetLevel(int entityId);

        /// <summary>Get total XP</summary>
        long GetTotalXP(int entityId);

        /// <summary>Calculate XP required for specific level</summary>
        long GetXPRequiredForLevel(int level);
    }
}
```

### ILevelTable Interface
```csharp
namespace RPGFramework.Progression
{
    public interface ILevelTable
    {
        /// <summary>Maximum level in this table</summary>
        int MaxLevel { get; }

        /// <summary>Get level info for specific level</summary>
        LevelInfo GetLevelInfo(int level);

        /// <summary>Calculate level from total XP</summary>
        int CalculateLevelFromXP(long totalXP);

        /// <summary>Get XP required for next level from current level</summary>
        long GetXPForNextLevel(int currentLevel);

        /// <summary>Get total XP required to reach level</summary>
        long GetTotalXPForLevel(int level);
    }
}
```

### IStatGrowthFormula Interface
```csharp
namespace RPGFramework.Progression
{
    public interface IStatGrowthFormula
    {
        /// <summary>Calculate stat value at specific level</summary>
        float CalculateStatAtLevel(string statId, int level);

        /// <summary>Get all stat changes when leveling up</summary>
        Dictionary<string, float> GetStatChangesForLevelUp(int fromLevel, int toLevel);

        /// <summary>Register stat growth data</summary>
        void RegisterStatGrowth(StatGrowthData growthData);
    }
}
```

### IUnlockManager Interface
```csharp
namespace RPGFramework.Progression
{
    public interface IUnlockManager
    {
        /// <summary>Register unlock</summary>
        void RegisterUnlock(UnlockData unlock);

        /// <summary>Check if unlock is available at level</summary>
        bool IsUnlockAvailable(string unlockId, int level);

        /// <summary>Get all unlocks for specific level</summary>
        List<UnlockData> GetUnlocksForLevel(int level);

        /// <summary>Get all unlocks up to and including level</summary>
        List<UnlockData> GetAllUnlocksUpToLevel(int level);

        /// <summary>Check if entity has unlocked feature</summary>
        bool HasUnlocked(int entityId, string unlockId);

        /// <summary>Mark unlock as achieved for entity</summary>
        void AchieveUnlock(int entityId, string unlockId);
    }
}
```

## Implementation Classes

### ProgressionTracker Class
```csharp
namespace RPGFramework.Progression
{
    public class ProgressionTracker : IProgressionTracker
    {
        private readonly ILevelTable _levelTable;
        private readonly Dictionary<int, ExperienceData> _entityExperience = new Dictionary<int, ExperienceData>();

        public ProgressionTracker(ILevelTable levelTable)
        {
            _levelTable = levelTable;
        }

        public ProgressionResult AddExperience(int entityId, long xpAmount)
        {
            if (!_entityExperience.TryGetValue(entityId, out var expData))
            {
                expData = new ExperienceData(1, 0, 0, _levelTable.GetXPForNextLevel(1));
            }

            int oldLevel = expData.Level;
            long newTotalXP = expData.TotalXP + xpAmount;
            int newLevel = _levelTable.CalculateLevelFromXP(newTotalXP);

            if (newLevel > _levelTable.MaxLevel)
                newLevel = _levelTable.MaxLevel;

            long currentLevelXP = _levelTable.GetTotalXPForLevel(newLevel);
            long nextLevelXP = newLevel < _levelTable.MaxLevel
                ? _levelTable.GetTotalXPForLevel(newLevel + 1)
                : currentLevelXP;

            long xpInCurrentLevel = newTotalXP - currentLevelXP;
            long xpToNextLevel = nextLevelXP - currentLevelXP;

            var newExpData = new ExperienceData(newLevel, xpInCurrentLevel, newTotalXP, xpToNextLevel);
            _entityExperience[entityId] = newExpData;

            return ProgressionResult.Create(oldLevel, newLevel, xpAmount, newExpData);
        }

        public void SetLevel(int entityId, int level)
        {
            long totalXP = _levelTable.GetTotalXPForLevel(level);
            long xpToNext = level < _levelTable.MaxLevel
                ? _levelTable.GetXPForNextLevel(level)
                : 0;

            _entityExperience[entityId] = new ExperienceData(level, 0, totalXP, xpToNext);
        }

        public ExperienceData GetExperienceData(int entityId)
        {
            return _entityExperience.TryGetValue(entityId, out var data)
                ? data
                : new ExperienceData(1, 0, 0, _levelTable.GetXPForNextLevel(1));
        }

        public int GetLevel(int entityId)
        {
            return GetExperienceData(entityId).Level;
        }

        public long GetTotalXP(int entityId)
        {
            return GetExperienceData(entityId).TotalXP;
        }

        public long GetXPRequiredForLevel(int level)
        {
            return _levelTable.GetTotalXPForLevel(level);
        }
    }
}
```

## Events

### ExperienceGainedEvent
```csharp
public struct ExperienceGainedEvent
{
    public int EntityId;
    public long XPAmount;
    public long TotalXP;
    public float Timestamp;
}
```

### LevelUpEvent
```csharp
public struct LevelUpEvent
{
    public int EntityId;
    public int OldLevel;
    public int NewLevel;
    public int LevelsGained;
    public float Timestamp;
}
```

### StatIncreasedEvent
```csharp
public struct StatIncreasedEvent
{
    public int EntityId;
    public string StatId;
    public float OldValue;
    public float NewValue;
    public int Level;
    public float Timestamp;
}
```

### UnlockAchievedEvent
```csharp
public struct UnlockAchievedEvent
{
    public int EntityId;
    public string UnlockId;
    public string UnlockType;
    public int Level;
    public float Timestamp;
}
```

## Test Plan (25+ tests)

### Experience & Leveling Tests (8 tests)
1. `AddExperience_ValidAmount_IncreasesTotalXP`
2. `AddExperience_MultipleGains_AccumulatesCorrectly`
3. `AddExperience_EnoughForLevelUp_IncreasesLevel`
4. `AddExperience_EnoughForMultipleLevels_JumpsCorrectly`
5. `AddExperience_AtMaxLevel_CapsAtMax`
6. `SetLevel_ValidLevel_UpdatesCorrectly`
7. `GetExperienceData_ReturnsCorrectProgress`
8. `ProgressToNextLevel_CalculatesPercentageCorrectly`

### Level Table Tests (6 tests)
1. `LinearLevelTable_CalculatesCorrectXP`
2. `ExponentialLevelTable_CalculatesCorrectXP`
3. `CalculateLevelFromXP_ReturnsCorrectLevel`
4. `GetXPForNextLevel_ReturnsCorrectAmount`
5. `MaxLevel_CapsCorrectly`
6. `CustomLevelTable_UsesProvidedValues`

### Stat Growth Tests (6 tests)
1. `LinearStatGrowth_CalculatesCorrectly`
2. `ExponentialStatGrowth_CalculatesCorrectly`
3. `PolynomialStatGrowth_CalculatesCorrectly`
4. `GetStatChangesForLevelUp_ReturnsAllChanges`
5. `MultipleStats_GrowIndependently`
6. `StatGrowthFormula_WithPercentage_AppliesCorrectly`

### Unlock System Tests (5 tests)
1. `RegisterUnlock_AddsToManager`
2. `IsUnlockAvailable_ChecksLevelRequirement`
3. `GetUnlocksForLevel_ReturnsCorrectUnlocks`
4. `AchieveUnlock_MarksAsUnlocked`
5. `HasUnlocked_ReturnsCorrectState`

## Success Criteria

- ✅ Zero Unity dependencies (noEngineReferences: true)
- ✅ Zero MythHunter coupling (additive only)
- ✅ Zero Framework dependencies (standalone)
- ✅ 25+ comprehensive tests (100% pass rate)
- ✅ .NET 8.0 test harness passes
- ✅ Static boundary validation passes
- ✅ GitHub Actions CI GREEN
- ✅ Supports multiple XP curve types
- ✅ Flexible stat growth formulas

## Timeline

- **Day 1:** Data structures + Interfaces (4-5 hours)
- **Day 2:** Core implementations + Events (5-6 hours)
- **Day 3:** Tests + Build infrastructure + CI (5-6 hours)
- **Total:** ~15-17 hours over 3 days

---

**Status:** Ready for implementation
**Target Completion:** EPIC 11 Phase 5
**Next Phase:** Phase 6 (Integration with MythHunter)
