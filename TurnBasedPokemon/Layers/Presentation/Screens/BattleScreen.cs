using Screens;

public class BattleScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private Player _player;
    private PokemonSpawner _pokemonSpawner;

    private List<Pokemon> _enemyTeam = new();
    private int _currentEnemyIndex = 0;
    private bool _isTrainerBattle = false;

    private Pokemon PlayerActivePokemon => _player.CurrentPokemon;
    private Pokemon EnemyActivePokemon => _enemyTeam[_currentEnemyIndex];

    public BattleScreen(ScreenManager screenManager, Player player, PokemonSpawner pokemonSpawner)
    {
        _screenManager = screenManager;
        // _battleRenderer = battleRenderer;
        _player = player;
        _pokemonSpawner = pokemonSpawner;
    }

    public void Initialize(object? data = null)
    {
        Console.Clear();
        // _currentEnemyIndex = 0;

        if (data is Pokemon wildPokemon)
        {
            // _enemyTeam = new List<Pokemon> { wildPokemon };
            _enemyTeam.Clear();
            _enemyTeam = new List<Pokemon> {wildPokemon};
            _isTrainerBattle = false;
        }
        else if (data is List<Pokemon> npcPokemonTeam)
        {
            _enemyTeam = npcPokemonTeam;
            _isTrainerBattle = true;
        }
    }

    public void RenderText()
    {
        Console.Clear();
    
        // 1. Render Enemy Info
        RenderEnemyPokemon(_enemyTeam[_currentEnemyIndex]);
        
        // 2. Render Player Info
        RenderPlayerPokemon(_player.CurrentPokemon);
        
        // 3. Render Battle Log hoặc Menu
        
    }

    private void HandleFightOption()
    {
        ShowMoveMenu();
        string input = Console.ReadLine() ?? "0";
        
        // Get the current active Pokemon for cleaner code
        Pokemon activePoke = _player.CurrentPokemon;

        // Validate input and ensure the move is selected from the learned 'Moves' list, not the template 'MoveSet'
        if (int.TryParse(input, out int moveIndex) && moveIndex > 0 && moveIndex <= activePoke.Moves.Count)
        {
            // Select the move that the Pokemon actually possesses
            PokemonMove selectedMove = activePoke.Moves[moveIndex - 1];
            ExecuteBattleTurn(selectedMove);
        }
    }

    private void HandleSwitchPokemon()
    {
        // Console.SetCursorPosition(0, 20);
        // Console.WriteLine("Switching Pokemon is not implemented yet!");
        Console.WriteLine("=== POKEMON TEAM ===");
        var currentTeam = _player.PokemonTeam;
        for (int i =0;i<currentTeam.Count;i++)
        {
            Pokemon p = currentTeam[i];
            string pokemonTypes = p.Specie.Types.Count == 1 ? $"{p.Specie.Types[0]}" : $"{p.Specie.Types[0]}/{p.Specie.Types[1]}";
            Console.WriteLine($"{i+1}. {p.Specie.Name} ({pokemonTypes}) [HP: {p.CurrentHP}/{p.MaxHP}]");
        }
        Console.WriteLine("0. Back");
        Console.WriteLine("Select: ");
        string input = Console.ReadLine() ?? "";

        if (input == "0") return;
        if (int.TryParse(input, out int index) && index > 0)
        {
            if (_player.CurrentPokemon == currentTeam[index - 1]) Console.WriteLine($"{_player.CurrentPokemon.Specie.Name} already in the field!");
            else
            {
                _player.CurrentPokemon = currentTeam[index - 1];
                Console.WriteLine($"You switch to {_player.CurrentPokemon.Specie.Name}");
            }
        }
        Console.ReadKey(true);
        // Thread.Sleep(800);
    }

    private void HandleOpenBag()
    {
        // Console.SetCursorPosition(0, 20);
        _screenManager.SwitchTo(ScreenType.Inventory, new InventoryContext(_player, EnemyActivePokemon));
        // Console.WriteLine("Bag is empty!");
        // Thread.Sleep(800);
    }

    private void HandleRun()
    {
        Console.Clear();
        Console.WriteLine("You got away safely!");
        Thread.Sleep(1000);
        _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
    }

    private void ExecuteBattleTurn(PokemonMove playerMove)
    {
        Pokemon playerPoke = _player.CurrentPokemon;
        Pokemon enemyPoke = EnemyActivePokemon;

        // --- STEP 1: PLAYER'S TURN ---
        bool enemyFainted = PerformAttack(playerPoke, enemyPoke, playerMove, "Player");
        
        if (enemyFainted)
        {
            // Use ExpCalculator to get the real reward
            int expReward = ExpCalculator.CalculateExpGain(
                _isTrainerBattle, 
                enemyPoke.Specie.BaseExp, 
                enemyPoke.Level
            );

            // This will trigger the LevelUp logic inside your Pokemon class
            playerPoke.GainExperience(expReward);
            
            // Let the player see the result before handling fainted logic (sending next Pokemon)
            Thread.Sleep(1000); 
            
            HandleEnemyFainted();
            return; 
        }

        // --- STEP 2: ENEMY'S TURN ---
        var enemyMove = GetEnemyMove();
        bool playerFainted = PerformAttack(enemyPoke, playerPoke, enemyMove, _isTrainerBattle ? "Gym Leader" : "Wild");

        if (playerFainted)
        {
            HandlePlayerFainted();
        }
    }

    private bool PerformAttack(Pokemon attacker, Pokemon target, PokemonMove move, string attackerLabel)
    {
        Console.Clear();
        RenderText(); // Refresh UI to show latest HP bars
        
        Console.SetCursorPosition(0, 15);
        Console.WriteLine($"{attackerLabel}'s {attacker.Specie.Name} used {move.Name}!");
        
        // Calculate damage based on your logic (using simplified 10 for now)
        // int damage = 10;
        int damage = DamageCalculator.CalculateDamage(attacker, target, move);
        target.TakeDamage(damage);
        
        Thread.Sleep(1000); // Wait for the player to read the combat message
        
        return target.CurrentHP <= 0; // Return true if target is defeated
    }

    private void HandleEnemyFainted()
    {
        // 1. Get references to participants
        Pokemon playerPoke = _player.CurrentPokemon;
        Pokemon enemyPoke = EnemyActivePokemon;

        Console.WriteLine($"\n{enemyPoke.Specie.Name} fainted!");
        Thread.Sleep(800);

        // 2. Calculate EXP gain using the formula
        int baseExp = enemyPoke.Specie.BaseExp; 
        int expGained = ExpCalculator.CalculateExpGain(_isTrainerBattle, baseExp, enemyPoke.Level, playerPoke.Level);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{playerPoke.Specie.Name} gained {expGained} EXP!");
        Console.ResetColor();

        // 3. Add EXP immediately to prevent logic loops
        // playerPoke.TotalExp += expGained;

        // 4. Process Level Ups and Evolutions sequentially
        // We check level by level so we don't skip evolution milestones
        while (ExpCalculator.CanLevelUp(playerPoke.Level, playerPoke.TotalExp))
        {
            // Increase level and update Stats (HP, Atk, etc.)
            playerPoke.LevelUp(); 

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n★ {playerPoke.Specie.Name} leveled up to Level {playerPoke.Level}!");
            Console.ResetColor();
            Thread.Sleep(1000); 

            // 5. Check for evolution at this specific level
            if (playerPoke.CanEvolve())
            {
                // Ensure EvolutionId is not null before accessing .Value
                if (playerPoke.Specie.EvolutionId.HasValue)
                {
                    int evolutionTargetId = playerPoke.Specie.EvolutionId.Value;
                    // Create the new evolved form from the spawner
                    Pokemon evolvedForm = _pokemonSpawner.SpawnPokemonById(evolutionTargetId, playerPoke.Level);

                    if (evolvedForm != null)
                    {
                        string oldName = playerPoke.Specie.Name;
                        playerPoke.Evolve(evolvedForm);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("\n*********************************");
                        Console.WriteLine($"What? {oldName} is evolving!");
                        Thread.Sleep(2000);
                        Console.WriteLine($"{oldName} evolved into {playerPoke.Specie.Name}!");
                        Console.WriteLine("*********************************\n");
                        Console.ResetColor();
                        Thread.Sleep(1500);
                        Console.ReadKey(true);
                    }
                }
            }
        }

        // 6. Refresh UI after all calculations are done
        // RenderPlayerPokemon(playerPoke); 

        // 7. Handle Post-Battle Flow
        if (_currentEnemyIndex >= _enemyTeam.Count - 1)
        {
            // Battle won - return to story/world map
            _currentEnemyIndex = 0;
            Console.WriteLine("\nYou won the battle!");
            Thread.Sleep(2000);
            _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
        }
        else
        {
            // Next opponent joins the battle
            _currentEnemyIndex++;
            Console.WriteLine($"\nOpponent is sending out {EnemyActivePokemon.Specie.Name}!");
            Thread.Sleep(1500);
            _screenManager.SwitchTo(ScreenType.Battle, _enemyTeam);
            // Console.Clear();
            // Console.ReadKey(true);
        }
    }

    private PokemonMove GetEnemyMove()
    {
        // Simple AI: Randomly pick a move from the enemy's available moves
        var random = new Random();
        int index = random.Next(EnemyActivePokemon.Moves.Count);
        return EnemyActivePokemon.Moves[index];
    }

    private void HandlePlayerFainted()
    {
        Console.WriteLine($"\n{_player.CurrentPokemon.Specie.Name} has fainted!");
        Console.WriteLine("You have no more usable Pokemon. Returning to safety...");
        Console.ReadKey(true);
        _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
    }

    private void RenderEnemyPokemon(Pokemon pokemon)
    {
        Console.WriteLine($"Current enemy pokemon: {pokemon.Specie.Name}");
        Console.WriteLine($"curren index: {_currentEnemyIndex}");
        // Console.ReadKey(true);
        // Thread.Sleep(1500);

        Console.SetCursorPosition(35, 1);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"{pokemon.Specie.Name.ToUpper()}");
        
        Console.SetCursorPosition(50, 1);
        Console.ResetColor();
        Console.Write($"Lv.{pokemon.Level}");

        Console.SetCursorPosition(35, 2);
        Console.Write("HP: ");
        DrawHPBar(pokemon.CurrentHP, pokemon.MaxHP); // Health bar - 20
    }

    private void RenderPlayerPokemon(Pokemon pokemon)
    {
        Console.SetCursorPosition(5, 8);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{pokemon.Specie.Name.ToUpper()}");

        Console.SetCursorPosition(20, 8);
        Console.ResetColor();
        Console.Write($"Lv.{pokemon.Level}");

        Console.SetCursorPosition(5, 9);
        Console.Write("HP: ");
        DrawHPBar(pokemon.CurrentHP, pokemon.MaxHP);

        // Display current pokemon's health under HP bar
        Console.SetCursorPosition(5, 10);
        Console.WriteLine($"    {pokemon.CurrentHP}/{pokemon.MaxHP}");

        // Render EXP Bar using your progress percentage
        float progress = ExpCalculator.GetLevelProgressPercentage(pokemon.Level, pokemon.TotalExp);
        
        Console.SetCursorPosition(5, 11);
        Console.Write("EXP: ");
        DrawExpBar(progress);
    }

    private void DrawExpBar(float progress)
    {
        int barLength = 20;
        // Use Math.Round or ensure at least 1 vạch if progress > 0
        int filledLength = (int)Math.Round(barLength * progress);
        
        // Safety check
        if (progress > 0 && filledLength == 0) filledLength = 1;
        if (filledLength > barLength) filledLength = barLength;

        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Blue;
        
        // Use a smoother character like '█' (Alt+219) if your console supports it
        // Or stick with '='
        Console.Write(new string('=', filledLength));
        
        Console.ResetColor();
        Console.Write(new string('-', barLength - filledLength));
        Console.Write("]"); // Removed WriteLine to keep cursor control
    }

    private void ShowCommandMenu()
    {
        // Console.SetCursorPosition(0, 13);
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($" What will {_player.CurrentPokemon.Specie.Name} do?");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("  1. FIGHT          2. BAG");
        Console.WriteLine("  3. POKEMON        4. RUN");
        Console.WriteLine(new string('═', 60));
        Console.Write("Select: ");
    }

    private void DrawHPBar(int currentHP, int maxHP)
    {
        float percentage = (float)currentHP / maxHP;
        int barLength = 20;
        int filledLength = (int)(barLength * percentage);

        Console.Write("[");
        // Change color based on HP percentage
        if (percentage > 0.5) Console.ForegroundColor = ConsoleColor.Green;
        else if (percentage > 0.2) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Red;

        Console.Write(new string('█', filledLength));
        Console.Write(new string('-', barLength - filledLength));
        
        Console.ResetColor();
        Console.WriteLine($"] {currentHP}/{maxHP}");
    }

    private void ShowMoveMenu()
    {
        Console.WriteLine("\n" + new string('═', 40));
        Console.WriteLine(" CHOOSE A MOVE (0 to Go Back):");
        Console.WriteLine(new string('─', 40));

        List<PokemonMove> moves = _player.CurrentPokemon.Moves;

        for (int i = 0; i < moves.Count; i++)
        {
            Console.WriteLine($" {i + 1}. {moves[i].Name} [{moves[i].Type}]");
        }

        Console.WriteLine(new string('═', 40));
        Console.Write("Select (1-{0}): ", moves.Count);
    }

    public void Update()
    {
        RenderText(); // Draw UI (HP, Name...)
        
        // If it's not your turn to attack (waiting for input)
        ShowCommandMenu();
        string input = Console.ReadLine() ?? string.Empty;

        if (UserInputValidator.ValidateCommandMenuInput(input))
        {
            switch (input)
            {
                case "1": HandleFightOption(); break;
                case "2": HandleOpenBag(); break;
                case "3": HandleSwitchPokemon(); break;
                case "4": HandleRun(); break;
            }
        }
    }

    public void Shutdown()
    {
        // Cleanup resources if needed
    }
}