# Disabled Code Audit Report

**Date:** 2026-10-24
**Status:** 0 compilation errors (all problematic code disabled with `#if FALSE`)
**Total Disabled Files:** 15

---

## Summary

All disabled files are wrapped in `#if FALSE...#endif` with TODO comments explaining what needs to be fixed. The code compiles successfully but these features are non-functional until fixed.

---

## Category 1: UI Panels (9 files)

**Problem:** These UI panels were generated assuming manager-style "System" interfaces exist (like `IAbilitySystem`, `IInventory`, `ICombatSystem`), but these interfaces don't exist in the Framework.

### Files:

1. **`Assets/_MythHunter/Code/UI/Panels/Abilities/AbilitiesUI.cs`**
   - Missing: `IAbilitySystem`
   - Framework has: `IAbilityExecutor`, `IAbilityRegistry`, `IAbility`
   - Fix needed: Use `IAbilityExecutor` for executing abilities, `IAbilityRegistry` for browsing

2. **`Assets/_MythHunter/Code/UI/Panels/Inventory/InventoryUI.cs`**
   - Missing: `IInventory`
   - Framework has: `IInventoryContainer<TItem>`, `IEquipmentSlot<TSlotType, TItem>`
   - Fix needed: Access inventory via ECS components directly

3. **`Assets/_MythHunter/Code/UI/Panels/Stats/StatsUI.cs`**
   - Missing: `IStatsSystem`, `IProgressionSystem`
   - Framework has: Stats and Progression as ECS components (Pattern B)
   - Fix needed: Access via `IEntityManager.GetComponent<>`

4. **`Assets/_MythHunter/Code/UI/Panels/Loot/LootUI.cs`**
   - Missing: `ILootSystem`, `ILootSource`
   - Framework has: ECS-based loot system
   - Fix needed: Access loot data through ECS components

5. **`Assets/_MythHunter/Code/UI/Panels/CombatLog/CombatLogUI.cs`**
   - Missing: `ICombatSystem`
   - Framework has: Combat as ECS components
   - Fix needed: Subscribe to combat events

6. **`Assets/_MythHunter/Code/UI/Panels/Crafting/CraftingUI.cs`**
   - Missing: `ICraftingSystem`
   - Framework has: `ICraftingManager` ✅ (Pattern A - Manager-based)
   - Fix needed: Change `ICraftingSystem` → `ICraftingManager`

7. **`Assets/_MythHunter/Code/UI/Panels/Dialogue/DialogueUI.cs`**
   - Missing: `IDialogueSystem`
   - Framework has: `IDialogueManager` ✅ (Pattern A)
   - Fix needed: Change `IDialogueSystem` → `IDialogueManager`

8. **`Assets/_MythHunter/Code/UI/Panels/Trading/TradingUI.cs`**
   - Missing: `ITradingSystem`
   - Framework has: `ITradingManager` ✅ (Pattern A)
   - Fix needed: Change `ITradingSystem` → `ITradingManager`

9. **`Assets/_MythHunter/Code/UI/Panels/StatusEffects/StatusEffectsUI.cs`**
   - Missing: `IStatusEffectSystem`
   - Framework has: `IStatusEffectManager` ✅ (Pattern A)
   - Fix needed: Change `IStatusEffectSystem` → `IStatusEffectManager`

### UI Manager:

10. **`Assets/_MythHunter/Code/UI/Managers/UIManager.cs`**
    - Problem: References all 9 disabled UI panels
    - Fix needed: Re-enable after fixing individual panels

---

## Category 2: Integration Adapters (1 file)

**Problem:** Integration adapter uses incorrect Framework API signatures.

11. **`Assets/_MythHunter/Code/GameLogic/Guilds/Adapters/GuildSystemIntegration.cs`**
    - **Issue 1:** `new Guild(guildData, _factionRegistry)` - Guild constructor takes only 1 parameter (GuildData)
    - **Issue 2:** `IGuildManager.DepositGold()` doesn't exist
    - **Issue 3:** `IGuildManager.WithdrawGold()` doesn't exist
    - **Issue 4:** `HasPermission()` signature mismatch
    - Fix needed: Check actual `IGuildManager` interface and `Guild` constructor

---

## Category 3: Sample/Example Code (4 files)

