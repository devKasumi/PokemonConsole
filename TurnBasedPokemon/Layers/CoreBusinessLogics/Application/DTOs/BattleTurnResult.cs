public record BattleTurnResult
{
    public bool IsEnemyFainted { get; init; }
    public bool IsPlayerFainted { get; init; }
    public string? MoveUsed { get; init; }
    public string? Message { get; init; }

    private BattleTurnResult() { }

    public static BattleTurnResult Continue() 
        => new BattleTurnResult { IsEnemyFainted = false, IsPlayerFainted = false };

    public static BattleTurnResult Continue(string moveName) 
        => new BattleTurnResult { IsEnemyFainted = false, IsPlayerFainted = false, MoveUsed = moveName };

    public static BattleTurnResult EnemyFainted() 
        => new BattleTurnResult { IsEnemyFainted = true };

    public static BattleTurnResult PlayerFainted() 
        => new BattleTurnResult { IsPlayerFainted = true };
}