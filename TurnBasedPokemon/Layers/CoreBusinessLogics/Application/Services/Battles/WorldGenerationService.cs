using PokemonEntity;

public class WorldGenerationService : IWorldGenerationService
{
    private readonly GameSession _gameSession;
    private readonly PokemonSpawner _pokemonSpawner;
    private readonly ItemSpawner _itemSpawner;

    public WorldGenerationService(GameSession session,
                                PokemonSpawner pokeSpawner,
                                ItemSpawner itemSpawner)
    {
        _gameSession = session;
        _pokemonSpawner = pokeSpawner;
        _itemSpawner = itemSpawner;
    }

    public Pokemon SpawnPokemonByName(string name, int level)
    {
        return _pokemonSpawner.SpawnPokemon(name, level);
    }

    public Pokemon SpawnPokemonById(int evolutionId, int currentLevel)
    {
        return _pokemonSpawner.SpawnPokemonById(evolutionId, currentLevel);
    }

    public Item SpawnItemByName(string name)
    {
        return _itemSpawner.SpawnItem(name);
    }
}