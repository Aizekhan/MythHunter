# Architecture card: Networking

Status: **Inventory created; semantic review pending.** Do not mark this card complete based on paths/file counts; verify call sites and direct dependencies first.

## Five-filter checklist
1. Is this needed by any RPG?
2. Does it depend on a specific game?
3. Can it be reused without changes?
4. Framework code or game data/presentation?
5. What architectural problem/action is supported by source evidence?

## C# files discovered
Total: 17
- [ ] `Assets/_MythHunter/Code/Networking/Client/INetworkClient.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Client/NetworkClient.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/ClientNetworkSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/INetworkMessageHandlerRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/INetworkSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/NetworkAuthority.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/NetworkMessageHandlerRegistry.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/NetworkSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Core/ServerNetworkSystem.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Messages/INetworkMessage.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Messages/NetworkEventMessage.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Security/NetworkSecurityProvider.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Serialization/BinaryNetworkSerializer.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Serialization/DeltaSerializer.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Serialization/INetworkSerializer.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Server/INetworkServer.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action
- [ ] `Assets/_MythHunter/Code/Networking/Server/NetworkServer.cs` — purpose / dependencies / consumers / universal-vs-game-specific / action

## Boundary hypothesis — verify, do not assume
Separate transport-neutral contracts from client/server transports, game payloads, and Unity/network implementation. Validate authority, serialization, trust boundaries, and testability.

## Review outcome
- Purpose: pending source inspection.
- Dependencies: pending direct-reference audit.
- Consumers: pending reference search.
- Framework relevance: pending five-filter assessment.
- Findings/actions: record after inspecting implementation and references.