public class CaptureItem : Item
{
    public double CatchRateMultiplier { get; set; }

    public override bool Use(Pokemon target)
    {
        Console.WriteLine($"\nYou threw a {this.Name}!");

        // Gọi logic từ CatchFormula
        CatchResult result = CatchFormula.AttemptCatch(target, this);
        
        Console.WriteLine($"shake: {result.Shakes}, iscaught: {result.IsCaught}");
        Console.ReadKey(true);

        for (int i = 0; i < result.Shakes; i++)
        {
            Thread.Sleep(1000);
            Console.WriteLine("Shake...");
        }

        Thread.Sleep(500);

        if (result.IsCaught)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Gotcha! {target.Specie.Name} was caught!");
            Console.ResetColor();
            return true;
        }
        else
        {
            if (result.Shakes == 0)
                Console.WriteLine($"Oh no! The ball missed the {target.Specie.Name}!");
            else
                Console.WriteLine($"Aww! It appeared to be caught! {target.Specie.Name} broke free!");
            
            return false;
        }
    }
}