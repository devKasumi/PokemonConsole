public record BattleTurnResult
{
    public bool IsEnemyFainted { get; init; }
    public bool IsPlayerFainted { get; init; }
    public string? MoveUsed { get; init; }
    
    // Upgrade: Using a List to store the complete sequence of events in a single turn
    public List<string> CombatLogs { get; init; } = new();

    // Private constructor to enforce the use of Factory Methods
    private BattleTurnResult() { }

    // Factory: Nothing happened (e.g., skipped turn due to paralysis/sleep)
    public static BattleTurnResult Continue() 
        => new BattleTurnResult { IsEnemyFainted = false, IsPlayerFainted = false };

    // Factory: Normal attack execution, passing the sequence of combat logs
    public static BattleTurnResult Continue(string moveName, List<string> logs) 
        => new BattleTurnResult 
        { 
            IsEnemyFainted = false, 
            IsPlayerFainted = false, 
            MoveUsed = moveName, 
            CombatLogs = logs 
        };

    // Factory: The enemy Pokemon fainted
    public static BattleTurnResult EnemyFainted(List<string> logs) 
        => new BattleTurnResult 
        { 
            IsEnemyFainted = true, 
            CombatLogs = logs 
        };

    // Factory: The player's Pokemon fainted
    public static BattleTurnResult PlayerFainted(List<string> logs) 
        => new BattleTurnResult 
        { 
            IsPlayerFainted = true, 
            CombatLogs = logs 
        };
}