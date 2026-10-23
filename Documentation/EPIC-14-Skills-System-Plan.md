# EPIC 14: Skills System Extraction & Integration

**Status**: 🔄 Planning
**Start Date**: 2026-10-17
**Target**: Extract Skills System to Framework, integrate into MythHunter

---

## Mission

Extract a **universal Skills System** from MythHunter into `RPGFramework.Skills.Runtime`, following the established patterns from EPIC 10-13. Create game-layer integration for MythHunter.

---

## Context

### What Exists Today

**In MythHunter:**
- `SkillsComponent.cs` - Basic passive/active skills tracking
- `SocialSkillsComponent.cs` - Social professions/ideologies
- Skills are simple `Skill` struct with `Id` and `Level`
- No Framework-level abstraction

**In Framework:**
- ❌ No `RPGFramework.Skills.Runtime` assembly
- ✅ Can integrate with `RPGFramework.Progression.Runtime` (skill unlocks at levels)
- ✅ Can integrate with `RPGFramework.Abilities.Runtime` (skills grant/modify abilities)
- ✅ Can integrate with `RPGFramework.Stats.Runtime` (skills provide stat bonuses)

### Why Skills System?

**Universal RPG Feature:**
- Skill trees / talent systems exist in: Diablo, Path of Exile, World of Warcraft, Skyrim, etc.
- Passive skills (stat bonuses)
- Active skills (unlock abilities)
- Prerequisite chains (skill A requires skill B)
- Multi-level progression (skill levels 1-10)

**Integration Points:**
- **Progression System**: Skills unlock at specific character levels
- **Abilities System**: Skills can unlock or enhance abilities
- **Stats System**: Skills provide passive stat bonuses
- **Inventory System**: Skills may affect item effectiveness (e.g., "Heavy Armor Proficiency")

---

## Five Framework Filters

1. **Needed by ANY RPG?** ✅ YES - Nearly all RPGs have skill/talent systems
2. **Depends on game-specific logic?** ❌ NO - Skill trees are generic concepts
3. **Reusable across RPG games?** ✅ YES - Applicable to any RPG (fantasy, sci-fi, modern)
4. **Framework Core or Game Layer?**
   - **Framework**: Skill definitions, prerequisites, leveling, unlocks, bonuses
   - **Game Layer**: Specific skill IDs, skill tree layouts, MythHunter-specific effects
5. **Proof of problem?** ✅ YES - MythHunter already has basic implementation needing enhancement

---

## Architecture Design

### Framework Module: `RPGFramework.Skills.Runtime`

**Core Contracts:**
```csharp
public interface ISkill
{
    string Id { get; }
    string Name { get; }
    int MaxLevel { get; }
    SkillType Type { get; } // Passive, Active, Combat, Crafting, etc.
    IReadOnlyList<ISkillPrerequisite> Prerequisites { get; }
}

public interface ISkillTree
{
    string Id { get; }
    string Name { get; }
    IReadOnlyList<ISkill> Skills { get; }
    bool CanLearnSkill(ISkillProgress progress, string skillId);
}

public interface ISkillProgress
{
    int GetSkillLevel(string skillId);
    bool HasSkill(string skillId);
    IReadOnlyList<string> LearnedSkills { get; }
}

public interface ISkillManager
{
    bool LearnSkill(int entityId, string skillId, int level = 1);
    bool IncreaseSkillLevel(int entityId, string skillId);
    int GetSkillLevel(int entityId, string skillId);
    bool CanLearnSkill(int entityId, string skillId);
    IReadOnlyList<string> GetLearnedSkills(int entityId);
}
```

**Data Structures:**
```csharp
public enum SkillType
{
    Passive,      // Stat bonuses (e.g., +10% HP)
    Active,       // Unlocks abilities
    Combat,       // Combat-specific (weapon proficiency)
    Crafting,     // Crafting bonuses
    Social,       // Dialogue/charisma
    Movement,     // Movement speed, jump height
    Magic         // Spell power, mana efficiency
}

public struct SkillData
{
    public string Id;
    public string Name;
    public string Description;
    public int MaxLevel;
    public SkillType Type;
    public List<SkillPrerequisite> Prerequisites;
    public List<SkillBonus> Bonuses; // Stat bonuses per level
}

public struct SkillPrerequisite
{
    public string SkillId;        // Required skill ID
    public int RequiredLevel;     // Required skill level
    public int RequiredCharLevel; // Required character level
}

public struct SkillBonus
{
    public string StatId;         // Stat to modify (e.g., "HP", "AttackPower")
    public float BonusPerLevel;   // Bonus amount per skill level
    public BonusType Type;        // Additive, Multiplicative, Override
}

public struct SkillProgressData
{
    public Dictionary<string, int> SkillLevels; // SkillId -> Level
}
```

