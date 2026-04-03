public interface IPokedexRepository
{
    void LoadData();
    PokemonSpecies? GetSpecies(string name);
    PokemonSpecies? GetSpeciesById(int id);
}