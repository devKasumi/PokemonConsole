using System.Text.Json;
using System.Text.Json.Serialization;

public class ItemRepository
{
    private readonly string _filePath = "items.json";

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
        switch (dto.Type.ToLower())
        {
            case "healing":
                return new HealingItem {
                    ItemId = dto.ItemId,
                    Name = dto.Name,
                    Description = dto.Description,
                    HealAmount = dto.HealAmount ?? 0,
                    Price = dto.Price
                };
            case "capture":
                return new CaptureItem {
                    ItemId = dto.ItemId,
                    Name = dto.Name,
                    Description = dto.Description,
                    CatchRateMultiplier = dto.CatchRateMultiplier ?? 1.0,
                    Price = dto.Price
                };
            default:
                throw new Exception($"Unknown item type found in JSON: {dto.Type}");
        }
    }

    public Item? GetItemByName(string name)
    {
        return _cache.TryGetValue(name, out var item) ? item : null;
    }

    public Item? GetItemById(string id)
    {
        // Search through the values to find a matching Id
        return _cache.Values.FirstOrDefault(i => i.ItemId == id);
    }

    // Get all items (useful for ItemSpawner)
    public List<Item> GetAllItems() => _cache.Values.ToList();
}