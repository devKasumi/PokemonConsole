-- Drop the database if it already exists to start fresh
-- DROP DATABASE IF EXISTS pokemondb;
-- CREATE DATABASE pokemondb;
-- USE pokemondb;

-- =========================================
-- PART 1: STATIC DATA (Items, Pokedex, Story)
-- =========================================

-- Table for storing Items (Healing, Capture, etc.)
-- CREATE TABLE items (
--     Id INT PRIMARY KEY,
--     Name VARCHAR(100) NOT NULL,
--     Type VARCHAR(50) NOT NULL, -- E.g., 'Healing', 'Capture'
--     Details TEXT -- Stores the entire Item object as a JSON string
-- );

-- -- Table for storing static Pokemon species data
-- CREATE TABLE pokedex (
--     Id INT PRIMARY KEY,
--     Name VARCHAR(100) NOT NULL,
--     Data LONGTEXT -- Stores the entire PokemonSpecies object (BaseStats, Moveset) as JSON
-- );

-- -- Table for storing main story phases
-- CREATE TABLE game_phases (
--     PhaseId INT PRIMARY KEY,
--     Name VARCHAR(100) NOT NULL
-- );

-- -- Table for storing specific areas within a phase
-- CREATE TABLE locations (
--     Id INT PRIMARY KEY,
--     PhaseId INT NOT NULL,
--     Name VARCHAR(100) NOT NULL,
--     MinLevel INT DEFAULT 1,
--     MaxLevel INT DEFAULT 1,
--     WildPokemons TEXT, -- JSON array of wild Pokemon names
--     Narratives TEXT,   -- JSON array of story narratives
--     NPCs LONGTEXT,     -- JSON array of NPC objects (including their Pokemon teams)
--     FOREIGN KEY (PhaseId) REFERENCES game_phases(PhaseId) ON DELETE CASCADE
-- );


-- -- =========================================
-- -- PART 2: DYNAMIC DATA (User Progress & Saves)
-- -- =========================================

-- -- Table for storing base user accounts
-- CREATE TABLE users (
--     Id INT PRIMARY KEY AUTO_INCREMENT,
--     Username VARCHAR(50) UNIQUE NOT NULL,
--     Password VARCHAR(255) NOT NULL
-- );

-- -- Table for storing player character progress
-- CREATE TABLE players (
--     UserId INT PRIMARY KEY,
--     Name VARCHAR(100),
--     CurrentPhase INT DEFAULT 1,
--     CurrentLocation VARCHAR(100),
--     Badges TEXT,         -- JSON array of collected badge names
--     InventoryData LONGTEXT, -- JSON string of the complex nested inventory dictionary
--     FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
-- );

-- -- Table for storing individual Pokemon owned by the player
-- CREATE TABLE pokemons (
--     Id INT PRIMARY KEY AUTO_INCREMENT,
--     PlayerId INT NOT NULL,
--     SpeciesId INT NOT NULL,
--     Level INT NOT NULL,
--     TotalExp INT DEFAULT 0,
--     CurrentExp INT DEFAULT 0,
--     MaxExpForNextLevel INT DEFAULT 0,
--     CurrentHP INT NOT NULL,
--     MaxHP INT NOT NULL,
--     IsFainted TINYINT(1) DEFAULT 0, -- 0 for False, 1 for True
--     Status INT DEFAULT 0, -- Enum integer representation (0 = None, 1 = Asleep, etc.)
--     MovesData TEXT,       -- JSON array of current learned moves
--     FOREIGN KEY (PlayerId) REFERENCES players(UserId) ON DELETE CASCADE
-- );

-- ALTER TABLE players 
-- ADD COLUMN IsChampion TINYINT(1) DEFAULT 0,
-- ADD COLUMN PvPWins INT DEFAULT 0,
-- ADD COLUMN PvPLosses INT DEFAULT 0;

USE pokemondb;

-- ALTER TABLE users ADD IsChampion TINYINT(1) DEFAULT 0;
-- ALTER TABLE users ADD PvPWins INT DEFAULT 0;
-- ALTER TABLE users ADD PvPLosses INT DEFAULT 0;

-- DESCRIBE users;

SELECT * FROM users;
SELECT * FROM players;
SELECT * FROM pokemons;
SELECT * FROM items;
SELECT * FROM pokedex;
SELECT * FROM locations;




