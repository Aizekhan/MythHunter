# Architecture Audit — EPIC 01

## Purpose
An evidence-based inventory before broad refactoring. Phase 1 is complete only when each top-level area has a one-page architecture card, dependency direction is mapped, universal versus game-specific code is identified, and refactoring candidates are recorded. Phase 2 begins only afterward.

## Confirmed repository inventory
Current tracked C# source counts from the recursive `dev` tree (navigation aid only, not a quality assessment):
| Area | C# files | Initial boundary hypothesis |
|---|---:|---|
| Core | 82 | Mixed infrastructure, DI, ECS, installers/bootstrap, Unity-facing services |
| UI | 66 | Presentation; must not be depended on by simulation core |
| Systems | 44 | Reusable lifecycle infrastructure mixed with game-specific systems |
| Events | 38 | Event bus infrastructure mixed with domain event types |
| Resources | 30 | Resource abstraction mixed with Unity adapters/pooling/preload |
| Components | 22 | ECS data mixed with Character/Combat/Lobby/Movement domains |
| Entities | 21 | Factories/serialization mixed with game archetypes/templates |
| Networking | 17 | Client/server/messages/security/serialization seams |
| Debug | 11 | Reusable observability mixed with Unity/UI-specific tools |
| Utils | 8 | Generic helpers mixed with Unity/game-specific helpers |
| Data | 7 | Static/configuration and serialization |
| Cloud | 5 | Optional external integration |
| Services | 5 | Game settings, hero data, prefab provider |
| Replay | 1 | Interface-only footprint in this source tree; verify consumers/completeness |

## Target dependency direction (to validate against call sites)
```text
Unity / Platform adapters
  ├─ UI, Scenes, Input, Addressables, MonoBehaviours
  ├─ Cloud and concrete persistence/network transports
  ↓
MythHunter composition / installers
  ↓
RPG feature modules (movement, combat, inventory, progression, AI)
  ↓
ECS runtime contracts + world/entity/component/query/system abstractions
  ↓
Low-level platform-neutral contracts
```
This is the intended target, not a claim the current implementation already follows it.

## Initial risk register
1. **ECS storage ownership:** `EntityManager` and `ComponentCache` overlap in storage/cache behavior per existing architecture notes. Verify consumers before assigning ownership.
2. **Cache ownership/update policy:** `ComponentCache<T>` is a secondary cache used by HeroSystem, MovementSystem and VisibilitySystem; some consumers construct it directly while others use `ComponentCacheRegistry`. PR #48 (merged at `0d97060d45d978cafbe376cd43f00eb8fdf9f175`) implements the targeted Incremental/OnDemand consistency changes and adds characterization tests. Exact merged-head Unity validation remains open; do not remove/centralize caches before consumer smoke tests, full reverse-reference review, and measured performance evidence.
3. **System-layer boundary:** system lifecycle/category contracts were recently stabilized, but sit in the MythHunter tree rather than isolated `Assets/_Framework/ECS`. Map dependencies before deciding what can be moved.
4. **Domain events mixed with infrastructure:** separate bus/dispatch abstractions from `Events/Domain/Gameplay/*` event vocabulary.
5. **Game-specific components:** Character/Combat/Lobby/Movement components must be judged individually; ECS-shaped data is not automatically universal.
6. **Archetypes ambiguity:** `Entities/Archetypes` appears to model game archetypes/templates; compare with storage archetypes in `Assets/_Framework/ECS` and avoid conflating these concepts.
7. **Unity leakage:** `UnityApiUtils`, resource/prefab/UI/scene services should stay outside a headless simulation runtime.
8. **Cloud optionality:** auth/data/analytics adapters must not be mandatory dependencies of core simulation.
9. **Networking maturity:** inspect transport and integration before treating interfaces/stubs as production-ready.

## Five filters
For every module and file:
1. Needed by any RPG?
2. Depends on a particular game?
3. Reusable unchanged?
4. Framework code or game data/presentation?
5. Architectural issue/action supported by evidence?

