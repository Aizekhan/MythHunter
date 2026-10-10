# RPGFramework ECS Runtime

Dependency-free, Unity-independent ECS building block.

## Public surface
- `IComponent`
- `IEntityManager`
- `EntityManager`

## Constraints
- Pure .NET code; no UnityEngine, UnityEditor, MythHunter, game-phase, logger, DI, or provider references.
- Integer entity IDs are retained for the initial migration so current MythHunter consumers can transition without changing data representation.
- Component storage is currently dictionary-based. Archetypes, caches, system lifecycle, and game-specific factories remain outside this assembly.

## Known behavior to harden later
- Adding a component for an ID that was never created currently creates storage for that ID.
- `GetComponent` returns `default` when a component is absent; use `HasComponent` / `TryGetComponent` when absence matters.
