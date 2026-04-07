using System;
using System.Threading;
using Screens;

public class PvPScreen : IScreen
{
    private readonly ScreenManager _sm;
    private readonly NetworkService _network;
    private readonly GameSession _session;
    
    private string _roomId = "";
    private bool _isWaitingForServer = false;
    
    // FIX TẠI ĐÂY: Dùng PvPTurnResult thay vì BattleTurnResult
    private PvPTurnResult? _lastResult = null; 

    public PvPScreen(ScreenManager sm, NetworkService network, GameSession session)
    {
        _sm = sm;
        _network = network;
        _session = session;
    }

    public void Initialize(object? data = null)
    {
        _isWaitingForServer = false;
        _lastResult = null; // Clear data cũ khi vào phòng mới
        
        // Extract room data passed from the Matchmaking Screen
        if (data != null)
        {
            _roomId = data.GetType().GetProperty("RoomId")?.GetValue(data)?.ToString() ?? "";
        }

        // Subscribe to server updates
        _network.OnTurnResultReceived += HandleTurnResult;
    }

    public void Shutdown()
    {
        // Prevent memory leaks
        _network.OnTurnResultReceived -= HandleTurnResult;
    }

    private void HandleTurnResult(PvPTurnResult result)
    {
        _lastResult = result;
        _isWaitingForServer = false; // Unlock the UI
    }

    public void Update()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"=== GLOBAL PVP ARENA ===");
        Console.WriteLine($"Room: {_roomId}");
        Console.ResetColor();

        // 1. Display the results from the previous turn
        if (_lastResult != null)
        {
            Console.WriteLine("\n--- TURN RESULTS ---");
            foreach (var log in _lastResult.CombatLogs)
            {
                Console.WriteLine($"> {log}");
            }
            Console.WriteLine("--------------------\n");

            // Kiểm tra kết thúc trận đấu (Tính năng của PvPTurnResult)
            if (_lastResult.IsGameOver)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[Match Over] Winner: {_lastResult.WinnerName}");
                Console.ResetColor();
                Console.WriteLine("Press any key to return to Main Menu...");
                Console.ReadKey(true);
                _sm.SwitchTo(ScreenType.MainMenu);
                return;
            }
        }

        // 2. Block UI while waiting for the opponent and server
        if (_isWaitingForServer)
        {
            Console.WriteLine("\n[System] Waiting for opponent to make a move...");
            Thread.Sleep(500); // Prevent console flickering while looping
            return;
        }

        // 3. Player Input Phase
        // Get the active Pokemon (assuming it's the first one in the team for now)
        var activePokemon = _session.Player.PokemonTeam[0];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nYour Turn! Choose an action for {activePokemon.Specie.Name}:");
        Console.ResetColor();

        // Dynamically render available moves
        for (int i = 0; i < activePokemon.Moves.Count; i++)
        {
            var move = activePokemon.Moves[i];
            
            // LƯU Ý: Đảm bảo class PokemonMove của bạn có các thuộc tính Type, CurrentPP và MaxPP
            // Nếu không có, hãy sửa lại thành: Console.WriteLine($"{i + 1}. {move.Name}");
            Console.WriteLine($"{i + 1}. {move.Name} (Type: {move.Type})");
        }

        Console.Write($"\nSelect move (1-{activePokemon.Moves.Count}): ");

        var input = Console.ReadLine();
        
        // Validate input based on the actual number of moves
        if (int.TryParse(input, out int moveChoice) && moveChoice >= 1 && moveChoice <= activePokemon.Moves.Count)
        {
            _isWaitingForServer = true; // Lock UI immediately
            Console.WriteLine("\n[Network] Sending move to server...");
            
            // Send move (Index is 0-based, so subtract 1 from user input)
            _ = _network.SubmitMoveAsync(_roomId, _session.Player.Name, moveChoice - 1); 
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Please select a valid move number.");
            Console.ResetColor();
            Thread.Sleep(1000);
        }
    }
}