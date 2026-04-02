public class ExpService : IExpService
{
    private readonly GameSession _gameSession;
    private readonly IWorldGenerationService _worldGenService;

    public ExpService(GameSession session, IWorldGenerationService worldGenerationService)
    {
        _gameSession = session;
        _worldGenService = worldGenerationService;
    }

    public ProcessExpResult GrantExperience(Pokemon pokemon, int amount)
    {   
        int oldLevel = pokemon.Level;
        string? evolutionName = null;

        pokemon.GainExperience(amount);

        bool leveledUp = pokemon.Level > oldLevel;

        if (pokemon.CanEvolve() && pokemon.Specie.EvolutionId.HasValue)
        {
            var evolvedForm = _worldGenService.SpawnPokemonById(pokemon.Specie.EvolutionId.Value, pokemon.Level);
            if (evolvedForm != null)
            {
                evolutionName = evolvedForm.Specie.Name;
                pokemon.Evolve(evolvedForm);
            }
        }

        return new ProcessExpResult(amount, leveledUp, evolutionName);
    }

    public int CalculateExpGain(Pokemon playerPoke, Pokemon enemyPoke)
    {
        return ExpCalculator.CalculateExpGain(_gameSession.IsTrainerBattle,
                                            enemyPoke.Specie.BaseExp,
                                            enemyPoke.Level,
                                            playerPoke.Level);
    }
}