# Architecture card: Data & Serialization

Status: **First-pass source inspection recorded; complete consumer graph pending.**

## Five filters
1. **Needed by any RPG?** Evaluate abstractions, not the directory as a whole.
2. **Specific-game dependency?** Identify Unity/MythHunter/game-domain dependencies.
3. **Reusable unchanged?** Only after transitive dependency and consumer checks.
4. **Framework vs Game Data?** Separate portable contract/runtime from adapters, installers and game policy.
5. **Architectural issue/action?** Record concrete evidence and leave unverified questions open.

## First-pass source review
`IComponentSerializer<T>` and `ComponentSerializerBase<T>` depend on `RPGFramework.ECS.IComponent`; the base class uses `BinaryWriter`/`MemoryStream`, a promising portable serialization seam. `ComponentSerializerRegistry` pulls in MythHunter DI and logger and is bound by `SerializationInstaller`; keep generic binary contract/core separate from DI composition and game-specific serializers. Audit wire format/versioning/schema migration before network or long-term persistence use.

## Dependencies / consumers that need confirmation
- [ ] Enumerate all reverse references, installer bindings, scene/prefab references and asmdef boundaries.
- [ ] Confirm behavior under supported runtime/player targets.
- [ ] Identify ownership, lifecycle and disposal contracts.
- [ ] Record versioning/migration or test requirements before any refactor.

## Provisional framework classification
Portable abstractions may be reusable after splitting engine/provider/game dependencies. Concrete Unity-facing behavior, MythHunter-specific events/data, and composition should remain in the application/game or adapter layer.

## Action
Audit only for now; do not change runtime code until EPIC 01 Phase 1 dependency map and five-filter decisions are approved.