# Architecture card: Cloud

Status: **First-pass semantic review recorded; broader reference audit pending.**

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

## First-pass review (based on inspected implementations and imports)
Current Cloud C# inventory contains analytics and interfaces for auth/cloud/data services; no concrete cloud adapter showed in the inspected folder. These are optional platform integrations. Core simulation must start without them; async abstractions should not require a particular provider.

## First-pass review (based on inspected implementations and imports)
Current cloud folder shows analytics/auth/cloud/data interfaces and event data, but no concrete cloud adapter in the inspected folder. Treat as optional platform integration. Core simulation must work without it and should not require a particular cloud provider.

## First-pass review (based on inspected implementations and imports)
Current cloud folder shows analytics/auth/cloud/data interfaces and event data, but no concrete cloud adapter in the inspected folder. Treat as optional platform integration. Core simulation must work without it and should not require a particular cloud provider.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.