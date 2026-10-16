# EPIC 12 Phase 4 — Quick Start Guide

**Manual Validation in Unity Editor**

---

## Prerequisites ✅

- Phase 1: Core Combat Integration ✅ Complete (`c52304d`)
- Phase 2: CombatSystem Integration ✅ Complete (`23dadc6`)
- Phase 3: Editor Migration Utility ✅ Complete (`1302b07`)
- Unity Editor 6000.0.45f1 installed
- Project compiled successfully

---

## Phase 4: Entity Migration and Validation

### Step 1: Open Unity Editor

```bash
# Project path:
D:\MythHunter-Git

# Unity version:
6000.0.45f1
```

**Wait for compilation to complete** before proceeding.

---

### Step 2: Migration Menu Location

In Unity Editor top menu:
```
MythHunter → Combat Migration → ...
```

**Available Commands:**
1. **Migrate All Entities** — Main migration
2. **Show Migration Statistics** — Check progress
3. **Remove Old Components (After Validation)** — Cleanup (destructive)
4. **Rollback Single Entity** — Emergency rollback

---

### Step 3: Pre-Migration Check

**Before migrating, check current state:**

1. Click: `MythHunter → Combat Migration → Show Migration Statistics`

**Expected Output:**
```
Total entities: X
Not migrated: X
Migrated (with old): 0
Migrated (cleaned): 0
Progress: 0.0%
```

---

### Step 4: Run Migration

1. Click: `MythHunter → Combat Migration → Migrate All Entities`

2. **Confirmation Dialog Appears:**
   ```
   This will migrate ALL entities from old CombatStatsComponent
   to new Framework-based components.

   The old components will be preserved for safety.

   Continue?
   ```

3. Click: **"Migrate"**

4. **Wait for migration to complete**

5. **Results Dialog:**
   ```
   Migration finished successfully!

   Total entities: X
   Not migrated: Y
   Migrated: Z
   Progress: X%

   Entities migrated this run: Z
   ```

6. Click: **"OK"**

---

### Step 5: Verify Migration

**Check statistics again:**

1. Click: `MythHunter → Combat Migration → Show Migration Statistics`

**Expected Output:**
```
Total entities: X
Not migrated: 0 (or small number)
Migrated (with old): X
Migrated (cleaned): 0
Progress: 100% (or close)
```

---

### Step 6: PlayMode Validation

**Enter PlayMode and test combat:**

#### 6a. Entity Spawning
- [ ] Spawn entities with combat capabilities
- [ ] Check Console for errors
- [ ] Verify entities have new components:
  - `MythHunterStatsComponent`
  - `MythHunterResourceComponent`
  - `CombatStateComponent`

#### 6b. Combat Engagement
- [ ] Start combat between two entities
- [ ] Verify `CombatStateComponent.IsInCombat` = true
- [ ] Check combat target assignment
- [ ] No errors in Console

#### 6c. Damage Calculation
- [ ] Entity A attacks Entity B
- [ ] Verify damage is applied
- [ ] Check Health decreases correctly
- [ ] Verify defense stat affects damage
- [ ] Check Console for calculation logs

#### 6d. Resource Updates
- [ ] **Health:** Decreases on damage taken
- [ ] **Mana:** Updates correctly (if used)
- [ ] **Rage:** Increases during combat
- [ ] **Concentration:** Decreases on dodge/block
- [ ] No negative values
- [ ] No overflow errors

#### 6e. Critical Hits
- [ ] Execute multiple attacks
- [ ] Observe critical hits occurring
- [ ] Verify critical multiplier applied correctly
- [ ] Check combat logs for "Critical Hit!" messages

#### 6f. Dodging Mechanics
- [ ] Entity with high dodge chance
- [ ] Verify dodge events trigger
- [ ] Check Concentration cost applied
- [ ] Verify dodge success rate reasonable

#### 6g. Blocking Mechanics
- [ ] Entity with high block chance
- [ ] Verify block events trigger
- [ ] Check Concentration cost applied
- [ ] Verify damage reduction on block

---

### Step 7: Extended Testing

**Run game for 10-15 minutes:**

- [ ] Multiple combat encounters
- [ ] Different entity types
- [ ] Various combat scenarios:
  - 1v1 combat
  - Multiple enemies
  - Boss fights (if applicable)
  - Long combat encounters
  - Quick combat encounters

**Monitor Console:**
- [ ] No errors
- [ ] No warnings (or acceptable warnings only)
- [ ] No stack traces
- [ ] No infinite loops

---

### Step 8: Post-Validation Decision

**If validation successful:**

#### Option A: Keep Old Components (Safe)
- Leave both old and new components
- Monitor for extended period (days/weeks)
- Remove old components later when confident

#### Option B: Remove Old Components (Cleanup)
⚠️ **CAUTION:** Destructive operation

