# RPG Framework - Strategic Focus Plan

**Date:** 2026-10-24
**Mission:** Create a **production-ready, reusable RPG Framework** for Unity

---

## Core Principle

> **MythHunter is NOT the product - it's just a test/demo project!**
>
> The Framework must be:
> - ✅ **Game-agnostic** - works for ANY RPG game
> - ✅ **Well-documented** - easy for other developers to use
> - ✅ **Fully tested** - high confidence in stability
> - ✅ **Independently packaged** - can be imported into any Unity project

---

## Current Framework Status

### What We Have (29 Systems):

**Core Infrastructure:**
- ✅ ECS (Entity Component System)
- ✅ Logging
- ✅ Serialization
- ✅ Delta Compression
- ✅ Pooling

**Gameplay Systems:**
- ✅ Combat
- ✅ Stats
- ✅ Inventory
- ✅ Items
- ✅ Loot
- ✅ Abilities
- ✅ Progression
- ✅ Skills
- ✅ Status Effects
- ✅ Quests
- ✅ Dialogue
- ✅ Crafting
- ✅ Trading
- ✅ Factions
- ✅ Achievements
- ✅ Guilds
- ✅ Companions
- ✅ Environment (Weather/Time/Seasons)
- ✅ DynamicEvents
- ✅ Housing
- ✅ Persistence
- ✅ Networking
- ✅ Tools (Debug/Profiling)

**Editor Tools:**
- ⚠️ Custom Editors (some disabled - low priority)
- ⚠️ Editor Windows (some disabled - low priority)
- ✅ Property Drawers
- ✅ Gizmos

### What Needs Work:

1. **Documentation** - Framework API reference missing
2. **Examples** - Minimal code examples (not full game integration)
3. **Testing** - Need to verify all 31 test suites pass
4. **Packaging** - Not ready as standalone Unity package

---

## New Mission Statement

### Primary Goal:
**Create a production-ready RPG Framework that any Unity developer can drop into their project and start using immediately**

### Success Criteria:
1. ✅ **100% test coverage** - All tests passing
2. ✅ **API Documentation** - Every public interface documented
3. ✅ **Code Examples** - Simple, focused usage examples for each system
4. ✅ **Package Structure** - Can be imported as UPM package
5. ✅ **Zero Dependencies** - Framework has NO dependencies on MythHunter
6. ✅ **Performance Validated** - Profiling data for all major operations

### Non-Goals (MythHunter-specific):
- ❌ Full game UI panels
- ❌ Game-specific integration adapters
- ❌ Sample game content (MythHunter guilds, companions, etc.)
- ❌ Complete playable demo game

---

## Refocused Roadmap

### Phase 1: Framework Validation (Current - Week 1)

**Goal:** Verify Framework itself is solid

1. ✅ **Compilation** - 0 errors (DONE)
2. 🔄 **Test Suite** - Run all 31 test files (IN PROGRESS)
3. **Fix Failures** - Address any failing tests
4. **Clean Editor Code** - Fix/document disabled Editor windows

**Deliverable:** Framework compiles clean, all tests pass

### Phase 2: Documentation (Week 2)

**Goal:** Make Framework usable by other developers

1. **API Reference** - Document every public interface/class
   - What it does
   - How to use it
   - Code example
   - Parameters/return values

2. **Architecture Guide** - Explain Framework design
   - ECS pattern
   - Manager vs ECS-direct systems
   - Event system
   - Data flow

3. **Getting Started** - Quick start guide
   - Installation
   - Basic setup
   - First quest/dialogue/combat

**Deliverable:** Complete documentation website/markdown

### Phase 3: Standalone Examples (Week 3)

**Goal:** Provide minimal working examples

Create **SimpleRPG** demo project (NOT MythHunter):
- Basic hero with stats/inventory
- Simple combat encounter
- Quest with dialogue
- Item crafting
- Loot drop

**Each example in isolation:**
- `Example_Combat.cs` - Minimal combat setup
- `Example_Quests.cs` - Create and complete quest
- `Example_Dialogue.cs` - Simple conversation
- `Example_Crafting.cs` - Craft an item
- etc.

**Deliverable:** Working examples folder in Framework

### Phase 4: Package & Distribution (Week 4)

**Goal:** Make Framework easily importable

1. **UPM Package Structure**
   ```
   Packages/
     com.rpgframework.core/
       package.json
       Runtime/
       Tests/
       Documentation~/
       Samples~/
   ```