**Problem:** Sample code uses non-existent or incorrect Framework APIs.

12. **`Assets/_MythHunter/Code/GameLogic/Companions/MythHunterCompanions.cs`**
    - Missing: `ICompanionManager.GetCompanionStats()`
    - Fix needed: Check actual `ICompanionManager` interface

13. **`Assets/_MythHunter/Code/GameLogic/Companions/CompanionUsageExample.cs`**
    - Missing: `ICompanionManager.GetCompanionStats()`, `GetCompanionBehavior()`
    - Fix needed: Check actual `ICompanionManager` interface

14. **`Assets/_MythHunter/Code/GameLogic/Achievements/Sample/SampleAchievements.cs`**
    - Missing: `IAchievementReward` namespace issue
    - Fix needed: Add `using RPGFramework.Achievements.Runtime;`

15. **`Assets/_MythHunter/Code/GameLogic/Guilds/Sample/SampleGuilds.cs`**
    - Problem: Depends on disabled `GuildSystemIntegration`
    - Fix needed: Re-enable after fixing GuildSystemIntegration

---

## Architecture Patterns Reference

### Pattern A: Manager-Based Systems
These have dedicated Manager classes:
- ✅ Crafting → `ICraftingManager`
- ✅ Dialogue → `IDialogueManager`
- ✅ Quest → `IQuestManager`
- ✅ StatusEffect → `IStatusEffectManager`
- ✅ Trading → `ITradingManager`
- ✅ Faction → `IFactionManager`
- ✅ Companion → `ICompanionManager`
- ✅ Guild → `IGuildManager`
- ✅ Achievement → `IAchievementManager`

### Pattern B: ECS Component-Based Systems
These work through direct ECS component access (NO Manager):
- ✅ Inventory → Use `IEntityManager` + `IInventoryContainer<TItem>`
- ✅ Abilities → Use `IAbilityExecutor`, `IAbilityRegistry`
- ✅ Stats → Use `IEntityManager.GetComponent<StatsComponent>`
- ✅ Progression → Use `IEntityManager.GetComponent<ProgressionComponent>`
- ✅ Loot → Use ECS components
- ✅ Combat → Use ECS components

---

## Recommended Fix Order

### Phase 1: Quick Wins (Manager-based UI) - 2 hours
Fix UI panels that just need interface rename:
1. CraftingUI: `ICraftingSystem` → `ICraftingManager`
2. DialogueUI: `IDialogueSystem` → `IDialogueManager`
3. TradingUI: `ITradingSystem` → `ITradingManager`
4. StatusEffectsUI: `IStatusEffectSystem` → `IStatusEffectManager`

### Phase 2: Integration Adapters - 4 hours
Fix the adapter that other code depends on:
5. GuildSystemIntegration: Fix constructor and missing methods
6. SampleGuilds: Re-enable after #5

### Phase 3: Sample Code - 2 hours
Fix or document sample code:
7. SampleAchievements: Add missing using statement
8. MythHunterCompanions: Fix API calls
9. CompanionUsageExample: Fix API calls

### Phase 4: ECS-based UI (Complex) - 8 hours
Rewrite UI panels that need ECS component access:
10. InventoryUI: Rewrite to use ECS + `IInventoryContainer`
11. AbilitiesUI: Rewrite to use `IAbilityExecutor` + `IAbilityRegistry`
12. StatsUI: Rewrite to use ECS components
13. LootUI: Rewrite to use ECS components
14. CombatLogUI: Rewrite to use combat events

### Phase 5: UI Manager - 1 hour
15. UIManager: Re-enable after all panels fixed

**Total Estimated Time:** ~17 hours

---

## Next Steps

1. ✅ **Document disabled code** (DONE - this file)
2. **Create Framework API Reference** - Document what interfaces/methods actually exist
3. **Start Phase 1 fixes** - Quick manager-based UI fixes
4. **Test after each fix** - Ensure 0 compilation errors maintained
5. **Run EditMode tests** - Verify Framework still works
6. **Create demo scene** - Show working systems

---

## Notes

- All disabled code is committed on branch `dev` (HEAD: 9575ea9)
- Zero compilation errors maintained
- Each disabled file has TODO comment explaining issue
- Framework itself is stable - issues are only in Game layer integration/samples
