# Smoke Tests — Manual Validation

Smoke tests verify that critical game systems initialize and run without crashing.

## Prerequisites
- **Player Build**: `D:\MythHunter-Git\Builds\MythHunter-DevBuild.exe`
- **Build Status**: ✅ Success (exit code 0)
- **Build Size**: 3.0 GB
- **Compiled with**: Visual Studio 2026 + IL2CPP

## Smoke Test Procedure

### 1. **Launch Test**
```bash
./Builds/MythHunter-DevBuild.exe
```
**Expected**: Game launches without crashes, reaches main scene/menu.
**Pass Criteria**: No exceptions in Player.log, game window opens.

### 2. **MovementSystem Test**
**Actions**:
- Create/spawn a character entity
- Verify character can move (WASD/arrow keys)
- Check movement is smooth, no stuttering

**Pass Criteria**:
- Entity moves in response to input
- Position updates correctly
- No errors in logs related to MovementSystem

### 3. **VisibilitySystem Test**
**Actions**:
- Observe multiple entities on screen
- Move camera/character to test FOV/visibility changes
- Verify entities appear/disappear based on visibility rules

**Pass Criteria**:
- Visibility updates work
- No flickering or incorrect culling
- No errors in logs related to VisibilitySystem

### 4. **HeroSystem Test**
**Actions**:
- Select/interact with hero entity
- Verify stats (Health, Stamina) display correctly
- Perform actions that consume resources (movement, abilities)

**Pass Criteria**:
- Stats update correctly
- Resource consumption works
- No errors in logs related to HeroSystem

### 5. **Integration Test**
**Actions**:
- Play for 5-10 minutes
- Perform typical gameplay actions
- Monitor for memory leaks, frame drops, or crashes

**Pass Criteria**:
- Game runs stable for 10 minutes
- No crashes or exceptions
- Frame rate stable (no degradation over time)

## Log Locations
- **Player Log**: `%APPDATA%\..\LocalLow\<CompanyName>\MythHunter\Player.log`
- **Build Log**: `D:\MythHunter-Git\PlayerBuild.log`

## Automated Validation (Future)
For automated smoke tests, consider:
1. Unity Test Runner (PlayMode tests) — requires assembly references setup
2. Custom test scene with automated entity spawning
3. Headless mode with -batchmode for CI/CD

## Current Status
- **Player Build**: ✅ SUCCESS
- **Manual Smoke Tests**: ✅ PASSED (validated 2026-10-13 at commit 92c9abe)
  - ✅ Launch Test: Game launches without crashes
  - ✅ Movement System: Character movement works correctly
  - ✅ Visibility System: Entity visibility updates work correctly
  - ✅ Hero System: Stats and resource consumption work correctly
  - ✅ Integration Test: Game runs stable, no crashes
- **Automated Tests**: ⏸️ Requires assembly reference configuration

## Notes
- Smoke tests are NOT exhaustive — they verify basic functionality only
- Full QA testing required before release
- Focus on "does it run without crashing?" not "does every feature work perfectly?"

## Success Criteria Summary
✅ Game launches
✅ Movement works
✅ Visibility works
✅ Hero stats work
✅ No crashes for 10 minutes

If all 5 pass → **Smoke tests PASSED** ✅
