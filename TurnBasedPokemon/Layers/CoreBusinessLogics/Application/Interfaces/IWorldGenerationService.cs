public interface IWorldGenerationService
{
    public Pokemon SpawnPokemonByName(string name, int level);
    public Pokemon SpawnPokemonById(int evolutionId, int currentLevel);
    public Item SpawnItemByName(string name);
}