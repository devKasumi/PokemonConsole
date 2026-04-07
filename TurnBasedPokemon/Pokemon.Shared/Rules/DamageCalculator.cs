using PokemonEntity;

public static class DamageCalculator
{
    private static readonly Random _rng = new Random();

    public static int CalculateDamage(Pokemon attacker, Pokemon defender, PokemonMove pokemonMove)
    {
        int level = attacker.Level;

        bool isCritical = _rng.Next(0, 16) == 0; 
        int critical = isCritical ? 2 : 1;

        int attackDamae = pokemonMove.MoveType == MoveType.Physical ? attacker.Specie.BaseStats.Attack : attacker.Specie.BaseStats.SpAttack;
        int defense = pokemonMove.MoveType == MoveType.Physical ? defender.Specie.BaseStats.Defense : defender.Specie.BaseStats.SpDefense;

        float baseDamage = ((2 * level * critical / 5 + 2) * pokemonMove.Power * attackDamae / defense) / 50 + 2;

        // --- FIX STAB (Same Type Attack Bonus) ---
        // Check if the move type matches any of the attacker's types safely
        float stab = 1.0f;
        foreach (var type in attacker.Specie.Types)
        {
            if (pokemonMove.Type == type)
            {
                stab = 1.5f;
                break;
            }
        }

        // --- FIX TYPE EFFECTIVENESS ---
        float typeMultiplier = 1.0f;
        
        // Always check the first type
        if (defender.Specie.Types.Count > 0)
        {
            typeMultiplier *= TypeChart.GetEffectivenessMultiplier(pokemonMove.Type, defender.Specie.Types[0]);
        }

        // Only check the second type if it exists
        if (defender.Specie.Types.Count > 1)
        {
            typeMultiplier *= TypeChart.GetEffectivenessMultiplier(pokemonMove.Type, defender.Specie.Types[1]);
        }

        int rand = _rng.Next(217, 256);
        float randomeFactor = rand / 255f;

        int finalDamage = (int)(baseDamage * stab * typeMultiplier * randomeFactor);

        if (finalDamage < 1) finalDamage = 1;

        // Show messages
        if (isCritical) Console.WriteLine("A critical hit!");
        if (typeMultiplier > 1) Console.WriteLine("It's super effective!");
        else if (typeMultiplier < 1) Console.WriteLine("It's not very effective...");

        return finalDamage;
    }
}