# Pokemon Console Game - Project Report

## Executive Summary
This document provides a comprehensive overview of the Pokemon Console Game project, including requirements, architecture, diagrams, database design, and implementation details.

---

## Table of Contents
1. Project Overview
2. Functional Requirements
3. System Architecture
4. Use Cases
5. Class Diagram Overview
6. Database Design
7. Testing Strategy
8. Installation Instructions

---

## 1. Project Overview

### Project Name
**Pokemon Console Game - Turn-Based Adventure**

### Purpose
A console-based Pokemon adventure game providing a complete gaming experience with story progression, battle system, NPC interactions, inventory management, and progression tracking.

### Target Users
- Pokemon fans seeking a retro console gaming experience
- Players interested in turn-based tactical combat
- Developers studying layered architecture patterns

### Key Features
- **User Authentication**: Registration and login system
- **Story Mode**: Multiple phases with locations and NPCs
- **Battle System**: Turn-based combat with type advantages
- **Pokemon Management**: Team building, evolution, and stat progression
- **Inventory System**: Items (Healing, Capture), Quest items
- **Experience & Leveling**: Pokemon gain EXP and evolve
- **Pokedex**: Track encountered and caught Pokemon
- **Save/Load**: Manual game progress persistence
- **PvP Mode**: Online player vs player battles (server-based)

### Project Statistics
- **Total Use Cases**: 18
- **Database Tables**: 7
- **Core Services**: 3+ major services
- **UI Screens**: 8+ different console screens
- **Repositories**: 2+ persistence implementations (JSON/MySQL)

---

## 2. Functional Requirements

### 2.1 Authentication & Account Management
- **FR1**: Players can register a new account with username/password
- **FR2**: Players can login with valid credentials
- **FR3**: Account credentials are validated against database
- **FR4**: Player progress is linked to user account

### 2.2 Game Initialization
- **FR5**: New players must select a starter Pokemon (Bulbasaur, Charmander, Squirtle)
- **FR6**: Starter selection initializes player team and Pokedex entry
- **FR7**: Game state is initialized with Phase 1 and Pallet Town location

### 2.3 Exploration & World
- **FR8**: Player can move between game locations within phases
- **FR9**: Player can progress through multiple story phases
- **FR10**: Each location displays narrative and encounter information
- **FR11**: Wild Pokemon can be encountered during exploration (60% encounter rate)

### 2.4 Battle System (PvE)
- **FR12**: Turn-based battles with player vs wild Pokemon
- **FR13**: Turn-based battles with player vs NPC trainers
- **FR14**: Gym Leaders/Elite Four/Champion trigger mandatory battles
- **FR15**: Type advantages affect damage calculation
- **FR16**: Fainted Pokemon cannot participate unless healed

### 2.5 NPC Interactions
- **FR17**: Player can encounter NPCs at specific locations
- **FR18**: NPCs provide dialogue, quests, or battle challenges
- **FR19**: NPC type (Gym Leader, Elite Four, Champion) determines interaction flow
- **FR20**: NPC teams are generated based on level and predefined setups

### 2.6 Item Management
- **FR21**: Player can carry healing items (Potion, Full Restore)
- **FR22**: Player can carry Pokeballs for catching Pokemon
- **FR23**: Healing items restore Pokemon HP or cure status
- **FR24**: Pokeballs have catch rate multipliers affecting success

### 2.7 Pokemon Progression
- **FR25**: Pokemon gain experience from victories
- **FR26**: Pokemon level up when experience threshold is reached
- **FR27**: Pokemon can evolve when level requirements are met
- **FR28**: Base stats increase on level up
- **FR29**: Moves are learned at specific levels

### 2.8 Data Persistence
- **FR30**: Player can manually save game progress to slot
- **FR31**: Player can load saved game state
- **FR32**: Save includes: Pokemon team, inventory, badges, current location, phase
- **FR33**: JSON and MySQL persistence options are available

### 2.9 Pokedex & Collection
- **FR34**: Pokemon are added to Pokedex when encountered
- **FR35**: Pokedex tracks "Seen" and "Caught" status
- **FR36**: Player can view collected Pokemon statistics

---

## 3. System Architecture

