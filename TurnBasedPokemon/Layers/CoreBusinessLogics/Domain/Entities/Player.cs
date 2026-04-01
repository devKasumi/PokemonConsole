public class Player
{
    public string Name { get; set; }

    // Progress Information
    public int CurrentPhase { get; set; } = 1;  // Starting phase
    public string CurrentLocation { get; set; } = "Pallet Town";  // Starting location
    public List<string> Badges { get; private set; } = new();
    
    public List<Pokemon> PokemonTeam { get; private set; } = new();
    // public List<Item> Inventory { get; private set; } = new();
    public Dictionary<int, Dictionary<string, List<Item>>> Inventory { get; private set; } = new();
    public Pokemon CurrentPokemon { get; set; }

    public Player()
    {
        foreach (ItemCategory cat in Enum.GetValues(typeof(ItemCategory)))
        {
            Inventory[(int)cat] = new Dictionary<string, List<Item>>();
        }
    }

    public void AddBadge(string badgeName)
    {
        if (!Badges.Contains(badgeName))
        {
            Badges.Add(badgeName);
        }
    }

    public void AddPokemon(Pokemon pokemon)
    {
        if (PokemonTeam.Count < 6)
        {
            PokemonTeam.Add(pokemon);
        }
        else
        {
            Console.WriteLine("Your team is full! You can't add more Pokemon.");
        }
    }

    public void AddItem(Item item)
    {
        int categoryKey = (int)item.Category;
        string itemName = item.Name;

        if (!Inventory.ContainsKey(categoryKey))
        {
            Inventory[categoryKey] = new Dictionary<string, List<Item>>();
        }

        if (!Inventory[categoryKey].ContainsKey(itemName))
        {
            Inventory[categoryKey][itemName] = new List<Item>();
        }

        Inventory[categoryKey][itemName].Add(item);
    }

    public void RemoveItem(Item item)
    {
        int categoryKey = (int)item.Category;
        string itemName = item.Name;

        // Check if the category and item name exist in our inventory
        if (Inventory.ContainsKey(categoryKey) && Inventory[categoryKey].ContainsKey(itemName))
        {
            var itemList = Inventory[categoryKey][itemName];
            
            // Remove the specific instance if it exists in the list
            if (itemList.Contains(item))
            {
                itemList.Remove(item);
            }

            // Cleanup: If the list is now empty, remove the key from inner dictionary
            if (itemList.Count == 0)
            {
                Inventory[categoryKey].Remove(itemName);
            }
        }
    }
}