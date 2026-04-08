using System.Threading;
using Screens;
using PokemonEntity;

public class MainMenuScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IPvPService _pvpService;
    private readonly IAuthenService _authenService;

    public MainMenuScreen(ScreenManager screenManager, GameSession session, IPvPService pvpService, IAuthenService authenService)
    {
        _screenManager = screenManager;
        _gameSession = session;
        _pvpService = pvpService;
        _authenService = authenService;
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
        bool success = _authenService.LoadProgress(username);

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
        if (!_pvpService.CanAccessPvP())
        {
            Console.WriteLine($"You can only participate in PvP after completing the game's main storyline.");
            Console.ReadKey(true);
            return;
        }

        // Connect to server first
        Console.WriteLine("[Network] Connecting to PvP server...");
        bool connected = _pvpService.Connect();
        if (!connected)
        {
            Console.WriteLine("Press any key to return to Main Menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("[Network] Finding PvP match...");
        Console.WriteLine("[System] Waiting for opponent... Press [Q] to cancel.\n");

        bool matchFound = false;
        bool wasRejected = false;
        string? matchedRoomId = null;
        string? matchedOpponent = null;

        void OnMatchFound(string roomId, string opponentName)
        {
            matchedRoomId = roomId;
            matchedOpponent = opponentName;
            matchFound = true;
        }
        void OnRejected(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[System] {reason}");
            Console.ResetColor();
            wasRejected = true;
        }
        _pvpService.OnMatchFound += OnMatchFound;
        _pvpService.OnMatchmakingRejected += OnRejected;
        _pvpService.FindMatch();

        // Wait for match or player cancellation
        while (!matchFound && !wasRejected)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                {
                    _pvpService.OnMatchFound -= OnMatchFound;
                    _pvpService.CancelMatchmaking();
                    Console.WriteLine("\n[System] PvP matchmaking cancelled. Returning to Main Menu.");
                    Console.ReadKey(true);
                    return;
                }
            }
            Thread.Sleep(200);
        }

        _pvpService.OnMatchFound -= OnMatchFound;
        _pvpService.OnMatchmakingRejected -= OnRejected;

        if (wasRejected)
        {
            Console.WriteLine("Press any key to return to Main Menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine($"\n[Network] Match found! Room: {matchedRoomId}, Opponent: {matchedOpponent}");
        Console.WriteLine("Press any key to enter the arena...");
        Thread.Sleep(500);
        _screenManager.SwitchTo(ScreenType.PvP, new { RoomId = matchedRoomId });
    }

    private void HandleSaveGame()
    {
        Console.CursorVisible = false; 
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        
        // The text we want to print slowly
        string savingText = "Saving progress... Please do not turn off the power.";
        
        // Typewriter effect: print character by character
        foreach (char c in savingText)
        {
            Console.Write(c);
            Thread.Sleep(50); // Wait 50 milliseconds between each character
        }
        Console.WriteLine();
        
        _authenService.SaveProgress();

        Console.ResetColor();
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true); // Wait for player input
        
        // Restore the cursor before returning to the main menu
        Console.CursorVisible = true;
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