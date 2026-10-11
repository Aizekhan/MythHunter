# Architecture card: Events

Status: **Inventory created; semantic review pending.** Do not mark this card complete based on paths/file counts; verify call sites and direct dependencies first.

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 38
- [ ] `Assets/_MythHunter/Code/Events/DebugEventMiddleware.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Debugging/EventLogger.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Debugging/EventStore.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/EntityEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/GameEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Gameplay/CombatEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Gameplay/GameplayEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Gameplay/HeroEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Gameplay/MovementEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Gameplay/Runes/RuneEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Loading/LoadingEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Lobby/LobbyEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Menu/MenuEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/PhaseEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Preload/PreloadEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/Profile/ProfileEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/SceneEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Domain/TimerEvents.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventBatcher.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventHandlerBase.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventPool.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventPriority.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/EventThrottler.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/Extensions/SystemEventExtensions.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IDebugEventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IEvent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IEventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IEventPool.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IEventSubscriber.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/IEventThrottler.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/EventNetworkAuthorityPolicy.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/EventNetworkMode.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/INetworkEvent.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/INetworkEventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/NetworkEventAttribute.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/NetworkEvents/NetworkEventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Events/SimpleEventBus.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Separate event bus/dispatch/pooling/pipeline infrastructure from MythHunter domain event types; persistent state should not be hidden in events.

## Review outcome
- Purpose: pending source inspection.
- Dependencies: pending direct-reference audit.
- Consumers: pending reference search.
- Framework relevance: pending five-filter assessment.
- Findings/actions: record after inspecting implementation and references.