## Phase 1 status
- [x] Inventory card created for Core; semantic review pending
- [x] Inventory card created for Components; semantic review pending
- [x] Inventory card created for Entities; semantic review pending
- [x] Inventory card created for Systems; semantic review pending
- [x] Inventory card created for Events; semantic review pending
- [x] Inventory card created for Networking; semantic review pending
- [x] Inventory card created for Cloud; semantic review pending
- [x] Inventory card created for UI; semantic review pending
- [x] Inventory card created for Services; semantic review pending
- [x] Inventory card created for Utils; semantic review pending
- [ ] Dependency map validated against real references
- [ ] All evidenced actions/issues recorded
- [ ] Phase 2 remains blocked until this checklist is complete

## Audit rules
- Folder and interface names are not proof of universality.
- No runtime refactor during the inventory checkpoint.
- Do not mark tasks complete from counts alone.

## First-pass semantic findings (reviewed implementation examples)
- **Core/ECS:** `EcsWorld` depends on `MythHunter.Systems.Core.ISystemRegistry` as well as `RPGFramework.ECS`, so world/runtime composition is not yet wholly isolated from MythHunter system infrastructure.
- **Entities vs storage archetypes:** `Entity` is an integer-ID wrapper; `Entities/Archetypes` defines game templates/required-component checkers and string-ID lookup. `EntityFactory` hard-codes Character/Enemy/Item and MythHunter component types. This is distinct from ECS storage archetypes and is game-specific.
- **Systems:** `GameplaySystemsInstaller` imports Combat, Movement, Phase and `MythHunter.Game.Systems.Phase`; separate generic system runtime/lifecycle from game composition.
- **Events:** `EventBus.cs` imports `UnityEditor` in a runtime code path and has dependencies on MythHunter domain event namespaces, DI and logging. This is a concrete Unity/game coupling risk; inspect conditional compilation/platform build behavior before changing it.
- **Networking:** `NetworkSystem` composes client/server/serializer/logger implementations through interfaces, but authority validation, message validation, unsubscribe/disposal behavior and production transports need audit before claiming it is a general-purpose runtime.
- **UI:** `UIService.HideScreen` is a placeholder and `IsScreenActive` always returns false in the inspected implementation; record as incomplete UI contract rather than framework-core functionality.
- **Services:** `LocalHeroDataService` uses `Application.persistentDataPath`, `JsonUtility`, and `HeroDataModel`, making it a Unity/game-specific persistence adapter.
- **Utils:** `UnityApiUtils` directly wraps UnityEngine object discovery, component lookup and destruction; it belongs in a Unity-facing adapter, not headless simulation core.

These findings are initial and source-backed. They do not complete the dependency map: call-site/reference validation is still required before Phase 1 can be closed.
## Reference validation checkpoint (2026-10-11)

### Confirmed by direct source inspection
- `Assets/_MythHunter/Code/Core/ECS/EcsWorld.cs` imports `MythHunter.Systems.Core` and `RPGFramework.ECS`; its constructor requires both `IEntityManager` and the MythHunter `ISystemRegistry`. This is a real code-level dependency from the legacy ECS world wrapper into game-tree system infrastructure.
- `Assets/_MythHunter/Code/Core/ECS/ComponentCache.cs` stores its own per-entity component dictionary and ID sets while delegating authoritative component operations to `IEntityManager`. It is explicitly a second-level cache, not necessarily duplicate authoritative ownership; its invalidation/coherency and consumers need mapping before removal.
- `Assets/_MythHunter/Code/Entities/EntityFactory.cs` directly knows Character/Enemy/Item archetype identifiers and concrete MythHunter `NameComponent`/`DescriptionComponent`/`ValueComponent`; it is a game factory, not generic Entity API.
- `Assets/_MythHunter/Code/Systems/Core/SystemRegistry.cs` stores category/priority/active/initialized metadata and drives lifecycle; `GameplaySystemsInstaller` imports Combat, Movement, Phase and MythHunter.Game.Systems.Phase. Composition and runtime registry responsibilities are therefore mixed at the larger tree level.
- `Assets/_MythHunter/Code/Events/EventBus.cs` imports `UnityEditor` along with game-domain event namespaces. This must be checked for conditional compilation/build-target safety; the file's mere import is a portability boundary concern, not yet proof of a player-build failure.
- `Assets/_MythHunter/Code/UI/Core/UIService.cs` `HideScreen` is a stub and `IsScreenActive` always returns false.
- `Assets/_MythHunter/Code/Services/Heroes/LocalHeroDataService.cs` uses `UnityEngine.Application.persistentDataPath`, `UnityEngine.JsonUtility`, `HeroDataModel`, and disk I/O, so it is a Unity/game-specific adapter.
- `Assets/_MythHunter/Code/Utils/UnityApiUtils.cs` wraps UnityEngine object/component APIs and belongs outside headless simulation runtime.
- `Assets/_Framework/ECS/Runtime` has its own runtime asmdef and separate `IComponent`, `IEntityManager`, `EntityManager`, README and isolated tests. This is the strongest current seam for the reusable ECS foundation.

