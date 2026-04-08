# Pokemon Game Database Diagrams Index

**Created:** April 7, 2026  
**Last Updated:** April 8, 2026  

> **Note:** The current implementation uses JSON file-based persistence (users.json, pokemon.json, item.json, story.json) via repository interfaces (`IUserRepository`, `IPokedexRepository`, `IItemRepository`, `IStoryRepository`). MySQL-backed implementations also exist under Infrastructure. These diagrams describe the **logical data model** used by both persistence strategies.

---

## 📋 Database Diagram Files

### Original Design
- **[PokemonDB_TableDesign.puml](PokemonDB_TableDesign.puml)**  
  Basic ERD with core tables: users, players, pokemons, pokedex, items, game_phases, locations

### Detailed Design  
- **[Detailed_ERD.puml](Detailed_ERD.puml)**  
  Comprehensive ERD matching the actual MySQL schema: users, players, pokemons, pokedex, items, game_phases, locations. Includes JSON field documentation and relationship annotations.

### Design Documentation
- **[DATABASE_DESIGN_DETAILS.md](DATABASE_DESIGN_DETAILS.md)**  
  Detailed table schemas, field descriptions, relationships, and JSON structure examples

---

## 🗄️ Database Overview

The Pokemon game database is designed with proper normalization and includes:

- **User Management**: Authentication (users) and player profiles (players)
- **Game Progress**: Pokemon ownership (pokemons), inventory (JSON in players), PvP stats (PvPWins/PvPLosses)
- **Static Data**: Pokemon species (pokedex), items, game phases, locations with NPCs (JSON)
- **Persistence**: Dual strategy — JSON files + MySQL via repository interfaces