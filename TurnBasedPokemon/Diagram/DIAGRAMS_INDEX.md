# Pokemon Turn-Based Game - Complete Use Case Diagrams Index

**Created:** April 7, 2026  
**Last Updated:** April 7, 2026
**Status:** ✅ All 19 Use Cases Documented with 3 Diagram Types Each  
**Total Diagrams Created:** 57 PlantUML Diagrams (19 UML + 19 Activity + 19 Sequence)

---

## 📋 Diagram Directory Structure

### 🎮 PHASE 1: CORE PvE & MECHANICS (5 Use Cases)

#### UC_1: Login / Authentication
- **Use Case Diagram:** [Detailed_UC1_Login.plantuml](Diagram/UsecaseDiagram/Detailed_UC1_Login.plantuml)
- **Activity Diagram:** [Detailed_UC1_Login_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC1_Login_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC1_Login_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC1_Login_Sequence.plantuml)

**Description:** User login system with authentication, password verification, and game session initialization.

#### UC_2: Battle Wild Pokemon
- **Use Case Diagram:** [Detailed_UC2_BattleWild.plantuml](Diagram/UsecaseDiagram/Detailed_UC2_BattleWild.plantuml)
- **Activity Diagram:** [Detailed_UC2_BattleWild_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC2_BattleWild_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC2_BattleWild_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC2_BattleWild_Sequence.plantuml)

**Description:** Turn-based battle system with wild Pokemon, including move selection, damage calculation, and encounter mechanics.

#### UC_3: Catch Pokemon
- **Use Case Diagram:** [Detailed_UC3_Catch.plantuml](Diagram/UsecaseDiagram/Detailed_UC3_Catch.plantuml)
- **Activity Diagram:** [Detailed_UC3_Catch_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC3_Catch_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC3_Catch_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC3_Catch_Sequence.plantuml)

**Description:** Pokémon catching mechanics including ball types, catch rate calculation, and Pokedex updates.

#### UC_4: Use Items (Potions/Pokeballs)
- **Use Case Diagram:** [Detailed_UC4_UseItems.plantuml](Diagram/UsecaseDiagram/Detailed_UC4_UseItems.plantuml)
- **Activity Diagram:** [Detailed_UC4_UseItems_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC4_UseItems_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC4_UseItems_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC4_UseItems_Sequence.plantuml)

**Description:** Inventory management including healing items, status effects, and consumable usage both in and out of battle.

#### UC_5: Manage Pokemon Team
- **Use Case Diagram:** [Detailed_UC5_ManageTeam.plantuml](Diagram/UsecaseDiagram/Detailed_UC5_ManageTeam.plantuml)
- **Activity Diagram:** [Detailed_UC5_ManageTeam_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC5_ManageTeam_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC5_ManageTeam_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC5_ManageTeam_Sequence.plantuml)

**Description:** Team composition management including adding, removing, and swapping Pokemon in the active team.

---

### 📖 PHASE 3: STORY MODE (2 Use Cases)

#### UC_6: Explore Story
- **Use Case Diagram:** [Detailed_UC6_ExploreStory.plantuml](Diagram/UsecaseDiagram/Detailed_UC6_ExploreStory.plantuml)
- **Activity Diagram:** [Detailed_UC6_ExploreStory_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC6_ExploreStory_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC6_ExploreStory_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC6_ExploreStory_Sequence.plantuml)

**Description:** Story mode progression including narrative playback, location exploration, and chapter advancement.

#### UC_7: Interact with NPCs
- **Use Case Diagram:** [Detailed_UC7_InteractNPC.plantuml](Diagram/UsecaseDiagram/Detailed_UC7_InteractNPC.plantuml)
- **Activity Diagram:** [Detailed_UC7_InteractNPC_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC7_InteractNPC_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC7_InteractNPC_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC7_InteractNPC_Sequence.plantuml)

**Description:** NPC interaction system including dialogue trees, quest giving, Pokemon trading, and NPC battles.

---

### 🌐 PHASE 5: PvP ONLINE (2 Use Cases)

#### UC_8: Join Matchmaking
- **Use Case Diagram:** [Detailed_UC8_Matchmaking.plantuml](Diagram/UsecaseDiagram/Detailed_UC8_Matchmaking.plantuml)
- **Activity Diagram:** [Detailed_UC8_Matchmaking_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC8_Matchmaking_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC8_Matchmaking_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC8_Matchmaking_Sequence.plantuml)

**Description:** PvP matchmaking queue system including player pairing based on ELO rating and opponent selection.

#### UC_9: Battle Other Players (PvP)
- **Use Case Diagram:** [Detailed_UC9_PvPBattle.plantuml](Diagram/UsecaseDiagram/Detailed_UC9_PvPBattle.plantuml)
- **Activity Diagram:** [Detailed_UC9_PvPBattle_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC9_PvPBattle_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC9_PvPBattle_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC9_PvPBattle_Sequence.plantuml)

**Description:** Real-time PvP battle system with turn synchronization, move submission timeout, and result calculation.

---

### 💾 PHASE 6: PERSISTENCE & ADMIN (3 Use Cases)

