using PokemonEntity;

public class CatchService : ICatchService
{
    private readonly GameSession _gameSession;

    public CatchService(GameSession session) 
    {
        _gameSession = session; 
    }
    
    public CatchResult ExecuteCapture(CaptureItem item, Pokemon target)
    {
        Player? player = _gameSession.Player;
        Pokemon targetPokemon = _gameSession.CurrentEnemyPokemon;
        if (targetPokemon == null) return new CatchResult {IsCaught = false, Shakes = 0};

        CatchResult result = CatchFormula.AttemptCatch(targetPokemon, item);
        player?.RemoveItem(item);

        if (result.IsCaught)
        {
            player?.PokemonTeam.Add(targetPokemon);
            _gameSession.ClearBattle();
        }

        return new CatchResult{
            IsCaught = result.IsCaught,
            Shakes = result.Shakes,
            DisplayMessage = GenerateMessage(result.IsCaught, result.Shakes, target.Specie.Name),
        };
    }

    private string GenerateMessage(bool caught, int shakes, string name)
    {
        if (caught) return $"Gotcha! {name} was caught!";
        return shakes == 0 ? $"Missed {name}!" : $"{name} broke free!";
    }
    
}