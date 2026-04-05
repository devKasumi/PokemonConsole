public class GameSession
{
    // --- Core & Persistence ---
    private IUserRepository _userRepo;
    public User? CurrentUser { get; set; }
    public Player? Player => CurrentUser?.PlayerData;

    // --- Story/World Status ---
    public LocationData CurrentLocation { get; set; }
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

    public GameSession(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }

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

    #region User & Progress

    public void Login(string user, string pass)
    {
        var foundUser = _userRepo.GetByUsername(user);
        
        if (foundUser == null)
        {
            CurrentUser = new User { Username = user, Password = pass };
            _userRepo.Save(CurrentUser);
            Console.WriteLine("Account registered! Please start a new game.");
        }
        else if (foundUser.Password == pass)
        {
            CurrentUser = foundUser;
            Console.WriteLine("Login successful!");
        }
    }

    public void StartNewGame()
    {
        if (CurrentUser == null) return;
        CurrentUser.PlayerData = new Player();
        CurrentUser.PlayerData.CurrentPhase = 1;
        CurrentUser.PlayerData.CurrentLocation = "";
        CurrentUser.PlayerData.PokemonTeam.Clear();
        _userRepo.Save(CurrentUser);
    }

    public bool LoadProgress(string username)
    {
        var loadedUser = _userRepo.GetByUsername(username);
        
        if (loadedUser != null)
        {
            Console.WriteLine(loadedUser.PlayerData != null 
                ? $"[LoadProgress] User '{username}' loaded with player data." 
                : $"[LoadProgress] User '{username}' loaded but has no player data.");


            Console.WriteLine($"Current PlayerData: {(loadedUser.PlayerData != null ? "Exists" : "Null")}");
            Console.WriteLine($"Current PLayer's PokemonTeam Count: {(loadedUser.PlayerData != null ? loadedUser.PlayerData.PokemonTeam.Count : "N/A")}");
            Console.WriteLine($"Current Player's Pokemon: {loadedUser.PlayerData?.CurrentPokemon.Specie.Name}");

            CurrentUser = loadedUser; 
            return true;
        }
        
        return false;
    }

    public void SaveProgress()
    {
        if (CurrentUser != null)
            _userRepo.Save(CurrentUser);
    }

    #endregion
}