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
        string oldName = pokemon.Specie.Name;
        string? evolutionName = null;
        List<string> newlyLearnedMoves = new List<string>();

        pokemon.GainExperience(amount);

        bool leveledUp = pokemon.Level > oldLevel;

        if (leveledUp)
        {
            newlyLearnedMoves = pokemon.LearnNewMoves();

            if (pokemon.CanEvolve() && pokemon.Specie.EvolutionId.HasValue)
            {
                var evolvedForm = _worldGenService.SpawnPokemonById(pokemon.Specie.EvolutionId.Value, pokemon.Level);
                if (evolvedForm != null)
                {
                    evolutionName = evolvedForm.Specie.Name;
                    pokemon.Evolve(evolvedForm);
                }
            }
        }

        return new ProcessExpResult(amount, leveledUp, oldName, evolutionName, newlyLearnedMoves);
    }

    public int CalculateExpGain(Pokemon playerPoke, Pokemon enemyPoke)
    {
        return ExpCalculator.CalculateExpGain(_gameSession.IsTrainerBattle,
                                            enemyPoke.Specie.BaseExp,
                                            enemyPoke.Level,
                                            playerPoke.Level);
    }
}