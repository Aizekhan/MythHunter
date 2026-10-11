# Architecture card: Networking

Status: **First-pass semantic review recorded; broader reference audit pending.**

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

## First-pass review (based on inspected implementations and imports)
NetworkSystem composes client/server/serializer/logger through interfaces, while depending on MythHunter DI and messages. Transport-neutral contracts may be reusable, but authority validation, message validation, subscription disposal, framing/limits and concrete transports require checks.

## First-pass review (based on inspected implementations and imports)
NetworkSystem composes INetworkClient, INetworkServer, INetworkSerializer and IMythLogger, and references MythHunter DI and message contracts. Transport contracts may be reusable, but authority, disposal/unsubscription, untrusted-message validation, serializer limits and actual transports need review before claims of production readiness. Client/server orchestration is optional to simulation core.

## First-pass review (based on inspected implementations and imports)
NetworkSystem composes INetworkClient, INetworkServer, INetworkSerializer and IMythLogger, and references MythHunter DI and message contracts. Transport contracts may be reusable, but authority, disposal/unsubscription, untrusted-message validation, serializer limits and actual transports need review before claims of production readiness. Client/server orchestration is optional to simulation core.

## Five-filter result (provisional)
- **Required by any RPG?** Core contracts may be; concrete components, services and installers are not automatically required.
- **Game-specific dependency?** Confirmed for types described above; not proven for every file in this module.
- **Reusable unchanged?** Not yet; the identified MythHunter/Unity/DI couplings prevent that conclusion.
- **Framework vs game data/presentation?** Split mixed folders by responsibility, not by naming alone.
- **Action:** continue dependency/reference audit, then decide move/adapt/keep/delete per file. No runtime refactor in the audit step.