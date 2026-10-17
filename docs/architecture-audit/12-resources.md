# Architecture card: Resources

Status: **First-pass source inspection recorded; complete consumer graph pending.**

## Five filters
1. **Needed by any RPG?** Evaluate abstractions, not the directory as a whole.
2. **Specific-game dependency?** Identify Unity/MythHunter/game-domain dependencies.
3. **Reusable unchanged?** Only after transitive dependency and consumer checks.
4. **Framework vs Game Data?** Separate portable contract/runtime from adapters, installers and game policy.
5. **Architectural issue/action?** Record concrete evidence and leave unverified questions open.

## First-pass source review
Resource management is Unity/platform-specific rather than a portable simulation core: `IResourceManager`/`IResourceProvider` use `UnityEngine.Object`, `ResourceManager` depends on MythHunter DI/logging/pooling, and providers target Unity Resources, Addressables and AssetBundles. `PreloadManager` also depends on game phase, event bus, pool manager and DI, and publishes MythHunter preload events. Reusable seam is the provider contract only if separated from Unity object types; the current contract is not engine-agnostic. `ResourceInstaller` is game composition.

## Dependencies / consumers that need confirmation
- [ ] Enumerate all reverse references, installer bindings, scene/prefab references and asmdef boundaries.
- [ ] Confirm behavior under supported runtime/player targets.
- [ ] Identify ownership, lifecycle and disposal contracts.
- [ ] Record versioning/migration or test requirements before any refactor.

## Provisional framework classification
Portable abstractions may be reusable after splitting engine/provider/game dependencies. Concrete Unity-facing behavior, MythHunter-specific events/data, and composition should remain in the application/game or adapter layer.

## Action
Audit only for now; do not change runtime code until EPIC 01 Phase 1 dependency map and five-filter decisions are approved.