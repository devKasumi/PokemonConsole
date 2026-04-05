using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {   
            string dbConnection = new ConfigurationBuilder()
                                    .AddJsonFile("gamesettings.json")
                                    .Build()
                                    .GetConnectionString("PokemonDb") 
                                    ?? throw new InvalidOperationException("Connection string 'PokemonDb' not found.");
            
            // Init File Service
            FileService fileService = new FileService();

            // Init Repo -> get Data first
            // UserRepository userRepo = new UserRepository(fileService);
            // PokedexRepository pokedexRepo = new PokedexRepository(fileService);
            // ItemRepository itemRepository = new ItemRepository(fileService);

            MySqlPokedexRepository mySqlPokedexRepo = new MySqlPokedexRepository(dbConnection);
            MySqlUserRepository mySqlUserRepo = new MySqlUserRepository(mySqlPokedexRepo, dbConnection);
            MySqlItemRepository mySqlItemRepo = new MySqlItemRepository(dbConnection);

            // GameSession gameSession = new GameSession(userRepo);
            GameSession gameSession = new GameSession(mySqlUserRepo);
            // PokemonSpawner pokemonSpawner = new PokemonSpawner(pokedexRepo);
            PokemonSpawner pokemonSpawner = new PokemonSpawner(mySqlPokedexRepo);
            // ItemSpawner itemSpawner = new ItemSpawner(itemRepository);
            ItemSpawner itemSpawner = new ItemSpawner(mySqlItemRepo);

            // var seeder = new DatabaseSeeder(mySqlUserRepo);
            // seeder.SeedAll();

            // Init Services
            MySqlStoryService mySqlStoryService = new MySqlStoryService(gameSession, pokemonSpawner, itemSpawner, dbConnection);

            ScreenManager screenManager = new ScreenManager();
            WorldGenerationService worldGenerationService = new WorldGenerationService(gameSession, pokemonSpawner, itemSpawner);
            // AuthenService authenService = new AuthenService(userRepo, gameSession);
            AuthenService authenService = new AuthenService(mySqlUserRepo, gameSession);
            // StoryService storyService = new StoryService(gameSession, pokemonSpawner, itemSpawner);
            ExpService expService = new ExpService(gameSession, worldGenerationService);
            // BattleService battleService = new BattleService(gameSession, userRepo, worldGenerationService, expService);
            BattleService battleService = new BattleService(gameSession, mySqlUserRepo, worldGenerationService, expService);
            CatchService catchService = new CatchService(gameSession);
            HealingService healingService = new HealingService(gameSession);

            // Init Application
            // Register Screens
            screenManager.RegisterScreen(ScreenType.Login, new LoginScreen(screenManager, gameSession, authenService));
            screenManager.RegisterScreen(ScreenType.MainMenu, new MainMenuScreen(screenManager, gameSession));
            // screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, storyService));
            screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, mySqlStoryService));
            screenManager.RegisterScreen(ScreenType.Battle, new BattleScreen(screenManager, gameSession, battleService));
            screenManager.RegisterScreen(ScreenType.Inventory, new InventoryScreen(screenManager, gameSession, catchService, healingService));

            // Run Application
            screenManager.Run();
        }
    }
}
