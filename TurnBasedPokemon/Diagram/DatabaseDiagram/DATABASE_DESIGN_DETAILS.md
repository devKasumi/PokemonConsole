# Pokemon Game Database Design Details

## Database: pokemondb

---

## Table: users
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | AUTO_INCREMENT |
| Username | VARCHAR(50) | UQ | NOT NULL, UNIQUE |
| Password | VARCHAR(255) | | NOT NULL |

---

## Table: players
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

---

## Table: pokemons
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

---

## Table: pokedex (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |
| Data | LONGTEXT | | JSON (BaseStats, Moveset, TypeChart) |

---

## Table: items (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |
| Type | VARCHAR(50) | | (Healing, Capture, General) |
| Details | TEXT | | JSON object |

---

## Table: game_phases (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| PhaseId | INT | PK | PRIMARY KEY |
| Name | VARCHAR(100) | | NOT NULL |

---

## Table: locations (Static Data)
| Field | Type | Key | Constraint |
|-------|------|-----|-----------|
| Id | INT | PK | PRIMARY KEY |
| PhaseId | INT | FK | FOREIGN KEY (game_phases.PhaseId) |
| Name | VARCHAR(100) | | NOT NULL |
| MinLevel | INT | | DEFAULT 1 |
| MaxLevel | INT | | DEFAULT 1 |
| WildPokemons | TEXT | | JSON array of Pokemon names |
| Narratives | TEXT | | JSON array of story text |
| NPCs | LONGTEXT | | JSON array of NPC objects |

---

## ER Diagram Relationships

### Primary Relationships:
1. **users → players**: One-to-One
   - users.Id → players.UserId (Foreign Key)
   - Cascade Delete

2. **players → pokemons**: One-to-Many
   - players.UserId → pokemons.PlayerId (Foreign Key)
   - Cascade Delete

3. **pokemons → pokedex**: Many-to-One
   - pokemons.SpeciesId → pokedex.Id (Implicit Reference)

4. **game_phases → locations**: One-to-Many
   - game_phases.PhaseId → locations.PhaseId (Foreign Key)
   - Cascade Delete

### Data Categories:

**Dynamic Data (User Progress):**
- users
- players
- pokemons

**Static Data (Game Configuration):**
- pokedex
- items
- game_phases
- locations

---

## JSON Storage Fields

### Badges (players.Badges)
```json
["Boulder Badge", "Cascade Badge", "Thunder Badge"]
```

### InventoryData (players.InventoryData)
```json
{
  "0": {
    "Healing Potion": [item1, item2],
    "Full Restore": [item3]
  },
  "1": {
    "Pokeball": [item4, item5, item6]
  }
}
```

### MovesData (pokemons.MovesData)
```json
[
  {"Name": "Tackle", "Type": "NORMAL", "Power": 40, "PP": 35},
  {"Name": "Growl", "Type": "NORMAL", "Power": 0, "PP": 40}
]
```

---

## Detailed ER Diagram

For a comprehensive view of all entities and their relationships, see:
- **[Detailed_ERD.puml](Detailed_ERD.puml)** - Complete Entity Relationship Diagram with all tables, fields, and relationships

This detailed ERD includes additional entities like:
- pokemon_species (normalized from pokedex)
- pokemon_moves
- inventory (separate table)
- game_sessions
- locations
- game_phases
- npcs
- battle_history

The diagram shows proper normalization, foreign key relationships, and cardinality.

### Data (pokedex.Data)
```json
{
  "Id": 1,
  "Name": "Bulbasaur",
  "Types": ["GRASS", "POISON"],
  "BaseStats": {"HP": 45, "Attack": 49, "Defense": 49, "SpAttack": 65, "SpDefense": 65, "Speed": 45},
  "MoveSet": [...],
  "CatchRate": 45,
  "BaseExp": 64,
  "EvolutionId": 2,
  "EvolutionLevel": 16
}
```

### NPCs (locations.NPCs)
```json
[
  {
    "Name": "Brock",
    "Role": "Gym Leader",
    "RequiredBadge": 1,
    "PokemonTeam": [
      {"Name": "Geodude", "Level": 12},
      {"Name": "Onix", "Level": 14}
    ]
  }
]
```

---

## Indexes (Performance Optimization)

Suggested indexes:
- `CREATE INDEX idx_users_username ON users(Username);`
- `CREATE INDEX idx_pokemons_playerid ON pokemons(PlayerId);`
- `CREATE INDEX idx_locations_phaseid ON locations(PhaseId);`