### Reference map — current hypothesis, not yet a complete compiler dependency graph
| Source area | Directly observed dependency/relationship | Boundary assessment |
|---|---|---|
| Framework ECS runtime | `RPGFramework.ECS.Runtime.asmdef`, `IComponent`, `IEntityManager`, `EntityManager` | Current reusable ECS seed; verify public API sufficiency and avoid reverse dependencies |
| Legacy Core/ECS | `MythHunter.Core.ECS` types bridge to `RPGFramework.ECS`; `EcsWorld` also depends on `MythHunter.Systems.Core` | Transitional adapter layer; avoid moving it wholesale |
| Entities | Entity archetypes/factory reference MythHunter components and named game templates | Game/RPG module, not low-level entity identity |
| Systems | Registry lifecycle can be reusable; concrete installers wire MythHunter gameplay domains | Split reusable scheduler/registry contracts from game installers/features |
| Events | EventBus implementation + MythHunter domain event names + UnityEditor using | Separate dispatch core, domain events, editor diagnostics; verify player compile |
| Networking | NetworkSystem composes client/server/serializer and MythHunter logging/DI | Adapters/infrastructure; isolate simulation state and validate message trust boundary |
| UI | factory/service/views/presenters and concrete game screens | Presentation adapter, strictly downstream of simulation |
| Local hero service | Unity file APIs and HeroDataModel | Game persistence adapter; reusable persistence interface only if consumer evidence warrants it |
| Utils | UnityEngine wrappers mixed with generic-looking helpers/logging | Split platform-specific and portable utilities based on references |

### Cross-reference evidence still missing
- The GitHub code-search connector did not return usable occurrence results for targeted call-site queries, so this checkpoint uses only files actually inspected and does **not** claim exhaustive reverse-reference counts.
- Next audit step: perform a local repository-wide reference scan/assembly dependency audit and populate the call-site column on each card. Phase 1 is not complete until that evidence is recorded.

## Framework boundary verified against ECS runtime source
- `Assets/_Framework/ECS/Runtime/README.md` describes this as the canonical, dependency-light ECS slice and states that MythHunter-facing systems, caches, factories, archetypes, serializers and DI consume `RPGFramework.ECS`; `EcsWorld`, system lifecycle, caches and gameplay policies remain in the Game Layer.
- `RPGFramework.ECS.Runtime.asmdef` isolates the runtime assembly. Its inspected code uses .NET standard library collections and contains no Unity, MythHunter or DI references.
- `EntityManager` uses `Dictionary<int, Dictionary<Type,IComponent>>` as per-entity component storage and `Dictionary<Type,HashSet<int>>` as a reverse index. The current implementation is dictionary-backed, not archetype storage.
- `MythHunter.Core.ECS.ComponentCache<T>` keeps a secondary per-type component dictionary and entity ID sets over `IEntityManager`. This is not a second authoritative entity store, but does introduce cache-coherency and mutation-invalidation questions that need consumer/call-site review.

