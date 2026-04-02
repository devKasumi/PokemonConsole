using Screens;

public class BattleScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IBattleService _battleService;

    public BattleScreen(
        ScreenManager screenManager, 
        GameSession session,
        IBattleService battleService)
    {
        _screenManager = screenManager;
        _gameSession = session;
        _battleService = battleService;
    }

    public void Initialize(object? data = null)
    {
        // Lưu ý: Logic khởi tạo EnemyTeam nên nằm ở Service/Manager 
        // trước khi Switch sang màn hình này.
        Console.Clear();
    }

    public void Update()
    {
        RenderText();
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

    #region Battle Logic Flow (UI Only)

    private void HandleFightOption()
    {
        ShowMoveMenu();
        string input = Console.ReadLine() ?? "0";
        var activePoke = _gameSession.Player.CurrentPokemon;

        if (int.TryParse(input, out int moveIndex) && moveIndex > 0 && moveIndex <= activePoke.Moves.Count)
        {
            ExecuteBattleTurn(activePoke.Moves[moveIndex - 1]);
        }
    }

    private void ExecuteBattleTurn(PokemonMove playerMove)
    {
        // --- 1. PLAYER'S TURN ---
        var playerResult = _battleService.ExecutePlayerTurn(playerMove);
        DisplayBattleMessage($"Player's {_gameSession.Player.CurrentPokemon.Specie.Name} used {playerMove.Name}!");

        if (playerResult.IsEnemyFainted)
        {
            HandleVictory();
            _gameSession.ClearBattle();
            return;
        }

        // --- 2. ENEMY'S TURN ---
        var enemyResult = _battleService.ExecuteEnemyTurn();
        DisplayBattleMessage($"Enemy's {_gameSession.CurrentEnemyPokemon.Specie.Name} used {enemyResult.MoveUsed}!");

        if (enemyResult.IsPlayerFainted)
        {
            HandlePlayerFainted();
        }
    }

    private void HandleVictory()
    {
        Console.WriteLine($"\n{_gameSession.CurrentEnemyPokemon.Specie.Name} fainted!");
        Thread.Sleep(800);

        // Call Service handle reward logic (EXP, Level Up, Tiến hóa)
        var expResult = _battleService.ProcessVictory();

        // Display result from DTO
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nGained {expResult.ExpGained} EXP!");
        Console.ResetColor();

        if (expResult.LeveledUp)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"★ Level Up! Now at Level {_gameSession.Player.CurrentPokemon.Level}!");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        if (!string.IsNullOrEmpty(expResult.EvolutionName))
        {
            DisplayEvolutionScene(expResult.EvolutionName);
        }

        CheckNextOpponent();
    }

    private void CheckNextOpponent()
    {
        if (_gameSession.HasMoreEnemies())
        {
            _gameSession.MoveToNextEnemy();
            Console.WriteLine($"\nOpponent is sending out {_gameSession.CurrentEnemyPokemon.Specie.Name}!");
            Thread.Sleep(1500);
        }
        else
        {
            Console.WriteLine("\nYou won the battle!");
            Thread.Sleep(2000);
            _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
        }
    }

    #endregion

    #region Rendering Methods (UI)

    public void RenderText()
    {
        Console.Clear();
        RenderEnemyPokemon(_gameSession.CurrentEnemyPokemon);
        RenderPlayerPokemon(_gameSession.Player.CurrentPokemon);
    }

    private void DisplayBattleMessage(string message)
    {
        RenderText();
        Console.SetCursorPosition(0, 15);
        Console.WriteLine(message);
        Thread.Sleep(1000);
    }

    private void DisplayEvolutionScene(string newName)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n*********************************");
        Console.WriteLine($"What? Your Pokemon is evolving!");
        Thread.Sleep(2000);
        Console.WriteLine($"It evolved into {newName}!");
        Console.WriteLine("*********************************\n");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    private void RenderEnemyPokemon(Pokemon pokemon)
    {
        Console.SetCursorPosition(35, 1);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"{pokemon.Specie.Name.ToUpper()}  Lv.{pokemon.Level}");
        Console.SetCursorPosition(35, 2);
        Console.Write("HP: ");
        DrawHPBar(pokemon.CurrentHP, pokemon.MaxHP);
    }

    private void RenderPlayerPokemon(Pokemon pokemon)
    {
        Console.SetCursorPosition(5, 8);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{pokemon.Specie.Name.ToUpper()}  Lv.{pokemon.Level}");
        Console.SetCursorPosition(5, 9);
        Console.Write("HP: ");
        DrawHPBar(pokemon.CurrentHP, pokemon.MaxHP);
        
        Console.SetCursorPosition(5, 11);
        Console.Write("EXP: ");
        float progress = ExpCalculator.GetLevelProgressPercentage(pokemon.Level, pokemon.TotalExp);
        DrawExpBar(progress);
    }

    private void DrawHPBar(int currentHP, int maxHP)
    {
        float percentage = (float)currentHP / maxHP;
        int barLength = 20;
        int filledLength = (int)(barLength * percentage);

        Console.Write("[");
        if (percentage > 0.5) Console.ForegroundColor = ConsoleColor.Green;
        else if (percentage > 0.2) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Red;

        Console.Write(new string('█', filledLength).PadRight(barLength, '-'));
        Console.ResetColor();
        Console.WriteLine($"] {currentHP}/{maxHP}");
    }

    private void DrawExpBar(float progress)
    {
        int barLength = 20;
        int filledLength = (int)Math.Round(barLength * Math.Clamp(progress, 0, 1));
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(new string('=', filledLength).PadRight(barLength, '-'));
        Console.ResetColor();
        Console.Write("]");
    }

    private void ShowCommandMenu()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine($" What will {_gameSession.Player.CurrentPokemon.Specie.Name} do?");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("  1. FIGHT          2. BAG");
        Console.WriteLine("  3. POKEMON        4. RUN");
        Console.WriteLine(new string('═', 60));
        Console.Write("Select: ");
    }

    private void ShowMoveMenu()
    {
        var moves = _gameSession.Player.CurrentPokemon.Moves;
        Console.WriteLine("\n" + new string('═', 40));
        for (int i = 0; i < moves.Count; i++)
            Console.WriteLine($" {i + 1}. {moves[i].Name} [{moves[i].Type}]");
        Console.WriteLine(new string('═', 40));
        Console.Write("Select: ");
    }

    private void HandlePlayerFainted()
    {
        Console.WriteLine($"\n{_gameSession.Player.CurrentPokemon.Specie.Name} has fainted!");
        Console.WriteLine("Game Over... Returning to safety.");
        Console.ReadKey(true);
        _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
    }

    private void HandleRun()
    {
        Console.Clear();
        Console.WriteLine("You got away safely!");
        Thread.Sleep(1000);
        _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
    }

    private void HandleSwitchPokemon()
    {
        Console.Clear();
        var team = _gameSession.Player.PokemonTeam;

        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      POKEMON TEAM                        ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════╣");

        for (int i = 0; i < team.Count; i++)
        {
            var p = team[i];
            bool isActive = (p == _gameSession.Player.CurrentPokemon);
            bool isFainted = p.CurrentHP <= 0;

            if (isActive) Console.ForegroundColor = ConsoleColor.Cyan;
            else if (isFainted) Console.ForegroundColor = ConsoleColor.DarkGray;
            else Console.ForegroundColor = ConsoleColor.White;

            string statusTag = isActive ? "[ON FIELD]" : (isFainted ? "[FAINTED]" : "          ");
            
            string line = $"║  {i + 1}. {p.Specie.Name.PadRight(15)} Lv.{p.Level.ToString().PadRight(3)} HP: {p.CurrentHP}/{p.MaxHP}".PadRight(50) + $"{statusTag}  ║";
            Console.WriteLine(line);
        }

        Console.ResetColor();
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine(" 0. Back");
        Console.Write("\n Select a Pokemon to switch: ");

        string input = Console.ReadLine() ?? "";
        if (input == "0") return;

        if (int.TryParse(input, out int index) && index > 0 && index <= team.Count)
        {
            var selectedPokemon = team[index - 1];

            // --- 3. CALL LOGIC FROM SERVICE ---
            // Service will check isFainted and CurrentPokemon
            if (_battleService.CanSwitchPokemon(selectedPokemon))
            {
                Console.WriteLine($"\n Enough! Come back, {team.FirstOrDefault(x => x != selectedPokemon && x.CurrentHP > 0)?.Specie.Name}..."); // Hoặc lưu tên con cũ trước khi đổi
                Thread.Sleep(600);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" Go! {selectedPokemon.Specie.Name}!");
                Console.ResetColor();
                Thread.Sleep(1000);

                var enemyRes = _battleService.ExecuteEnemyTurn();
                DisplayBattleMessage($"Enemy's {_gameSession.CurrentEnemyPokemon.Specie.Name} used {enemyRes.MoveUsed}!");
                
                if (enemyRes.IsPlayerFainted) HandlePlayerFainted();
            }
            else
            {
                // FAIL:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n [!] That Pokemon cannot be sent out right now!");
                Console.ResetColor();
                Console.ReadKey(true);
            }
        }
    }

    private void HandleOpenBag() => _screenManager.SwitchTo(ScreenType.Inventory);

    public void Shutdown() { }

    #endregion
}