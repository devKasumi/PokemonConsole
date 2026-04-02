namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init Repo -> get Data first
            UserRepository userRepo = new UserRepository();
            PokedexRepository pokedexRepo = new PokedexRepository();
            pokedexRepo.LoadData();
            ItemRepository itemRepository = new ItemRepository();
            itemRepository.LoadData();

            GameSession gameSession = new GameSession();
            PokemonSpawner pokemonSpawner = new PokemonSpawner(pokedexRepo);
            ItemSpawner itemSpawner = new ItemSpawner(itemRepository);

            // Init Services
            ScreenManager screenManager = new ScreenManager();
            WorldGenerationService worldGenerationService = new WorldGenerationService(gameSession, pokemonSpawner, itemSpawner);
            AuthenService authenService = new AuthenService(userRepo, gameSession);
            StoryService storyService = new StoryService(gameSession, pokemonSpawner, itemSpawner);
            BattleService battleService = new BattleService(gameSession, userRepo, worldGenerationService);
            CatchService catchService = new CatchService(gameSession);
            HealingService healingService = new HealingService(gameSession);

            // init Application
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
