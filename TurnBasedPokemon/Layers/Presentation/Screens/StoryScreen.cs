using System;
using System.Threading;
using Screens;
using PokemonEntity;

public class StoryScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IStoryService _storyService;
    private readonly object? data;

    public StoryScreen(ScreenManager sm, GameSession gameSession, IStoryService ss)
    {
        _screenManager = sm;
        _gameSession = gameSession;
        _storyService = ss;
    }

    public void Initialize(object? data = null)
    {
        Console.Clear();
        if (data is string cmd) _storyService.InitializeStory(cmd);
    }

    public void Update()
    {
        Console.Clear();

        if (_gameSession.Player.PokemonTeam.Count == 0)
        {
            ShowStarterMenu();
        }

        if (_storyService.ShouldPlayNarrative())
        {
            DisplayNarrative();
            _storyService.MarkNarrativeAsPlayed();
        }

        RenderMenu();
    }

    private void DisplayNarrative()
    {
        Console.Clear();
        var loc = _storyService.GetCurrentLocation();
        foreach (var line in loc.Narratives)
        {
            RenderText("System", line, ConsoleColor.Gray);
        }
    }

    private void RenderMenu()
    {
        var loc = _storyService.GetCurrentLocation();
        Console.Clear();
        
        // Header
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("============================================================");
        Console.WriteLine($"   CURRENT LOCATION: [ {loc.Name.ToUpper()} ]");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("   Status: Peacefully resting in the breeze.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        
        int i = 1;
        // Social
        Console.WriteLine("\n  --- POPULATION ---");
        foreach (var npc in loc.NPCs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [{i++}] 🗣️  Talk to {npc.Name} ({npc.Role})");
        }

        // Facilities & Movement
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n  --- FACILITIES & NAVIGATION ---");
        int centerIdx = i++;
        int fwdIdx = i++;
        int backIdx = i++;
        Console.WriteLine($"  [{centerIdx}] 🏥  Visit Pokemon Center");
        Console.WriteLine($"  [{fwdIdx}] ⬆️  Move Forward");
        Console.WriteLine($"  [{backIdx}] ⬇️  Move Backward");

        // Adventure
        Console.WriteLine("\n  --- ADVENTURE & CHALLENGE ---");
        int wildIdx = i++;
        int gymIdx = i++;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"  [{wildIdx}] 🌿  Explore Wild Area");
        Console.ForegroundColor = ConsoleColor.Red;
        if (_gameSession.CurrentLocation.Name == "Indigo Plateau") 
            Console.WriteLine($"  [{gymIdx}] 🏆  Challenge Pokemon League");
        else Console.WriteLine($"  [{gymIdx}] 🏆  Challenge Gym Leader");

        // System
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("\n  --- SYSTEM ---");
        int exitIdx = i++;
        Console.WriteLine($"  [{exitIdx}] ⚙️   Back to Main Menu");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("============================================================");
        Console.ResetColor();
        Console.Write("  What would you like to do? Select action: ");

        string input = Console.ReadLine() ?? "";
        if (int.TryParse(input, out int choice))
        {
            if (choice <= loc.NPCs.Count) HandleTalk(loc.NPCs[choice - 1]);
            else if (choice == centerIdx) HandleResult(_storyService.HealTeamAtCenter(), ConsoleColor.Green);
            else if (choice == fwdIdx)    HandleResult(_storyService.Move(true), ConsoleColor.Cyan);
            else if (choice == backIdx)   HandleResult(_storyService.Move(false), ConsoleColor.Cyan);
            else if (choice == wildIdx)   HandleWildArea();
            else if (choice == gymIdx)
            {
                if (_gameSession.CurrentLocation.Name == "Indigo Plateau") 
                    HandlePokemonLeagueBattle();
                else HandleGymBattle();
            }
            else if (choice == exitIdx)   _screenManager.SwitchTo(ScreenType.MainMenu);
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
            Console.ReadKey(true);
            _screenManager.SwitchTo(ScreenType.Story);
        }
    }

    private void ShowStarterMenu()
    {
        Menu.StarterSelectionMenu();

        string input = Console.ReadLine();
        string selectedName = input switch
        {
            "1" => "Bulbasaur",
            "2" => "Charmander",
            "3" => "Squirtle",
            _ => ""
        };

        if (string.IsNullOrEmpty(selectedName))
        {
            Console.Clear();
            RenderText("Oak", "Invalid choice. Don't be shy, pick one!", ConsoleColor.Yellow);
            ShowStarterMenu();
        }

        var result = _storyService.GiveStarter(selectedName);

        if (result.Success)
        {
            RenderText("Oak", result.Message, ConsoleColor.Cyan);
            RenderText("System", "You also received 5 Poke Balls!", ConsoleColor.Green);
            RenderText("System", "Press any key to begin your adventure...", ConsoleColor.White);
        }
        else
        {
            RenderText("System", result.Message, ConsoleColor.Red);
        }
    }

    private void HandleResult(string message, ConsoleColor color)
    {
        RenderText("System", message, color);
    }

    private void HandleTalk(NpcData npc)
    {
        foreach (var msg in npc.Dialogue) 
        {
            RenderText(npc.Name, msg, ConsoleColor.Yellow);
        }

        if (npc.Action == "StartBattle") 
        {
            var result = _storyService.TryChallengeTrainer(npc);

            if (!result.Success)
            {
                RenderText("System", result.Message, ConsoleColor.Red);
                return;
            }

            RenderText("System", result.Message, ConsoleColor.Magenta);
            RenderText("System", "!!! TRAINER BATTLE START !!!", ConsoleColor.Red);
            
            _screenManager.SwitchTo(ScreenType.Battle, result.Team);
        }
        else
        {
            _screenManager.SwitchTo(ScreenType.Story);
        }
    }

    private void HandleWildArea()
    {
        RenderText("System", "You walk into the tall grass...", ConsoleColor.DarkGreen);
        var result = _storyService.ExploreWildArea();

        if (result.FoundItem != null)
            RenderText("System", $"[!] You found a {result.FoundItem.Name}!", ConsoleColor.Yellow);

        RenderText("System", result.Message, result.Success ? ConsoleColor.Red : ConsoleColor.Gray);

        if (result.Success && result.WildPokemon != null)
        {
            _gameSession.CurrentEnemyTeam.Add(result.WildPokemon);
            _screenManager.SwitchTo(ScreenType.Battle);
        }
        else
        {
            _screenManager.SwitchTo(ScreenType.Story);
        }
    }

    private void HandleGymBattle()
    {
        var result = _storyService.TryChallengeGym();

        if (!result.Success)
        {
            RenderText("System", result.Message, ConsoleColor.Red);
            return;
        }

        RenderText("System", result.Message, ConsoleColor.Magenta);

        if (result.Leader != null)
        {
            foreach (var line in result.Leader.Dialogue) 
                RenderText(result.Leader.Name, line, ConsoleColor.Yellow);
        }

        if (result.Team != null)
        {
            RenderText("System", "!!! BATTLE START !!!", ConsoleColor.Red);
            _screenManager.SwitchTo(ScreenType.Battle);
        }
        else
        {
            _screenManager.SwitchTo(ScreenType.Story);
        }
    }

    private void HandlePokemonLeagueBattle()
    {
        var loc = _storyService.GetCurrentLocation();
        
        // Get all Elite Four members and the Champion from the current location
        var leagueMembers = loc.NPCs.Where(n => n.Role == "Elite Four" || n.Role == "Champion").ToList();

        if (leagueMembers.Count == 0)
        {
            RenderText("System", "The Pokemon League members is currently empty.", ConsoleColor.Red);
            return;
        }

        RenderText("System", "Welcome to the Pokemon League!", ConsoleColor.Cyan);

        // Loop through each member sequentially
        foreach (var member in leagueMembers)
        {
            // 1. Try to challenge the member using our unified method
            var result = _storyService.TryChallengePokemonLeague(member);

            if (!result.Success)
            {
                RenderText("System", result.Message, ConsoleColor.Red);
                return; // Stops the gauntlet (e.g., not enough badges)
            }

            // 2. Play their dialogue
            RenderText("System", result.Message, ConsoleColor.Magenta);
            foreach (var line in member.Dialogue) 
            {
                RenderText(member.Name, line, ConsoleColor.Yellow);
            }

            RenderText("System", $"!!! BATTLE START: {member.Name} !!!", ConsoleColor.Red);

            // 3. Switch to battle screen
            // NOTE: If your ScreenManager doesn't pause here, you will need to store 
            // a "LeagueIndex" variable in GameSession instead of using a foreach loop.
            _screenManager.SwitchTo(ScreenType.Battle);

            // 4. After the battle finishes, check if the player survived
            if (_gameSession.Player.PokemonTeam.All(p => p.CurrentHP <= 0))
            {
                RenderText("System", "You were defeated... Your Pokemon League challenge ends here.", ConsoleColor.Red);
                return; // Break out of the loop, player lost
            }
        }

        // If the loop finishes and the player is still alive, they beat everyone!
        ShowHallOfFame();
        _gameSession.HandlePlayerBecomeChampion();

    }

    public void ShowHallOfFame()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("====================================================");
        Console.WriteLine("                HALL OF FAME                        ");
        Console.WriteLine("====================================================");
        Console.ResetColor();

        Console.WriteLine($"\nChampion: {_gameSession.Player.Name}");
        Console.WriteLine("Your winning team has been registered:\n");

        foreach (var pkm in _gameSession.Player.PokemonTeam)
        {
            Console.WriteLine($"- LV.{pkm.Level} {pkm.Specie.Name}");
        }

        Console.WriteLine("\n----------------------------------------------------");
        Console.WriteLine("Thank you for playing! You have completed the game.");
        Console.WriteLine("You can now continue to explore or start a new game.");
        Console.WriteLine("----------------------------------------------------");
        
        Console.WriteLine("\nPress any key to return to Main Menu...");
        Console.ReadKey(true);
    }

    private void RenderText(string speaker, string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        if (speaker != "System") Console.Write($"\n[{speaker}]: ");
        else Console.Write("\n");

        foreach (char c in text)
        {
            Console.Write(c);
            if (!Console.KeyAvailable) Thread.Sleep(15);
        }
        
        while (Console.KeyAvailable) Console.ReadKey(true);
        Console.ResetColor();
        Console.WriteLine("\n(Press any key to continue...)");
        Console.ReadKey(true);
    }

    public void Shutdown() { }
}