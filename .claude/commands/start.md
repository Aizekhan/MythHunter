# Start command

Read the permanent rules from `CLAUDE.md`, then gather current session context:

1. Fetch and read `STATUS.md` and `HANDOFF_NEXT_CHAT.md` from [Aizekhan/RPG-Framework-Roadmap](https://github.com/Aizekhan/RPG-Framework-Roadmap) main branch via `gh api`.

2. Identify the single ACTIVE roadmap item from `STATUS.md`.

3. For the active PR (if any):
   - Check PR state: `gh pr view <number> --json state,headRefOid,headRefName,url`
   - Verify HEAD SHA matches what's documented.
   - Check CI status on exact HEAD SHA.

4. Check `git status --short --branch` in all local worktrees:
   - `D:\MythHunter-Git`
   - `D:\MythHunter-PR48-Validation`
   - `D:\MythHunter-PR50-Validation`
   - Other worktrees if known

5. Report in 5–8 lines:
   - ACTIVE item and its current state
   - PR HEAD SHA and CI status
   - Local worktree status
   - Next immediate action

6. Continue work on the ACTIVE item unless user requests otherwise.
