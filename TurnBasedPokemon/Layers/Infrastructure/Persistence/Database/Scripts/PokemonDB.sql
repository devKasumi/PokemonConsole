-- =====================================================================
-- Pokemon: Eclipse of Legends - MySQL schema
-- Creates the pokemondb database and every table the game uses.
-- Safe to run more than once (everything uses IF NOT EXISTS).
-- Next step: fill the tables from the JSON files with
--     dotnet run -- --seed
-- =====================================================================

CREATE DATABASE IF NOT EXISTS pokemondb;
USE pokemondb;

-- =========================================
-- PART 1: STATIC DATA (Items, Pokedex, Story)
-- =========================================

-- Table for storing Items (Healing, Capture, etc.)
CREATE TABLE IF NOT EXISTS items (
    Id INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Type VARCHAR(50) NOT NULL, -- E.g., 'Healing', 'Capture'
    Details TEXT -- Stores the entire Item object as a JSON string
);

-- Table for storing static Pokemon species data
CREATE TABLE IF NOT EXISTS pokedex (
    Id INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Data LONGTEXT -- Stores the entire PokemonSpecies object (BaseStats, Moveset) as JSON
);

-- Table for storing main story phases
CREATE TABLE IF NOT EXISTS game_phases (
    PhaseId INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL
);

-- Table for storing specific areas within a phase
CREATE TABLE IF NOT EXISTS locations (
    Id INT PRIMARY KEY,
    PhaseId INT NOT NULL,
    Name VARCHAR(100) NOT NULL,
    MinLevel INT DEFAULT 1,
    MaxLevel INT DEFAULT 1,
    WildPokemons TEXT, -- JSON array of wild Pokemon names
    Narratives TEXT,   -- JSON array of story narratives
    NPCs LONGTEXT,     -- JSON array of NPC objects (including their Pokemon teams)
    FOREIGN KEY (PhaseId) REFERENCES game_phases(PhaseId) ON DELETE CASCADE
);


-- =========================================
-- PART 2: DYNAMIC DATA (User Progress & Saves)
-- =========================================

-- Table for storing base user accounts
CREATE TABLE IF NOT EXISTS users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL -- BCrypt hash
);

-- Table for storing player character progress
CREATE TABLE IF NOT EXISTS players (
    UserId INT PRIMARY KEY,
    Name VARCHAR(100),
    CurrentPhase INT DEFAULT 1,
    CurrentLocation VARCHAR(100),
    Badges TEXT,            -- JSON array of collected badge names
    InventoryData LONGTEXT, -- JSON string of the complex nested inventory dictionary
    IsChampion TINYINT(1) DEFAULT 0, -- Set after beating the Pokemon League, unlocks PvP
    PvPWins INT DEFAULT 0,
    PvPLosses INT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
);

-- Table for storing individual Pokemon owned by the player
CREATE TABLE IF NOT EXISTS pokemons (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PlayerId INT NOT NULL,
    SpeciesId INT NOT NULL,
    Level INT NOT NULL,
    TotalExp INT DEFAULT 0,
    CurrentExp INT DEFAULT 0,
    MaxExpForNextLevel INT DEFAULT 0,
    CurrentHP INT NOT NULL,
    MaxHP INT NOT NULL,
    IsFainted TINYINT(1) DEFAULT 0, -- 0 for False, 1 for True
    Status INT DEFAULT 0, -- Enum integer representation (0 = None, 1 = Asleep, etc.)
    MovesData TEXT,       -- JSON array of current learned moves
    FOREIGN KEY (PlayerId) REFERENCES players(UserId) ON DELETE CASCADE
);


-- =========================================
-- Handy queries for checking the data
-- =========================================
-- SELECT * FROM users;
-- SELECT * FROM players;
-- SELECT * FROM pokemons;
-- SELECT * FROM items;
-- SELECT * FROM pokedex;
-- SELECT * FROM locations;
