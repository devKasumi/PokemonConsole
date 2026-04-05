using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

public class MySqlPokedexRepository : IPokedexRepository
{
    private readonly string _connectionString;
    private Dictionary<string, PokemonSpecies> _cache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public MySqlPokedexRepository(string connection)
    {
        _connectionString = connection;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } // Helps with ElementType/MoveType Enums
        };
        LoadData();
    }

    /// <summary>
    /// Loads static Pokemon species data into memory from the database.
    /// </summary>
    private void LoadData()
    {
        try
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            // The entire PokemonSpecies object is stored inside the 'Data' column
            string sql = "SELECT Data FROM pokedex";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            _cache.Clear();
            while (reader.Read())
            {
                string json = reader.GetString("Data");
                
                // Magically map the entire JSON blob back into the C# object
                var species = JsonSerializer.Deserialize<PokemonSpecies>(json, _jsonOptions);

                if (species != null && !string.IsNullOrEmpty(species.Name))
                {
                    _cache[species.Name] = species;
                }
            }
            Console.WriteLine($"[MySqlPokedexRepository] Successfully loaded {_cache.Count} species from DB.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Database Error in PokedexRepository: {ex.Message}");
        }
    }

    public PokemonSpecies? GetSpecies(string name) => _cache.GetValueOrDefault(name);
    public PokemonSpecies? GetSpeciesById(int id) => _cache.Values.FirstOrDefault(s => s.Id == id);
}