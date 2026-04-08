# Pokemon Turn-Based Game - Complete Diagrams Index

**Created:** April 7, 2025  
**Last Updated:** June 2025  
**Status:** All 18 Use Cases Documented with 3 Diagram Types Each + Class/State/Database Diagrams  
**Architecture:** Clean Architecture (.NET 9.0, C#, JSON file persistence, SignalR PvP)

---

## Diagram Directory Structure

### PHASE 0: ACCOUNT SETUP (2 Use Cases)

#### UC_17: Select Starter Pokemon
- **Use Case Diagram:** [Detailed_UC17_SelectStarter.plantuml](UsecaseDiagram/Detailed_UC17_SelectStarter.plantuml)
- **Activity Diagram:** [Detailed_UC17_SelectStarter_Activity.plantuml](ActivityDiagram/Detailed_UC17_SelectStarter_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC17_SelectStarter_Sequence.plantuml](SequenceDiagram/Detailed_UC17_SelectStarter_Sequence.plantuml)

**Description:** New game starter selection via StoryService.GiveStarter() and PokemonSpawner.

#### UC_18: Register / Create Account
- **Use Case Diagram:** [Detailed_UC18_Register.plantuml](UsecaseDiagram/Detailed_UC18_Register.plantuml)
- **Activity Diagram:** [Detailed_UC18_Register_Activity.plantuml](ActivityDiagram/Detailed_UC18_Register_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC18_Register_Sequence.plantuml](SequenceDiagram/Detailed_UC18_Register_Sequence.plantuml)

**Description:** Account registration via AuthenService.Register(RegisterRequest) with JSON persistence.

---

### PHASE 1: CORE PvE & MECHANICS (5 Use Cases)

#### UC_1: Login / Authentication
- **Use Case Diagram:** [Detailed_UC1_Login.plantuml](UsecaseDiagram/Detailed_UC1_Login.plantuml)
- **Activity Diagram:** [Detailed_UC1_Login_Activity.plantuml](ActivityDiagram/Detailed_UC1_Login_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC1_Login_Sequence.plantuml](SequenceDiagram/Detailed_UC1_Login_Sequence.plantuml)

**Description:** Login via AuthenService.Login(LoginRequest), loads Player into GameSession.

#### UC_2: Battle Wild Pokemon
- **Use Case Diagram:** [Detailed_UC2_BattleWild.plantuml](UsecaseDiagram/Detailed_UC2_BattleWild.plantuml)
- **Activity Diagram:** [Detailed_UC2_BattleWild_Activity.plantuml](ActivityDiagram/Detailed_UC2_BattleWild_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC2_BattleWild_Sequence.plantuml](SequenceDiagram/Detailed_UC2_BattleWild_Sequence.plantuml)

**Description:** Turn-based battle via BattleService, DamageResult calculation, BattleTurnResult.

#### UC_3: Catch Pokemon
- **Use Case Diagram:** [Detailed_UC3_Catch.plantuml](UsecaseDiagram/Detailed_UC3_Catch.plantuml)
- **Activity Diagram:** [Detailed_UC3_Catch_Activity.plantuml](ActivityDiagram/Detailed_UC3_Catch_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC3_Catch_Sequence.plantuml](SequenceDiagram/Detailed_UC3_Catch_Sequence.plantuml)

**Description:** Catching via CatchService.ExecuteCapture(), CatchFormula, CatchResult with shake count.

#### UC_4: Use Items (Potions/Pokeballs)
- **Use Case Diagram:** [Detailed_UC4_UseItems.plantuml](UsecaseDiagram/Detailed_UC4_UseItems.plantuml)
- **Activity Diagram:** [Detailed_UC4_UseItems_Activity.plantuml](ActivityDiagram/Detailed_UC4_UseItems_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC4_UseItems_Sequence.plantuml](SequenceDiagram/Detailed_UC4_UseItems_Sequence.plantuml)

**Description:** Item usage via ICatchService (Pokeballs) and IHealingService (potions) in InventoryScreen.

#### UC_5: Manage Pokemon Team *(Under Development)*
- **Use Case Diagram:** [Detailed_UC5_ManageTeam.plantuml](UsecaseDiagram/Detailed_UC5_ManageTeam.plantuml)
- **Activity Diagram:** [Detailed_UC5_ManageTeam_Activity.plantuml](ActivityDiagram/Detailed_UC5_ManageTeam_Activity_UnderDevelopment.plantuml)
- **Sequence Diagram:** [Detailed_UC5_ManageTeam_Sequence.plantuml](SequenceDiagram/Detailed_UC5_ManageTeam_Sequence_UnderDevelopment.plantuml)

**Description:** Team management — currently handled inline via CatchService and BattleScreen switching.

---

### PHASE 1.5: BATTLE MECHANICS (2 Use Cases)

#### UC_12: Level Up / Evolve Pokemon
- **Use Case Diagram:** [Detailed_UC12_LevelUp.plantuml](UsecaseDiagram/Detailed_UC12_LevelUp.plantuml)
- **Activity Diagram:** [Detailed_UC12_LevelUp_Activity.plantuml](ActivityDiagram/Detailed_UC12_LevelUp_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC12_LevelUp_Sequence.plantuml](SequenceDiagram/Detailed_UC12_LevelUp_Sequence.plantuml)

**Description:** ExpService.ProcessVictory() → ProcessExpResult, evolution via WorldGenService.

#### UC_13: Heal Team at Pokemon Center
- **Use Case Diagram:** [Detailed_UC13_HealTeam.plantuml](UsecaseDiagram/Detailed_UC13_HealTeam.plantuml)
- **Activity Diagram:** [Detailed_UC13_HealTeam_Activity.plantuml](ActivityDiagram/Detailed_UC13_HealTeam_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC13_HealTeam_Sequence.plantuml](SequenceDiagram/Detailed_UC13_HealTeam_Sequence.plantuml)

**Description:** StoryService.HealTeamAtCenter() restores all Pokemon HP.

---

### PHASE 3: STORY MODE (5 Use Cases)

#### UC_6: Explore Story
- **Use Case Diagram:** [Detailed_UC6_ExploreStory.plantuml](UsecaseDiagram/Detailed_UC6_ExploreStory.plantuml)
- **Activity Diagram:** [Detailed_UC6_ExploreStory_Activity.plantuml](ActivityDiagram/Detailed_UC6_ExploreStory_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC6_ExploreStory_Sequence.plantuml](SequenceDiagram/Detailed_UC6_ExploreStory_Sequence.plantuml)

**Description:** Story mode via StoryScreen → StoryService, location-based progression from story.json.

#### UC_7: Interact with NPCs
- **Use Case Diagram:** [Detailed_UC7_InteractNPC.plantuml](UsecaseDiagram/Detailed_UC7_InteractNPC.plantuml)
- **Activity Diagram:** [Detailed_UC7_InteractNPC_Activity.plantuml](ActivityDiagram/Detailed_UC7_InteractNPC_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC7_InteractNPC_Sequence.plantuml](SequenceDiagram/Detailed_UC7_InteractNPC_Sequence.plantuml)

**Description:** NPC interaction via StoryScreen.HandleTalk(npc), battle trigger on npc.Action.

#### UC_14: Challenge Gym Leader
- **Use Case Diagram:** [Detailed_UC14_ChallengeGym.plantuml](UsecaseDiagram/Detailed_UC14_ChallengeGym.plantuml)
- **Activity Diagram:** [Detailed_UC14_ChallengeGym_Activity.plantuml](ActivityDiagram/Detailed_UC14_ChallengeGym_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC14_ChallengeGym_Sequence.plantuml](SequenceDiagram/Detailed_UC14_ChallengeGym_Sequence.plantuml)

**Description:** Gym challenge via StoryService.TryChallengeGym() with PokemonSpawner team generation.

#### UC_15: Challenge Pokemon League
- **Use Case Diagram:** [Detailed_UC15_ChallengePokemonLeague.plantuml](UsecaseDiagram/Detailed_UC15_ChallengePokemonLeague.plantuml)
- **Activity Diagram:** [Detailed_UC15_ChallengePokemonLeague_Activity.plantuml](ActivityDiagram/Detailed_UC15_ChallengePokemonLeague_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC15_ChallengePokemonLeague_Sequence.plantuml](SequenceDiagram/Detailed_UC15_ChallengePokemonLeague_Sequence.plantuml)

**Description:** League via StoryService.TryChallengePokemonLeague(boss), AuthenService.SetChampion().

#### UC_16: Challenge Trainer *(Under Development)*
- **Use Case Diagram:** [Detailed_UC16_ChallengeTrainer.plantuml](UsecaseDiagram/Detailed_UC16_ChallengeTrainer.plantuml)
- **Activity Diagram:** [Detailed_UC16_ChallengeTrainer_Activity.plantuml](ActivityDiagram/Detailed_UC16_ChallengeTrainer_Activity_UnderDevelopment.plantuml)
- **Sequence Diagram:** [Detailed_UC16_ChallengeTrainer_Sequence.plantuml](SequenceDiagram/Detailed_UC16_ChallengeTrainer_Sequence.plantuml)

**Description:** Trainer battle via StoryService.TryChallengeTrainer(npc).

---

### PHASE 5: PvP ONLINE (2 Use Cases)

#### UC_8: Join Matchmaking
- **Use Case Diagram:** [Detailed_UC8_Matchmaking.plantuml](UsecaseDiagram/Detailed_UC8_Matchmaking.plantuml)
- **Activity Diagram:** [Detailed_UC8_Matchmaking_Activity.plantuml](ActivityDiagram/Detailed_UC8_Matchmaking_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC8_Matchmaking_Sequence.plantuml](SequenceDiagram/Detailed_UC8_Matchmaking_Sequence.plantuml)

**Description:** PvP matchmaking via PvPService → NetworkService → BattleHub → MatchmakingService.

#### UC_9: Battle Other Players (PvP)
- **Use Case Diagram:** [Detailed_UC9_PvPBattle.plantuml](UsecaseDiagram/Detailed_UC9_PvPBattle.plantuml)
- **Activity Diagram:** [Detailed_UC9_PvPBattle_Activity.plantuml](ActivityDiagram/Detailed_UC9_PvPBattle_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC9_PvPBattle_Sequence.plantuml](SequenceDiagram/Detailed_UC9_PvPBattle_Sequence.plantuml)

**Description:** PvP battle via PvPScreen → INetworkService, server-side PvPManager with DamageResult.

---

### PHASE 6: PERSISTENCE (2 Use Cases)

#### UC_10: Save/Load Game Data
- **Use Case Diagram:** [Detailed_UC10_SaveLoad.plantuml](UsecaseDiagram/Detailed_UC10_SaveLoad.plantuml)
- **Activity Diagram:** [Detailed_UC10_SaveLoad_Activity.plantuml](ActivityDiagram/Detailed_UC10_SaveLoad_Activity.plantuml)
- **Sequence Diagram:** [Detailed_UC10_SaveLoad_Sequence.plantuml](SequenceDiagram/Detailed_UC10_SaveLoad_Sequence.plantuml)

**Description:** Save/Load via AuthenService.SaveProgress() / LoadProgress() using JSON file persistence.

#### UC_11: View Match History *(Under Development)*
- **Use Case Diagram:** [Detailed_UC11_MatchHistory.plantuml](UsecaseDiagram/Detailed_UC11_MatchHistory.plantuml)
- **Activity Diagram:** [Detailed_UC11_MatchHistory_Activity.plantuml](ActivityDiagram/Detailed_UC11_MatchHistory_Activity_UnderDevelopment.plantuml)
- **Sequence Diagram:** [Detailed_UC11_MatchHistory_Sequence.plantuml](SequenceDiagram/Detailed_UC11_MatchHistory_Sequence.plantuml)

**Description:** Only Player.PvPWins/PvPLosses counters exist. Detailed tracking not yet implemented.

---

## Additional Diagram Types

### Class Diagrams
Located in `ClassDiagram/` — See [ClassDiagrams_Index.md](ClassDiagram/ClassDiagrams_Index.md)
- Domain Layer, Application Layer, Infrastructure Layer, Presentation Layer
- Detailed service diagrams (Battle, Catch, Exp, Story, Network, etc.)
- Project overview and layer interaction diagrams

### State Diagram
- **[GameStateAndStory.plantuml](StateDiagram/GameStateAndStory.plantuml)** — Game state machine: LoginScreen → MainMenu → StoryScreen/PvP → BattleScreen

### Database Diagrams
Located in `DatabaseDiagram/` — See [DatabaseDiagrams_Index.md](DatabaseDiagram/DatabaseDiagrams_Index.md)
- Logical data model (currently using JSON file persistence)
- ERD with entity relationships

---

## Diagram Types Guide

### 1. Use Case Diagrams
- **Purpose:** Shows actors and their interactions with the system
- **Shows:** What actors can do, relationships between use cases
- **Best for:** Understanding system boundaries and user interactions

### 2. Activity Diagrams
- **Purpose:** Shows the flow of activities and decision points
- **Shows:** Steps, conditions, loops, and parallel processes
- **Best for:** Understanding business logic and workflow details

### 3. Sequence Diagrams
- **Purpose:** Shows interactions between system components over time
- **Shows:** Objects/components, message flows, activation periods, alternatives
- **Best for:** Understanding architecture and component communication

---

## Key Architecture Notes

- **Clean Architecture:** Domain → Application → Infrastructure → Presentation
- **Persistence:** JSON files (users.json, pokemon.json, item.json, story.json) via Repositories
- **PvP:** SignalR-based (NetworkService → BattleHub → PvPManager)
- **Services:** AuthenService, BattleService, CatchService, ExpService, StoryService, PvPService, HealingService, WorldGenService
- **Screens:** LoginScreen, MainMenuScreen, StoryScreen, BattleScreen, PvPScreen, InventoryScreen

---

## File Locations

```
Diagram/
├── ClassDiagram/       (13 files - class structure)
├── SequenceDiagram/    (18 files - component interactions)
├── ActivityDiagram/    (18 files - workflow logic)
├── UsecaseDiagram/     (18 files - actor interactions)
├── StateDiagram/       (1 file  - game state machine)
├── DatabaseDiagram/    (4 files - data model)
└── DIAGRAMS_INDEX.md   (this file)
```

---

## Use Case Coverage

| Phase | Use Cases | Count |
|-------|-----------|-------|
| Phase 0: Account Setup | Register (UC18), Select Starter (UC17) | 2 |
| Phase 1: Core PvE | Login (UC1), Battle Wild (UC2), Catch (UC3), Use Items (UC4), Manage Team (UC5*) | 5 |
| Phase 1.5: Battle Mechanics | Level Up (UC12), Heal Team (UC13) | 2 |
| Phase 3: Story Mode | Explore Story (UC6), NPC (UC7), Gym (UC14), League (UC15), Trainer (UC16*) | 5 |
| Phase 5: PvP Online | Matchmaking (UC8), PvP Battle (UC9) | 2 |
| Phase 6: Persistence | Save/Load (UC10), Match History (UC11*) | 2 |
| **TOTAL** | *(\* = Under Development)* | **18** |
