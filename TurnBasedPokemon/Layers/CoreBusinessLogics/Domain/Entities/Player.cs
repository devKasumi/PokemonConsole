public class Player
{
    public string Name { get; set; }

    // Progress Information
    public int CurrentPhase { get; set; } = 1;  // Starting phase
    public string CurrentLocation { get; set; } = "Pallet Town";  // Starting location
    public List<string> Badges { get; private set; } = new();
    
    public List<Pokemon> PokemonTeam { get; private set; } = new();
    public List<Item> Inventory { get; private set; } = new();
    public Pokemon CurrentPokemon => PokemonTeam.FirstOrDefault();

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

    public void AddItem(Item item) => Inventory.Add(item);

    public void RemoveItem(Item item) => Inventory.Remove(item);

    public IEnumerable<(Item Item, int Count)> GetGroupedItems()
    {
        return Inventory.GroupBy(i => i.Name)
                        .Select(g => (Item: g.First(), Count: g.Count()));
    }
}