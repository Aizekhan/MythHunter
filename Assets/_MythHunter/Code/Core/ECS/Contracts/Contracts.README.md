# Framework.ECS.Contracts — staged boundary

The assembly is intended to own only low-level ECS contracts.

Current types:
- `IComponent`
- `IEntityManager`

The namespace remains `MythHunter.Core.ECS` to avoid a simultaneous namespace migration.

Dependencies are intentionally empty, with Unity engine references disabled.

## Integration status
This contract assembly has been staged in draft PR #18. It is not a completed migration until the existing consumers are explicitly connected to it, focused tests are added, and a Unity compile succeeds. The contract-only assembly was found to be an incomplete first cut because existing consumers compile in Unity's predefined assembly, which cannot directly reference a user asmdef assembly. A wider assembly layout requires auditing direct Editor imports and engine/package dependencies first.

Do not move the implementation/storage layer into this assembly. Keep `EntityManager`, `EcsWorld`, caches, archetypes, serializers, factories, concrete components and game systems out of the contracts layer.
