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

        CatchAttemptResult attempt = CatchFormula.AttemptCatch(targetPokemon, item);
        player?.RemoveItem(item);

        if (attempt.IsCaught)
        {
            player?.PokemonTeam.Add(targetPokemon);
            _gameSession.ClearBattle();
        }

        return new CatchResult{
            IsCaught = attempt.IsCaught,
            Shakes = attempt.Shakes,
            DisplayMessage = GenerateMessage(attempt.IsCaught, attempt.Shakes, target.Specie.Name),
        };
    }

    private string GenerateMessage(bool caught, int shakes, string name)
    {
        if (caught) return $"Gotcha! {name} was caught!";
        return shakes == 0 ? $"Missed {name}!" : $"{name} broke free!";
    }
    
}