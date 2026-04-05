// Persistence/Repositories/PokedexRepository.cs
public class PokedexRepository : IPokedexRepository
{
    private readonly string _filePath = "pokemon.json";
    private readonly IFileService _fileService;
    private Dictionary<string, PokemonSpecies> _cache = new();

    public PokedexRepository(IFileService fileService)
    {
        _fileService = fileService;
        LoadData();
    }

    /// <summary>
    /// Loads static Pokemon species data into memory.
    /// </summary>
    private void LoadData()
    {
        var speciesList = _fileService.Load<List<PokemonSpecies>>(_filePath) ?? new();

        _cache.Clear();
        foreach (var species in speciesList)
        {
            _cache[species.Name] = species;
        }

        Console.WriteLine($"[PokedexRepository] Successfully loaded {_cache.Count} species.");
    }

    public PokemonSpecies? GetSpecies(string name) => _cache.GetValueOrDefault(name);
    public PokemonSpecies? GetSpeciesById(int id) => _cache.Values.FirstOrDefault(s => s.Id == id);
}