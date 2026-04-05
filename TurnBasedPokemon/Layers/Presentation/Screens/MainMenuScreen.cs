using System.Threading;
using Screens;

public class MainMenuScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;

    public MainMenuScreen(ScreenManager screenManager, GameSession session)
    {
        _screenManager = screenManager;
        _gameSession = session;
    }

    public void Initialize(object? data = null)
    {
        Console.Clear();
    }

    public void Update()
    {
        Console.Clear();
        Menu.MainMenu();
        string input = Console.ReadLine() ?? string.Empty;

        if (!UserInputValidator.ValidateMainMenuInput(input))
        {
            Console.Clear();
            Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
            return;
        }

        switch (input)
        {
            case "1" : HandleStartNewGame(); break;
            case "2" : HandleLoadGame(); break;
            case "3" : HandlePvP(); break;
            case "4" : HandleSaveGame(); break;
            case "5" : HandleBackToLogin(); break;
        }
    }

    private void HandleStartNewGame()
    {
        _screenManager.SwitchTo(ScreenType.Story, "NewGame");
    }

    private void HandleLoadGame()
    {
        Console.Clear();
        Console.WriteLine("=================================");
        Console.WriteLine("           LOAD GAME             ");
        Console.WriteLine("=================================");

        // 1. Safety Check: Ensure there is a username to look for
        // If CurrentUser is null, it means they haven't even logged in yet
        if (_gameSession.CurrentUser == null || string.IsNullOrEmpty(_gameSession.CurrentUser.Username))
        {
            Console.WriteLine("\n[Error] No active session found. Please login again.");
            Console.ReadKey();
            _screenManager.SwitchTo(ScreenType.Login);
            return;
        }

        string username = _gameSession.CurrentUser.Username;
        Console.WriteLine($"\n[System] Loading save data for: {username}...");
        
        // 2. Call LoadProgress from GameSession
        // This will update _gameSession.CurrentUser with data from JSON
        bool success = _gameSession.LoadProgress(username);

        if (success)
        {
            // 3. Check if the loaded user actually has PlayerData
            // (Sometimes a user exists but has never started a game)
            if (_gameSession.Player == null)
            {
                Console.WriteLine("\n[Notice] Account found, but no character data exists.");
                Console.WriteLine("Please select 'Start New Game' to create your character.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                _screenManager.SwitchTo(ScreenType.MainMenu);
                return;
            }

            // 4. Success feedback
            Console.WriteLine($"\n[Success] Welcome back, {username}!");
            Console.WriteLine($"[Stats] Phase: {_gameSession.Player.CurrentPhase}");
            Console.WriteLine($"[Team] Pokemon: {_gameSession.Player.PokemonTeam.Count}");
            
            Console.WriteLine("\nPress any key to continue your journey...");
            Console.ReadKey();

            // 5. Transition to Story Screen
            _screenManager.SwitchTo(ScreenType.Story);
        }
        else
        {
            // Case: Username not found in users.json
            Console.WriteLine("\n[Error] Save file not found.");
            Console.WriteLine("Please start a new adventure first!");
            Console.ReadKey();
            _screenManager.SwitchTo(ScreenType.MainMenu);
        }
    }

    private void HandlePvP()
    {
        // TODO: Implement PvP mode
    }

    private void HandleSaveGame()
    {
        // TODO: Implement save game logic
        _gameSession.SaveProgress();
    }

    private void HandleBackToLogin()
    {
        _screenManager.SwitchTo(ScreenType.Login);
    }

    public void Shutdown()
    {
        // Cleanup resources if needed
    }
}