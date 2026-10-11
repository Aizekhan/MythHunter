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
