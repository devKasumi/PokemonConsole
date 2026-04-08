using PokemonEntity;

public static class DamageCalculator
{
    private static readonly Random _rng = new Random();

    public static int CalculateDamage(Pokemon attacker, Pokemon defender, PokemonMove pokemonMove)
    {
        int level = attacker.Level;

        bool isCritical = _rng.Next(0, 16) == 0; 
        int critical = isCritical ? 2 : 1;

        int attackStat = pokemonMove.MoveType == MoveType.Physical ? attacker.Specie.BaseStats.Attack : attacker.Specie.BaseStats.SpAttack;
        int defense = pokemonMove.MoveType == MoveType.Physical ? defender.Specie.BaseStats.Defense : defender.Specie.BaseStats.SpDefense;

        // Use float arithmetic to avoid integer truncation at low levels
        float baseDamage = ((2f * level * critical / 5f + 2f) * pokemonMove.Power * (float)attackStat / defense) / 50f + 2f;

        // --- STAB (Same Type Attack Bonus) ---
        float stab = 1.0f;
        foreach (var type in attacker.Specie.Types)
        {
            if (pokemonMove.Type == type)
            {
                stab = 1.5f;
                break;
            }
        }

        // --- TYPE EFFECTIVENESS ---
        float typeMultiplier = 1.0f;
        foreach (var defType in defender.Specie.Types)
        {
            typeMultiplier *= TypeChart.GetEffectivenessMultiplier(pokemonMove.Type, defType);
        }

        int rand = _rng.Next(217, 256);
        float randomFactor = rand / 255f;

        int finalDamage = (int)(baseDamage * stab * typeMultiplier * randomFactor);

        // Minimum damage: at least 1 (unless immune)
        if (typeMultiplier == 0f)
            finalDamage = 0;
        else if (finalDamage < 1)
            finalDamage = 1;

        // Show messages
        if (isCritical) Console.WriteLine("A critical hit!");
        if (typeMultiplier == 0f) Console.WriteLine("It doesn't affect the opponent...");
        else if (typeMultiplier > 1f) Console.WriteLine("It's super effective!");
        else if (typeMultiplier < 1f) Console.WriteLine("It's not very effective...");

        return finalDamage;
    }
}