namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Init File Service
            FileService fileService = new FileService();

            // Init Repo -> get Data first
            UserRepository userRepo = new UserRepository(fileService);
            PokedexRepository pokedexRepo = new PokedexRepository(fileService);
            ItemRepository itemRepository = new ItemRepository(fileService);

            GameSession gameSession = new GameSession(userRepo);
            PokemonSpawner pokemonSpawner = new PokemonSpawner(pokedexRepo);
            ItemSpawner itemSpawner = new ItemSpawner(itemRepository);

            // Init Services
            ScreenManager screenManager = new ScreenManager();
            WorldGenerationService worldGenerationService = new WorldGenerationService(gameSession, pokemonSpawner, itemSpawner);
            AuthenService authenService = new AuthenService(userRepo, gameSession);
            StoryService storyService = new StoryService(gameSession, pokemonSpawner, itemSpawner);
            ExpService expService = new ExpService(gameSession, worldGenerationService);
            BattleService battleService = new BattleService(gameSession, userRepo, worldGenerationService, expService);
            CatchService catchService = new CatchService(gameSession);
            HealingService healingService = new HealingService(gameSession);

            // Init Application
            // Register Screens
            screenManager.RegisterScreen(ScreenType.Login, new LoginScreen(screenManager, gameSession, authenService));
            screenManager.RegisterScreen(ScreenType.MainMenu, new MainMenuScreen(screenManager, gameSession));
            screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, storyService));
            screenManager.RegisterScreen(ScreenType.Battle, new BattleScreen(screenManager, gameSession, battleService));
            screenManager.RegisterScreen(ScreenType.Inventory, new InventoryScreen(screenManager, gameSession, catchService, healingService));

            // Run Application
            screenManager.Run();
        }
    }
}
