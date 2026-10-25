# Reference Game Design Document

**Purpose**: Demonstrate RPG Framework capabilities through a complete, playable demo game.

**Target Duration**: 30-60 minutes of gameplay
**Genre**: Fantasy Action RPG
**Scope**: Tutorial + 2 zones, 8-10 quests, 5 enemy types, full progression system

---

## Core Pillars

1. **Showcase Framework Systems**: Every Framework module must be used
2. **Complete Gameplay Loop**: Quest → Combat → Loot → Inventory → Progression
3. **Quality over Quantity**: Small but polished
4. **Reusable Template**: Other developers can use as starting point

---

## Game Overview

### Setting
**"Goblin Valley"** - A small valley infested by goblins threatening a nearby village.

### Player Goal
Clear the goblin threat, rescue villagers, defeat the Goblin King boss.

### Core Loop
1. Accept quest from NPC
2. Travel to location
3. Combat enemies
4. Collect loot
5. Return and complete quest
6. Level up and upgrade equipment
7. Unlock next quest

---

## Zones & Locations

### Zone 1: Thornwood Village (Safe Zone)
- **NPCs**: Elder Garrick (quest giver), Merchant Mira (shop), Blacksmith Torvin
- **Functions**: Quest hub, equipment shop, safe respawn point
- **Size**: Small (200x200 units)

### Zone 2: Goblin Valley (Combat Zone)
- **Sub-areas**:
  - Valley Entrance (levels 1-2)
  - Goblin Camp (levels 3-4)
  - Ancient Ruins (levels 4-5)
  - Goblin King's Throne (boss area, level 5)
- **Size**: Medium (500x500 units)

---

## Quest Design

### Tutorial Quest Chain (3 quests)

**Q1: "Welcome to Thornwood"**
- Type: Talk to NPC
- Objective: Speak with Elder Garrick, Merchant Mira, Blacksmith Torvin
- Reward: 50 XP, 10 gold
- Purpose: Introduce UI and NPCs

**Q2: "First Blood"**
- Type: Kill quest
- Objective: Kill 3 goblins in Valley Entrance
- Reward: 100 XP, 25 gold, Health Potion x2
- Purpose: Introduce combat and loot

**Q3: "Gather Resources"**
- Type: Collection quest
- Objective: Collect 5 goblin ears (100% drop from goblins)
- Reward: 150 XP, 50 gold, Leather Gloves (Common)
- Purpose: Introduce inventory management

### Main Quest Chain (5 quests)

**Q4: "The Goblin Threat"**
- Type: Kill quest
- Prerequisite: Complete Q3
- Objective: Kill 10 goblins (any type)
- Reward: 250 XP, 100 gold, Iron Sword (Uncommon)
- Purpose: Introduce stronger equipment

**Q5: "Rescue the Villagers"**
- Type: Interact quest
- Prerequisite: Complete Q4
- Objective: Free 3 captured villagers in Goblin Camp
- Reward: 300 XP, 150 gold, Health Potion x5
- Purpose: Introduce exploration and navigation

**Q6: "Ancient Power"**
- Type: Explore quest
- Prerequisite: Complete Q5
- Objective: Discover the Ancient Ruins
- Reward: 350 XP, 200 gold, Enchanted Ring (Rare)
- Purpose: Introduce rare loot and area unlocking

**Q7: "Elite Patrol"**
- Type: Kill quest
- Prerequisite: Complete Q6, Level 4 required
- Objective: Defeat 3 Goblin Elites in Ancient Ruins
- Reward: 500 XP, 300 gold, Plate Armor (Rare)
- Purpose: Introduce challenging combat

**Q8: "The Goblin King"**
- Type: Boss quest
- Prerequisite: Complete Q7, Level 5 required
- Objective: Defeat the Goblin King in his throne room
- Reward: 1000 XP, 1000 gold, King's Crown (Epic), Goblin King's Treasure (Legendary)
- Purpose: Final challenge and epic loot showcase

---

## Enemy Design

### Enemy 1: Goblin Scout (Level 1-2)
- **Stats**: 50 HP, 5 Damage, 2 Defense
- **Behavior**: Passive patrol → Aggressive when player in range (10 units)
- **Loot Table**: Gold (5-15), Health Potion Minor (50%), Leather Scrap (30%), Goblin Ear (100% for Q3)
- **AI**: Simple chase → melee attack
- **Spawn**: Valley Entrance (5-7 active)

