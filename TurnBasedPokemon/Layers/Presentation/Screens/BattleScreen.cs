using System.Linq.Expressions;
using Screens;
using PokemonEntity;

public class BattleScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IBattleService _battleService;
    private string? _pvpRoomId;
    private bool _isPvP = false;

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
        Console.Clear();
        if (data != null)
        {
            // Use reflection or cast to a dynamic/specific type to get PvP info
            var pvpData = data.GetType().GetProperty("IsPvP")?.GetValue(data, null);
            _isPvP = pvpData is bool b && b;

            if (_isPvP)
            {
                _pvpRoomId = data.GetType().GetProperty("RoomId")?.GetValue(data, null) as string;
                Console.WriteLine($"[PvP Mode] Connected to Room: {_pvpRoomId}");
            }
        }
    }

    public void Update()
    {
        Console.Clear();
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
            _gameSession.ClearBattle();
        }
    }

    private void HandleVictory()
    {
        var enemyName = _gameSession.CurrentEnemyPokemon.Specie.Name;
        var playerPoke = _gameSession.Player.CurrentPokemon;

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  [ VICTORY! ]");
        Console.ResetColor();
        Console.WriteLine($"  {enemyName} fainted!");
        Thread.Sleep(1000);

        var expResult = _battleService.ProcessVictory();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  >> Gained {expResult.ExpGained} EXP!");
        Console.ResetColor();
        
        var expStats = ExpCalculator.GetProgressStats(playerPoke.Level, playerPoke.TotalExp);
        PrintExpBar(expStats.currentExpInLevel, expStats.expNeededForNextLevel);
        Thread.Sleep(1200);

        if (expResult.LeveledUp)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ★ LEVEL UP!");
            Console.WriteLine($"  {playerPoke.Specie.Name} reached Level {playerPoke.Level}!");
            Console.ResetColor();

            if (expResult.NewMovesLearned.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var moveName in expResult.NewMovesLearned)
                {
                    Console.WriteLine($"  [!] {playerPoke.Specie.Name} learned {moveName}!");
                    Thread.Sleep(800);
                }
                Console.ResetColor();
                
                Console.WriteLine("  (Press any key to confirm new moves)");
                Console.ReadKey(true);
            }
            else 
            {
                Thread.Sleep(1500);
            }
        }

        if (!string.IsNullOrEmpty(expResult.EvolutionName))
        {
            DisplayEvolutionScene(expResult.OldName ?? playerPoke.Specie.Name, expResult.EvolutionName);
        }

        Console.WriteLine("\n  Press any key to continue...");
        Console.ReadKey(true);
        
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
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine(" [ WILD BATTLE ] ");
        Console.WriteLine("------------------------------------------------------------");
        RenderEnemyPokemon(_gameSession.CurrentEnemyPokemon);
        Console.WriteLine("\n          ( VS )\n");
        RenderPlayerPokemon(_gameSession.Player.CurrentPokemon);
    }

    private void DisplayBattleMessage(string message)
    {
        RenderText();
        Console.SetCursorPosition(0, 15);
        Console.WriteLine(message);
        Thread.Sleep(1000);
    }

    private void DisplayEvolutionScene(string oldName, string newName)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n\n  What?");
        Thread.Sleep(1500);
        
        Console.WriteLine($"  {oldName} is evolving!");
        Thread.Sleep(1000);
        
        for (int i = 0; i < 4; i++)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Thread.Sleep(100);
            
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Thread.Sleep(250);
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n  ⭐ CONGRATULATIONS! ⭐");
        Console.WriteLine($"  Your {oldName} evolved into {newName}!");
        Console.ResetColor();
        
        Console.Beep(440, 200); 
        Console.Beep(659, 200); 
        Console.Beep(880, 500);
    }

    private void RenderEnemyPokemon(Pokemon pokemon)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{"",30}{pokemon.Specie.Name.ToUpper()} [Lv. {pokemon.Level}]");
        Console.Write($"{"",30}HP: ");
        PrintHealthBar(pokemon.CurrentHP, pokemon.MaxHP);
        Console.WriteLine($" {pokemon.CurrentHP}/{pokemon.MaxHP}");
    }

    private void RenderPlayerPokemon(Pokemon pokemon)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  {pokemon.Specie.Name.ToUpper()} [Lv. {pokemon.Level}]");
        Console.Write("  HP:  ");
        PrintHealthBar(pokemon.CurrentHP, pokemon.MaxHP);
        Console.WriteLine($" {pokemon.CurrentHP}/{pokemon.MaxHP}");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  EXP: ");
        PrintExpBar(pokemon.CurrentExp, pokemon.MaxExpForNextLevel);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("------------------------------------------------------------");
    }

    private void PrintHealthBar(int current, int max)
    {
        float percentage = (float)current / max;
        int barCount = (int)(percentage * 16);

        if (percentage > 0.5) Console.ForegroundColor = ConsoleColor.Green;
        else if (percentage > 0.2) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Red;

        Console.Write("[");
        Console.Write(new string('█', barCount));
        Console.Write(new string('-', 16 - barCount));
        Console.Write("]");
    }

    private void PrintExpBar(int currentExp, int nextLevelExp)
    {
        if (nextLevelExp <= 0) nextLevelExp = 100; 

        float percentage = Math.Min((float)currentExp / nextLevelExp, 1.0f);
        int barLength = 20;
        int filledLength = (int)(percentage * barLength);
        int remaining = nextLevelExp - currentExp;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[");
        Console.Write(new string('=', filledLength));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('-', barLength - filledLength));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("]");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($" {currentExp}/{nextLevelExp} EXP");
        
        if (remaining > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"       (Need {remaining} more EXP to Level Up!)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("       (Ready to Evolve/Level Up!)");
        }
        
        Console.ResetColor();
    }

    private void ShowCommandMenu()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine($" What will {_gameSession.Player.CurrentPokemon.Specie.Name} do?");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [1] ⚔️ FIGHT           [2] 🎒 BAG");
        Console.WriteLine("  [3] 🔁 POKEMON        [4] 🏃 RUN");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        Console.Write("  Select: ");
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
        if (_gameSession.IsTrainerBattle)
        {
            Console.WriteLine("You can not run, this is a trainer battle!");
            Thread.Sleep(1500);
            return;
        }
        Console.WriteLine("You got away safely!");
        Thread.Sleep(1000);
        _gameSession.ClearBattle();
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
            Pokemon selectedPokemon = team[index - 1];

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