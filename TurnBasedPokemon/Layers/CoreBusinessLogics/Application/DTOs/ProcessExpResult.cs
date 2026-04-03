public record ProcessExpResult(
    int ExpGained, 
    bool LeveledUp,
    string? OldName,
    string? EvolutionName,
    List<string> LearnedMoves
)
{
    public bool HasEvolved => !string.IsNullOrEmpty(EvolutionName);
}