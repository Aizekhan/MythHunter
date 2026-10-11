# RPG Framework Architecture

## Goal
MythHunter is being refactored into a reusable RPG Framework/Core. The Core must not depend on concrete game rules.

## Current repository inventory reviewed
- Core/ECS: EcsWorld, EntityManager, ComponentCache, EcsOptimizer, SystemBase.
- Core/DI: custom dependency injection container and lifecycle/installer infrastructure.
- Events: typed EventBus with priorities, synchronous/asynchronous processing and queues.
- Systems: grouped/ordered gameplay systems, currently including phase, movement, visibility, combat.
- Networking: separate Client/Core/Messages/Security/Serialization/Server layers.
- Components: Character, Combat, Core, Lobby, Movement and other domains.

## Architectural principles
1. Entity = identity only.
2. Components = data/state only.
3. Systems = behavior/logic over component data.
4. Query = the normal way for a system to select entities; gameplay systems should not manually scan the whole world with chains of HasComponent checks.
5. Events represent occurrences; ECS components represent persistent runtime state.
6. Capabilities/stats are different from runtime statuses.
7. DI wires infrastructure; it must not become a hidden gameplay state container.
8. Core may define abstractions, but must not depend on concrete game modules.
9. System ordering must be explicit and deterministic.
10. One module owns one responsibility; avoid duplicate ownership of the same state.
11. Presentation/Unity adapters must be outside simulation core.
12. Networking must replicate authoritative simulation state, not create a parallel gameplay state.
13. Persistence must have explicit versioning and migration.
14. Any optimization, including archetype storage, must preserve the public ECS API.

## Dependency direction
Platform/Unity adapters
        |
Presentation / Game modules
        |
RPG modules
        |
ECS / Simulation Core
        |
Low-level infrastructure abstractions

No dependency from ECS Core upward into concrete RPG/Game modules.

## Definition of Done for an architecture task
- Responsibility is explicit.
- Public API is documented.
- Dependencies are one-directional.
- No duplicate owner of the same state.
- At least one automated test or executable validation exists.
- No MythHunter-specific rule leaks into reusable Core.

## Important current findings
EntityManager currently stores EntityId -> Dictionary<Type,IComponent> plus Type -> HashSet<EntityId>.
ComponentCache duplicates component storage/cache behavior. This is functional but should remain behind an abstraction so storage can later become archetype-based without changing gameplay APIs.
GameplaySystemsInstaller currently wires PhaseSystem, movement/pathfinding/visibility and combat systems into phase-specific groups. This is game-specific and should eventually live outside universal Core.
CombatSystem currently owns several responsibilities: active combat tracking, combat lifecycle, resources/rage/concentration, phase handling, async combat loop and events. It should be decomposed later.

## EPIC 05 progress checkpoint (2026-10-11)
- EPIC 05.4 merged: SystemRegistry/SystemGroup lifecycle correctness (33/33 Unity EditMode tests on validated head).
- EPIC 05.5 merged: removed DI-container service-locator exposure, concrete registry checks, and reflection/emergency-provider bypasses. Validation: 36/36 Unity EditMode tests; Minimal CI and RPGFramework ECS Runtime passed; developer confirmed full-project compile, clean Console, and bootstrap/game smoke.
- EPIC 05.6 merged: explicit initialization categories are now carried through the registry contract into SystemRegistry; existing overload retains attribute-based selection and OnDemand fallback. Validation: 41/41 Unity EditMode tests; both CI workflows passed; developer confirmed integration checks.
- Issue #26 closed.

## Working rule
Complete Epic 01 and understand/document the architecture before broad implementation refactors. The EPIC 05 changes above are completed stability/contract fixes made during active ECS work; resume sequential architecture audit and update this document from evidence.