### Enemy 2: Goblin Warrior (Level 2-3)
- **Stats**: 100 HP, 10 Damage, 5 Defense
- **Behavior**: Aggressive patrol
- **Loot Table**: Gold (15-30), Health Potion (40%), Rusty Dagger (20%)
- **AI**: Chase → melee attack with combo (2 hits)
- **Spawn**: Valley Entrance + Goblin Camp (3-5 active)

### Enemy 3: Goblin Archer (Level 3-4)
- **Stats**: 75 HP, 12 Damage, 3 Defense
- **Behavior**: Ranged attacker, keeps distance
- **Loot Table**: Gold (20-40), Arrows (60%), Leather Boots (15%)
- **AI**: Keep distance (15 units) → ranged attack → retreat if player approaches
- **Spawn**: Goblin Camp (2-3 active)

### Enemy 4: Goblin Elite (Level 4-5)
- **Stats**: 200 HP, 20 Damage, 10 Defense
- **Behavior**: Aggressive, uses abilities
- **Loot Table**: Gold (50-100), Health Potion Greater (70%), Iron Equipment (30%), Enchanted Ring (10%)
- **AI**: Chase → melee combo → special ability (stun attack every 15s)
- **Spawn**: Ancient Ruins (2-3 active)

### Enemy 5: Goblin King (Level 5 Boss)
- **Stats**: 1000 HP, 35 Damage, 20 Defense
- **Behavior**: 3-phase boss fight
  - Phase 1 (100%-60% HP): Melee attacks + summon 2 Goblin Scouts
  - Phase 2 (60%-30% HP): Melee combos + area damage ability
  - Phase 3 (30%-0% HP): Enrage (increased speed + damage)
- **Loot Table**: Guaranteed drops:
  - Gold (500-1000)
  - King's Crown (Epic equipment, +50 HP, +10 Defense)
  - Goblin King's Treasure (Legendary quest item)
  - Boss Soul Fragment
  - XP Orb (1000 XP)
- **AI**: BehaviorTree with phase transitions
- **Spawn**: Goblin King's Throne (single spawn, respawns on quest reset)

---

## Loot Tables (using Loot Table Designer)

### goblin_scout_loot
- Drops: 1-2 items
- Entries:
  - gold_coins (100 weight, 80% chance, 5-15 quantity)
  - health_potion_minor (60 weight, 50% chance)
  - leather_scrap (40 weight, 30% chance)
- Guaranteed: goblin_ear (for quest)

### goblin_warrior_loot
- Drops: 1-3 items
- Entries:
  - gold_coins (100 weight, 90% chance, 15-30 quantity)
  - health_potion (70 weight, 40% chance)
  - rusty_dagger (30 weight, 20% chance)

### goblin_elite_loot
- Drops: 2-4 items
- Entries:
  - gold_coins (100 weight, 100% chance, 50-100 quantity)
  - health_potion_greater (80 weight, 70% chance)
  - iron_sword (40 weight, 30% chance)
  - enchanted_ring (20 weight, 10% chance)

### goblin_king_loot
- Drops: 5-8 items
- Entries:
  - gold_coins (100 weight, 100% chance, 500-1000 quantity)
  - rare_gem (70 weight, 90% chance, 2-5 quantity)
- Guaranteed:
  - kings_crown
  - goblin_kings_treasure
  - boss_soul_fragment
  - exp_orb_huge

---

## Item Database (using Item Database Manager)

### Equipment (Weapons)
- **Rusty Dagger** (Common): +5 Damage, Level 1
- **Iron Sword** (Uncommon): +15 Damage, Level 2
- **Steel Sword** (Rare): +30 Damage, Level 4
- **King's Blade** (Epic): +50 Damage, +20 HP, Level 5

### Equipment (Armor)
- **Leather Gloves** (Common): +5 Defense, Level 1
- **Leather Armor** (Uncommon): +15 Defense, Level 2
- **Plate Armor** (Rare): +30 Defense, +10 HP, Level 4
- **King's Crown** (Epic): +50 HP, +10 Defense, Level 5

### Equipment (Accessories)
- **Simple Ring** (Common): +10 HP, Level 1
- **Enchanted Ring** (Rare): +30 HP, +10 Mana, Level 3
- **Goblin King's Treasure** (Legendary): +100 HP, +50 Mana, +20 Damage, Level 5

### Consumables
- **Health Potion Minor** (Common): Restores 50 HP, 30s cooldown, stackable (99)
- **Health Potion** (Common): Restores 100 HP, 30s cooldown, stackable (99)
- **Health Potion Greater** (Uncommon): Restores 250 HP, 30s cooldown, stackable (99)
- **Mana Potion** (Common): Restores 50 Mana, 30s cooldown, stackable (99)

