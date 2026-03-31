public static class ExpCalculator
{
    /// <summary>
    /// Calculate the amount of EXP gained after defeating an opponent.
    /// Formula: EXP = (a * b * L) / (7 * s)
    /// </summary>
    /// <param name="isTrainerBattle">Match with a Trainer (x1.5) or wild Pokemon (x1.0)</param>
    /// <param name="baseExp">Base EXP value of the defeated Pokemon (from Pokedex)</param>
    /// <param name="opponentLevel">Cấp độ của Pokemon bị đánh bại</param>
    /// <param name="opponentLevel">Defeated Pokemon's level</param>
    /// <param name="participantsCount">The amount of Pokemon participating in the battle (default is 1)</param>
    public static int CalculateExpGain(bool isTrainerBattle, int baseExp, int opponentLevel, int participantsCount = 1)
    {
        float a = isTrainerBattle ? 1.5f : 1.0f;
        int b = baseExp;
        int L = opponentLevel;
        int s = participantsCount;

        // Standard formula for EXP gain, ensuring that the result is at least 1
        int expGain = (int)((a * b * L * 3) / (7 * s));

        return expGain > 0 ? expGain : 1; // Minimum EXP gain is always 1
    }

    /// <summary>
    /// Check if the Pokemon has enough EXP to level up.
    /// Use "Medium Fast" experience curve (Exp required = Level^3)
    /// </summary>
    public static bool CanLevelUp(int currentLevel, int totalExp)
    {
        if (currentLevel >= 100) return false;

        int expRequiredForNextLevel = GetRequiredExpForLevel(currentLevel + 1);
        return totalExp >= expRequiredForNextLevel;
    }

    /// <summary>
    /// Calculate the total EXP required to reach a specific level.
    /// </summary>
    public static int GetRequiredExpForLevel(int level)
    {
        // Using "Medium Fast" experience curve: EXP required = Level^3 
        return (int)Math.Pow(level, 1);
        // return 10;
    }
    
    /// <summary>
    /// Calculate the current level progress percentage (for UI progress bar)
    /// </summary>
    public static float GetLevelProgressPercentage(int currentLevel, int totalExp)
    {
        int currentLevelExp = GetRequiredExpForLevel(currentLevel);
        int nextLevelExp = GetRequiredExpForLevel(currentLevel + 1);
        
        float progress = (float)(totalExp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Math.Clamp(progress, 0f, 1f);
    }
}