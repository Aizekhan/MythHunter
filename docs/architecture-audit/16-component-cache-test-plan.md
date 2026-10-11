# ComponentCache consistency test plan

Status: **Audit/test design only. No runtime behavior changed.** Based on `ComponentCache<T>`, `ComponentCacheRegistry`, and MovementSystem/VisibilitySystem call sites.

## Contract to establish
- Authoritative state belongs to `RPGFramework.ECS.IEntityManager`; cache is a derived read model and never a second source of truth.
- After `Update`/explicit invalidation, each strategy has documented freshness and membership behavior.
- A cache must not return destroyed entities or entities that no longer have T unless stale-snapshot semantics are explicitly part of the contract.
- Cache iteration order is not assumed deterministic unless promised by API.

## Required tests per strategy

### Full
- [ ] Initial population includes all entities that have component T.
- [ ] Existing component value updated in IEntityManager then cache Update refreshes the stored struct.
- [ ] Adding T after initial population is reflected after Update.
- [ ] Removing T is reflected after Update.
- [ ] Destroying an entity is reflected after Update.
- [ ] Empty world/cache are safe.

### Incremental
- [ ] New component IDs are discovered across >100 IDs and batches terminate.
- [ ] Existing value changes: explicitly decide whether Incremental refreshes IDs already in the cache or requires explicit invalidation.
- [ ] Component removal and entity destruction prune IDs or mark them invalid.
- [ ] Re-adding T to an entity is recovered without duplicate entries.
- [ ] If Incremental intentionally only adds, document the limitation and test callers that need freshness.

### OnDemand
- [ ] First Get for a present component reads IEntityManager and caches it.
- [ ] Get for absent T returns documented result and doesn't confuse default value with a cache hit.
- [ ] After authoritative value mutation, define expected behavior: invalidate/refresh or explicit stale-hit contract.
- [ ] After component removal/entity destroy, Get and GetAllEntityIds don't expose invalid membership beyond documented policy.

## Registry/cache lifecycle
- [ ] EntityCreated/EntityDestroyed events affect only registered cache instances; direct system caches are separately owned.
- [ ] Registry-created cache is reused by GetCache<T>.
- [ ] UpdateInterval is deterministic and not in conflict with consumers requiring per-frame freshness.
- [ ] Phase-specific update changes only intended cache types.
- [ ] Dispose unsubscribes phase/event handlers and clears registry-owned caches.

## Consumer regression tests
- [ ] MovementSystem: Position/Movement/Path mutations produce correct next-tick behavior after refresh.
- [ ] VisibilitySystem: removal/destroy between updates cannot leave stale target or observer IDs.
- [ ] HeroSystem: Stats/Health freshness matches intended frame semantics; registry interval doesn't conflict with manual Update.
- [ ] Benchmark direct IEntityManager.GetEntitiesWith<T>() against snapshot iteration before deciding on cache consolidation/removal.

## Acceptance gate
1. Tests fail against current implementation for each confirmed stale-value/membership defect.
2. Runtime fix changes one contract at a time; no broad cache removal.
3. Unity EditMode, full-project compile/clean Console, and relevant smoke checks pass.
4. Benchmark representative entity counts before claiming performance improvement.

Do not implement until fixture approach and expected Incremental/OnDemand semantics are selected.