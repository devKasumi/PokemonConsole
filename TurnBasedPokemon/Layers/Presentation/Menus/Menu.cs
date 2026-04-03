public static class Menu
{
    public static void LoginMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine("              POKEMON: ECLIPSE OF LEGENDS              ");
        Console.WriteLine("============================================================");
        
        // ASCII Art for "POKEMON"
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"          ___          _                                ");
        Console.WriteLine(@"         | _ \ ___  __| | ___  _ __  ___  _ _           ");
        Console.WriteLine(@"         |  _// _ \/ _| |/ -_)| '  \/ _ \| ' \          ");
        Console.WriteLine(@"         |_|  \___/\__|_|\___||_|_|_\___/|_||_          ");
        Console.WriteLine(@"                                                        ");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        
        Console.WriteLine("\n  Your journey begins here!");
        Console.WriteLine();
        Console.WriteLine("  [1] CONTINUE JOURNEY (Login)");
        Console.WriteLine("  [2] BEGIN NEW ADVENTURE (Register)");
        Console.WriteLine("  [3] EXIT GAME");
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.Write("  Select your path (1-3): ");
    }

    public static void MainMenu()
    {
        Console.Clear();
    
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine("              CENTRAL HUB - TRAINER TERMINAL              ");
        Console.WriteLine("============================================================");
        
        Console.ForegroundColor = ConsoleColor.White;
        // string location = _session.Player?.CurrentLocation ?? "Unknown";
        // int badges = _session.Player?.Badges?.Count ?? 0;
        Console.WriteLine($"  Welcome back, Trainer!");
        // Console.WriteLine($"  Location: [ {location} ] | Badges: [ {badges}/8 ]");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();

        PrintOption("1", "START NEW JOURNEY", "Reset progress and start from the beginning", ConsoleColor.Yellow);
        PrintOption("2", "CONTINUE ADVENTURE", "Resume your journey from the last save point", ConsoleColor.Green);
        PrintOption("3", "ENTER BATTLE ARENA (PvP)", "Test your skills against other trainers", ConsoleColor.Magenta);
        PrintOption("4", "SYNC DATA (Save Game)", "Secure your current progress", ConsoleColor.Blue);
        PrintOption("5", "LOGOUT", "Return to the title screen", ConsoleColor.Gray);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        Console.Write("  Select Action: ");
    }

    public static void PokemonCenterMenu()
    {
        Console.WriteLine("=== POKEMON CENTER MENU ===");
        Console.WriteLine("1. Heal Pokemon");
        Console.WriteLine("2. Access PC (Manage Pokemon - Comming Soon)");
        Console.WriteLine("3. Goodbye (Leave Pokemon Center)");
        Console.Write("Select: ");
    }

    public static void BattleMenu()
    {
        Console.SetCursorPosition(0, 13);
        Console.WriteLine(new string('═', 60));
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("  1. FIGHT          2. BAG");
        Console.WriteLine("  3. POKEMON        4. RUN");
        Console.WriteLine(new string('═', 60));
    }

    public static void InventoryMenu()
    {
        Console.WriteLine("=== Inventory ===");
        Console.WriteLine("1. Pokeballs");
        Console.WriteLine("2. Potions");
        Console.WriteLine("3. Back to Battle Menu");
        Console.Write("Select: ");
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine($"  {title.ToUpper()}");
        Console.WriteLine("============================================================");
    }

    private static void PrintOption(string key, string title, string description, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"  [{key}] ◈ {title}\n");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"      ({description})");
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void StarterSelectionMenu()
    {
        Console.Clear();
    
        // Header
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.WriteLine("              PROFESSOR OAK'S LABORATORY              ");
        Console.WriteLine("============================================================");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" \"Hello there! It's time to choose your very first partner.");
        Console.WriteLine("  Each one has its own unique strength. Choose wisely!\"");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();

        // Option 1: Bulbasaur
        PrintStarterOption("1", "🍃 BULBASAUR", "Grass Type", 
            "A strange seed was planted on its back at birth.", ConsoleColor.Green);

        // Option 2: Charmander
        PrintStarterOption("2", "🔥 CHARMANDER", "Fire Type", 
            "The flame on its tail shows its life force.", ConsoleColor.Red);

        // Option 3: Squirtle
        PrintStarterOption("3", "💧 SQUIRTLE", "Water Type", 
            "Shoots water at prey from inside its shell.", ConsoleColor.Blue);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        Console.Write("  Which Pokemon will you journey with? (1-3): ");
    }

    private static void PrintStarterOption(string key, string name, string type, string description, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"  [{key}] {name.PadRight(12)} ({type})");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"      \"{description}\"");
        Console.WriteLine();
        Console.ResetColor();
    }
}