// Layers/CoreBusinessLogics/Application/Services/BattleService.cs
public class BattleService : IBattleService
{
    private readonly GameSession _gameSession;
    private readonly IUserRepository _userRepo;
    private readonly IWorldGenerationService _worldGen;
    private readonly IExpService _expService;

    public BattleService(GameSession session, IUserRepository userRepo, IWorldGenerationService worldGen, IExpService expService)
    {
        _gameSession = session;
        _userRepo = userRepo;
        _worldGen = worldGen;
        _expService = expService;
    }

    public BattleTurnResult ExecutePlayerTurn(PokemonMove move)
    {
        var playerPoke = _gameSession?.Player?.CurrentPokemon;
        var enemyPoke = _gameSession?.CurrentEnemyPokemon;
        
        int damage = DamageCalculator.CalculateDamage(playerPoke, enemyPoke, move);
        enemyPoke.TakeDamage(damage);

        if (enemyPoke.CurrentHP <= 0) return BattleTurnResult.EnemyFainted();
        
        return BattleTurnResult.Continue();
    }

    public BattleTurnResult ExecuteEnemyTurn()
    {
        var enemyPoke = _gameSession.CurrentEnemyPokemon;
        var playerPoke = _gameSession.Player.CurrentPokemon;

        var move = enemyPoke.Moves[new Random().Next(enemyPoke.Moves.Count)];
        int damage = DamageCalculator.CalculateDamage(enemyPoke, playerPoke, move);
        playerPoke.TakeDamage(damage);

        if (playerPoke.CurrentHP <= 0) return BattleTurnResult.PlayerFainted();
        
        return BattleTurnResult.Continue(move.Name);
    }

    public ProcessExpResult ProcessVictory()
    {
        var playerPoke = _gameSession.Player.CurrentPokemon;
        var enemyPoke = _gameSession.CurrentEnemyPokemon;

        int expGained = _expService.CalculateExpGain(playerPoke, enemyPoke);

        var result = _expService.GrantExperience(playerPoke, expGained);

        // _userRepo.Save(_gameSession.CurrentUser);

        return result;
    }

    public bool CanSwitchPokemon(Pokemon target)
    {
        if (target.CurrentHP <= 0 || target == _gameSession.Player.CurrentPokemon)
        {
            return false; 
        }

        _gameSession.Player.CurrentPokemon = target;
        return true;
    }
}