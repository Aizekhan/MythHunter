# Handoff command

Update roadmap documentation with actual current state, then create/update roadmap PR.

## CRITICAL: STATUS.md Update Checklist

**MUST update ALL fields in STATUS.md:**

1. **Last reconciled date**: `> Last reconciled against GitHub: YYYY-MM-DD.`
2. **Current source head**: Get exact SHA with `git log -1 --format=%H` on MythHunter dev
3. **Verified completed sequence**: Add ALL recent merges with their SHAs
4. **Active roadmap item**: Update to current PR or mark complete
5. **PR section**: Update branch, HEAD SHA, scope, validation status

## Step-by-step procedure:

1. **Get current state**:
   ```bash
   cd D:\MythHunter-Git
   git checkout dev && git pull
   git log --oneline -5  # Get recent merges
   ```

2. Prepare updates to `STATUS.md` and `HANDOFF_NEXT_CHAT.md`:
   - Record exact commit SHAs for all verified work.
   - Document proof (test reports, build results) with exact SHAs.
   - List open gates (Unity EditMode, compile, Player Build, smoke tests, etc.).
   - Update ACTIVE item status or mark complete and identify next ACTIVE item.

2. Clone or fetch latest from `Aizekhan/RPG-Framework-Roadmap` (main branch).

3. Create feature branch for handoff update (e.g., `handoff/epic-05-9-completion`).

4. Apply updates to `STATUS.md` and `HANDOFF_NEXT_CHAT.md`.

5. Commit with clear message describing what milestone/stage is being handed off.

6. Push branch and open PR to main in roadmap repository.

7. Show PR URL and summary of changes.

8. Short report to user:
   - What changed in source repository
   - Which SHAs were verified with what proof
   - What was merged (if any)
   - What gates remain open
   - Next ACTIVE item