### 3.1 Layered Architecture
```
┌─────────────────────────────────────┐
│   Presentation Layer (UI/Console)   │
│  LoginScreen, BattleScreen, etc.    │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│ Application Layer (Services)        │
│  BattleService, StoryService, etc.  │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│    Domain Layer (Entities)          │
│  Player, Pokemon, NPC, Item, etc.   │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│  Infrastructure Layer               │
│  Repositories, Database, File I/O   │
└─────────────────────────────────────┘
```

### 3.2 Key Design Patterns
- **Strategy Pattern**: Item hierarchy (HealingItem, CaptureItem, GeneralItem)
- **Repository Pattern**: Data persistence abstraction (IUserRepository)
- **Dependency Injection**: Service component coupling
- **Factory Pattern**: Pokemon and item spawning

### 3.3 Core Components

#### GameSession
- Manages current user, player, and battle state
- Handles login/logout and game flow
- Tracks battle information and enemy teams

#### BattleService
- Executes turn-based combat mechanics
- Calculates damage using type advantages
- Grants experience and handles fainting

#### StoryService
- Manages world exploration and movement
- Generates wild Pokemon encounters
- Handles NPC challenges and dialogue

#### UserRepository
- Abstracts data storage (JSON or MySQL)
- Handles user persistence
- Manages player progression data

---

## 4. Use Cases Overview

### 4.1 Use Case Classification
The project implements 18 distinct use cases:

**User Management (UC1-UC2)**
- UC1: Register Account
- UC2: Login

**Exploration & Narrative (UC3-UC7)**
- UC3: Select Starter Pokemon
- UC4: Explore World
- UC5-UC7: Interact with NPCs (varies by NPC role)

**Battle System (UC8-UC10)**
- UC8: Battle Wild Pokemon
- UC9: Battle NPC Trainer
- UC10: Special Battle (Gym Leader/Elite/Champion)

**Inventory & Items (UC11)**
- UC11: Use Items (Healing, Capture, etc.)

**Pokemon Management (UC12-UC13)**
- UC12: Level Up Pokemon
- UC13: Heal Team at Pokemon Center

**Tournament Progression (UC14-UC16)**
- UC14: Challenge Gym Leader
- UC15: Challenge Pokemon League
- UC16: Challenge Trainer

**Game Management (UC17-UC18)**
- UC17: Save Game
- UC18: Load Game

### 4.2 Example Use Case: Battle

**UC8: Battle Wild Pokemon**
- **Actor**: Player
- **Precondition**: Player is in an area with wild Pokemon
- **Main Flow**:
  1. Player selects attack / item / switch / run
  2. Damage is calculated and applied
  3. Enemy attacks in response
  4. Turn continues until victory, defeat, or run
- **Success Condition**: Player defeats Pokemon, gains experience and items
- **Alternative Courses**:
  - Player Pokemon is defeated → GameOver state
  - Player uses Pokeball → Pokemon caught (added to team if space allows)
  - Player runs → Battle ends, return to exploration

---

## 5. Class Diagram Overview

### 5.1 Core Entity Classes

#### User
```
User
├── Username: string
├── Password: string
└── PlayerData: Player?
```

#### Player
```
Player
├── Name: string
├── CurrentPhase: int
├── CurrentLocation: string
├── Badges: List<string>
├── PokemonTeam: List<Pokemon>
├── Inventory: Dictionary<int, Dictionary<string, List<Item>>>
├── CurrentPokemon: Pokemon
├── IsChampion: bool
├── PvPWins: int
└── PvPLosses: int
```

#### Pokemon
```
Pokemon
├── Specie: PokemonSpecies
├── Level: int
├── CurrentHP: int
├── MaxHP: int
├── Status: PokemonStatus
├── Moves: List<PokemonMove>
├── TotalExp: int
├── IsFainted: bool
├── GainExperience(amount: int)
├── LevelUp()
├── TakeDamage(damage: int)
└── Heal(hpAmount: int)
```

#### PokemonSpecies
```
PokemonSpecies
├── Id: int
├── Name: string
├── Types: List<ElementType>
├── BaseStats: Stats
├── MoveSet: List<PokemonMove>
├── CatchRate: int
├── BaseExp: int
├── EvolutionId: int?
└── EvolutionLevel: int?
```