## Audit correction
- The hypothesis that Framework ECS and a legacy EntityManager are simultaneously authoritative is not supported by the current runtime README. It states that the old `MythHunter.Core.ECS.IComponent`, `IEntityManager`, and duplicate `EntityManager` were removed.
- Do not start another EntityManager migration or add wrappers based on old inventory notes. Inspect `ComponentCache<T>` consumers and benchmark before deciding whether to keep, simplify, or remove it.
## Reference findings update
- Cache consumers found via repository code search: `HeroSystem` caches stats/health, `MovementSystem` caches position/movement/path and refreshes every update, `VisibilitySystem` caches position/visibility and refreshes every update. Some cache snapshots are constructed directly, outside `ComponentCacheRegistry`.
- `CoreInstaller` initially binds `IEventBus` to `SimpleEventBus`; `EventsInstaller` replaces that binding with `EventBus` in offline mode or aliases the `NetworkEventBus` instance through `IEventBus` for client/server modes. `NetworkEventBus` inherits `EventBus`.
- `EventBus.cs` has an unconditional `using UnityEditor;` and no other matching editor API or `#if/#endif` in the inspected file. Treat this as a probable player-build defect requiring a targeted fix/build check, not as a proven player-build failure until validated.
## EventBus portability fix follow-up
- PR #34 merged as `0313e8afd0e621fa310e31373ebe014cf5fe9889`: removed unused `using UnityEditor;` from `EventBus.cs`.
- `Minimal CI` and `RPGFramework ECS Runtime` passed on the PR head.
- Local Unity EditMode/full-project compile and a non-Editor player build remain to be run to fully close the portability check.
## Additional modules required by EPIC 01
- Added first-pass cards for Resources, Data & Serialization, Debug, and Replay at `docs/architecture-audit/12-resources.md` through `15-replay.md`. These were missing from the original ten mandatory cards but are present in the source tree and matter to the dependency boundary.
- **Resources:** inspected manager/provider contracts use `UnityEngine.Object`; concrete managers/providers depend on Unity Resources/Addressables/AssetBundles and MythHunter DI/logging/pooling. Treat as Unity adapters, not portable simulation core.
- **Data & Serialization:** generic `IComponentSerializer<T>`/`ComponentSerializerBase<T>` depend on `RPGFramework.ECS.IComponent` and standard binary streams; registry/installer remain coupled to MythHunter DI/logger. Serialization wire format/versioning is still an open concern.
- **Debug:** `IDebugTool` is a fairly general diagnostics contract, but `DebugDashboard` is a Unity MonoBehaviour and `DebugToolFactory` hardcodes MythHunter tools. `PreloadDebugTool` also imports `UnityEditor` from a runtime-tree path; its build safety must be inspected separately.
- **Replay:** only `IReplaySystem` was found in its folder; it depends on `UniTask` and MythHunter `IEventBus`, and no concrete implementation was found there. No claim of complete replay support until recording semantics, concrete implementations, event/state/input coverage and versioned persistence are verified.
## Runtime dependency / reference audit — Resources, Serialization, Debug, Replay
- `IPreloadManager` has concrete consumers in `GameFlowManager`, `AutoPreloadConfigurator`, and `PreloadDebugTool`; it is registered by `ResourceInstaller`. `PreloadManager` depends on resource manager, phase provider, logger, event bus, pool manager, DI container, UnityEngine resources and MythHunter preload events. It is orchestration/platform functionality, not simulation core.
- `IResourceManager` and `IResourceProvider` constrain results to `UnityEngine.Object`, so they are not engine-neutral contracts as written. Concrete providers target Unity Resources, Addressables and AssetBundles; `ResourceInstaller` conditionally registers Addressables and wires default provider, asset bundles, pool and preload services.
- Serialization is accessed through `IComponentSerializerRegistry`, registered in `SerializationInstaller`. `IComponentSerializer<T>` and `ComponentSerializerBase<T>` use the Framework ECS component contract and standard binary streams; concrete defaults serialize MythHunter component types. `VersionedSerializer` exists with a format version and warns for newer versions, so the open concern is migration/compatibility behavior rather than lack of versioning.
- `IReplaySystem` exposes recording/playback lifecycle, save/load, playback speed and `ReplayEventRecord` (timestamp, type ID and serialized bytes), but no concrete implementation was found under the Replay folder. Repository search did not establish a registered consumer; do not claim an end-to-end replay feature.
- `IDebugTool` is a largely portable interface, but `DebugToolFactory` constructs concrete MythHunter tools and `DebugDashboard` is a Unity MonoBehaviour. `PreloadDebugTool` in the runtime source tree had an unused unguarded UnityEditor import; PR #40 removes it and awaits local Unity/player build confirmation.

