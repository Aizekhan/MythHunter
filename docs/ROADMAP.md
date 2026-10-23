# RPG Framework Roadmap

Future features and planned releases.

---

## Version 1.0.0 (Current Release) ✓

**Status:** Released October 2026

### Completed Features
- 21 specialized modules
- 312+ source files
- 286+ unit tests
- Complete documentation
- Sample game demonstration
- Developer tools

---

## Version 1.1.0 (Q1 2027)

**Theme:** Environment & Advanced Features

### Planned Features

#### Weather System Module
- Dynamic weather conditions (rain, snow, fog, etc.)
- Weather effects on gameplay (visibility, movement speed)
- Time of day system
- Seasonal changes
- Weather-based events

#### Guild/Clan Management Module
- Guild creation and membership
- Guild ranks and permissions
- Guild bank/storage
- Guild quests and objectives
- Guild vs Guild features
- Guild reputation system

#### Companion AI Module
- Companion recruitment and dismissal
- AI behavior trees
- Companion commands (follow, attack, defend)
- Companion equipment and inventory
- Companion leveling and skills
- Companion loyalty system

#### Enhancements
- Performance optimizations
- Additional developer tools
- Extended API documentation
- More sample content

**Estimated Release:** March 2027

---

## Version 1.2.0 (Q2 2027)

**Theme:** Housing & Dynamic World

### Planned Features

#### Housing System Module
- Player-owned houses
- Furniture placement and decoration
- House upgrades and expansions
- Storage containers
- Crafting stations in houses
- Visitors and permissions

#### Dynamic Events Module
- Random world events
- Timed events (seasonal festivals, invasions)
- Event chains and consequences
- Player-triggered events
- Event rewards and participation tracking

#### Achievements System (Enhanced)
- Achievement categories
- Hidden achievements
- Achievement rewards
- Progress tracking
- Achievement UI integration

#### Enhancements
- Visual quest/dialogue editor
- Improved modding support
- Performance profiling tools
- Extended integration examples

**Estimated Release:** June 2027

---

## Version 1.3.0 (Q3 2027)

**Theme:** Multiplayer & Networking

### Planned Features

#### Networking Module (Enhanced)
- Client-server architecture
- Player synchronization
- Combat synchronization
- Inventory synchronization
- Quest state synchronization
- Chat system

#### Party System Module
- Party formation and management
- Party invites and kicks
- Shared experience and loot
- Party chat
- Party quests
- Party roles (tank, healer, DPS)

#### Trade System Enhancements
- Player-to-player trading
- Auction house
- Mail system
- Trade history
- Secure trading

#### Enhancements
- Dedicated server support
- Matchmaking system
- Anti-cheat measures
- Network optimization

**Estimated Release:** September 2027

---

## Version 2.0.0 (Q4 2027)

**Theme:** Performance & Advanced Architecture

### Planned Features

#### ECS Enhancements
- Archetype-based storage
- Cache-friendly memory layout
- Parallel system execution
- Job system integration
- SIMD optimizations

#### Advanced AI Module
- Behavior tree designer
- Utility AI system
- Goal-oriented action planning (GOAP)
- Blackboard system
- AI debugging tools

#### Procedural Generation Module
- Dungeon generation
- Loot generation (enhanced)
- Quest generation
- NPC generation
- World generation tools

#### Visual Scripting
- Node-based quest editor
- Dialogue tree visual editor
- Behavior tree editor
- Event system visualizer

#### Enhancements
- Hot reloading support
- Live debugging tools
- Memory profiling
- Performance benchmarks

**Estimated Release:** December 2027

---

## Long-Term Vision (2028+)

### Potential Features

#### Mobile Support
- Touch controls integration
- Mobile-optimized UI
- Performance optimizations for mobile
- Cloud save integration

#### VR/AR Support
- VR interaction system
- Spatial UI
- Motion controls
- Room-scale support

#### Advanced Graphics
- Procedural terrain
- Dynamic LOD system
- Particle effects system
- Shader integration

#### Machine Learning
- AI opponents using ML
- Dynamic difficulty adjustment
- Player behavior prediction
- Content recommendation

#### Modding Framework
- Mod loader
- Mod management UI
- Mod distribution platform
- Modding API documentation

#### Asset Store Package
- Unity Asset Store listing
- Sample projects
- Video tutorials
- Premium support

---

## Community Requests

Features requested by the community (vote on GitHub Discussions):

### High Priority
- [ ] Console port support (PlayStation, Xbox, Switch)
- [ ] Localization system for multiple languages
- [ ] Animation state machine integration
- [ ] Sound system integration
- [ ] Save file encryption

### Medium Priority
- [ ] Pet system
- [ ] Mount system
- [ ] Fishing system
- [ ] Mini-games framework
- [ ] Reputation decay over time

### Low Priority
- [ ] Cooking system
- [ ] Farming system
- [ ] Relationship/romance system
- [ ] Karma/morality system
- [ ] Time travel mechanics

---

## Release Schedule

| Version | Release Date | Status |
|---------|--------------|--------|
| 1.0.0 | Oct 2026 | Released ✓ |
| 1.1.0 | Mar 2027 | Planned |
| 1.2.0 | Jun 2027 | Planned |
| 1.3.0 | Sep 2027 | Planned |
| 2.0.0 | Dec 2027 | Planned |

---

## How to Influence the Roadmap

We value community input! Here's how you can help shape the future:

1. **Vote on Features:** Use GitHub Discussions reactions to vote on requested features
2. **Submit Ideas:** Create a new Discussion with your feature request
3. **Contribute Code:** Implement features and submit Pull Requests
4. **Sponsor Development:** Support specific features through sponsorship
5. **Report Issues:** Help prioritize fixes by reporting bugs

---

## Breaking Changes Policy

We follow semantic versioning:

- **Major versions (2.0.0):** May include breaking changes (with migration guide)
- **Minor versions (1.1.0):** New features, backward compatible
- **Patch versions (1.0.1):** Bug fixes, backward compatible

Breaking changes will be:
- Clearly documented in CHANGELOG
- Accompanied by migration guides
- Announced in advance
- Minimized whenever possible

---

## Experimental Features

Features in early development (use at your own risk):

- **Archetype ECS:** High-performance ECS storage (experimental branch)
- **Visual Scripting:** Node-based editors (alpha)
- **ML AI:** Machine learning integration (prototype)

---

## Deprecation Policy

Before removing features:
1. Mark as `[Obsolete]` in code
2. Document in CHANGELOG
3. Provide migration path
4. Keep for at least one major version
5. Remove in next major version

---

## Stay Updated

- **GitHub:** Watch the repository for releases
- **Changelog:** Check CHANGELOG.md for updates
- **Discussions:** Join conversations about future features
- **Releases:** Subscribe to release notifications

---

**Last Updated:** October 23, 2026

This roadmap is subject to change based on community feedback, technical constraints, and project priorities.

**Questions about the roadmap?** Open a discussion on GitHub!
