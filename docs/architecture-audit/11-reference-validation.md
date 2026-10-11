# Reference validation — ECS cache and EventBus

Status: **Partial validation.** This is a source-backed check of directly inspected code, not a complete reverse-reference graph.

## ComponentCache ownership

### Directly confirmed
- `RPGFramework.ECS.EntityManager` is the canonical storage implementation described by `Assets/_Framework/ECS/Runtime/README.md`.
- `MythHunter.Core.ECS.ComponentCache<T>` is a derived per-component-type cache. It holds a dictionary of component copies and entity IDs; it calls `IEntityManager` to initially populate or lazily fetch cache entries.
- `ComponentCacheRegistry` creates caches, schedules refreshes, and exposes statistics. It is registered as a singleton by `EntitiesInstaller`.
- `ComponentCacheRegistry` auto-creates caches for `NameComponent`, `DescriptionComponent`, and `ValueComponent`, explicitly coupling this generic-looking cache service to concrete MythHunter components.
- `ComponentCacheRegistry` subscribes to entity-created and entity-destroyed events. On destroy it removes an entity from caches through reflection. On create the inspected handler logs archetype metadata but does not immediately add or refresh component data.
- `UpdateAllCaches`, phase-specific updates and scheduled updates invoke `Update` by reflection. Cache refresh defaults to every five calls to `Update()` and depends on `_autoUpdate`.

### Architectural conclusion
This is **not duplicate authoritative storage**, but it adds a second copy of component values whose freshness is time/event-driven rather than structurally guaranteed. Do not delete it without call-site/performance evidence. Next inspect every consumer of `IComponentCacheRegistry.GetCache<T>()`, whether callers depend on stale-cache semantics, and benchmarks against using `IEntityManager.GetEntitiesWith<T>()` directly.

### Risks to verify
- `ComponentCache<T>.UpdateIncrementalCache` schedules previously unseen entity IDs, but its inspected path should be checked for pruning destroyed entities or entities that lost component T.
- `ComponentCache<T>.Get` returns `default` for misses, which can mask absence for value-type components unless callers check `Contains`.
- Mutating a component directly through `IEntityManager` may leave a cached struct stale until the cache refreshes; determine whether any cache mutation APIs are used as authoritative writes.
- Reflection-based `Update`/ `Remove` calls hide compile-time contracts and could be replaced by a small internal typed-cache interface, if profiling and consumers justify a refactor.

## EventBus UnityEditor reference

### Directly confirmed
- `Assets/_MythHunter/Code/Events/EventBus.cs` has an unconditional `using UnityEditor;` at line 12.
- In the complete inspected file, the only match among `UnityEditor`, `EditorApplication`, `AssetDatabase`, `EditorUtility`, and preprocessor `#if/#endif` was that using directive; no editor API use or conditional compilation was found in those matches.

### Architectural conclusion
The import appears unused and is a **probable player-build portability defect** because the runtime file references an editor-only namespace. This was not validated by a non-Editor player build, so do not mark it as a proven build failure. Recommended action is a focused source fix removing the unused import, followed by Unity Editor compile/tests and a player build check. Keep this out of the documentation-only audit branch unless there is an explicit scope decision.

## Dependency boundary snapshot
```text
RPGFramework.ECS.EntityManager (authoritative storage)
                 ↑
ComponentCache<T> (derived cached copies)
                 ↑
ComponentCacheRegistry (update scheduling + statistics)
                 ↑
EntitiesInstaller (MythHunter DI composition)

IEventBus contract
   ├─ EventBus (priority/pipeline implementation; imports UnityEditor + domain namespaces)
   └─ SimpleEventBus (simpler implementation selected initially by CoreInstaller)
```

## Remaining evidence gaps
- Search all code and scenes/assets for `IComponentCacheRegistry`, `GetCache<T>`, `FindEntities<T>`, `ForEach<T>`, and the cache extension methods; GitHub code search has not provided reliable occurrence results, and the current inspected sources are insufficient for an exhaustive consumer list.
- Confirm whether `EventsInstaller` replaces the `SimpleEventBus` binding in each supported bootstrap path.
- Run a non-Editor Unity player build to validate `UnityEditor` import behavior before calling it a reproducible build failure.

