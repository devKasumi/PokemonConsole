using System.Text.Json;
using System.Text.Json.Serialization;

public class ItemRepository
{
    private readonly string _filePath = "item.json";

    // Cache to store items by their Name for fast lookup
    private Dictionary<string, Item> _cache = new Dictionary<string, Item>();

    public void LoadData()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"Warning: {_filePath} not found.");
            return;
        }

        var json = File.ReadAllText(_filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Note: If you have complex inheritance, you might need a custom JsonConverter.
        // For simple DTO mapping, we can deserialize to a list of a middle-man class (DTO).
        List<ItemDTO> dtos = JsonSerializer.Deserialize<List<ItemDTO>>(json, options) 
                            ?? new List<ItemDTO>();

        foreach (var dto in dtos)
        {
            Item item = MapDtoToEntity(dto);
            _cache[item.Name] = item;
        }

        Console.WriteLine($"Loaded {_cache.Count} items into memory.");
    }

    // Helper to convert plain JSON data into specific Class types
    private Item MapDtoToEntity(ItemDTO dto)
    {
        // Console.WriteLine($"curren _cache count: {_cache.Count}");
        string typeLower = dto.Type?.ToLower() ?? "";

        // Convert string from JSON to C# Enum
        ItemCategory category = typeLower switch
        {
            "healing" => ItemCategory.Healing,
            "capture" => ItemCategory.Capture,
            _         => ItemCategory.Utility
        };

        // Based on the string 'Type' from JSON, create the concrete class
        switch (typeLower)
        {
            case "healing":
                return new HealingItem 
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Category = category, // Assign the converted Enum here
                    HealAmount = dto.HealAmount ?? 0
                };

            case "capture":
                return new CaptureItem 
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Category = category, // Assign the converted Enum here
                    CatchRateMultiplier = dto.CatchRateMultiplier ?? 1.0
                };

            default:
                throw new NotSupportedException($"Item type '{dto.Type}' is not mapped in Repository.");
        }
    }

    public Item? GetItemByName(string name)
    {
        if (!_cache.TryGetValue(name, out var template)) return null;

        return template switch
        {
            HealingItem h => new HealingItem { 
                Id = h.Id, Name = h.Name, Description = h.Description, 
                Price = h.Price, Category = h.Category, HealAmount = h.HealAmount 
            },
            CaptureItem c => new CaptureItem { 
                Id = c.Id, Name = c.Name, Description = c.Description, 
                Price = c.Price, Category = c.Category, CatchRateMultiplier = c.CatchRateMultiplier 
            },
            _ => null
        };
    }

    public Item? GetItemById(int id)
    {
        // Search through the values to find a matching Id
        return _cache.Values.FirstOrDefault(i => i.Id == id);
    }

    // Get all items (useful for ItemSpawner)
    public List<Item> GetAllItems() => _cache.Values.ToList();
}