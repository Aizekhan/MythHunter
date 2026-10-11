# Architecture card: Services

Status: **Inventory created; semantic review pending.** Do not mark this card complete based on paths/file counts; verify call sites and direct dependencies first.

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

## Review outcome
- Purpose: pending source inspection.
- Dependencies: pending direct-reference audit.
- Consumers: pending reference search.
- Framework relevance: pending five-filter assessment.
- Findings/actions: record after inspecting implementation and references.