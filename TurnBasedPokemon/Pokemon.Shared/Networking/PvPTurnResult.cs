using System.Collections.Generic;

// Using 'record' is excellent for DTOs (Data Transfer Objects) sent over the network!
public record PvPTurnResult
{
    public bool IsGameOver { get; init; }
    public string? WinnerName { get; init; }
    public int Player1Hp { get; init; }
    public int Player2Hp { get; init; }
    public List<string> CombatLogs { get; init; } = new();

    // New: Full state for both active Pokémon (nullable for default constructor)
    public PvPMonState? Player1Active { get; init; }
    public PvPMonState? Player2Active { get; init; }

    public PvPTurnResult() { }

    public static PvPTurnResult Continue(
        int p1Hp, int p2Hp, List<string> logs,
        PvPMonState? p1Active, PvPMonState? p2Active)
        => new PvPTurnResult
        {
            IsGameOver = false,
            Player1Hp = p1Hp,
            Player2Hp = p2Hp,
            CombatLogs = logs,
            Player1Active = p1Active,
            Player2Active = p2Active
        };

    public static PvPTurnResult GameOver(
        string winnerName, List<string> logs,
        PvPMonState? p1Active, PvPMonState? p2Active)
        => new PvPTurnResult
        {
            IsGameOver = true,
            WinnerName = winnerName,
            CombatLogs = logs,
            Player1Active = p1Active,
            Player2Active = p2Active
        };
}

public record PvPMonState
{
    public string Name { get; init; } = string.Empty;
    public int CurrentHP { get; init; }
    public int MaxHP { get; init; }
    public bool IsFainted { get; init; }
    public List<PvPMoveState> Moves { get; init; } = new();
}

public record PvPMoveState
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int Power { get; init; }
    public int PP { get; init; }
}