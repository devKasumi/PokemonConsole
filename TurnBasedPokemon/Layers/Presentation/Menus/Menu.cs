public static class Menu
{
    public static void LoginMenu()
    {
        Console.WriteLine("=== POKEMON ECLIPSE OF LEGENDS ===\n");
        Console.WriteLine("Welcome to the Pokemon Game!\n");
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Register");
        Console.WriteLine("3. Exit\n\n");
        Console.Write("Select: ");
    }

    public static void MainMenu()
    {
        Console.WriteLine("Welcome to the Pokemon Game!");
        Console.WriteLine("1. Start New Game");
        Console.WriteLine("2. Load Game");
        Console.WriteLine("3. Match Making (PvP)");
        Console.WriteLine("4. Save Game");
        Console.WriteLine("5. Back to Login Menu");
        Console.Write("Select: ");
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
        // Console.WriteLine($" What will {_playerActivePokemon.Name} do?");
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
}