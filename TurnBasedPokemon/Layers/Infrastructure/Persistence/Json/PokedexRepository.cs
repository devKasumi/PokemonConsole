using System.Text.Json;
using System.Text.Json.Serialization;
public class PokedexRepository

{
    private readonly string _filePath = "pokemon.json";

    private Dictionary<string, PokemonSpecies> _cache =
        new Dictionary<string, PokemonSpecies>();

    public void LoadData()
    {
        var json = File.ReadAllText(_filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        List<PokemonSpecies> speciesList =
            JsonSerializer.Deserialize<List<PokemonSpecies>>(json, options)
            ?? new List<PokemonSpecies>();

        foreach (var s in speciesList)
        {
            // Console.WriteLine($"{s.Name} : {s.Types[0]}\n");
            _cache[s.Name] = s;
        }

        Console.WriteLine($"Loaded {_cache.Count} Pokemon species.");
    }

    public PokemonSpecies? GetSpecies(string name)
    {
        return _cache.ContainsKey(name)
            ? _cache[name]
            : null;
    }

    public PokemonSpecies? GetSpeciesById(int id)
    {
        // Search through the values of the dictionary to find a matching Id
        return _cache.Values.FirstOrDefault(s => s.Id == id);
    }
}