#### Item Hierarchy
```
Item (abstract)
├── HealingItem
│   └── HealAmount: int
├── CaptureItem
│   └── CatchRateMultiplier: double
└── GeneralItem
    └── SoldRate: double
```

#### Trainer/NPC
```
Trainer (abstract)
├── Name: string
├── Team: List<Pokemon>
└── IsDefeated: bool

NPC : Trainer
├── Role: NPCRole (GymLeader, EliteFour, Champion, etc.)
├── ChallengeQuote: string
├── DefeatQuote: string
└── RequiredBadge: int
```

### 5.2 Service Classes

#### BattleService
- ExecutePlayerTurn(moveIndex)
- ExecuteEnemyTurn()
- ProcessVictory()
- CanSwitchPokemon(target)

#### StoryService
- InitializeStory(command)
- Move(forward)
- ExploreWildArea()
- TryChallengeGym()
- HealTeamAtCenter()

#### GameSession
- Login(user, pass)
- StartNewGame()
- LoadProgress(username)
- SaveProgress()
- SetupBattle(enemies, isTrainer)

---

## 6. Database Design

### 6.1 Database Architecture
- **Database Name**: pokemondb
- **Total Tables**: 7
- **Total Relationships**: 4 foreign keys

### 6.2 Table Structure

#### Table: users
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | AUTO_INCREMENT |
| Username | VARCHAR(50) | UQ | NOT NULL, UNIQUE |
| Password | VARCHAR(255) | | NOT NULL |

#### Table: players
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| UserId | INT | PK, FK | PRIMARY KEY, FOREIGN KEY (users.Id) |
| Name | VARCHAR(100) | | |
| CurrentPhase | INT | | DEFAULT 1 |
| CurrentLocation | VARCHAR(100) | | |
| Badges | TEXT | | JSON array |
| InventoryData | LONGTEXT | | JSON object |
| IsChampion | TINYINT(1) | | DEFAULT 0 |
| PvPWins | INT | | DEFAULT 0 |
| PvPLosses | INT | | DEFAULT 0 |

#### Table: pokemons
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | AUTO_INCREMENT |
| PlayerId | INT | FK | FOREIGN KEY (players.UserId) |
| SpeciesId | INT | | References pokedex(Id) |
| Level | INT | | NOT NULL |
| TotalExp | INT | | DEFAULT 0 |
| CurrentExp | INT | | DEFAULT 0 |
| MaxExpForNextLevel | INT | | DEFAULT 0 |
| CurrentHP | INT | | NOT NULL |
| MaxHP | INT | | NOT NULL |
| IsFainted | TINYINT(1) | | DEFAULT 0 (0=False, 1=True) |
| Status | INT | | DEFAULT 0 (0=None, 1=Asleep, etc.) |
| MovesData | TEXT | | JSON array of moves |

#### Table: pokedex (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |
| Data | LONGTEXT | | JSON (BaseStats, Moveset, TypeChart) |

#### Table: items (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |
| Type | VARCHAR(50) | | (Healing, Capture, General) |
| Details | TEXT | | JSON object |

#### Table: game_phases (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| PhaseId | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |

#### Table: locations (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| PhaseId | INT | FK | FOREIGN KEY (game_phases.PhaseId) |
| Name | VARCHAR(100) | | NOT NULL |
| MinLevel | INT | | DEFAULT 1 |
| MaxLevel | INT | | DEFAULT 1 |
| WildPokemons | TEXT | | JSON array |
| Narratives | TEXT | | JSON array |
| NPCs | LONGTEXT | | JSON array |

### 6.3 Data Relationships
```
users (1) ──────→ (1) players
      └─→ CASCADE DELETE

players (1) ──────→ (*) pokemons
        └─→ CASCADE DELETE

pokemons (*) ────→ (1) pokedex

game_phases (1) ─→ (*) locations
          └─→ CASCADE DELETE
```

### 6.4 JSON Data Examples

**Badges (JSON Array)**
```json
["Boulder Badge", "Cascade Badge", "Thunder Badge", "Rainbow Badge"]
```

**InventoryData (Nested Dictionary)**
```json
{
  "0": {
    "Potion": [{"Id": 1, "Name": "Potion", "Category": 0}],
    "Full Restore": [{"Id": 3, "Name": "Full Restore", "Category": 0}]
  },
  "1": {
    "Pokeball": [{"Id": 4, "Name": "Pokeball", "Category": 1}, ...],
    "Great Ball": [...]
  }
}
```