2. **Clean Dependencies**
   - Remove Unity-specific coupling where possible
   - Document required Unity version
   - List any external dependencies

3. **Release Preparation**
   - Semantic versioning
   - Changelog
   - License
   - README

**Deliverable:** Installable Unity package

---

## What About MythHunter?

### MythHunter's New Role:

**MythHunter becomes a "Reference Implementation"** - it shows how to USE the Framework in a real game, but it's separate.

```
Assets/
  _Framework/          ← The product (what we're building)
    ECS/
    Combat/
    Quests/
    ...

  _MythHunter/         ← Reference implementation (optional)
    GameLogic/
    UI/
    Scenes/
    ...
```

### MythHunter Work (Low Priority):
- Fix integration adapters → Shows proper Framework integration patterns
- Create game UI → Shows how to build UI with Framework
- Add game content → Demonstrates Framework capabilities

**But:** Framework comes first. MythHunter is secondary.

---

## Immediate Next Actions

### Today (Oct 24):
1. ✅ Fix compilation - DONE
2. 🔄 Run Framework tests - IN PROGRESS
3. ⏳ Review test results
4. ⏳ Create Framework inventory document

### Tomorrow (Oct 25):
1. Fix any failing Framework tests
2. Start API documentation (pick 3 most important systems)
3. Create simple usage examples for those 3 systems

### This Week:
1. All Framework tests passing
2. First draft of API docs
3. At least 5 working code examples

---

## Framework Quality Checklist

Before calling Framework "production-ready":

### Code Quality:
- [ ] 100% of tests passing
- [ ] Zero compilation warnings
- [ ] No `#if FALSE` disabled code in Framework
- [ ] All public APIs have XML documentation comments
- [ ] Code follows consistent style

### Documentation:
- [ ] API reference for all systems
- [ ] Architecture overview
- [ ] Getting started guide
- [ ] Code examples for each system
- [ ] Migration guide (if updating)

### Testing:
- [ ] Unit tests for all systems
- [ ] Integration tests for cross-system features
- [ ] Performance benchmarks documented
- [ ] Edge cases covered

### Packaging:
- [ ] Clean package structure
- [ ] Proper assembly definitions
- [ ] No circular dependencies
- [ ] Minimal external dependencies
- [ ] Works in fresh Unity project

### Examples:
- [ ] At least 10 standalone examples
- [ ] Examples cover all major systems
- [ ] Examples are simple and focused
- [ ] Examples include explanatory comments

---

## Success Metrics

### How we'll know Framework is ready:

1. **"5-Minute Test"** - New developer can:
   - Import package
   - Read quick start
   - Have working example in 5 minutes

2. **"Independence Test"** - Framework can:
   - Work in empty Unity project
   - No references to MythHunter
   - All features accessible through public APIs

3. **"Confidence Test"** - We can:
   - Run full test suite in < 1 minute
   - All tests green
   - Performance profiling shows acceptable benchmarks

---

## Resources Needed

### Documentation:
- API doc generator (or manual markdown)
- Example code templates
- README template

### Testing:
- Unity Test Framework (already have)
- Performance profiler
- Code coverage tool (optional)

### Packaging:
- UPM package structure knowledge
- Git tags/releases setup

---

## Timeline

**4-Week Sprint to Production-Ready Framework:**

- **Week 1:** Validation & Cleanup (Oct 24-31)
- **Week 2:** Documentation (Nov 1-7)
- **Week 3:** Examples (Nov 8-14)
- **Week 4:** Packaging & Polish (Nov 15-21)

**Target Release:** November 22, 2026

---

## Team Communication

### What Changed:
- **OLD FOCUS:** Build MythHunter game with integrated Framework
- **NEW FOCUS:** Build reusable RPG Framework, MythHunter is just a demo

### Why It Matters:
- Framework quality > Game features
- Generic solutions > MythHunter-specific code
- Documentation > Implementation speed
- Reusability > Quick hacks

### How It Affects Work:
- Stop working on MythHunter UI/gameplay
- Start creating Framework documentation
- Think "What would another developer need?"
- Question "Is this Framework or game logic?"

---

## Next Session Goals

When we continue, we should:
1. ✅ Review test results
2. ✅ Pick first 3 systems to document
3. ✅ Create example code for those 3
4. ✅ Start API reference documentation

---

**Remember:** We're not building a game. We're building a tool that helps others build games faster.
