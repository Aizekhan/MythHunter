# Architecture card: Systems

Status: **First-pass semantic review recorded; broader reference audit pending.**

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 44
- [ ] `Assets/_MythHunter/Code/Systems/AI/SimpleLobbyAI.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/EventThrottlerUpdateSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/IEventThrottlerUpdateSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/IPhaseFilteredSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/ISystemRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/ITimerSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/ParallelSystemRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/SystemCategoryAttribute.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/SystemInitializationCategory.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/SystemPriorities.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/SystemRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Core/TimerSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatAbilitySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatDetectionSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/ICombatAbilitySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/ICombatDetectionSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Combat/ICombatSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/EntitySpawnSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/IEntitySpawnSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/IMovementSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/IPathfindingSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/IVisibilitySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/MovementSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/MovementUISystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/PathfindingSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Gameplay/Movement/VisibilitySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Groups/PhaseSystemGroup.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Groups/SystemGroup.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Heroes/ClassBonuses.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Heroes/HeroSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Heroes/IRaceClassBonusSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Heroes/RaceClassBonusSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Heroes/RacialBonuses.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Lobby/HeroSelectionSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Lobby/IHeroSelectionSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Lobby/ILobbySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Lobby/LobbySystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Phase/EmergencyPhaseProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Phase/GamePhaseProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Phase/IPhaseProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Phase/IPhaseSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Systems/Phase/PhaseSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Separate system lifecycle/registry abstractions from game-specific movement, combat, phase, lobby and AI behavior. Verify ordering, category lifecycle, dependencies, and initialization ownership.

## First-pass review (based on inspected implementations and imports)
SystemRegistry and ParallelSystemRegistry own lifecycle, initialization categories, ordering and execution. But GameplaySystemsInstaller imports Combat, Movement, Phase and MythHunter.Game.Systems.Phase, confirming that composition is game-specific. Separate system lifecycle runtime from feature and game installers. Also inspect ParallelSystemRegistry override paths for initialized/active filtering parity before moving anything.

## First-pass review (based on inspected implementations and imports)
SystemRegistry/ParallelSystemRegistry own system lifecycle, categories, ordering and execution. GameplaySystemsInstaller imports Combat, Movement, Phase and MythHunter.Game.Systems.Phase, confirming game-specific composition. Separate reusable lifecycle/runtime from game feature systems and installers. Inspect parity of ParallelSystemRegistry execution with initialized/active lifecycle.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.