# MythHunter Dynamic Events Integration

This folder contains MythHunter-specific integrations for the RPG Framework's Dynamic Events System.

## Sample Events

### 1. Village Under Attack (Defense)
- **Type**: Defense
- **Difficulty**: Normal
- **Duration**: 15 minutes
- **Objectives**:
  - Defeat 50 attacking monsters (required)
  - Protect 5 village structures (required)
  - Rescue 10 trapped villagers (optional)
- **Tags**: combat, defense, heroic

### 2. Dragon Sighting (Boss)
- **Type**: Boss
- **Difficulty**: Epic
- **Duration**: 20 minutes
- **Objectives**:
  - Defeat the Ancient Dragon (required)
  - Destroy 3 dragon nests (optional)
- **Tags**: boss, epic, dragon, legendary_reward

### 3. Caravan Escort (Escort)
- **Type**: Escort
- **Difficulty**: Normal
- **Duration**: 10 minutes
- **Objectives**:
  - Escort caravan to destination (required)
  - Defeat 25 ambushing bandits (required)
  - Keep all 4 wagons intact (optional)
- **Tags**: escort, protection, trade

### 4. Demon Invasion (Invasion Chain)
- **Type**: Invasion (3-wave chain)
- **Difficulty**: Hard → Epic → Legendary
- **Duration**: Variable per wave

#### Wave 1
- Close 3 demon portals (required)
- Defeat 30 invading demons (required)

#### Wave 2
- Defeat 3 Demon Lieutenants (required)
- Destroy 5 summoning circles (required)

#### Wave 3 (Final)
- Defeat the Demon Lord (required)
- Seal the main portal (required)

### 5. Ancient Ruins Discovered (Exploration)
- **Type**: Exploration
- **Difficulty**: Normal
- **Duration**: 20 minutes
- **Objectives**:
  - Explore 8 ruin chambers (required)
  - Collect 15 ancient artifacts (required)
  - Defeat 10 awakened guardians (optional)
  - Decipher 5 ancient inscriptions (optional)
- **Tags**: exploration, discovery, ancient, lore

## Usage

```csharp
// Register all MythHunter event templates
var eventManager = new EventManager();
MythHunterEventTemplates.RegisterAll(eventManager);

// Create a specific event
var villageEvent = eventManager.CreateEvent("village_under_attack", "village_meadowbrook");

// Create the demon invasion chain
var demonChain = MythHunterEventTemplates.CreateDemonInvasionChain();
```

## Features Demonstrated

1. **Multiple Event Types**: Defense, Boss, Escort, Invasion, Exploration
2. **Difficulty Scaling**: From Trivial to Legendary
3. **Event Chains**: Demon Invasion demonstrates 3-wave progressive chain
4. **Contribution Tracking**: All objectives track individual player contributions
5. **Optional Objectives**: Bonus objectives for extra rewards
6. **Time Limits**: Each event has appropriate time constraints
7. **Participation Radius**: Different events have different engagement ranges
8. **Tags**: Flexible categorization and filtering system

## Integration Points

- **Combat System**: Kill objectives integrate with combat
- **Quest System**: Similar objective tracking patterns
- **Loot System**: Reward distribution based on participation
- **Faction System**: Events can affect faction reputation
- **World State**: Events can trigger based on world conditions
