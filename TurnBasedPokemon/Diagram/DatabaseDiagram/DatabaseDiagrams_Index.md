# Pokemon Game Database Diagrams Index

**Created:** April 7, 2026  
**Last Updated:** June 2025  

> **Note:** The current implementation uses JSON file-based persistence (users.json, pokemon.json, item.json, story.json) via `UserRepository`, `PokedexRepository`, `ItemRepository`, and `FileService`. These diagrams describe the **logical data model** and may serve as reference for a future database migration.

---

## 📋 Database Diagram Files

### Original Design
- **[PokemonDB_TableDesign.puml](PokemonDB_TableDesign.puml)**  
  Basic ERD with core tables: users, players, pokemons, pokedex, items, game_phases, locations

### Detailed Design  
- **[Detailed_ERD.puml](Detailed_ERD.puml)**  
  Comprehensive ERD with normalized tables, proper relationships, and additional entities like pokemon_species, pokemon_moves, inventory, game_sessions, battle_history, etc.

### Design Documentation
- **[DATABASE_DESIGN_DETAILS.md](DATABASE_DESIGN_DETAILS.md)**  
  Detailed table schemas, field descriptions, relationships, and JSON structure examples

---

## 🗄️ Database Overview

The Pokemon game database is designed with proper normalization and includes:

- **User Management**: Authentication and player profiles
- **Game Progress**: Pokemon ownership, inventory, locations, story phases
- **Static Data**: Pokemon species, moves, items, NPCs
- **Battle Records**: PvP history and statistics
- **Session Management**: Current game state persistence

All diagrams use PlantUML ERD notation for clear visualization of table structures and relationships.