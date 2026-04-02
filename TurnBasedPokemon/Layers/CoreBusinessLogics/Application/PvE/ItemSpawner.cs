// File: Application/Services/ItemSpawner.cs
public class ItemSpawner
{
    private readonly ItemRepository _itemRepo;
    private readonly Random _random = new Random();

    public ItemSpawner(ItemRepository itemRepo)
    {
        _itemRepo = itemRepo;
    }

    /// <summary>
    /// Spawns a specific item by its name (useful for fixed rewards)
    /// </summary>
    public Item SpawnItem(string name)
    {
        // 1. Retrieve the item template from the repository
        Item? originalItem = _itemRepo.GetItemByName(name);
        if (originalItem == null) return null;

        // 2. Clone the item to ensure it's a unique instance
        return CloneItem(originalItem);
    }

    /// <summary>
    /// Roll for a random item encounter in the Wild Area
    /// </summary>
    public Item? RollForRandomItem()
    {
        // 30% chance to find an item
        if (_random.NextDouble() > 0.8) return null;

        // Get all available items from the cache
        List<Item> allItems = _itemRepo.GetAllItems();
        Console.WriteLine($"curren item get from json count: {allItems.Count}");
        if (allItems.Count == 0) return null;

        // Pick one randomly
        Item randomTemplate = allItems[_random.Next(allItems.Count)];

        return CloneItem(randomTemplate);
    }

    /// <summary>
    /// Creates a deep copy of the item based on its specific type
    /// </summary>
    private Item CloneItem(Item original)
    {
        // Use pattern matching to identify the concrete type
        if (original is HealingItem healing)
        {
            return new HealingItem
            {
                Id = healing.Id,
                Name = healing.Name,
                Description = healing.Description,
                Price = healing.Price,
                Category = ItemCategory.Healing,
                HealAmount = healing.HealAmount,
            };
        }
        
        if (original is CaptureItem capture)
        {
            return new CaptureItem
            {
                Id = capture.Id,
                Name = capture.Name,
                Description = capture.Description,
                Price = capture.Price,
                Category = ItemCategory.Capture,
                CatchRateMultiplier = capture.CatchRateMultiplier
            };
        }

        // If it reaches here, it means a new Item type was added to JSON 
        // but not implemented in the Spawner logic.
        throw new NotSupportedException($"Item type '{original.GetType().Name}' is not supported for cloning.");
    }
}