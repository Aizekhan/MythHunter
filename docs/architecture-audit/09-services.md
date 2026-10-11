# Architecture card: Services

Status: **First-pass semantic review recorded; broader reference audit pending.**

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 5
- [ ] `Assets/_MythHunter/Code/Services/GameSettingsService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Services/Heroes/IHeroDataService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Services/Heroes/LocalHeroDataService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Services/Prefabs/IPrefabProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Services/Prefabs/PrefabProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Classify settings/prefab/hero-data services; keep Unity prefab loading and MythHunter hero persistence outside universal simulation core.

## First-pass review (based on inspected implementations and imports)
LocalHeroDataService depends directly on UnityEngine.Application.persistentDataPath and JsonUtility, and persists HeroDataModel from MythHunter.Entities.Heroes. It is a game-specific persistence adapter, not a reusable core service. Extract a generic persistence contract only if multiple consumers require it; put Unity filesystem/JSON implementation in an adapter.

## First-pass review (based on inspected implementations and imports)
LocalHeroDataService directly depends on UnityEngine.Application.persistentDataPath and JsonUtility and persists MythHunter HeroDataModel. It is a game-specific persistence adapter, not universal core. Extract a generic persistence contract only if multiple consumers justify it.

## First-pass review (based on inspected implementations and imports)
LocalHeroDataService directly depends on UnityEngine.Application.persistentDataPath and JsonUtility and persists MythHunter HeroDataModel. It is a game-specific persistence adapter, not universal core. Extract a generic persistence contract only if multiple consumers justify it.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.