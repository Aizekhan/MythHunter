# EPIC 11 — New Chat Handoff

**Date:** 2026-10-14
**Previous EPIC:** EPIC 10 (RPG Foundation Domain Modules) — ✅ COMPLETE
**New EPIC:** EPIC 11 (Optional Gameplay Modules)
**Objective:** Extract inventory, loot, abilities, and progression systems into universal RPG Framework modules

---

## Context Summary

### What Was Just Completed

**EPIC 10: RPG Foundation Domain Modules** — ALL 5 PHASES COMPLETE 🎉

**Delivered (2026-10-01 to 2026-10-14):**
- ✅ Phase 1: Resource System (HP, MP, Stamina) — PR #65, SHA `8b5dc22`
- ✅ Phase 2: Stat System (Stats, Modifiers) — PR #67, SHA `999405c`
- ✅ Phase 3: Damage Calculation — PR #68, SHA `2fd0b99`
- ✅ Phase 4: Status Effects (Buffs, Debuffs, DOT/HOT) — PR #69, SHA `f26bad5`
- ✅ Phase 5: Death/Life State — PR #70, SHA `1475b2c`

**Metrics:**
- 112 files (+9274 insertions)
- 124 tests (100% pass rate)
- 2 assemblies: RPGFramework.Combat.Runtime, RPGFramework.Stats.Runtime
- Zero Unity dependencies ✅
- Zero game coupling ✅

**Current dev HEAD:** `1475b2c` (merged 2026-10-14T07:21:00Z)

---

## EPIC 11 Scope

### Goal
Extract gameplay systems (inventory, loot, abilities, progression) from MythHunter into universal, reusable RPG Framework modules.

### Proposed Phases

**Phase 1: Inventory System**
- Item containers (bags, equipment slots)
- Inventory capacity and stacking
- Item instance management
- Add/remove/transfer operations

**Phase 2: Item System**
- Item types and properties
- Equipment system (equip/unequip)
- Item stat modifiers (integrates with EPIC 10 Phase 2)
- Rarity and quality tiers

**Phase 3: Loot System**
- Loot tables and drop pools
- Rarity-based drop chances
- Loot generation and rewards
- Integration with combat (death triggers loot)

**Phase 4: Ability System**
- Skill/ability definitions
- Cooldown management
- Resource costs (integrates with EPIC 10 Phase 1)
- Ability effects (integrates with EPIC 10 Phase 3-4)

**Phase 5: Progression System**
- XP and leveling
- Stat growth on level-up
- Unlock systems
- Skill trees (optional)

**Phase 6: Integration (Optional)**
- Complete MythHunter adapters
- End-to-end gameplay flow
- Performance optimization

---

## Prerequisites (All Met ✅)

- ✅ EPIC 06: Serialization Foundation (for item data persistence)
- ✅ EPIC 07: Modular Game Composition (for clean boundaries)
- ✅ EPIC 08: Lifecycle Stabilization (for proper init/cleanup)
- ✅ EPIC 09 Phase 1: Generic Pooling (for item instance pooling)
- ✅ EPIC 10 All Phases: Combat foundation (for integration)

---

## Starting Point

### Repository State

**Main Repository:** Aizekhan/MythHunter
- Branch: `dev`
- HEAD: `1475b2c`
- Status: Clean ✅

**Roadmap Repository:** Aizekhan/RPG-Framework-Roadmap
- Branch: `main`
- Latest: EPIC 10 completion documented
- STATUS.md: Ready for EPIC 11 activation

### Key Files to Read First

**In MythHunter repository:**
1. `CLAUDE.md` — Project rules and workflow
2. `Documentation/EPIC-10-Complete-Next-Steps.md` — Rationale for EPIC 11

**In Roadmap repository (fetch via gh api):**
1. `STATUS.md` — Current operational state
2. `HANDOFF_NEXT_CHAT.md` — Execution handoff
3. `RPG_FRAMEWORK_MASTER_PLAN.md` — Full roadmap context

### Existing MythHunter Code to Audit

**Inventory/Items:**
- Search for: `Inventory`, `Item`, `Equipment`, `Container`, `Slot`
- Likely locations: `Assets/_MythHunter/Code/Components/`, `Assets/_MythHunter/Code/Systems/`

**Loot:**
- Search for: `Loot`, `Drop`, `Reward`, `LootTable`
- May be in combat or entity systems

**Abilities:**
- Search for: `Ability`, `Skill`, `Cooldown`, `CombatAbility`
- Already seen: `CombatAbilitySystem` in CombatSystem audit

**Progression:**
- Search for: `Level`, `Experience`, `XP`, `Progression`, `Unlock`
- May be in hero or entity systems

---

## First Steps in New Chat

### 1. Execute `/start` Command

This will:
- Fetch STATUS.md and HANDOFF_NEXT_CHAT.md from roadmap
- Identify ACTIVE roadmap item
- Check git status in all worktrees
- Report next action

