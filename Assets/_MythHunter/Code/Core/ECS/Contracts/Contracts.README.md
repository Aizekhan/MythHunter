# Framework.ECS.Contracts — staged boundary

The assembly is intended to own only low-level ECS contracts.

Current types:
- `IComponent`
- `IEntityManager`

The namespace remains `MythHunter.Core.ECS` to avoid a simultaneous namespace migration.

Dependencies are intentionally empty, with Unity engine references disabled.

## Integration status
This contract assembly has been staged in draft PR #18. It is not a completed migration until the existing consumers are explicitly connected to it, focused tests are added, and a Unity compile succeeds. Existing consumers are currently in Unity's predefined assemblies unless moved into named assemblies; Unity does not let predefined assemblies directly reference user asmdef assemblies, so integration needs a deliberate layout decision.

Do not move the implementation/storage layer into this assembly. Keep `EntityManager`, `EcsWorld`, caches, archetypes, serializers, factories, concrete components and game systems out of the contracts layer.
