public static class CatchFormula
{
    public static bool AttemptCatch(Pokemon pokemon, Item pokeball)
    {
        // Simple catch formula based on Pokemon's current HP and the type of Pokeball used
        float catchRate = (float)(pokemon.MaxHP - pokemon.CurrentHP) / pokemon.MaxHP;
        
        // Modify catch rate based on Pokeball type (for simplicity, we assume all Pokeballs have the same effect)
        catchRate *= 1.0f; // No modification for now
        
        // Generate a random number between 0 and 1
        Random rand = new Random();
        float randomValue = (float)rand.NextDouble();
        
        return randomValue < catchRate;
    }
}