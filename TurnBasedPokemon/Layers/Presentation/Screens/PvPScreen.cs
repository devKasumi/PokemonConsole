using System;
using System.Threading;
using Screens;

public class PvPScreen : IScreen
{
    private readonly ScreenManager _sm;
    private readonly NetworkService _network;
    private readonly GameSession _session;
    
    private string _roomId = "";
    private volatile PvPTurnResult? _lastResult = null;
    private bool _initialized = false;
    private volatile bool _waitingForResult = false;

    public PvPScreen(ScreenManager sm, NetworkService network, GameSession session)
    {
        _sm = sm;
        _network = network;
        _session = session;
    }

    public void Initialize(object? data = null)
    {
        // GUARD: ScreenManager calls Initialize every loop iteration.
        // Only run setup once per PvP session.
        if (_initialized) return;
        _initialized = true;

        _lastResult = null;
        _waitingForResult = false;
        
        if (data != null)
        {
            _roomId = data.GetType().GetProperty("RoomId")?.GetValue(data)?.ToString() ?? "";
        }

        _network.OnTurnResultReceived += HandleTurnResult;

        // Pick up any initial state that arrived before we subscribed
        if (_network.LastTurnResult != null)
        {
            _lastResult = _network.LastTurnResult;
        }
    }

    public void Shutdown()
    {
        _network.OnTurnResultReceived -= HandleTurnResult;
        _initialized = false;
    }

    private void HandleTurnResult(PvPTurnResult result)
    {
        _lastResult = result;
        _waitingForResult = false;
    }

    public void Update()
    {
        Console.Clear();

        // 0. Check if player is in a room
        if (string.IsNullOrEmpty(_roomId))
        {
            Console.WriteLine("[Error] You are not in a PvP room. Returning to Main Menu.");
            Console.ReadKey(true);
            _sm.SwitchTo(ScreenType.MainMenu);
            return;
        }

        // Always show room header
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine("                   [ PVP ARENA ]");
        Console.WriteLine($"                   Room: {_roomId}");
        Console.WriteLine("============================================================");
        Console.ResetColor();

        // Capture a snapshot of the current state to avoid race conditions
        // (HandleTurnResult fires on SignalR thread and can change _lastResult mid-render)
        var result = _lastResult;
        bool waiting = _waitingForResult;

        // 1. Wait for initial battle state from server
        if (result == null)
        {
            Console.WriteLine("\n[System] Waiting for battle data from server...");
            Thread.Sleep(500);
            return;
        }

        // 2. Render the battle scene
        RenderBattleScene(result);

        // 3. Show combat logs
        if (result.CombatLogs.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("------------------------------------------------------------");
            foreach (var log in result.CombatLogs)
            {
                Console.WriteLine($"  > {log}");
            }
            Console.WriteLine("------------------------------------------------------------");
            Console.ResetColor();
        }

        // 4. Game Over check
        if (result.IsGameOver)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ★ MATCH OVER! Winner: {result.WinnerName} ★");
            Console.ResetColor();
            Console.WriteLine("\n  Press any key to return to Main Menu...");
            Console.ReadKey(true);
            _initialized = false;
            _sm.SwitchTo(ScreenType.MainMenu);
            return;
        }

        // 5. Waiting for server to process (already submitted, waiting for opponent)
        if (waiting)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n  [System] Waiting for opponent to make a move...");
            Console.ResetColor();
            Thread.Sleep(500);
            return;
        }

        // 6. Player Input Phase
        var myActive = result.Player1Active;
        if (myActive == null)
        {
            Console.WriteLine("[Error] No active Pokémon data from server.");
            Console.ReadKey(true);
            _initialized = false;
            _sm.SwitchTo(ScreenType.MainMenu);
            return;
        }

        ShowMoveMenu(myActive);

        // 60-second timer for move selection
        var deadline = DateTime.UtcNow.AddSeconds(60);
        int selectedMove = -1;

        while (DateTime.UtcNow < deadline)
        {
            int remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
            Console.Write($"\r  Select (Time left: {remaining}s): ");

            if (Console.KeyAvailable)
            {
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int moveChoice)
                    && moveChoice >= 1 && moveChoice <= myActive.Moves.Count)
                {
                    selectedMove = moveChoice - 1;
                    break;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [!] Invalid input. Please enter a number from 1 to {myActive.Moves.Count}.");
                Console.ResetColor();
            }
            Thread.Sleep(200);
        }

        if (selectedMove < 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n  [System] Time's up! Your turn has been skipped.");
            Console.ResetColor();
        }

        // Submit move to server (fire-and-forget, don't block the thread)
        Console.WriteLine("\n  [Network] Sending move to server...");
        _waitingForResult = true;
        _ = _network.SubmitMoveAsync(_roomId, _session.CurrentUser!.Username, selectedMove, _session.Player!.PokemonTeam[0]);

        // Update() returns → game loop calls Update() again → sees _waitingForResult=true → shows "Waiting..."
        // When server responds → HandleTurnResult sets _waitingForResult=false and _lastResult
        // → next Update() renders updated HP
    }

    #region Rendering Methods (mirroring BattleScreen style, no EXP)

    private void RenderBattleScene(PvPTurnResult result)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("------------------------------------------------------------");

        // Render opponent's Pokémon first (top, like enemy in BattleScreen)
        RenderOpponentPokemon(result.Player2Active);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n                       ( VS )\n");

        // Render player's Pokémon second (bottom, like player in BattleScreen)
        RenderPlayerPokemon(result.Player1Active);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("------------------------------------------------------------");
        Console.ResetColor();
    }

    private void RenderPlayerPokemon(PvPMonState? mon)
    {
        if (mon == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [No Pokémon data]");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  {mon.Name.ToUpper()} [HP: {mon.CurrentHP}/{mon.MaxHP}]");
        Console.Write("  HP:  ");
        PrintHealthBar(mon.CurrentHP, mon.MaxHP);
        Console.WriteLine($" {mon.CurrentHP}/{mon.MaxHP}");
        Console.ResetColor();
    }

    private void RenderOpponentPokemon(PvPMonState? mon)
    {
        if (mon == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{"",30}[No Pokémon data]");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{"",30}{mon.Name.ToUpper()} [HP: {mon.CurrentHP}/{mon.MaxHP}]");
        Console.Write($"{"",30}HP: ");
        PrintHealthBar(mon.CurrentHP, mon.MaxHP);
        Console.WriteLine($" {mon.CurrentHP}/{mon.MaxHP}");
        Console.ResetColor();
    }

    private void PrintHealthBar(int current, int max)
    {
        if (max <= 0) max = 1;
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

    private void ShowMoveMenu(PvPMonState myActive)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($"  What will {myActive.Name} do?");
        Console.ForegroundColor = ConsoleColor.Yellow;

        for (int i = 0; i < myActive.Moves.Count; i++)
        {
            var move = myActive.Moves[i];
            Console.WriteLine($"  [{i + 1}] {move.Name} (Type: {move.Type}, Power: {move.Power})");
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
        Console.Write("  Select: ");
    }

    #endregion
}