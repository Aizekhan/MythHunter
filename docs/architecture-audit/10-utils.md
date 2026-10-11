# Architecture card: Utils

Status: **Inventory created; semantic review pending.** Do not mark this card complete based on paths/file counts; verify call sites and direct dependencies first.

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 8
- [ ] `Assets/_MythHunter/Code/Utils/Extensions/MathExtensions.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Extensions/MythTaskExtensions.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Logging/IMythLogger.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Logging/MythLogger.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/UnityApiUtils.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Validation/Ensure.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Validation/IValidator.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Utils/Validation/Validator.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Split deterministic platform-independent helpers from UnityApiUtils, Unity framework helpers, and MythHunter-specific utilities.

## Review outcome
- Purpose: pending source inspection.
- Dependencies: pending direct-reference audit.
- Consumers: pending reference search.
- Framework relevance: pending five-filter assessment.
- Findings/actions: record after inspecting implementation and references.

## First-pass review (based on inspected implementations and imports)
UnityApiUtils directly wraps UnityEngine object discovery/component access/destruction and belongs in a Unity adapter, not headless runtime. IMythLogger may be a portable contract, but implementation and consumer dependencies still need review. Don't move utility code solely because it looks generic.
