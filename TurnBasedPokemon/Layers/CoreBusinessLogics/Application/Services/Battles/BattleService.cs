// Layers/CoreBusinessLogics/Application/Services/BattleService.cs
public class BattleService : IBattleService
{
    private readonly GameSession _gameSession;
    private readonly IUserRepository _userRepo;
    private readonly IWorldGenerationService _worldGen;
    // private readonly 

    public BattleService(GameSession session, IUserRepository userRepo, IWorldGenerationService worldGen)
    {
        _gameSession = session;
        _userRepo = userRepo;
        _worldGen = worldGen;
    }

    public BattleTurnResult ExecutePlayerTurn(PokemonMove move)
    {
        var playerPoke = _gameSession?.Player?.CurrentPokemon;
        var enemyPoke = _gameSession?.CurrentEnemyPokemon;
        
        // 1. Tính sát thương
        int damage = DamageCalculator.CalculateDamage(playerPoke, enemyPoke, move);
        enemyPoke.TakeDamage(damage);

        if (enemyPoke.CurrentHP <= 0) return BattleTurnResult.EnemyFainted();
        
        return BattleTurnResult.Continue();
    }

    public BattleTurnResult ExecuteEnemyTurn()
    {
        var enemyPoke = _gameSession.CurrentEnemyPokemon;
        var playerPoke = _gameSession.Player.CurrentPokemon;

        // AI đơn giản
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

        int expGained = ExpCalculator.CalculateExpGain(_gameSession.IsTrainerBattle, enemyPoke.Specie.BaseExp, enemyPoke.Level, playerPoke.Level);
        playerPoke.GainExperience(expGained);

        bool leveledUp = false;
        string? evolutionName = null;

        // Check Level up & Evolution logic... (Đưa logic từ Screen vào đây)
        
        // _userRepo.Save(_gameSession.CurrentUser); // Lưu sau trận đấu
        return new ProcessExpResult(expGained, leveledUp, evolutionName);
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