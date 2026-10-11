# Module dependency map — EPIC 01

Source baseline: current `dev` tree at `fecb1857e82b4b64804b049049fac018b4b53970`. `357` C# files under `Assets/_MythHunter/Code`. Folder counts are navigation metadata, not audit completion.

| Module | C# files | Directly observed edges | Reusable-framework status |
|---|---:|---|---|
| Core | 82 | EcsWorld → RPGFramework.ECS + MythHunter.Systems.Core; CoreInstaller wires DI, EntityManager, SimpleEventBus and game state/runtime services | Mixed infrastructure/game composition; split candidates need consumer mapping |
| Components | 22 | Namespaces grouped into Character, Combat, Core, Lobby, Movement | Mixed data types; classify each by domain vocabulary/platform references |
| Entities | 21 | EntityFactory → IArchetypeSystem + MythHunter Name/Description/Value components; ArchetypeSystem → DI/events/logging | Game/RPG feature layer; not generic unchanged |
| Systems | 44 | SystemRegistry → IEventBus/logger; GameplaySystemsInstaller → Combat/Movement/Phase and MythHunter.Game.Systems.Phase | Registry/lifecycle may be reusable; gameplay systems/installers are game-specific |
| Events | 38 | EventBus → MythHunter DI/logger/domain events; NetworkEventBus inherits EventBus | IEventBus candidate abstraction; concrete event vocabulary is game-specific. PR #34 removed unused UnityEditor using |
| Networking | 17 | NetworkSystem → client + server + serializer + logger + MythHunter DI/messages; NetworkEventBus derives from EventBus | Optional transport/integration layer; keep out of simulation core |
| Cloud | 5 | Current inspected files are mainly interfaces/events; no provider implementation found in this subtree | Optional platform adapter |
| UI | 66 | UIService → IUIViewFactory + logger + MythHunter DI; placeholder HideScreen/IsScreenActive | Presentation layer; never a dependency of ECS runtime |
| Services | 5 | LocalHeroDataService → Unity Application.persistentDataPath/JsonUtility + HeroDataModel | Game/platform adapter |
| Utils | 8 | UnityApiUtils → UnityEngine scene/component/destruction APIs | Unity adapter; not headless simulation core |

## Validated dependency direction (partial)
```text
Unity/platform adapters (UI, LocalHeroDataService, UnityApiUtils, resource/prefab/scene)
             ↑ used by
MythHunter composition/installers
             ↑ wires
RPG-specific systems/entities/components/events/networking/cloud
             ↑ consumes
RPGFramework.ECS.Runtime (noEngineReferences=true; no assembly references)
```

The arrows above mean consumers depend on layers below them. The actual graph remains only partially validated: selected direct edges are source-backed, but reverse references and all transitive dependencies aren't exhaustive.

## Concrete dependency defects / clean-up candidates
1. **EventBus Unity boundary**: addressed in PR #34; unused `using UnityEditor;` was removed. Local Unity/player build confirmation remains outstanding.
2. **Cache ownership/update policy**: HeroSystem uses registry caches; MovementSystem and VisibilitySystem instantiate direct caches and refresh each update. Shared component types receive separate derived snapshots. Do not delete without usage and performance evidence.
3. **EntityFactory scope**: named Character/Enemy/Item templates and concrete MythHunter components should remain in game feature layer, not ECS runtime.
4. **Core/ECS wrapper**: EcsWorld couples framework `IEntityManager` to MythHunter `ISystemRegistry`; this is probably an application/world orchestration layer, not a minimal ECS package type.
5. **NetworkSystem composition**: client/server/serialization and event buses need further trust-boundary, disposal and actual transport review; do not label production-ready from interface shapes alone.
6. **UIService incomplete contract**: HideScreen/IsScreenActive are placeholders; decide if needed and implement via UI manager/view ownership rather than keeping false-success stubs.

## Phase 1 exit gates (still open)
- [ ] All source/asset/scene/asmdef consumers of cross-module references enumerated
- [ ] Cache invalidation/update ownership verified by tests or profiling
- [ ] Unity EditMode/full compile/player build after PR #34 confirmed
- [ ] Dependencies audited for Resources, Data, Debug and Replay modules as well as the ten mandatory cards
- [ ] Architecture review approves all module classifications

## Rule
Do not move files or refactor runtime modules while these gates are open. Every proposal must pass the five filters: required by any RPG; specific-game coupling; reusable unchanged; framework vs game data; evidenced architectural issue/action.