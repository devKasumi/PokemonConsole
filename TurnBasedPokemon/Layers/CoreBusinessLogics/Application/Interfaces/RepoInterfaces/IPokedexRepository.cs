namespace Application.RepoInterfaces;

public interface IPokedexRepository
{
    PokemonSpecies? GetSpecies(string name);
    PokemonSpecies? GetSpeciesById(int id);
}