## Consumer references found in repository code search
- `Assets/_MythHunter/Code/Systems/Heroes/HeroSystem.cs`: receives `IComponentCacheRegistry`; caches `StatsComponent` and `HealthComponent`; calls both caches' `Update()` on every `HeroSystem.Update`. This creates per-frame refreshes independent of the registry's every-five-updates cadence.
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/MovementSystem.cs`: holds `ComponentCache<PositionComponent>`, `ComponentCache<MovementComponent>`, and `ComponentCache<PathComponent>` created directly from `IEntityManager` rather than via `IComponentCacheRegistry`.
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/VisibilitySystem.cs`: holds `ComponentCache<PositionComponent>` and `ComponentCache<VisibilityComponent>`, likewise constructed directly.
- `ComponentCacheExtensions` exposes FindEntities/FindEntity/ForEach/WithComponent/WithoutComponent helpers. Current GitHub code search returned definitions, but did not establish consumer call sites for these helpers.

### Updated cache conclusion
The cache subsystem has at least three direct gameplay system consumers and two separate construction paths (registry-managed versus directly constructed). Removing it would require migration and behavior/performance comparison. A more targeted candidate is normalizing cache ownership and update policy, but only after profiling and checking whether direct cache consumers expect a fresh snapshot each frame.
## Additional direct consumers and lifecycle evidence
- `HeroSystem` updates its `StatsComponent` and `HealthComponent` caches on every `Update` call. `MovementSystem` calls `Update()` on Position/Movement/Path caches in both `Initialize` and every `Update`; `VisibilitySystem` similarly updates Position/Visibility caches during initialization and each update. These consumers are not relying solely on the registry's configured five-frame cadence.
- The same cache component type may be represented by a registry-managed cache and one or more directly constructed caches (e.g. PositionComponent appears in both MovementSystem and VisibilitySystem). This duplicates cached snapshots and refresh work, though not authoritative entity storage.
- `ComponentCacheRegistry`'s constructor also subscribes to `IPhaseProvider` callbacks and entity created/destroyed events, and hardcodes the Name/Description/Value components as auto-create types. This makes its policy MythHunter-specific rather than a generic cache catalog.

## Event bus binding chain verified
- `CoreInstaller` initially binds `IEventBus -> SimpleEventBus` as a fallback.
- `EventsInstaller` resolves `EventNetworkMode`; Offline mode registers `IEventBus -> EventBus`, while client/server modes register `INetworkEventBus -> NetworkEventBus` then alias the same instance to `IEventBus`.
- `NetworkEventBus` inherits `EventBus`, so the unconditional `using UnityEditor;` in EventBus also affects the network bus compilation path. No other UnityEditor API usage or preprocessor guard was found in the inspected file. The unused editor-only namespace import should be removed in a dedicated source fix and validated by a player build before describing it as a confirmed build failure.
## Additional consumer search results
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/MovementSystem.cs` has three concrete cache fields and manually updates them.
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/VisibilitySystem.cs` has two concrete cache fields and manually updates them.
- `Assets/_MythHunter/Code/Systems/Heroes/HeroSystem.cs` obtains Stats/Health caches from registry and manually updates them.
- The current repository search result for `ComponentCache<` also surfaces an unnamed component-cache enumeration helper inside `ComponentCache.cs`; inspect that nested helper for whether it duplicates iteration APIs.

## Search coverage caveat
These are positive matches from GitHub's indexed code search, not proof that every usage is returned. The exact reverse-reference count is still unverified.
## Follow-up after PR #34
- PR #34 removed the unused `using UnityEditor;` directive from `EventBus.cs`; merged as `0313e8afd0e621fa310e31373ebe014cf5fe9889`.
- Both `Minimal CI` and `RPGFramework ECS Runtime` passed on the PR head. These workflows do not replace the still-pending local Unity EditMode/full-project compile and non-Editor player-build verification.
- The source-level portability concern is addressed; the final player-build test is now confirmation rather than a reason to block the trivial unused-import cleanup.