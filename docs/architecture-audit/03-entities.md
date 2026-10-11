# Architecture card: Entities

Status: **First-pass semantic review recorded; broader reference audit pending.**

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 21
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeDetector.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeTemplateBuilder.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeTemplateRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/EntityArchetypeBase.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/HeroArchetypeSO.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeDetector.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeTemplateBuilder.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeTemplateRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Archetypes/IEntityArchetype.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/EntityFactory.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Heroes/HeroArchetypeRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Heroes/HeroClass.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Heroes/HeroDataModels.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Heroes/HeroFactory.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/Heroes/HeroRace.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/IComponentSerializerRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Entities/IEntityFactory.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Compare game archetype templates and factories to ECS storage/archetype concepts. Entity identity/lifecycle should stay generic; hero templates and game spawn defaults should not leak into Core.

## First-pass review (based on inspected implementations and imports)
Entity in MythHunter.Core.ECS is currently only an integer ID wrapper. Separately, Entities/Archetypes implements IEntityArchetype-based templates/required-component checks and ArchetypeRegistry maintains entity-to-archetype string maps. EntityFactory directly creates Character/Enemy/Item by string archetype IDs and concrete MythHunter components. This game factory/template layer is distinct from ECS storage archetypes and is not universal unchanged.

## First-pass review (based on inspected implementations and imports)
Entity is currently an integer ID wrapper. Separately, Entities/Archetypes defines IEntityArchetype templates/required-component checks and ArchetypeRegistry maps entity IDs to string archetype IDs. EntityFactory creates Character/Enemy/Item by hard-coded archetype IDs and MythHunter component types. This game template layer is distinct from ECS storage archetypes and is not universally reusable unchanged.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.