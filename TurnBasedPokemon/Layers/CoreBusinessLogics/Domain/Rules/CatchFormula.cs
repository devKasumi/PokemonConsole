using PokemonEntity;

// public record CatchResult(bool IsCaught, int Shakes);
public static class CatchFormula
{
    private static readonly Random _rand = new Random();

    public static CatchResult AttemptCatch(Pokemon target, CaptureItem ball)
    {
        // 1. If a Master Ball is used, the Pokémon is caught.
        if (ball.Name == "Master Ball") return new CatchResult{IsCaught = true, Shakes = 3};

        // 2. Generate a random number, N, depending on the type of ball used.
        int nMax = ball.Name switch { "Poke Ball" => 255, "Great Ball" => 200, _ => 150 };
        int N = _rand.Next(0, nMax + 1);

        // 3. Status Threshold
        int statusThreshold = target.Status switch {
            PokemonStatus.Asleep or PokemonStatus.Frozen => 25,
            PokemonStatus.Paralyzed or PokemonStatus.Burned or PokemonStatus.Poisoned => 12,
            _ => 0
        };

        bool brokeFreeEarly = false;

        // 4. The Pokémon is caught if... (Status check)
        if (statusThreshold > 0 && N < statusThreshold)
        {
            return new CatchResult{IsCaught = true, Shakes = 3}; // Caught!
        }
        // Otherwise, if N minus the status threshold is greater than catch rate, it breaks free.
        else if ((N - statusThreshold) > target.Specie.CatchRate)
        {
            brokeFreeEarly = true; // Sẽ tính số lần rung ở phần dưới
        }

        // 5. Generate a random value, M, between 0 and 255.
        int M = _rand.Next(0, 256);

        // 6. Calculate f
        int ballValueForF = (ball.Name == "Great Ball") ? 8 : 12;
        
        // f = floor( (HPmax * 255 * 4) / (HPcurrent * Ball) )
        double fCalc = ((double)target.MaxHP * 255.0 * 4.0) / ((double)target.CurrentHP * ballValueForF);
        int f = (int)Math.Floor(fCalc);
        f = Math.Clamp(f, 1, 255); // The minimum value is 1 and maximum is 255.

        // 7. Final Catch Check
        if (!brokeFreeEarly)
        {
            if (f >= M) return new CatchResult{IsCaught = true, Shakes = 3}; // Caught!
        }

        // ====================================================================
        // IF THE CODE REACHES THIS LINE, IT MEANS A FAILURE. COUNT THE NUMBER OF BALL SHAKES:
        // ====================================================================
        
        // 8. Calculate d: d = floor(catchRate * 100 / Ball)
        int ballValueForShake = ball.Name switch { "Poke Ball" => 255, "Great Ball" => 200, _ => 150 };
        int d = (target.Specie.CatchRate * 100) / ballValueForShake;

        // If d is greater than or equal to 256, the ball shakes three times
        if (d >= 256) return new CatchResult{IsCaught = true, Shakes = 3};

        // 9. Calculate x = floor(d * f / 255) + s
        int s = target.Status switch {
            PokemonStatus.Asleep or PokemonStatus.Frozen => 10,
            PokemonStatus.Paralyzed or PokemonStatus.Burned or PokemonStatus.Poisoned => 5,
            _ => 0
        };

        int x = (d * f / 255) + s;

        // 10. Decide the shakes based on x
        if (x < 10) return new CatchResult{IsCaught = false, Shakes = 0}; // Misses completely
        if (x < 30) return new CatchResult{IsCaught = false, Shakes = 1}; // Shakes once
        if (x < 70) return new CatchResult{IsCaught = false, Shakes = 2}; // Shakes twice
        return new CatchResult{IsCaught = false, Shakes = 3};             // Shakes three times before breaking free
    }
}