1. Exit PlayMode
2. Click: `MythHunter → Combat Migration → Remove Old Components (After Validation)`
3. **First Confirmation:**
   ```
   WARNING: This will permanently remove old CombatStatsComponent
   from ALL migrated entities!

   Only do this AFTER validating that combat works correctly.

   This action cannot be undone without restoring from version control.

   Continue?
   ```
   Click: **"Remove"**

4. **Second Confirmation:**
   ```
   Are you absolutely sure?

   This will delete old components from migrated entities.
   ```
   Click: **"Yes, Remove"**

5. **Results Dialog:**
   ```
   Old components removed successfully!

   Components removed: X
   Remaining migrated (cleaned): X
   Remaining not migrated: Y
   ```

6. **Re-test combat** to ensure still works!

---

### Step 9: Rollback (If Issues Found)

**If validation fails:**

#### Single Entity Rollback
1. Click: `MythHunter → Combat Migration → Rollback Single Entity`
2. Enter entity ID
3. Click: **"OK"**
4. Verify rollback successful

#### Full Project Rollback
```bash
cd D:\MythHunter-Git
git status
# If uncommitted changes exist:
git stash

# Rollback to pre-migration state:
git checkout 7203deb

# Or reset to before Phase 2:
git reset --hard 7203deb
```

⚠️ **Note:** Only use full rollback if migration completely broken

---

### Step 10: Report Results

**After validation:**

1. **Document results:**
   - Migration statistics (total entities, migrated count)
   - Validation results (pass/fail for each test)
   - Any errors or issues encountered
   - Performance observations

2. **Update roadmap:**
   - Mark Phase 4 as complete (if successful)
   - Document any issues found
   - Note any follow-up work needed

3. **Create commit (if cleanup performed):**
   ```bash
   cd D:\MythHunter-Git
   git status
   # If changes exist from removing old components
   git add .
   git commit -m "feat: EPIC 12 Phase 4 - Entity migration validation complete"
   git push origin dev
   ```

---

## Known Limitations

### UpdateCombatStatsBasedOnStance
- **Status:** Only works with old components
- **Impact:** Stance modifiers disabled for migrated entities
- **Workaround:** Keep old component for entities using stance system
- **Future:** Will be redesigned to use StatModifier system

---

## Troubleshooting

### Issue: Migration shows 0 entities
**Cause:** No entities with `CombatStatsComponent` exist yet
**Solution:** Normal if fresh project, entities will be created at runtime

### Issue: Compilation errors after migration
**Cause:** Migration changed component types
**Solution:** Check CombatSystem is using integration helpers, not direct component access

### Issue: Combat not working after migration
**Cause:** Possible integration helper issue
**Solution:**
1. Check Console for errors
2. Verify `CombatSystemIntegration` is injected
3. Test with rollback
4. Report issue with error logs

### Issue: Resources not updating
**Cause:** Helper methods may have issues
**Solution:**
1. Check `MythHunterResourceComponent` values in Inspector
2. Verify SetRage/SetConcentration methods called
3. Check for clamp errors (negative/overflow)

### Issue: Stats showing incorrect values
**Cause:** StatContainer may not be initialized
**Solution:**
1. Check `MythHunterStatsComponent.Stats` in Inspector
2. Verify stat types registered correctly
3. Check stat defaults in migration code

---

## Success Criteria

**Phase 4 is complete when:**

✅ All entities successfully migrated (100% or acceptable %)
✅ Combat works correctly with new components
✅ No errors in Console during combat
✅ All resource types update correctly (Health, Mana, Rage, Concentration)
✅ Critical hits and dodging work as expected
✅ Extended testing (10-15 min) passes without issues
✅ Decision made: keep old components OR remove them
✅ Results documented in roadmap

---

## Files Reference

**Migration Logic:**
- `Assets/_MythHunter/Code/GameLogic/Combat/Migration/CombatComponentMigration.cs`

**Editor Utility:**
- `Assets/_MythHunter/Editor/Combat/CombatComponentMigrationEditor.cs`

**Integration Helper:**
- `Assets/_MythHunter/Code/GameLogic/Combat/Adapters/CombatSystemIntegration.cs`

**Updated System:**
- `Assets/_MythHunter/Code/Systems/Gameplay/Combat/CombatSystem.cs`

**Documentation:**
- `Documentation/EPIC-12-Phases-2-3-Summary.md` — Complete technical summary
- `D:\RPG-Framework-Roadmap\HANDOFF_NEXT_CHAT.md` — Detailed Phase 4 guide

---

## Support

**If issues arise:**
1. Check Console for error messages
2. Review `Documentation/EPIC-12-Phases-2-3-Summary.md`
3. Check roadmap `HANDOFF_NEXT_CHAT.md`
4. Use rollback if necessary
5. Document issue for next session

---

**Ready to start Phase 4!** 🚀

**Next Action:** Open Unity Editor and begin Step 1.
