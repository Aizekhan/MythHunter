# Architecture card: Components

Status: **First-pass semantic review recorded; broader reference audit pending.**

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 22
- [ ] `Assets/_MythHunter/Code/Components/Character/HeroIdentityComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/InventoryComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/SkillsComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/SocialSkillsComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/StatRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/StatType.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Character/StatsComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Combat/CombatAbilityComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Combat/CombatStyleComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Combat/HealthComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Combat/TeamComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Core/DescriptionComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Core/IdComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Core/NameComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Core/ValueComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Lobby/HeroSelectionComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Lobby/LobbyStateComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Movement/MovementComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Movement/PathComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Movement/PositionComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Components/Movement/VisibilityComponent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Classify each component as generic runtime state or game-specific Character/Combat/Lobby/Movement/Tags domain data. Data-only ownership is the desired default.

## First-pass review (based on inspected implementations and imports)
Character, Combat, Lobby and Movement groupings signal game-domain component data. The `Core` namespace may hold reusable primitives, but each struct/class must be checked for domain terms, managed references, Unity types, or behavior before classification.

## First-pass review (based on inspected implementations and imports)
Components are grouped by Character, Combat, Core, Lobby and Movement, signalling a mixture of potentially reusable primitive state and MythHunter domain concepts. Classify each type and verify data-only behavior and platform dependencies before calling the folder universal.

## First-pass review (based on inspected implementations and imports)
Components are grouped by Character, Combat, Core, Lobby and Movement, signalling a mixture of potentially reusable primitive state and MythHunter domain concepts. Classify each type and verify data-only behavior and platform dependencies before calling the folder universal.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.