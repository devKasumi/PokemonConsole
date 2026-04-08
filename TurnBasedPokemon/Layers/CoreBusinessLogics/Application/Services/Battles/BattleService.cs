using PokemonEntity;
using Application.RepoInterfaces;

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

    public BattleTurnResult ExecutePlayerTurn(int moveIndex)
    {
        var logs = new List<string>();
        var playerPokemon = _gameSession.Player.PokemonTeam[0];
        var enemyPokemon = _gameSession.CurrentEnemyTeam[0];
        var move = playerPokemon.Moves[moveIndex];

        // 1. Log the move usage
        logs.Add($"{playerPokemon.Specie.Name} used {move.Name}!");

        // 2. Calculate and apply damage
        int damage = DamageCalculator.CalculateDamage(playerPokemon, enemyPokemon, move);
        enemyPokemon.TakeDamage(damage);
        logs.Add($"{enemyPokemon.Specie.Name} took {damage} damage.");

        // 3. Check for fainting conditions
        if (enemyPokemon.IsFainted)
        {
            logs.Add($"{enemyPokemon.Specie.Name} fainted!");
            return BattleTurnResult.EnemyFainted(logs);
        }

        // Return the result encapsulating the full log of events
        return BattleTurnResult.Continue(move.Name, logs);
    }

    public BattleTurnResult ExecuteEnemyTurn()
    {
        var logs = new List<string>();
        
        var enemyPoke = _gameSession.CurrentEnemyPokemon;
        var playerPoke = _gameSession.Player.CurrentPokemon;

        // Randomly select a move for the enemy
        var move = enemyPoke.Moves[new Random().Next(enemyPoke.Moves.Count)];
        
        // 1. Log the move usage (Adding "Enemy" to distinguish who is attacking)
        logs.Add($"Enemy {enemyPoke.Specie.Name} used {move.Name}!");

        // 2. Calculate and apply damage
        int damage = DamageCalculator.CalculateDamage(enemyPoke, playerPoke, move);
        playerPoke.TakeDamage(damage);
        logs.Add($"{playerPoke.Specie.Name} took {damage} damage.");

        // 3. Check for fainting conditions
        if (playerPoke.IsFainted) // Dùng IsFainted cho đồng nhất với Model
        {
            logs.Add($"{playerPoke.Specie.Name} fainted!");
            return BattleTurnResult.PlayerFainted(logs);
        }
        
        // Return the result encapsulating the full log of events
        return BattleTurnResult.Continue(move.Name, logs);
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