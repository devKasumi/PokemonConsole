using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Screens;
using Application.RepoInterfaces;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {   
            FileService fileService = new FileService(); // Used to read gamesettings.json for the DB connection string
            // 1. Get the database connection string from gamesettings.json
            string dbConnection = new ConfigurationBuilder()
                                    .AddJsonFile("gamesettings.json")
                                    .Build()
                                    .GetConnectionString("PokemonDb") 
                                    ?? throw new InvalidOperationException("Connection string 'PokemonDb' not found.");
            
            // 2. Initialize Repositories (Shared between the Seeder and the Game)
            IPokedexRepository pokedexRepository = new PokedexRepository(fileService); // Used by the seeder to read pokemon.json
            IUserRepository userRepository = new UserRepository(fileService); // Used by the seeder to create users with starter pokemon
            IItemRepository itemRepository = new ItemRepository(fileService); // Used by the seeder
            IPokedexRepository mySqlPokedexRepo = new MySqlPokedexRepository(dbConnection);
            IUserRepository mySqlUserRepo = new MySqlUserRepository(mySqlPokedexRepo, dbConnection);
            IItemRepository mySqlItemRepo = new MySqlItemRepository(dbConnection);

            // ==========================================
            // PROCESS DATABASE SEEDING VIA COMMAND LINE
            // ==========================================
            if (args.Contains("--seed"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Detected '--seed' argument. Starting Database Seeder...");
                Console.ResetColor();

                var seeder = new DatabaseSeeder(mySqlUserRepo, dbConnection);
                seeder.SeedAll();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Seeding completed successfully! Exiting program.");
                Console.ResetColor();
                
                return; // Stop execution here, prevent the Game from loading into memory
            }
            // ==========================================


            // 3. Initialize the remaining Core Services to run the Game normally
            // Create a single NetworkService instance for the whole app
            var networkService = new NetworkService();
            // GameSession gameSession = new GameSession(mySqlUserRepo);
            GameSession gameSession = new GameSession();
            PokemonSpawner pokemonSpawner = new PokemonSpawner(mySqlPokedexRepo);
            // PokemonSpawner pokemonSpawner = new PokemonSpawner(pokedexRepository);
            // ItemSpawner itemSpawner = new ItemSpawner(itemRepository);
            ItemSpawner itemSpawner = new ItemSpawner(mySqlItemRepo);

            IStoryRepository storyRepository = new JsonStoryRepository();
            IStoryService storyService = new StoryService(gameSession, pokemonSpawner, itemSpawner, storyRepository);

            ScreenManager screenManager = new ScreenManager();
            IWorldGenerationService worldGenerationService = new WorldGenerationService(gameSession, pokemonSpawner, itemSpawner);
            AuthenService authenService = new AuthenService(mySqlUserRepo, gameSession);
            // IAuthenService authenService = new AuthenService(userRepository, gameSession);
            var pvpService = new PvPService(gameSession, networkService, authenService);
            IExpService expService = new ExpService(gameSession, worldGenerationService);
            // IBattleService battleService = new BattleService(gameSession, userRepository, worldGenerationService, expService);
            BattleService battleService = new BattleService(gameSession, mySqlUserRepo, worldGenerationService, expService);
            ICatchService catchService = new CatchService(gameSession);
            IHealingService healingService = new HealingService(gameSession);

            // 4. Register Screens for the UI
            screenManager.RegisterScreen(ScreenType.Login, new LoginScreen(screenManager, gameSession, authenService));
            screenManager.RegisterScreen(ScreenType.MainMenu, new MainMenuScreen(screenManager, gameSession, pvpService, authenService));
            // screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, mySqlStoryService));
            screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, storyService, authenService));
            screenManager.RegisterScreen(ScreenType.Battle, new BattleScreen(screenManager, gameSession, battleService));
            screenManager.RegisterScreen(ScreenType.Inventory, new InventoryScreen(screenManager, gameSession, catchService, healingService));
            screenManager.RegisterScreen(ScreenType.PvP, new PvPScreen(screenManager, gameSession, pvpService));

            // 5. Run Application
            screenManager.Run();
        }
    }
}