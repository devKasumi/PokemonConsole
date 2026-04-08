using System;
using System.Threading;
using Screens;

public class PvPScreen : IScreen
{
    private readonly ScreenManager _sm;
    private readonly GameSession _session;
    private readonly IPvPService _pvpService;
    
    private string _roomId = "";
    private volatile PvPTurnResult? _lastResult = null;
    private bool _initialized = false;
    private volatile bool _waitingForResult = false;

    public PvPScreen(ScreenManager sm, GameSession session, IPvPService pvpService)
    {
        _sm = sm;
        _session = session;
        _pvpService = pvpService;
    }

    public void Initialize(object? data = null)
    {
        if (_initialized) return;
        _initialized = true;

        _lastResult = null;
        _waitingForResult = false;
        
        if (data != null)
        {
            _roomId = data.GetType().GetProperty("RoomId")?.GetValue(data)?.ToString() ?? "";
        }

        _pvpService.SubscribeTurnResult(HandleTurnResult);

        if (_pvpService.LastTurnResult != null)
        {
            _lastResult = _pvpService.LastTurnResult;
        }
    }

    public void Shutdown()
    {
        _pvpService.UnsubscribeTurnResult(HandleTurnResult);
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

        if (!ValidateRoom()) return;

        RenderRoomHeader();

        var result = _lastResult;
        bool waiting = _waitingForResult;

        if (result == null)
        {
            Console.WriteLine("\n[System] Waiting for battle data from server...");
            Thread.Sleep(500);
            return;
        }

        RenderBattleScene(result);
        RenderCombatLogs(result);

        if (result.IsGameOver) { HandleGameOver(result); return; }
        if (waiting) { HandleWaitingForOpponent(); return; }

        HandlePlayerInput(result);
    }

    private bool ValidateRoom()
    {
        if (!string.IsNullOrEmpty(_roomId)) return true;

        Console.WriteLine("[Error] You are not in a PvP room. Returning to Main Menu.");
        Console.ReadKey(true);
        _sm.SwitchTo(ScreenType.MainMenu);
        return false;
    }

    private void RenderRoomHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine("                   [ PVP ARENA ]");
        Console.WriteLine($"                   Room: {_roomId}");
        Console.WriteLine("============================================================");
        Console.ResetColor();
    }

    private void RenderCombatLogs(PvPTurnResult result)
    {
        if (result.CombatLogs.Count == 0) return;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("------------------------------------------------------------");
        foreach (var log in result.CombatLogs)
        {
            Console.WriteLine($"  > {log}");
        }
        Console.WriteLine("------------------------------------------------------------");
        Console.ResetColor();
    }

    private void HandleGameOver(PvPTurnResult result)
    {
        bool isWin = _pvpService.RecordMatchResult(result.WinnerName ?? "");

        Console.WriteLine();
        Console.ForegroundColor = isWin ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(isWin
            ? $"  ★ VICTORY! You won the match! ★"
            : $"  ✖ DEFEAT! {result.WinnerName} won the match. ✖");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Record: {_session.Player?.PvPWins}W - {_session.Player?.PvPLosses}L");
        Console.ResetColor();
        Console.WriteLine("\n  Press any key to return to Main Menu...");
        Console.ReadKey(true);
        _initialized = false;
        _sm.SwitchTo(ScreenType.MainMenu);
    }

    private void HandleWaitingForOpponent()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\n  [System] Waiting for opponent to make a move...");
        Console.ResetColor();
        Thread.Sleep(500);
    }

    private void HandlePlayerInput(PvPTurnResult result)
    {
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
        int selectedMove = ReadMoveSelection(myActive.Moves.Count);
        SubmitMove(selectedMove);
    }

    private int ReadMoveSelection(int moveCount)
    {
        var deadline = DateTime.UtcNow.AddSeconds(60);

        while (DateTime.UtcNow < deadline)
        {
            int remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
            Console.Write($"\r  Select (Time left: {remaining}s): ");

            if (Console.KeyAvailable)
            {
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int moveChoice)
                    && moveChoice >= 1 && moveChoice <= moveCount)
                {
                    return moveChoice - 1;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [!] Invalid input. Please enter a number from 1 to {moveCount}.");
                Console.ResetColor();
            }
            Thread.Sleep(200);
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\n  [System] Time's up! Your turn has been skipped.");
        Console.ResetColor();
        return -1;
    }

    private void SubmitMove(int selectedMove)
    {
        Console.WriteLine("\n  [Network] Sending move to server...");
        _waitingForResult = true;
        _pvpService.SubmitMove(_roomId, selectedMove);
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