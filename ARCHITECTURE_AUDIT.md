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
2. **System-layer boundary:** system lifecycle/category contracts were recently stabilized, but sit in the MythHunter tree rather than isolated `Assets/_Framework/ECS`. Map dependencies before deciding what can be moved.
3. **Domain events mixed with infrastructure:** separate bus/dispatch abstractions from `Events/Domain/Gameplay/*` event vocabulary.
4. **Game-specific components:** Character/Combat/Lobby/Movement components must be judged individually; ECS-shaped data is not automatically universal.
5. **Archetypes ambiguity:** `Entities/Archetypes` appears to model game archetypes/templates; compare with storage archetypes in `Assets/_Framework/ECS` and avoid conflating these concepts.
6. **Unity leakage:** `UnityApiUtils`, resource/prefab/UI/scene services should stay outside a headless simulation runtime.
7. **Cloud optionality:** auth/data/analytics adapters must not be mandatory dependencies of core simulation.
8. **Networking maturity:** inspect transport and integration before treating interfaces/stubs as production-ready.

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
