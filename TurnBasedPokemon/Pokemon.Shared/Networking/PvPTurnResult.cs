using System.Collections.Generic;

// Using 'record' is excellent for DTOs (Data Transfer Objects) sent over the network!
public record PvPTurnResult
{
    // Absolute perspective: Use ConnectionId or Player Name instead of relative "Player/Enemy"
    public bool IsGameOver { get; init; }
    public string? WinnerName { get; init; }
    
    // The server must synchronize HP to both clients to prevent desync or memory hacking
    public int Player1Hp { get; init; } 
    public int Player2Hp { get; init; }

    public List<string> CombatLogs { get; init; } = new();

    // Private constructor forces the use of Factory Methods
    private PvPTurnResult() { }

    // Factory: The battle continues
    public static PvPTurnResult Continue(int p1Hp, int p2Hp, List<string> logs) 
        => new PvPTurnResult 
        { 
            IsGameOver = false, 
            Player1Hp = p1Hp, 
            Player2Hp = p2Hp, 
            CombatLogs = logs 
        };

    // Factory: Someone fainted / The match has ended
    public static PvPTurnResult GameOver(string winnerName, List<string> logs) 
        => new PvPTurnResult 
        { 
            IsGameOver = true, 
            WinnerName = winnerName, 
            CombatLogs = logs 
        };
}