#### UC_10: Save/Load Game Data
- **Use Case Diagram:** [Detailed_UC10_SaveLoad.plantuml](Diagram/UsecaseDiagram/Detailed_UC10_SaveLoad.plantuml)
- **Activity Diagram:** [Detailed_UC10_SaveLoad_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC10_SaveLoad_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC10_SaveLoad_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC10_SaveLoad_Sequence.plantuml)

**Description:** Game persistence including auto-save, manual save, load functionality, and game state serialization/deserialization.

#### UC_11: View Match History
- **Use Case Diagram:** [Detailed_UC11_MatchHistory.plantuml](Diagram/UsecaseDiagram/Detailed_UC11_MatchHistory.plantuml)
- **Activity Diagram:** [Detailed_UC11_MatchHistory_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC11_MatchHistory_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC11_MatchHistory_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC11_MatchHistory_Sequence.plantuml)

**Description:** Match history tracking and replay system including detailed battle statistics and opponent information.

#### UC_12: Manage Game Content (Admin)
- **Use Case Diagram:** [Detailed_UC12_ManageContent.plantuml](Diagram/UsecaseDiagram/Detailed_UC12_ManageContent.plantuml)
- **Activity Diagram:** [Detailed_UC12_ManageContent_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC12_ManageContent_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC12_ManageContent_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC12_ManageContent_Sequence.plantuml)

**Description:** Admin panel for managing Pokemon stats, items, user accounts, story content, and audit logging.

---

### 🎮 PHASE 1.5: BATTLE MECHANICS (1 Use Case)

#### UC_13: Level Up / Evolve Pokemon
- **Use Case Diagram:** [Detailed_UC13_LevelUp.plantuml](Diagram/UsecaseDiagram/Detailed_UC13_LevelUp.plantuml)
- **Activity Diagram:** [Detailed_UC13_LevelUp_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC13_LevelUp_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC13_LevelUp_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC13_LevelUp_Sequence.plantuml)

**Description:** Pokemon experience gain, level up mechanics, stat growth, new move learning, and evolution system.

#### UC_14: Heal Team at Pokemon Center
- **Use Case Diagram:** [Detailed_UC14_HealTeam.plantuml](Diagram/UsecaseDiagram/Detailed_UC14_HealTeam.plantuml)
- **Activity Diagram:** [Detailed_UC14_HealTeam_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC14_HealTeam_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC14_HealTeam_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC14_HealTeam_Sequence.plantuml)

**Description:** Pokemon Center healing system including HP restoration, PP recovery, and status effect removal.

---

### 📖 PHASE 3.1: STORY BATTLES (3 Use Cases)

#### UC_15: Challenge Gym Leader
- **Use Case Diagram:** [Detailed_UC15_ChallengeGym.plantuml](Diagram/UsecaseDiagram/Detailed_UC15_ChallengeGym.plantuml)
- **Activity Diagram:** [Detailed_UC15_ChallengeGym_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC15_ChallengeGym_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC15_ChallengeGym_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC15_ChallengeGym_Sequence.plantuml)

**Description:** Gym challenge system including gym requirements, gym puzzles, battle mechanics, badge rewards, and story progression.

#### UC_16: Challenge Pokemon League
- **Use Case Diagram:** [Detailed_UC16_ChallengePokemonLeague.plantuml](Diagram/UsecaseDiagram/Detailed_UC16_ChallengePokemonLeague.plantuml)
- **Activity Diagram:** [Detailed_UC16_ChallengePokemonLeague_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC16_ChallengePokemonLeague_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC16_ChallengePokemonLeague_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC16_ChallengePokemonLeague_Sequence.plantuml)

**Description:** Pokemon League system including Elite Four battles, Champion battle, Hall of Fame, and post-game unlocks.

#### UC_17: Challenge Trainer
- **Use Case Diagram:** [Detailed_UC17_ChallengeTrainer.plantuml](Diagram/UsecaseDiagram/Detailed_UC17_ChallengeTrainer.plantuml)
- **Activity Diagram:** [Detailed_UC17_ChallengeTrainer_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC17_ChallengeTrainer_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC17_ChallengeTrainer_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC17_ChallengeTrainer_Sequence.plantuml)

**Description:** Wild trainer encounters and battles including money rewards and trainer tracking.

---

### 🎮 PHASE 0: ACCOUNT SETUP (2 Use Cases)

#### UC_18: Select Starter Pokemon
- **Use Case Diagram:** [Detailed_UC18_SelectStarter.plantuml](Diagram/UsecaseDiagram/Detailed_UC18_SelectStarter.plantuml)
- **Activity Diagram:** [Detailed_UC18_SelectStarter_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC18_SelectStarter_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC18_SelectStarter_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC18_SelectStarter_Sequence.plantuml)

**Description:** New game initialization including starter Pokemon selection and initial inventory setup.

