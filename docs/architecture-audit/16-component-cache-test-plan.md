# ComponentCache consistency test plan

Status: **Implementation/tests added in PR #48 and merged to `dev` as `0d97060d45d978cafbe376cd43f00eb8fdf9f175`; exact-current-head Unity validation pending.** The historical 44/44 local report is for HEAD `40b6296`, not the merged head.

## Contract to establish
- Authoritative state belongs to `RPGFramework.ECS.IEntityManager`; cache is a derived read model and never a second source of truth.
- After `Update`/explicit invalidation, each strategy has documented freshness and membership behavior.
- A cache must not return destroyed entities or entities that no longer have T unless stale-snapshot semantics are explicitly part of the contract.
- Cache iteration order is not assumed deterministic unless promised by API.

## Required tests per strategy

### Full
- [x] Characterization test covers population, changed value refresh, removal, and destroyed entity behavior (`ComponentCache_FullRefreshesChangedValuesAndRemovesMissingMembership`).
- [ ] Explicit empty-world/cache behavior test.
- [ ] Re-run this coverage on exact merged `dev` head; current-head Unity execution has not been confirmed.

### Incremental
- [x] Characterization test covers >100 IDs, rotating batch convergence, changed values, component removal, re-add, and entity destruction (`ComponentCache_IncrementalRefreshesValuesAndPrunesRemovedComponentsInBatches`).
- [x] Chosen contract in PR #48: refresh up to 100 values per `Update()` with a rotating cursor; reconcile component membership every update.
- [ ] Re-run this coverage on exact merged `dev` head; current-head Unity execution has not been confirmed.

### OnDemand
- [x] Characterization test covers present Get, authoritative value change, removal, re-add and destruction (`ComponentCache_OnDemandRevalidatesValuesAndMembershipAgainstEntityManager`).
- [x] Chosen contract in PR #48: `Get` reads the current value from `IEntityManager` on every call and prunes membership on a missing component.
- [ ] Decide/test positive-membership semantics: `Contains`/ID enumeration prune stale IDs but do not discover new IDs not yet cached.
- [ ] Re-run this coverage on exact merged `dev` head; current-head Unity execution has not been confirmed.

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
1. The targeted cache-consistency implementation is merged, but current merged-head Unity results remain unconfirmed.
2. Exact-current-head Unity EditMode, full-project compile/clean Console, Player Build, and Movement/Visibility/Hero smoke checks pass.
3. Complete source reverse-reference, asmdef, and Unity scene/prefab serialized-reference review.
4. Clarify OnDemand positive-membership and cache-local `Add`/authoritative-write semantics with consumers.
5. Benchmark representative entity counts before claiming performance improvement or proposing cache consolidation/removal.

Do not start broad ECS extraction until EPIC 01 Phase 1 exits.