**Events:**
```csharp
public struct SkillLearnedEvent : IEvent
{
    public int EntityId;
    public string SkillId;
    public int Level;
    public float TimeStamp;
}

public struct SkillLevelUpEvent : IEvent
{
    public int EntityId;
    public string SkillId;
    public int OldLevel;
    public int NewLevel;
    public float TimeStamp;
}
```

### Game Layer: MythHunter Integration

**Component:**
```csharp
public struct MythHunterSkillsComponent
{
    public SkillProgressData ProgressData;           // Framework data
    public List<string> FavoriteSkills;              // Game-specific: favorited skills
    public float LastSkillLearnTime;                 // Game-specific: analytics
    public long TotalSkillPointsSpent;               // Game-specific: achievements

    public static MythHunterSkillsComponent CreateEmpty();
    public static MythHunterSkillsComponent CreateWithSkills(List<string> skillIds);
}
```

**Adapter:**
```csharp
public class SkillSystemIntegration
{
    private readonly IEntityManager _entityManager;
    private readonly ISkillManager _skillManager;
    private readonly ISkillTree _skillTree;

    public bool LearnSkill(int entityId, string skillId, int level = 1);
    public bool IncreaseSkillLevel(int entityId, string skillId);
    public int GetSkillLevel(int entityId, string skillId);
    public List<ISkill> GetAvailableSkills(int entityId);
    public List<ISkill> GetLearnedSkills(int entityId);
    public bool CanLearnSkill(int entityId, string skillId);
}
```

---

## Phases

### Phase 1: Framework Skills Module (Greenfield)
**Deliverables:**
- `RPGFramework.Skills.Runtime.asmdef`
- Core contracts: `ISkill`, `ISkillTree`, `ISkillProgress`, `ISkillManager`
- Data structures: `SkillData`, `SkillPrerequisite`, `SkillBonus`, `SkillProgressData`
- Events: `SkillLearnedEvent`, `SkillLevelUpEvent`
- Implementations: `Skill`, `SkillTree`, `SkillManager`
- .NET 8.0 test harness (20-25 tests)
- GitHub Actions CI workflow

**Acceptance Criteria:**
- Zero Unity dependencies (`noEngineReferences: true`)
- Zero MythHunter dependencies
- 100% test pass rate
- Static boundary validation

### Phase 2: MythHunter Integration (Dual-Component Pattern)
**Deliverables:**
- `MythHunterSkillsComponent.cs` - Game-layer component
- `SkillSystemIntegration.cs` - Adapter bridging Framework ↔ MythHunter
- Sample skill definitions (Warrior, Mage, Rogue skill trees)
- Integration with ProgressionSystemIntegration (skills unlock at levels)
- Integration with AbilitySystemIntegration (skills unlock abilities)
- Integration with Stats System (skills provide stat bonuses)

**Acceptance Criteria:**
- Dual-component pattern (Framework data + MythHunter metadata)
- Zero modifications to existing Framework code
- MythHunter compilation clean

### Phase 3: Tests & Validation
**Deliverables:**
- `SkillSystemIntegrationTests.cs` (20-25 integration tests)
- Test categories:
  - Skill learning and leveling
  - Prerequisite validation
  - Skill tree traversal
  - Stat bonus application
  - Integration with Progression (level-based unlocks)
  - Integration with Abilities (skill-granted abilities)
- Full project compilation validation
- Test assembly: `MythHunter.Skills.Tests.asmdef`

**Acceptance Criteria:**
- 100% integration test pass rate
- Full project compiles (exit code 0)
- Zero breaking changes to existing systems