### Quest Items
- **Goblin Ear** (Quest): Quest item for "Gather Resources"
- **Boss Soul Fragment** (Quest): Quest item for final quest chain

---

## Progression System

### Level Progression (1-5)
- **Level 1**: Start, 0 XP
- **Level 2**: 150 XP (cumulative: 150)
- **Level 3**: 350 XP (cumulative: 500)
- **Level 4**: 600 XP (cumulative: 1100)
- **Level 5**: 900 XP (cumulative: 2000)

### Stat Progression per Level
- **HP**: +50 per level (Base: 100, Level 5: 300)
- **Mana**: +20 per level (Base: 50, Level 5: 130)
- **Damage**: +5 per level (Base: 10, Level 5: 30)
- **Defense**: +3 per level (Base: 5, Level 5: 17)

### Economy Balance
- **Gold Sources**: Quest rewards (10-1000), enemy drops (5-100)
- **Gold Sinks**: Equipment shop (100-500 per item), consumables (10-50 per stack)

---

## UI Panels (All 11 Framework UI Panels)

1. **Character Sheet** (C): View stats, level, XP
2. **Inventory** (I): Manage items, use consumables
3. **Equipment** (E): Equip/unequip gear
4. **Quest Log** (Q): View active/completed quests, track objectives
5. **Loot Panel** (Auto-open): Loot enemy drops
6. **Merchant Shop** (Interact with NPC): Buy/sell items
7. **Dialogue Panel** (Interact with NPC): Quest dialogues
8. **Ability Bar**: Use abilities and consumables (hotkeys 1-6)
9. **Health/Mana Bar**: HUD display
10. **Quest Tracker**: On-screen quest objective display
11. **Level Up Notification**: Popup on level gain

---

## Technical Implementation

### Framework Systems Used
✅ **Quest System**: All 8 quests using QuestAsset
✅ **Combat System**: 5 enemy types with different behaviors
✅ **Loot System**: 5 loot tables using LootTableAsset
✅ **Inventory System**: Item management and stacking
✅ **Items System**: 15+ items using ItemAsset
✅ **Equipment System**: Equipment slots and stat modifiers
✅ **Progression System**: XP, levels, stat scaling
✅ **AI System**: StateMachine for simple enemies, BehaviorTree for boss
✅ **Pathfinding System**: Enemy navigation
✅ **ECS System**: Entity-Component-System architecture
✅ **UI System**: All 11 UI panels

### Asset Requirements
- **3D Models**: Goblin (4 variants + King), Player character, NPCs (3), Village buildings, Trees/rocks
- **Textures**: Terrain, UI sprites, Item icons (20+)
- **Audio**: Background music (2 tracks), Combat SFX (10+), UI sounds (5+)
- **Animations**: Player (idle, walk, attack), Goblins (idle, walk, attack, death)

### Scene Structure
```
MainScene
├── Environment
│   ├── Thornwood Village
│   └── Goblin Valley
├── Entities
│   ├── Player
│   ├── NPCs (3)
│   └── Enemies (spawn points)
├── UI
│   └── Canvas (11 panels)
└── Managers
    ├── QuestManager
    ├── CombatManager
    ├── LootManager
    └── ProgressionManager
```

---

## Success Criteria

✅ All 8 quests completable
✅ All 5 enemy types functional with AI
✅ Full progression 1→5 achievable
✅ All loot tables drop correctly
✅ All equipment modifies stats
✅ All UI panels functional
✅ Boss fight has 3 phases
✅ Game completable in 30-60 minutes
✅ Zero crashes/game-breaking bugs
✅ All Framework systems demonstrated

---

## Development Phases

**Phase 1: Foundation** (Create all assets using Inspector Tooling)
- Create all 8 quests with Quest Editor
- Create all 5 loot tables with Loot Table Designer
- Create all 15+ items with Item Database Manager

**Phase 2: Implementation** (Game Layer code)
- Scene setup (zones, spawn points)
- Enemy AI implementation
- NPC interaction system
- Quest integration
- UI wiring

**Phase 3: Content** (Assets and polish)
- 3D models and animations
- Textures and materials
- Audio integration
- Visual effects

**Phase 4: Testing & Polish**
- Full playthrough testing
- Balance tuning
- Bug fixing
- Performance optimization

---

## Next Steps

1. ✅ Design complete
2. Start Phase 1: Create game assets using Inspector Tooling
3. Implement Phase 2: Game Layer code
4. Add Phase 3: Content and assets
5. Test Phase 4: Final polish
