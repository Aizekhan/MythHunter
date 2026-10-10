# RPGFramework ECS Runtime

A standalone, dependency-light ECS runtime slice being established as the future canonical implementation for the RPG Framework.

## Public surface
- `IComponent`
- `IEntityManager`
- `EntityManager`

## Dependencies
- .NET standard library only.
- No UnityEngine, UnityEditor, MythHunter, game-phase, logger, DI-container, or provider references.

## Migration boundary
This module intentionally coexists with the current `MythHunter.Core.ECS` implementation during the migration stage. The existing MythHunter gameplay still uses its legacy interfaces/implementation in this PR; the two are not yet wired together. This is a temporary migration seam, not the final architecture. A follow-up task must move consumers and remove the legacy implementation without leaving duplicate runtime behavior.

The integer entity ID representation is retained for the first migration step. Archetypes, caches, system lifecycle, serializers and game-specific factories are deliberately outside this assembly.

## Behavior retained for compatibility in this slice
- IDs start at 1 and increase for the manager lifetime.
- Destroying an unknown ID is a no-op.
- Adding a component for an unknown or destroyed ID throws `ArgumentException`; it does not create a phantom entity. This is covered by a focused test.
- `GetComponent` returns `default` when a component is absent; use `HasComponent` / `TryGetComponent` when absence matters.

## Validation status
- Headless .NET test project: 8 passed, 0 failed, 0 skipped in GitHub Actions.
- Unity Test Runner and Unity project compilation: not yet validated against this branch.

Do not merge until Unity imports the assembly definitions, the full project compiles, and the focused Unity tests run.
