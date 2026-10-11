# Architecture card: Cloud

Status: **Inventory created; semantic review pending.** Do not mark this card complete based on paths/file counts; verify call sites and direct dependencies first.

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 5
- [ ] `Assets/_MythHunter/Code/Cloud/Analytics/AnalyticsEvent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Cloud/Analytics/IAnalyticsService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Cloud/Core/IAuthService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Cloud/Core/ICloudService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Cloud/Core/IDataService.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
External services should be optional adapters behind contracts; core simulation must work without cloud services.

## Review outcome
- Purpose: pending source inspection.
- Dependencies: pending direct-reference audit.
- Consumers: pending reference search.
- Framework relevance: pending five-filter assessment.
- Findings/actions: record after inspecting implementation and references.