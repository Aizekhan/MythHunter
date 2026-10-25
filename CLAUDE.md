# Claude Code Process — MythHunter / RPG Framework

> Permanent rules for Claude Code when working on MythHunter RPG Framework.

## Mission
Build a **universal RPG Framework** that is reusable across any RPG game.

**Framework Core principles:**
- Framework Core does NOT depend on MythHunter game-specific logic, UnityEditor, game policies, or adapters.
- Adapters depend on Framework; Framework does NOT depend on adapters.
- No empty modules, broad refactorings, or duplication of single source of truth.

**Five filters for every change:**
1. Is it needed by ANY RPG game (not just MythHunter)?
2. Does it depend on game-specific logic?
3. Is it reusable across different RPG games?
4. Is it Framework Core or Game Layer?
5. Is there proof of the problem being solved?

## Git guardrails
**Prohibited operations:**
- `git reset --hard`, `git clean`, `git stash`, `git checkout` (switching branches) without analyzing working tree.
- `git cherry-pick` without analyzing working tree.
- Writing directly to `dev` branch — ONLY branches and PRs.
- Deleting user files, NUnit XML results, `.meta` files, or `.vscode` configuration.

**Workflow:**
- All source changes through feature/fix branches and pull requests.
- Never merge without explicit user confirmation.

## Unity environment
**Unity Editor:** 6000.0.45f1
**Unity CLI:** 1.0.0-beta.13

**Standard commands:**
```bash
# Recompile project
unity recompile --project-path "<path>" --timeout 180 --verbose

# Run EditMode tests
unity test "<path>" --mode EditMode --output "<xml>" --verbose --timeout <seconds>
```

**Log locations:**
- Editor log: `C:\Users\Admin\AppData\Local\Unity\Editor\Editor.log`
- CLI log: `C:\Users\Admin\AppData\Roaming\UnityHub\logs\cli-log.json`

**Unity CLI guardrails:**
- Do NOT kill Unity CLI Server process (`unity.exe ... serve`).
- Do NOT delete Unity caches without proven cause.
- Do NOT repeat long-running commands without a new hypothesis.
- Run long-running commands in background with log monitoring.

## Proof and validation
**Test/build result validity:**
- Results are valid ONLY for the exact commit SHA tested.
- CI in this repository does NOT compile `Assets/_MythHunter` (Minimal CI = echo, ECS workflow compiles only `Assets/_Framework`).
- Green CI is NOT proof for changes in `_MythHunter`.

**Unity validation sequence:**
1. Unity EditMode tests with NUnit XML report.
2. Full project compile with clean Console.
3. Player Build.
4. Smoke tests (Movement, Visibility, Hero systems).

## Session process
**On session start:**
1. Read `STATUS.md` and `HANDOFF_NEXT_CHAT.md` from [Aizekhan/RPG-Framework-Roadmap](https://github.com/Aizekhan/RPG-Framework-Roadmap) (main branch).
2. Verify exactly ONE ACTIVE item in `STATUS.md`.
3. When documents contradict, reconcile them BEFORE implementation.

**During session:**
- Track work with TodoWrite tool.
- Use AskUserQuestion tool when clarification is needed.
- Maintain exactly ONE in_progress todo at a time.

**At milestone completion:**
1. Update `STATUS.md` and `HANDOFF_NEXT_CHAT.md` with actual state (exact SHAs, proof, open gates).
2. Open/update PR in roadmap repository.
3. Short report: what changed, which SHAs verified, what passed, what merged, next step.

## Execution discipline
- Use `/start` command at session beginning.
- Use `/handoff` command when milestone is complete.
- Check active PR state, HEAD SHA, and CI status before continuing work.
- Verify `git status` in all local worktrees before making changes.
- Keep focus on the single ACTIVE roadmap item.
