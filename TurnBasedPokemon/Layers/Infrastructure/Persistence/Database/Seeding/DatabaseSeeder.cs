using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

public class DatabaseSeeder
{
    private readonly string _connection;
    private readonly MySqlUserRepository _userRepo;
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() } // This tells the parser to read "None" and map it to PokemonStatus.None
    };

    public DatabaseSeeder(MySqlUserRepository userRepo, string connection) 
    { 
        _connection = connection;
        _userRepo = userRepo; 
    }

    /// <summary>
    /// Executes the full database seeding process from the local JSON files.
    /// </summary>
    public void SeedAll()
    {
        Console.WriteLine("--- STARTING DATABASE SEEDING ---");
        SeedItems();
        SeedPokedex();
        SeedStory();
        SeedUsers();
        Console.WriteLine("--- SEEDING COMPLETE ---");
    }

    private void SeedItems()
    {
        if (!File.Exists("item.json")) return;
        var items = JsonSerializer.Deserialize<List<dynamic>>(File.ReadAllText("item.json"), _options);
        using var conn = new MySqlConnection(_connection); 
        conn.Open();
        
        foreach (var item in items) {
            string sql = "INSERT INTO items VALUES (@id, @name, @type, @details) ON DUPLICATE KEY UPDATE Details=@details";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", item.GetProperty("Id").GetInt32());
            cmd.Parameters.AddWithValue("@name", item.GetProperty("Name").GetString());
            cmd.Parameters.AddWithValue("@type", item.GetProperty("Type").GetString());
            cmd.Parameters.AddWithValue("@details", JsonSerializer.Serialize(item));
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("[v] Successfully seeded items from item.json");
    }

    private void SeedPokedex()
    {
        if (!File.Exists("pokemon.json")) return;
        string text = File.ReadAllText("pokemon.json").Trim();
        var list = new List<dynamic>();

        // Handle both single object {} and array [] formats flexibly
        if (text.StartsWith("[")) list = JsonSerializer.Deserialize<List<dynamic>>(text, _options);
        else list.Add(JsonSerializer.Deserialize<dynamic>(text, _options));

        using var conn = new MySqlConnection(_connection); 
        conn.Open();
        
        foreach (var pkm in list) {
            string sql = "INSERT INTO pokedex VALUES (@id, @name, @data) ON DUPLICATE KEY UPDATE Data=@data";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", pkm.GetProperty("Id").GetInt32());
            cmd.Parameters.AddWithValue("@name", pkm.GetProperty("Name").GetString());
            cmd.Parameters.AddWithValue("@data", JsonSerializer.Serialize(pkm));
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("[v] Successfully seeded pokedex from pokemon.json");
    }

    private void SeedStory()
    {
        if (!File.Exists("story.json")) return;
        var gameData = JsonSerializer.Deserialize<GameData>(File.ReadAllText("story.json"), _options);
        using var conn = new MySqlConnection(_connection); 
        conn.Open();

        foreach (var phase in gameData.Phases) {
            new MySqlCommand($"INSERT INTO game_phases VALUES ({phase.PhaseId}, '{phase.Name}') ON DUPLICATE KEY UPDATE Name='{phase.Name}'", conn).ExecuteNonQuery();
            
            foreach (var loc in phase.Locations) {
                string sql = "INSERT INTO locations VALUES (@id, @pid, @n, @min, @max, @wild, @narr, @npcs) ON DUPLICATE KEY UPDATE NPCs=@npcs";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", loc.Id); 
                cmd.Parameters.AddWithValue("@pid", phase.PhaseId);
                cmd.Parameters.AddWithValue("@n", loc.Name); 
                cmd.Parameters.AddWithValue("@min", loc.MinLevel); 
                cmd.Parameters.AddWithValue("@max", loc.MaxLevel);
                cmd.Parameters.AddWithValue("@wild", JsonSerializer.Serialize(loc.WildPokemons));
                cmd.Parameters.AddWithValue("@narr", JsonSerializer.Serialize(loc.Narratives));
                cmd.Parameters.AddWithValue("@npcs", JsonSerializer.Serialize(loc.NPCs));
                cmd.ExecuteNonQuery();
            }
        }
        Console.WriteLine("[v] Successfully seeded locations and NPCs from story.json");
    }

    private void SeedUsers()
    {
        if (!File.Exists("users.json")) return;
        
        // Utilize the SqlUserRepository to properly parse and distribute the data across 3 tables
        var users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("users.json"), _options);
        foreach (var user in users) {
            _userRepo.Save(user); 
        }
        Console.WriteLine("[v] Successfully seeded accounts and progress from users.json");
    }
}