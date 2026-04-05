// Persistence/Repositories/ItemRepository.cs
public class ItemRepository : IItemRepository
{
    private readonly string _filePath = "item.json";
    private readonly IFileService _fileService;
    private Dictionary<string, Item> _cache = new();

    public ItemRepository(IFileService fileService)
    {
        _fileService = fileService;
        LoadData();
    }

    /// <summary>
    /// Loads item data from the file and populates the memory cache.
    /// </summary>
    private void LoadData()
    {
        var dtos = _fileService.Load<List<ItemDTO>>(_filePath) ?? new List<ItemDTO>();

        _cache.Clear();
        foreach (var dto in dtos)
        {
            // Fix: Correctly assign the mapped entity to the cache
            Item item = MapDtoToEntity(dto);
            _cache[item.Name] = item;
        }

        Console.WriteLine($"[ItemRepository] Successfully loaded {_cache.Count} items.");
    }

    private Item MapDtoToEntity(ItemDTO dto)
    {
        string typeLower = dto.Type?.ToLower() ?? "";
        ItemCategory category = typeLower switch
        {
            "healing" => ItemCategory.Healing,
            "capture" => ItemCategory.Capture,
            _         => ItemCategory.Utility
        };

        return typeLower switch
        {
            "healing" => new HealingItem 
            {
                Id = dto.Id, Name = dto.Name, Description = dto.Description,
                Price = dto.Price, Category = category, HealAmount = dto.HealAmount ?? 0
            },
            "capture" => new CaptureItem 
            {
                Id = dto.Id, Name = dto.Name, Description = dto.Description,
                Price = dto.Price, Category = category, CatchRateMultiplier = dto.CatchRateMultiplier ?? 1.0
            },
            _ => throw new NotSupportedException($"Type '{dto.Type}' is not mapped.")
        };
    }

    public Item? GetItemByName(string name) => _cache.GetValueOrDefault(name);
    public Item? GetItemById(int id) => _cache.Values.FirstOrDefault(i => i.Id == id);
    public List<Item> GetAllItems() => _cache.Values.ToList();
}