### Phase 4: Documentation & Polish
**Deliverables:**
- `EPIC-14-PHASE1-COMPLETE.md` - Framework extraction summary
- `EPIC-14-PHASE2-COMPLETE.md` - Integration summary
- `EPIC-14-PHASE3-COMPLETE.md` - Testing summary
- `EPIC-14-COMPLETE.md` - Overall completion summary
- Sample skill trees (JSON/ScriptableObject examples)
- Integration examples (code snippets)
- Roadmap updates (STATUS.md, HANDOFF_NEXT_CHAT.md)

**Acceptance Criteria:**
- Comprehensive documentation
- Code examples for skill tree creation
- Clear next steps for EPIC 15

---

## Integration with Existing Systems

### With Progression System (EPIC 13 Phase 4)
```csharp
// When character levels up, unlock skills
_progressionIntegration.AddExperience(
    entityId,
    xp,
    source,
    time,
    onLevelUp: (entId, oldLevel, newLevel) =>
    {
        // Check for skills unlocked at newLevel
        var unlockedSkills = _skillSystemIntegration.GetSkillsUnlockedAtLevel(newLevel);
        foreach (var skill in unlockedSkills)
        {
            _skillSystemIntegration.MakeSkillAvailable(entId, skill.Id);
        }
    }
);
```

### With Abilities System (EPIC 13 Phase 3)
```csharp
// When skill is learned, grant associated ability
_skillSystemIntegration.LearnSkill(
    entityId,
    "skill_fireball_mastery",
    onSkillLearned: (entId, skillId) =>
    {
        var abilityId = _skillTree.GetAbilityForSkill(skillId);
        if (!string.IsNullOrEmpty(abilityId))
        {
            _abilitySystemIntegration.GrantAbility(entId, abilityId);
        }
    }
);
```

### With Stats System (EPIC 10 Phase 2)
```csharp
// Apply skill bonuses to stats
var skillLevel = _skillSystemIntegration.GetSkillLevel(entityId, "skill_vitality");
var hpBonus = skillLevel * 10f; // +10 HP per skill level

// Use StatContainer to apply modifier
statContainer.AddModifier("HP", new StatModifier("skill_vitality", hpBonus, ModifierType.Additive));
```

---

## Validation Sequence

Following `CLAUDE.md` validation protocol:

1. **Unity EditMode Tests** - Run `MythHunter.Skills.Tests` assembly (20-25 tests)
2. **Full Project Compile** - Verify zero compilation errors
3. **Integration Smoke Tests** - Manual verification:
   - Learn a skill
   - Level up a skill
   - Verify prerequisites work
   - Verify stat bonuses apply
   - Verify skill unlocks ability

---

## Success Metrics

**Framework Module:**
- Assembly size: ~50-80KB (similar to Abilities.Runtime)
- Test count: 20-25 comprehensive tests
- Zero Unity dependencies
- Zero MythHunter dependencies

**Integration:**
- Integration test count: 20-25 tests
- 100% test pass rate
- Zero breaking changes
- Clean compilation

**Total Effort:**
- Files: ~25-30 files
- Tests: ~40-50 tests (Framework + Integration)
- Phases: 4 phases (similar to EPIC 13)

---

## Risks & Mitigations

**Risk 1: Skill Prerequisites Complexity**
- **Mitigation**: Start with simple prerequisite model (skill level + character level), iterate if needed

**Risk 2: Stat Bonus Application**
- **Mitigation**: Use existing `StatContainer` from EPIC 10 Phase 2, leverage modifier system

**Risk 3: Ability Integration**
- **Mitigation**: Abilities System (EPIC 13 Phase 3) already has grant/revoke API

**Risk 4: Skill Tree UI Data**
- **Mitigation**: Framework provides data model only, UI is game-layer concern (out of scope)

---

## Next Steps

1. Create `Assets/_Framework/Skills/` directory structure
2. Create `RPGFramework.Skills.Runtime.asmdef`
3. Implement core contracts and data structures
4. Write .NET 8.0 test harness
5. Validate Framework module isolation
6. Create MythHunter integration components
7. Write integration tests
8. Validate full compilation
9. Document completion

---

**Ready to start EPIC 14 Phase 1!** 🚀