#### UC_19: Register / Create Account
- **Use Case Diagram:** [Detailed_UC19_Register.plantuml](Diagram/UsecaseDiagram/Detailed_UC19_Register.plantuml)
- **Activity Diagram:** [Detailed_UC19_Register_Activity.plantuml](Diagram/ActivityDiagram/Detailed_UC19_Register_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC19_Register_Sequence.plantuml](Diagram/SequenceDiagram/Detailed_UC19_Register_Sequence.plantuml)

**Description:** User account registration including username/password validation, password hashing, and database initialization.

### 1️⃣ Use Case Diagrams
- **Purpose:** Shows actors and their interactions with the system
- **Shows:** What actors can do, relationships between use cases (<<include>>, <<extend>>, <<invoke>>)
- **Best for:** Understanding system boundaries and user interactions

### 2️⃣ Activity Diagrams
- **Purpose:** Shows the flow of activities and decision points
- **Shows:** Steps, conditions, loops, and parallel processes
- **Best for:** Understanding business logic and workflow details
- **Key elements:** Start/End, decisions, actions, notes explaining complex logic

### 3️⃣ Sequence Diagrams
- **Purpose:** Shows interactions between system components over time
- **Shows:** Objects/components, message flows, activation periods, alternatives
- **Best for:** Understanding system architecture and component communication
- **Key elements:** Participants, messages, loops, alt blocks (conditional flows)

---

## 🔗 Key Features Documented

### Battle Mechanics
- ✅ Turn order calculation (speed-based)
- ✅ Damage calculation with type advantages
- ✅ Status effects and conditions
- ✅ Critical hit chance
- ✅ Pokemon fainting and team switching
- ✅ Experience gain and level up

### PvP System
- ✅ ELO-based matchmaking
- ✅ Real-time turn synchronization
- ✅ Timeout handling (30-second move submission)
- ✅ Match result recording
- ✅ Rank updates and statistics tracking

### Story System
- ✅ Narrative progression
- ✅ NPC dialogue trees
- ✅ Quest system integration
- ✅ Story milestone tracking
- ✅ Location exploration

### Data Persistence
- ✅ Auto-save on key events
- ✅ Manual save file management
- ✅ Game state serialization (JSON)
- ✅ Database synchronization
- ✅ Audit trail for admin actions

---

## 💡 How to Use These Diagrams

1. **For Development:** Use Sequence Diagrams to understand component interactions when coding features
2. **For Testing:** Use Activity Diagrams to create comprehensive test cases
3. **For Documentation:** Use all three to create complete feature documentation
4. **For Code Review:** Compare implementations against Sequence Diagrams
5. **For Stakeholders:** Show Use Case Diagrams for high-level overview

---

## 📂 File Locations

All diagrams are organized in three main folders:

```
Diagram/
├── UsecaseDiagram/
│   ├── Detailed_UC1_Login.plantuml
│   ├── Detailed_UC2_BattleWild.plantuml
│   ├── Detailed_UC3_Catch.plantuml
│   ├── Detailed_UC4_UseItems.plantuml
│   ├── Detailed_UC5_ManageTeam.plantuml
│   ├── Detailed_UC6_ExploreStory.plantuml
│   ├── Detailed_UC7_InteractNPC.plantuml
│   ├── Detailed_UC8_Matchmaking.plantuml
│   ├── Detailed_UC9_PvPBattle.plantuml
│   ├── Detailed_UC10_SaveLoad.plantuml
│   ├── Detailed_UC11_MatchHistory.plantuml
│   └── Detailed_UC12_ManageContent.plantuml
├── ActivityDiagram/
│   ├── Detailed_UC1_Login_Activity.plantuml
│   ├── Detailed_UC2_BattleWild_Activity.plantuml
│   ├── ... (12 activity diagrams total)
└── SequenceDiagram/
    ├── Detailed_UC1_Login_Sequence.plantuml
    ├── Detailed_UC2_BattleWild_Sequence.plantuml
    ├── ... (12 sequence diagrams total)
```

---

## 🎯 Next Steps

To use these diagrams in your documentation:

1. **Render with PlantUML:** Use VS Code PlantUML extension or online plantuml.com
2. **Export to PNG/SVG:** For documentation and presentations
3. **Extract to Documentation:** Create MD files referencing each diagram
4. **Code Generation:** Use activities to guide implementation order

---

**Created by:** GitHub Copilot  
**Format:** PlantUML v1.2024.x compatible  
**Status:** ✅ Ready for implementation reference

---

## 📈 Complete Use Case Coverage

| Phase | Use Cases | Count |
|-------|-----------|-------|
| Phase 0: Account Setup | Register, Select Starter | 2 |
| Phase 1: Core PvE | Login, Battle Wild, Catch, Use Items, Manage Team | 5 |
| Phase 1.5: Battle Mechanics | Level Up, Heal Team | 2 |
| Phase 3: Story Mode | Explore Story, NPC, Gym, League, Trainer | 5 |
| Phase 5: PvP Online | Matchmaking, Battle PvP | 2 |
| Phase 6: Persistence | Save/Load, Match History, Manage Content | 3 |
| **TOTAL** | | **19** |

All use cases now have complete documentation with UML diagrams, Activity diagrams showing workflows, and Sequence diagrams showing component interactions!
