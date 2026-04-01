using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.VisualBasic;
using Screens;

public class StoryScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private GameData _gameData;
    private Player _player;
    private PokemonSpawner _pokemonSpawner;
    private ItemSpawner _itemSpawner;
    private bool isNewGame;
    private bool _hasPlayedNarrative = false;
    LocationData _currentLocation;

    public StoryScreen(ScreenManager screenManager, Player player, PokemonSpawner pokemonSpawner, ItemSpawner itemSpawner)
    {
        _screenManager = screenManager;
        _player = player;
        _pokemonSpawner = pokemonSpawner;
        _itemSpawner = itemSpawner;
        isNewGame = false;
        LoadData();
    }

    private void LoadData(object? data = null)
    {
        string json = File.ReadAllText("story.json");
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        _gameData = JsonSerializer.Deserialize<GameData>(json, options) ?? new GameData();
    }

    public void Initialize(object? data = null)
    {
        // Console.Clear();

        if (data is string command)
        {
            if (command == "NewGame")
            {
                isNewGame = true;
                ResetPlayerProgress();
                _hasPlayedNarrative = false; // Reset to restart the game from the beginning.
            }
            else if (command == "MainStoryMenu")
            {
                isNewGame = false;
                _hasPlayedNarrative = true; // No running narrative if played
            }
        }
    }

    public void Update()
    {
        if (isNewGame)
        {
            ShowStarterMenu();
        }

        if (!_hasPlayedNarrative)
        {
            PlayCurrentLocationStory();
            _hasPlayedNarrative = true; 
        }
        else
        {
            ShowNpcMenu(_currentLocation.NPCs);
        }
    }

    private void ResetPlayerProgress()
    {
        _player.CurrentPhase = 1;
        _player.CurrentLocation = "";
        _player.PokemonTeam.Clear();
    }

    private void ShowStarterMenu()
    {
        // Console.Clear();
        Console.WriteLine("=== SELECT YOUR STARTER POKEMON ===");
        Console.WriteLine("1. Bulbasaur (Grass Type)");
        Console.WriteLine("2. Charmander (Fire Type)");
        Console.WriteLine("3. Squirtle (Water Type)");
        Console.Write("Select: ");

        string input = Console.ReadLine();
        string selectedName = input switch
        {
            "1" => "Bulbasaur",
            "2" => "Charmander",
            "3" => "Squirtle"
        };

        // CALL SPAWNER: Create a Level 5 Pokemon from the Database
        var starter = _pokemonSpawner.SpawnPokemon(selectedName, 5);

        if (starter != null)
        {
            _player.PokemonTeam.Add(starter); // Add to your pokemon team
            _player.CurrentPokemon = starter;
            // Console.Clear();
            RenderText("System", $"Congratulations! {selectedName} has joined your team!", ConsoleColor.Green);
            Console.WriteLine("\nPress any key to continue the journey....");
            Console.ReadKey(true);
        }

        // Auto give player 5 poke balls
        for (int i = 0; i < 5; i++)
        {
            Item? pokeball = _itemSpawner.SpawnItem("Poke Ball");
            if (pokeball != null) 
            {
                _player.AddItem(pokeball);
            }
        }

        isNewGame = false;
    }

    private void PlayCurrentLocationStory()
    {
        // Find the current phase based on player's progress
        var phaseData = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _player.CurrentPhase);
        if (phaseData == null) return;

        // Find the current location data based on player's current location
        var currentlocationData = phaseData.Locations.FirstOrDefault(l => l.Name == _player.CurrentLocation);
        if (currentlocationData == null) 
        {
            // If no matching location found, default to the first location of the phase
            currentlocationData = phaseData.Locations[0];
            _player.CurrentLocation = currentlocationData.Name;
        }

        _currentLocation = currentlocationData;

        // 1. Run narrative
        foreach (var line in currentlocationData.Narratives)
        {
            RenderText("System", line, ConsoleColor.Gray);
            Console.ReadKey(true);
        }

        // 2. Show NPC interaction menu
        ShowNpcMenu(currentlocationData.NPCs);
    }

    private void ShowNpcMenu(List<NpcData> npcs)
    {
        while (true)
        {
            Console.Clear(); // Clear old text to prevent UI overlapping
            Console.WriteLine($"--- Location: {_player.CurrentLocation} (Phase: {_player.CurrentPhase}) ---");
            
            for (int i = 0; i < npcs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Talk to {npcs[i].Name}");
            }

            int offset = npcs.Count;
            Console.WriteLine($"{offset + 1}. Move to Pokemon Center");
            Console.WriteLine($"{offset + 2}. Move to NEXT area");
            Console.WriteLine($"{offset + 3}. Move to PREVIOUS area");
            Console.WriteLine($"{offset + 4}. Enter wild area (Train/Catch)");
            Console.WriteLine($"{offset + 5}. Battle Gym Leader");
            Console.WriteLine($"{offset + 6}. Back to Main Menu");
            Console.WriteLine("----------------------------");
            Console.Write("Choose an option: ");

            // Clear input buffer
            while (Console.KeyAvailable) Console.ReadKey(true);

            string input = Console.ReadLine();

            if (int.TryParse(input, out int choice))
            {
                if (choice > 0 && choice <= npcs.Count) 
                    StartDialogue(npcs[choice - 1]);
                else if (choice == offset + 1) { HandleEnterPokemonCenter(); break;}
                else if (choice == offset + 2) { ChangeLocation(true); break; }  // Forward
                else if (choice == offset + 3) { ChangeLocation(false); break; } // Backward
                else if (choice == offset + 4) { HandleEnterWildArea(); break; }
                else if (choice == offset + 5) { HandleBattleGymLeader(); break; }
                else if (choice == offset + 6) { _screenManager.SwitchTo(ScreenType.MainMenu); break; }
            }
        }
    }

    private void StartDialogue(NpcData npc)
    {
        foreach (var line in npc.Dialogue)
        {
            RenderText(npc.Name, line, ConsoleColor.Yellow);
            Console.ReadKey(true);
        }

        // TODO: handle NPC action after dialogue
        if (npc.Action == "StartBattle" && _player.Badges.Count == npc.RequiredBadge)
        {
            _screenManager.SwitchTo(ScreenType.Battle);
        }
    }

    private void ChangeLocation(bool isForward)
    {
        var phaseData = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _player.CurrentPhase);
        if (phaseData == null) return;

        // Find the current location index in the current Phase's location list
        int currentIndex = phaseData.Locations.FindIndex(l => l.Name == _player.CurrentLocation);
        int nextIndex = isForward ? currentIndex + 1 : currentIndex - 1;

        // CASE 1: Move within the same Phase
        if (nextIndex >= 0 && nextIndex < phaseData.Locations.Count)
        {
            _player.CurrentLocation = phaseData.Locations[nextIndex].Name;
            _hasPlayedNarrative = false; // Reset to play narrative/show menu for the new area
            PlayCurrentLocationStory();
        }
        // CASE 2: Move Forward to the next Phase
        else if (isForward && nextIndex >= phaseData.Locations.Count)
        {
            _player.CurrentPhase++;
            var nextPhase = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _player.CurrentPhase);
            
            if (nextPhase != null)
            {
                _player.CurrentLocation = nextPhase.Locations[0].Name;
                _hasPlayedNarrative = false;
                PlayCurrentLocationStory();
            }
            else
            {
                // End of Game logic
                RenderText("System", "Congratulations! You've finished the game!", ConsoleColor.Cyan);
                _screenManager.SwitchTo(ScreenType.MainMenu);
            }
        }
        // CASE 3: Move Backward to the previous Phase
        else if (!isForward && nextIndex < 0)
        {
            if (_player.CurrentPhase > 1)
            {
                _player.CurrentPhase--;
                var prevPhase = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _player.CurrentPhase);
                
                // When going back a phase, land on the LAST location of that previous phase
                if (prevPhase != null)
                {
                    _player.CurrentLocation = prevPhase.Locations.Last().Name;
                    _hasPlayedNarrative = false;
                    PlayCurrentLocationStory();
                }
            }
            else
            {
                RenderText("System", "You are already at the starting area!", ConsoleColor.Red);
                _screenManager.SwitchTo(ScreenType.Story);
                Console.ReadKey(true);
            }
        }
    }

    private void HandleEnterPokemonCenter()
    {
        // Console.Clear();
        Menu.PokemonCenterMenu();
        string input = Console.ReadLine() ?? string.Empty;

        if (!UserInputValidator.ValidatePokemonCenterMenuInput(input))
        {
            // Console.Clear();
            Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
            return;
        }

        switch (input)
        {
            case "1" : HandleHealPokemon(); break;
            case "2" : break;
            case "3" : break;
        }

        ShowNpcMenu(_currentLocation.NPCs);
    }

    private void HandleHealPokemon()
    {
        foreach (Pokemon p in _player.PokemonTeam)
        {
            p.Heal(p.MaxHP);
        }
    }

    private void HandleEnterWildArea()
    {
        if (_player.PokemonTeam.Count == 0 || _player.PokemonTeam.All(p => p.CurrentHP <= 0))
        {
            Console.Clear();
            RenderText("System", "All your Pokemon have fainted! You should head to the Pokemon Center before entering the wild.", ConsoleColor.Red);
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            return; // Exit the method so no encounter happens
        }

        // Console.Clear();
        RenderText("System", "You Entered wild area...", ConsoleColor.Green);
        Thread.Sleep(1000);

        FindRandomItem();
        StartWildBattle();
    }

    private void StartWildBattle()
    {
        // Get Current location data
        var phaseData = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _player.CurrentPhase);
        var currentLocation = phaseData?.Locations.FirstOrDefault(l => l.Name == _player.CurrentLocation);

        if (currentLocation == null || currentLocation.WildPokemons == null || currentLocation.WildPokemons.Count == 0)
        {
            RenderText("System", "It seems there are no wild Pokemon here.", ConsoleColor.Gray);
            return;
        }

        Random rand = new Random();
        int encounterChance = rand.Next(1, 101); // 1 - 100

        if (encounterChance <= 60) // 60% chance of encountering a Pokemon
        {
            // Randomly select 1 Pokemon from the Location list.
            string pokemonName = currentLocation.WildPokemons[rand.Next(currentLocation.WildPokemons.Count)];
            
            // Generate a random level (e.g., Player's level +/- 2)
            int pokemonLevel = rand.Next(currentLocation.MinLevel, currentLocation.MaxLevel + 1);

            Pokemon wildPokemon = _pokemonSpawner.SpawnPokemon(pokemonName, pokemonLevel);

            // Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\n   !!! WILD ENCOUNTER !!!");
            RenderText("System", $"A wild {pokemonName} (Lv. {pokemonLevel}) appeared!", ConsoleColor.Yellow);

            // TODO: Transfer this Pokemon data to BattleScreen
            _screenManager.SwitchTo(ScreenType.Battle, wildPokemon);
        }
        else
        {
            RenderText("System", "You searched for a while but found nothing...", ConsoleColor.Gray);
            ShowNpcMenu(_currentLocation.NPCs);
        }
    }

    private void FindRandomItem()
    {
        Item? foundItem = _itemSpawner.RollForRandomItem();

        if (foundItem != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[!] You found a {foundItem.Name}!");
            Console.ResetColor();
            
            _player.AddItem(foundItem); 
            
            Console.WriteLine($"Added {foundItem.Name} to your Bag.");
        }
        else
        {
            Console.WriteLine("You found nothing!");
        }
        Thread.Sleep(2000);
    }

    private void HandleBattleGymLeader()
    {
        // Console.Clear();
        
        // 1. Find a Gym Leader or Boss at your current location.
        var gymLeader = _currentLocation.NPCs.FirstOrDefault(n => 
            n.Role == "Gym Leader" || 
            n.Role == "Elite Four" || 
            n.Role == "Champion");

        if (gymLeader == null)
        {
            RenderText("System", "There is no Gym Leader in this area.", ConsoleColor.Gray);
            Console.ReadKey(true);
            ShowNpcMenu(_currentLocation.NPCs);
            return;
        }

        // 2. Pre-match commentary
        RenderText("System", $"You are challenging {gymLeader.Role} {gymLeader.Name}!", ConsoleColor.Cyan);
        Console.ReadKey(true);

        foreach (var line in gymLeader.Dialogue)
        {
            RenderText(gymLeader.Name, line, ConsoleColor.Yellow);
            Console.ReadKey(true);
        }

        // 3. Preparing the opponent's lineup
        // Since PokemonTeam in JSON is just raw data, we need to use Spawner to create the actual Pokemon object.
        List<Pokemon> gymLeaderPokemonTeam = new List<Pokemon>();
        foreach (NpcPokemon npcPokemon in gymLeader.PokemonTeam)
        {
            Console.WriteLine($"checking gymleader pokemon!!!");
            string pokemonName = npcPokemon.Name;
            int pokemonLevel = npcPokemon.Level;
            Pokemon pokemon = _pokemonSpawner.SpawnPokemon(pokemonName, pokemonLevel);
            if (pokemon != null)
            {
                gymLeaderPokemonTeam.Add(pokemon);
            }
        }

        if (gymLeaderPokemonTeam.Count > 0)
        {
            // Console.Clear();
            Console.WriteLine("!!! THE BATTLE IS STARTING !!!");
            Thread.Sleep(1000);

            // 4. Switch to BattleScreen with the data being the list of Pokemon (Trainer Battles).
            _screenManager.SwitchTo(ScreenType.Battle, gymLeaderPokemonTeam);
        }
    }

    public void RenderText(string speaker, string text, ConsoleColor color)
    {
        // Console.Clear();
        Console.ForegroundColor = color;
        if (speaker != "System") Console.WriteLine($"[{speaker}]:");

        // 1. Running text effect
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(15); 
        }

        // 2. Important: Clear any "extra" key presses that might have been made during the text rendering
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true); 
        }

        Console.ResetColor();
        Console.WriteLine("\n\n(Press any key to continue...)");
    }

    public void Shutdown() { }
}