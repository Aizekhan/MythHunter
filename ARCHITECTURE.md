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

## Project tracking

This file defines architecture principles and source-backed architecture findings; it is not a project-progress register. To prevent conflicting duplicate state:
- Current active task, exact PR/head SHA and unmet validation gates: [RPG Framework STATUS.md](https://github.com/Aizekhan/RPG-Framework-Roadmap/blob/main/STATUS.md).
- Milestone order, durable completion evidence and acceptance criteria: [RPG Framework Master Plan](https://github.com/Aizekhan/RPG-Framework-Roadmap/blob/main/RPG_FRAMEWORK_MASTER_PLAN.md).
- EPIC 01 source findings and Phase 1 exit gates remain in this repository's `ARCHITECTURE_AUDIT.md` and module cards.

Do not copy EPIC completion checkpoints into this architecture document. If these documents disagree, stop and reconcile them before implementation.