**MovesData (JSON Array)**
```json
[
  {"Name": "Tackle", "Type": "NORMAL", "Power": 40, "PP": 35, "MoveType": 0},
  {"Name": "Growl", "Type": "NORMAL", "Power": 0, "PP": 40, "MoveType": 1}
]
```

---

## 7. Testing Strategy

### 7.1 Unit Tests
- **Authentication**: Valid/invalid credentials, account creation
- **Battle Mechanics**: Damage calculation, type advantages, status effects
- **Experience System**: Level up thresholds, stat growth
- **Item Usage**: Healing amount, catch rate modifiers
- **Pokemon State**: HP management, fainting conditions

### 7.2 Integration Tests
- **Battle Flow**: Player turn → Enemy turn → Victory/Defeat
- **NPC Encounters**: Gym Leader battles, dialogue flow
- **Save/Load**: Persistence and state restoration
- **Evolution Chain**: Level-up triggers and stat updates

### 7.3 System Tests
- **Story Progression**: Phase transitions, narrative consistency
- **Inventory Management**: Item pickup and usage
- **Team Management**: Pokemon switching, party composition
- **Database Integrity**: Referential constraints, cascade operations

### 7.4 Test Cases Summary
- ~50+ unit test cases
- ~20+ integration test cases
- ~10+ system test cases

---

## 8. Installation Instructions

### 8.1 Prerequisites
- **.NET SDK**: Version 9.0+
- **MySQL Server**: Version 5.7+ or 8.0+
- **IDE**: Visual Studio 2022 or VS Code
- **Git**: For cloning repository (optional)

### 8.2 Setup Steps

**Step 1: Clone Repository**
```bash
git clone https://github.com/your-repo/PokemonConsole.git
cd PokemonConsole/TurnBasedPokemon
```

**Step 2: Create Database**
```bash
mysql -u root -p < Layers/Infrastructure/Persistence/Database/Scripts/PokemonDB.sql
```

**Step 3: Configure Connection String**
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=pokemondb;User=root;Password=your_password;"
  }
}
```

**Step 4: Build Project**
```bash
dotnet restore
dotnet build
```

**Step 5: Run Application**
```bash
dotnet run
```

### 8.3 Project Structure
```
TurnBasedPokemon/
├── Layers/
│   ├── CoreBusinessLogics/          # Application & Domain logic
│   ├── Infrastructure/              # Data access & persistence
│   └── Presentation/                # Console UI
├── Pokemon.Shared/                  # Shared classes
├── PokemonServer/                   # Server for PvP
├── Diagram/                         # All diagrams
├── Program.cs                       # Entry point
└── TurnBasedPokemon.csproj         # Project file
```

---

## 9. Deployment & Maintenance

### 9.1 Deployment Options
- **Console Application**: Run locally on Windows/Linux/Mac
- **Server Architecture**: Host PokemonServer for multiplayer
- **Database**: Deploy MySQL server to production database

### 9.2 Performance Considerations
- Database indexes on frequently queried fields
- JSON serialization optimization
- Pokemon spawning performance tuned for encounters

### 9.3 Future Enhancements
- Multiplayer PvP implementation
- Mobile app version
- Cloud database migration
- Additional Pokemon generations
- Expanded storyline and regions

---

## 10. Conclusion

The Pokemon Console Game project represents a comprehensive demonstration of:
- Layered software architecture
- Database design with normalization
- Object-oriented design patterns
- Game logic and turn-based mechanics
- Data persistence strategies

This project serves as both a functional game and an educational reference for game development architecture patterns.

---

## Appendix: Document References

**Generated Diagrams:**
- `Diagram/ClassDiagram/ProjectClassDiagram.puml` - Class diagram
- `Diagram/DatabaseDiagram/PokemonDB_TableDesign.puml` - Database ER diagram
- Various Use Case, Activity, and Sequence diagrams for each use case

**Supporting Documents:**
- `REQUIREMENTS.md` - Detailed requirements
- `DATABASE_DESIGN_DETAILS.md` - Database schema details
- `PokemonDB.sql` - SQL script for database creation
