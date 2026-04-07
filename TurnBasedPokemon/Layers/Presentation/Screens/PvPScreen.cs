using Screens;
using System;
using System.Threading.Tasks;
using PokemonEntity;

public class PvPScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _session;
    private readonly NetworkService _network;
    private bool _isSearching = false;

    public PvPScreen(ScreenManager sm, GameSession session, NetworkService network)
    {
        _screenManager = sm;
        _session = session;
        _network = network;
    }

    public void Initialize(object? data = null)
    {
        _isSearching = false;

        // Subscribe to events when the screen becomes active
        _network.OnMatchFound += HandleMatchFound;
        _network.OnWaiting += HandleWaiting;
    }

    public void Shutdown()
    {
        // UNSUBSCRIBE to prevent duplicate calls and memory leaks
        _network.OnMatchFound -= HandleMatchFound;
        _network.OnWaiting -= HandleWaiting;
    }

    private void HandleMatchFound(string roomId, string opponent)
    {
        Console.WriteLine($"\n[!] Match Found! Opponent: {opponent}");
        Console.WriteLine("Redirecting to Battle Arena...");
        
        // Pass PvP-specific data to the Battle Screen
        _screenManager.SwitchTo(ScreenType.Battle, new { RoomId = roomId, IsPvP = true, OpponentName = opponent });
    }

    private void HandleWaiting()
    {
        Console.WriteLine("\n[System] Standing by... Searching for a worthy opponent.");
    }

    public void Update()
    {
        // If we are already waiting for a match, don't redraw the menu
        if (_isSearching) return;

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================");
        Console.WriteLine("          POKEMON PVP ARENA          ");
        Console.WriteLine("======================================");
        Console.ResetColor();

        // Requirement: Only Champions can enter PvP
        if (!_session.Player.IsChampion)
        {
            Console.WriteLine("\n[LOCKED] This area is reserved for the Pokemon League Champion.");
            Console.WriteLine("Come back after defeating the Elite Four!");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            _screenManager.SwitchTo(ScreenType.MainMenu);
            return;
        }

        Console.WriteLine($"\nWelcome, Champion {_session.Player.Name}!");
        Console.WriteLine("1. Find an Opponent");
        Console.WriteLine("2. Back to Main Menu");
        Console.Write("\nSelect an option: ");

        var input = Console.ReadLine();
        if (input == "1")
        {
            _isSearching = true;
            _ = StartSearchTask(); // Run the async search in the background
        }
        else
        {
            _screenManager.SwitchTo(ScreenType.MainMenu);
        }
    }

    private async Task StartSearchTask()
    {
        try 
        {
            Console.WriteLine("\n[Network] Connecting to Global Battle Hub...");
            await _network.Connect();
            await _network.FindMatch(_session.Player.Name);
        } 
        catch (Exception ex) 
        {
            _isSearching = false;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Error] Could not connect to server: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("Press any key to retry...");
            Console.ReadKey(true);
        }
    }
}