### 2. Begin EPIC 11 Phase 1: Inventory System Audit

**Recommended approach:**
```
1. Search MythHunter codebase for inventory-related code:
   - Glob for *Inventory*.cs, *Item*.cs, *Container*.cs, *Slot*.cs
   - Grep for "class.*Inventory", "interface.*IInventory"
   - Read existing implementations

2. Create audit document:
   - Documentation/EPIC-11-Gameplay-Domain-Audit.md
   - Document what exists (similar to EPIC-10-RPG-Domain-Audit.md)
   - Identify Framework vs Game boundaries
   - Record dependencies and coupling

3. Plan Phase 1 extraction:
   - Documentation/EPIC-11-Phase1-Implementation-Plan.md
   - Define interfaces (IInventory, IItemContainer, IItem)
   - Design component structure
   - Plan test coverage (aim for 20-30 tests like EPIC 10)

4. Implement Phase 1:
   - Create branch: feature/epic11-phase1-inventory-system
   - Build Runtime files in Assets/_Framework/Inventory/Runtime/
   - Build Tests in Assets/_Framework/Inventory/Tests/
   - Update .NET test harness
   - Create PR

5. Validate and merge:
   - .NET tests pass (CI)
   - Static boundary validation
   - Unity EditMode tests (optional but recommended)
   - Merge to dev
```

### 3. Follow EPIC 10 Pattern

**What worked well in EPIC 10:**
- ✅ Comprehensive audit first (EPIC-10-RPG-Domain-Audit.md)
- ✅ Detailed implementation plans per phase (500-600 lines)
- ✅ .NET test harness for fast CI (seconds vs minutes)
- ✅ Static boundary validation (prevents Unity/game coupling)
- ✅ 20-35 tests per phase (thorough coverage)
- ✅ Additive only (zero breaking changes to MythHunter)
- ✅ GitHub Actions validation before merge

**Apply same pattern to EPIC 11!**

---

## Key Principles (from CLAUDE.md)

### Framework Core Principles
- Framework Core does NOT depend on MythHunter, UnityEditor, game policies, or adapters
- Adapters depend on Framework; Framework does NOT depend on adapters
- No empty modules, broad refactorings, or duplication of single source of truth

### Five Filters for Every Change
1. Is it needed by ANY RPG game (not just MythHunter)?
2. Does it depend on game-specific logic?
3. Is it reusable across different RPG games?
4. Is it Framework Core or Game Layer?
5. Is there proof of the problem being solved?

### Git Guardrails
- Never write directly to `dev` — ONLY branches and PRs
- Never `git reset --hard`, `git clean`, `git stash` without analyzing working tree
- All source changes through feature/fix branches and pull requests
- Never merge without explicit user confirmation

### Unity Validation
- Unity EditMode tests with NUnit XML report
- Full project compile with clean Console
- .NET tests (fast CI feedback)
- Static boundary validation (prevents dependencies)

---

## Expected Workflow

**For each phase (1-5):**

1. **Audit** (1-2 hours)
   - Search existing code
   - Document findings
   - Identify Framework boundaries

2. **Plan** (1-2 hours)
   - Create implementation plan
   - Define interfaces and components
   - Design test coverage

3. **Implement** (2-4 hours)
   - Create Runtime files (10-15 files typically)
   - Create Tests (20-35 tests)
   - Update .NET harness

4. **Validate** (30-60 min)
   - .NET tests pass
   - Static boundary check
   - GitHub Actions GREEN

5. **PR & Merge** (30 min)
   - Create descriptive PR
   - Wait for CI
   - Merge to dev

**Total per phase:** ~6-10 hours
**Total for EPIC 11:** ~30-50 hours (5-6 phases)

---

## Success Criteria

**By end of EPIC 11:**
- ✅ Inventory system extracted to Framework
- ✅ Item system extracted to Framework
- ✅ Loot system extracted to Framework
- ✅ Ability system extracted to Framework
- ✅ Progression system extracted to Framework
- ✅ 100-150+ tests total (20-30 per phase)
- ✅ Zero Unity dependencies
- ✅ Zero MythHunter coupling
- ✅ All GitHub Actions GREEN
- ✅ MythHunter still works (additive only)

---

## Immediate Next Command

In new chat, type:

```
/start
```

This will load the roadmap context and guide you to the first action.

Then proceed with EPIC 11 Phase 1 audit:

```
I'm ready to start EPIC 11 Phase 1: Inventory System.
First, let me audit the existing MythHunter inventory implementation.
```

---

## Notes

- Current chat used 130K+ tokens (getting long)
- Fresh context in new chat will improve efficiency
- EPIC 10 took ~2 weeks with similar scope
- EPIC 11 may be slightly larger (6 phases vs 5)
- Expect similar timeline: 2-3 weeks

---

**Good luck with EPIC 11! Follow the EPIC 10 pattern and it will go smoothly.** 🚀
