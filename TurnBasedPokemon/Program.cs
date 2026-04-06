using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {   
            // 1. Get the database connection string from gamesettings.json
            string dbConnection = new ConfigurationBuilder()
                                    .AddJsonFile("gamesettings.json")
                                    .Build()
                                    .GetConnectionString("PokemonDb") 
                                    ?? throw new InvalidOperationException("Connection string 'PokemonDb' not found.");
            
            // 2. Initialize Repositories (Shared between the Seeder and the Game)
            MySqlPokedexRepository mySqlPokedexRepo = new MySqlPokedexRepository(dbConnection);
            MySqlUserRepository mySqlUserRepo = new MySqlUserRepository(mySqlPokedexRepo, dbConnection);
            MySqlItemRepository mySqlItemRepo = new MySqlItemRepository(dbConnection);

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
            FileService fileService = new FileService(); // Retained in case of legacy dependencies

            GameSession gameSession = new GameSession(mySqlUserRepo);
            PokemonSpawner pokemonSpawner = new PokemonSpawner(mySqlPokedexRepo);
            ItemSpawner itemSpawner = new ItemSpawner(mySqlItemRepo);

            MySqlStoryService mySqlStoryService = new MySqlStoryService(gameSession, pokemonSpawner, itemSpawner, dbConnection);

            ScreenManager screenManager = new ScreenManager();
            WorldGenerationService worldGenerationService = new WorldGenerationService(gameSession, pokemonSpawner, itemSpawner);
            AuthenService authenService = new AuthenService(mySqlUserRepo, gameSession);
            ExpService expService = new ExpService(gameSession, worldGenerationService);
            BattleService battleService = new BattleService(gameSession, mySqlUserRepo, worldGenerationService, expService);
            CatchService catchService = new CatchService(gameSession);
            HealingService healingService = new HealingService(gameSession);

            // 4. Register Screens for the UI
            screenManager.RegisterScreen(ScreenType.Login, new LoginScreen(screenManager, gameSession, authenService));
            screenManager.RegisterScreen(ScreenType.MainMenu, new MainMenuScreen(screenManager, gameSession));
            screenManager.RegisterScreen(ScreenType.Story, new StoryScreen(screenManager, gameSession, mySqlStoryService));
            screenManager.RegisterScreen(ScreenType.Battle, new BattleScreen(screenManager, gameSession, battleService));
            screenManager.RegisterScreen(ScreenType.Inventory, new InventoryScreen(screenManager, gameSession, catchService, healingService));

            // 5. Run Application
            screenManager.Run();
        }
    }
}