## Reverse-reference coverage status
- Code-search results identify the consumers and bindings listed above, but indexed search is not an exhaustive dependency graph. Scene/prefab serialized references and all concrete type usages remain unverified.
- Before moving code, complete reverse edges using local `rg`/IDE Find All References and review Unity asmdef references. The portable Framework ECS assembly currently has no assembly references and `noEngineReferences=true`.
## Prioritized architecture backlog (audit findings, no runtime changes)

| Priority | Finding | Why it matters | Acceptance evidence |
|---|---|---|---|
| P0 — validation | PR #50 merged the `PreloadDebugTool.RenderGUI` editor-only guard/qualified `UnityEditor.EditorStyles`; PR #48 merged the ComponentCache consistency change. CI is green on the PR heads, but exact merged-head Unity validation remains unconfirmed | Prevent treating static CI or an older local test report as proof of full Unity/player build safety | Unity EditMode + full compile/clean Console + non-Editor Player Build + Movement/Visibility/Hero smoke on exact `dev` SHA `0d97060d45d978cafbe376cd43f00eb8fdf9f175` |
| P1 — data correctness | PR #48 implements Incremental membership pruning/value refresh and OnDemand live Get. Residual semantics: OnDemand `Contains`/ID enumeration do not discover newly-added components unless first cached; cache-local `Add` must be distinguished from authoritative writes | Gameplay reads can diverge from authoritative `IEntityManager` state if consumers rely on unverified freshness semantics | Run merged-head tests; clarify positive-membership and write-through behavior; Movement/Visibility/Hero smoke checks |
| P1 — deterministic resource loading | `ResourceManager.RegisterProvider(provider, priority)` logs the supplied priority but sorts by `IPrioritizable.Priority` | API contract permits caller's requested ordering to be ignored | Issue #44: explicit ordering API documented and precedence tested |
| P1 — serialization robustness | `VersionedSerializer` warns and continues for newer version/type mismatch; trusts declared length and doesn't validate exact bytes read | Unsupported/malformed payloads may be passed to arbitrary deserializers | Tests for bad type/version/length/truncated payload and explicit reject/migration policy |
| P2 — composition purity | `PreloadManager` resolves `IPoolManager` from its DI container despite having constructor injection and redundantly assigns the field | Service-locator usage hides declared dependency and complicates tests | Replace with injected field under a focused PR; constructor/disposal tests pass |
| P2 — framework boundary | Resource APIs return `UnityEngine.Object`; UI/debug/services and gameplay installers are coupled to Unity/MythHunter | These should remain adapters/game layer, not enter `RPGFramework.ECS.Runtime` | Assembly reference review shows no Unity/game references from framework runtime |
| P2 — feature completeness | Replay directory contains interface/record type only; no concrete implementation or binding found there | Interface presence doesn't prove replay works end-to-end | Find/implement registered recorder/playback path and record determinism/versioning tests, or mark unsupported |

## Recommended execution order (refreshed after PR #48/#50 merges)
1. Validate exact current `dev` head `0d97060d45d978cafbe376cd43f00eb8fdf9f175`: Unity EditMode, full compile/clean Console, Player Build and Movement/Visibility/Hero smoke.
2. Complete Phase 1 reverse-reference/asmdef/Unity scene-prefab checks using local `rg`/IDE Find All References; GitHub code search alone is not exhaustive.
3. Keep provider ordering (issue #44) and serializer input/version handling as separate bounded audit/runtime candidates; don't bundle them with ComponentCache.
4. Only after Phase 1 is approved, begin EPIC 05.9/Phase 2 file-by-file ownership design and framework extraction proposals.

This backlog prioritizes findings; it does not mark EPIC 01 complete.

## Cache consistency test design
- Added `docs/architecture-audit/16-component-cache-test-plan.md` describing expected coverage for Full/Incremental/OnDemand strategies, registry lifecycle, and Movement/Visibility/Hero system regression cases.
- PR #48 has now implemented the targeted runtime changes and added the planned strategy-level characterization tests. This document records test coverage and contract choices, not proof that the tests pass on the current merged `dev` SHA; local Unity validation must be recorded against that exact SHA.
