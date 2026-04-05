using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

public class MySqlItemRepository : IItemRepository
{
    private readonly string _connection;
    private Dictionary<string, Item> _cache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public MySqlItemRepository(string connection)
    {
        _connection = connection;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } // Crucial for parsing ItemCategory Enum
        };
        LoadData(); 
    }

    /// <summary>
    /// Loads item data from the database and populates the memory cache.
    /// </summary>
    private void LoadData()
    {
        try
        {
            using var conn = new MySqlConnection(_connection);
            conn.Open();
            // We only need Type to determine the class, and Details to map the properties
            string sql = "SELECT Type, Details FROM items";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            _cache.Clear();
            while (reader.Read())
            {
                string type = reader.GetString("Type").ToLower();
                string detailsJson = reader.GetString("Details");

                // Deserialize directly into the specific child class based on the Type column
                Item item = type switch
                {
                    "healing" => JsonSerializer.Deserialize<HealingItem>(detailsJson, _jsonOptions),
                    "capture" => JsonSerializer.Deserialize<CaptureItem>(detailsJson, _jsonOptions),
                    _ => throw new NotSupportedException($"Item type '{type}' is not supported.")
                };

                if (item != null && !string.IsNullOrEmpty(item.Name))
                {
                    _cache[item.Name] = item;
                }
            }
            Console.WriteLine($"[MySqlItemRepository] Successfully loaded {_cache.Count} items from DB.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Database Error in ItemRepository: {ex.Message}");
        }
    }

    public Item? GetItemByName(string name) => _cache.GetValueOrDefault(name);
    public Item? GetItemById(int id) => _cache.Values.FirstOrDefault(i => i.Id == id);
    public List<Item> GetAllItems() => _cache.Values.ToList();
}