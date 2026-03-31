public class CaptureItem : Item
{
    // The multiplier for the catch chance (e.g., PokeBall = 1.0, GreatBall = 1.5)
    public double CatchRateMultiplier { get; set; }

    public override bool Use(Pokemon target)
    {
        // 1. Logic Check: Cannot catch Trainer's Pokemon
        // You'll need to pass the battle context or a flag here
        // if (isTrainerBattle) { Console.WriteLine("You can't steal!"); return false; }

        Console.WriteLine($"You threw a {this.Name}!");

        // 2. Standard Capture Formula (Simplified)
        // Probability = ((MaxHP * 3 - CurrentHP * 2) * CatchRate * Multiplier) / (MaxHP * 3)
        double chance = ((target.MaxHP * 3 - target.CurrentHP * 2) * target.Specie.CatchRate * CatchRateMultiplier) / (target.MaxHP * 3);
        
        // Randomize the result
        Random rand = new Random();
        int shakeCount = 0;

        // Visual feedback for shaking
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(500); 
            if (rand.NextDouble() * 255 < chance) 
                shakeCount++;
            else 
                break;
            
            Console.WriteLine("Shake...");
        }

        if (shakeCount == 3)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Gotcha! {target.Specie.Name} was caught!");
            Console.ResetColor();
            return true; // Successfully caught
        }
        else
        {
            Console.WriteLine($"Oh no! The {target.Specie.Name} broke free!");
            return false; // Failed to catch
        }
    }
}