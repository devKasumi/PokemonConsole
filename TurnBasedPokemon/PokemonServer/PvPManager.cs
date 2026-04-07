using System.Collections.Concurrent;

public class PvPManager
{
    // Thread-safe dictionary to hold pending moves for each battle room
    private readonly ConcurrentDictionary<string, List<BattleActionRequest>> _pendingMoves = new();

    public void RegisterMove(BattleActionRequest request)
    {
        var moves = _pendingMoves.GetOrAdd(request.RoomId, _ => new List<BattleActionRequest>());
        
        lock (moves)
        {
            // Prevent players from spamming requests
            if (!moves.Any(m => m.PlayerName == request.PlayerName))
            {
                moves.Add(request);
            }
        }
    }

    public bool IsRoomReady(string roomId)
    {
        return _pendingMoves.TryGetValue(roomId, out var moves) && moves.Count == 2;
    }

    public BattleTurnResult ProcessTurn(string roomId)
    {
        _pendingMoves.TryGetValue(roomId, out var moves);
        var result = new BattleTurnResult();

        // --- AUTHORITATIVE CALCULATION HAPPENS HERE ---
        // 1. Sort by Speed (Who attacks first?)
        // 2. Calculate damage using Shared DamageCalculator
        // 3. Update HP in the server's memory

        // Mock data for testing the flow:
        result.CombatLogs.Add($"[Server] {moves[0].PlayerName} used Move {moves[0].MoveIndex}!");
        result.CombatLogs.Add($"[Server] {moves[1].PlayerName} used Move {moves[1].MoveIndex}!");
        
        result.Player1Hp = 80; // Replace with actual calculated HP
        result.Player2Hp = 60; // Replace with actual calculated HP
        result.IsGameOver = false;

        // Clear the buffer for the next turn
        _pendingMoves.TryRemove(roomId, out _);

        return result;
    }
}