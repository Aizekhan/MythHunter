# Framework.ECS.Contracts

This assembly is the first extraction boundary. It may contain only neutral ECS contracts.

## Current contract scope
- `IComponent`: marker for ECS component data.
- `IEntityManager`: existing integer-ID-based contract preserved for compatibility.

## Allowed dependencies
- .NET standard library APIs only.

## Forbidden dependencies
- MythHunter runtime implementation assemblies
- UnityEngine and UnityEditor
- game phases, domain events, DI container, loggers, resource providers, system registry

## Not part of this assembly
- `EntityManager`, `EcsWorld`, `Entity`, ComponentCache, archetypes, serializers, factories, game components and systems.

## Status
Staged on a feature branch and not yet Unity-compiled. Do not merge until the Unity import/compile is successful and focused tests are added and run.
