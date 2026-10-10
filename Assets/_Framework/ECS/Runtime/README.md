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
- Static assembly-boundary/GUID validation: passed in GitHub Actions.
- Headless .NET test project: 8 passed, 0 failed, 0 skipped in GitHub Actions.
- Unity CLI EditMode run on PR head `3917e6a48344d4469fd95a2049b1a2e60d983223` with Unity `6000.0.45f1`: Framework test assembly discovered; 8 ECS tests passed, and the full EditMode run passed 9/9 tests, 0 failed, 0 skipped.
- The successful EditMode test invocation also exercised Unity's project/test assembly import sufficiently to discover and execute the test assembly; this is not represented as a separate clean-room build command.

The Unity ECS test gate is passed. Before merge, complete final review and confirm CI on the current PR head.
