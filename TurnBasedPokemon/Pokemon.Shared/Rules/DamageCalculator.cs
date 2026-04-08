using PokemonEntity;

/// <summary>
/// Result of a damage calculation, including the final damage and any combat messages.
/// </summary>
public record DamageResult(int Damage, bool IsCritical, float TypeMultiplier)
{
    public bool IsImmune => TypeMultiplier == 0f;
    public bool IsSuperEffective => TypeMultiplier > 1f;
    public bool IsNotVeryEffective => TypeMultiplier > 0f && TypeMultiplier < 1f;
}

public static class DamageCalculator
{
    private static readonly Random _rng = new Random();

    /// <summary>
    /// Calculates damage and returns a DamageResult with metadata (no Console output).
    /// </summary>
    public static DamageResult Calculate(Pokemon attacker, Pokemon defender, PokemonMove pokemonMove)
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

        return new DamageResult(finalDamage, isCritical, typeMultiplier);
    }

    /// <summary>
    /// Legacy wrapper: calculates damage and returns only the int value.
    /// </summary>
    public static int CalculateDamage(Pokemon attacker, Pokemon defender, PokemonMove pokemonMove)
    {
        return Calculate(attacker, defender, pokemonMove).Damage;
    }
}