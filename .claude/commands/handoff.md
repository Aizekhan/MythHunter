# Handoff command

Update roadmap documentation with actual current state, then create/update roadmap PR.

1. Prepare updates to `STATUS.md` and `HANDOFF_NEXT_CHAT.md`:
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
