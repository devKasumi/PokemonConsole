namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            PokedexRepository pokedexRepo = new PokedexRepository();
            pokedexRepo.LoadData();

            // Khởi tạo Spawner (Tầng Application)
            PokemonSpawner pokemonSpawner = new PokemonSpawner(pokedexRepo);
            Player player = new Player();
            ScreenManager screenManager = new ScreenManager();
            AuthenService authenService = new AuthenService(new UserRepository());
            PokemonMove pokemonMove = new PokemonMove("Ember", ElementType.Fire, 40, 8, MoveType.Physical);
            screenManager.RegisterScreen(ScreenType.Login, new LoginScreen(screenManager, authenService));
            screenManager.RegisterScreen(ScreenType.MainMenu, new MainMenuScreen(screenManager));
            screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, player, pokemonSpawner));
            // screenManager.RegisterScreen(ScreenType.WildArea, new WildAreaScreen(screenManager));
            screenManager.RegisterScreen(ScreenType.Battle, new BattleScreen(screenManager, player, pokemonSpawner));
            screenManager.RegisterScreen(ScreenType.Inventory, new InventoryScreen(screenManager));
            // screenManager.RegisterScreen(ScreenType.Exit, new ExitScreen(screenManager));
            // Pokemon defense = pokemonSpawner.SpawnPokemon("Venusaur", 34);
            // Pokemon attack = pokemonSpawner.SpawnPokemon("Charizard", 34);
            // int damage = DamageCalculator.CalculateDamage(attack, defense, pokemonMove);
            // Console.WriteLine($"damage: {damage}");


            screenManager.Run();
        }
    }
}
