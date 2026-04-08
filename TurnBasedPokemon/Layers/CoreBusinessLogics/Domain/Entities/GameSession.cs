using PokemonEntity;

public class GameSession
{
    // --- Core ---
    public User? CurrentUser { get; set; }
    public Player? Player => CurrentUser?.PlayerData;

    // --- Story/World Status ---
    public LocationData CurrentLocation { get; set; } = new LocationData();
    public bool HasPlayedNarrative { get; set; }

    // --- Battle Status ---
    public List<Pokemon> CurrentEnemyTeam { get; set; } = new();
    public int CurrentEnemyTeamIndex { get; set; } = 0;
    public bool IsTrainerBattle { get; set; }
    
    // Safe attribute to get curren enemy pokemon
    public Pokemon? CurrentEnemyPokemon => 
        (CurrentEnemyTeam != null && CurrentEnemyTeamIndex < CurrentEnemyTeam.Count) 
        ? CurrentEnemyTeam[CurrentEnemyTeamIndex] 
        : null;

    #region Battle Management

    /// <summary>
    /// Prepare data before enter battle
    /// </summary>
    public void SetupBattle(List<Pokemon> enemies, bool isTrainer)
    {
        CurrentEnemyTeam = enemies;
        CurrentEnemyTeamIndex = 0;
        IsTrainerBattle = isTrainer;
    }

    /// <summary>
    /// Check is there is still more enemies
    /// </summary>
    public bool HasMoreEnemies()
    {
        return CurrentEnemyTeamIndex < CurrentEnemyTeam.Count - 1;
    }

    /// <summary>
    /// Switch to next enemy
    /// </summary>
    public void MoveToNextEnemy()
    {
        if (HasMoreEnemies())
        {
            CurrentEnemyTeamIndex++;
        }
    }

    /// <summary>
    /// Clean up the battle (Win/Run/Lose)
    /// </summary>
    public void ClearBattle()
    {
        CurrentEnemyTeam.Clear();
        CurrentEnemyTeamIndex = 0;
        IsTrainerBattle = false;
    }

    #endregion
}