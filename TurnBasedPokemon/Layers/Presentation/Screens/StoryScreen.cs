using System;
using System.Threading;
using Screens;

public class StoryScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IStoryService _storyService;

    public StoryScreen(ScreenManager sm, GameSession gameSession, IStoryService ss)
    {
        _screenManager = sm;
        _gameSession = gameSession;
        _storyService = ss;
    }

    public void Initialize(object? data = null)
    {
        if (data is string cmd) _storyService.InitializeStory(cmd);
    }

    public void Update()
    {
        Console.WriteLine($"curren pokemon team count: {_gameSession.CurrentUser.PlayerData.PokemonTeam.Count}");
        Console.WriteLine($"curren player pokemon count: {_gameSession.Player.PokemonTeam.Count}");
        Console.ReadKey(true);
        if (_gameSession.Player.PokemonTeam.Count == 0)
        {
            // Console.WriteLine($"current player team: {_gameSession.Player.PokemonTeam.Count}");
            // Console.ReadKey(true);
            ShowStarterMenu();
            // return;
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
        Console.WriteLine($"╔══════════════════════════════════════════════════╗");
        Console.WriteLine($"║ AREA: {loc.Name.PadRight(42)} ║");
        Console.WriteLine($"╚══════════════════════════════════════════════════╝");
        
        int i = 1;
        foreach (var npc in loc.NPCs) Console.WriteLine($" {i++}. Talk to {npc.Name} ({npc.Role})");
        
        int centerIdx = i++, fwdIdx = i++, backIdx = i++, wildIdx = i++, gymIdx = i++, exitIdx = i++;

        Console.WriteLine($" {centerIdx}. Enter Pokemon Center");
        Console.WriteLine($" {fwdIdx}. Move Forward");
        Console.WriteLine($" {backIdx}. Move Backward");
        Console.WriteLine($" {wildIdx}. Explore Wild Area");
        Console.WriteLine($" {gymIdx}. Challenge Gym Leader");
        Console.WriteLine($" {exitIdx}. Back to Main Menu");
        Console.Write("\n Choose an action: ");

        if (!int.TryParse(Console.ReadLine(), out int choice)) return;

        if (choice <= loc.NPCs.Count) HandleTalk(loc.NPCs[choice - 1]);
        else if (choice == centerIdx) HandleResult(_storyService.HealTeamAtCenter(), ConsoleColor.Green);
        else if (choice == fwdIdx)    HandleResult(_storyService.Move(true), ConsoleColor.Cyan);
        else if (choice == backIdx)   HandleResult(_storyService.Move(false), ConsoleColor.Cyan);
        else if (choice == wildIdx)   HandleWildArea();
        else if (choice == gymIdx)    HandleGymBattle();
        else if (choice == exitIdx)   _screenManager.SwitchTo(ScreenType.MainMenu);
    }

    private void ShowStarterMenu()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║            PROFESSOR OAK'S LABORATORY            ║");
        Console.WriteLine("╠══════════════════════════════════════════════════╣");
        Console.WriteLine("║  Please choose your first Pokemon partner:       ║");
        Console.WriteLine("║                                                  ║");
        Console.WriteLine("║  1. Bulbasaur (Grass)                            ║");
        Console.WriteLine("║  2. Charmander (Fire)                            ║");
        Console.WriteLine("║  3. Squirtle (Water)                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.Write(" Select (1-3): ");

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
            RenderText("Oak", "Invalid choice. Don't be shy, pick one!", ConsoleColor.Yellow);
            ShowStarterMenu();
            // _screenManager.SwitchTo(ScreenType.Story);
            return;
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

    // Hàm tiện ích để in thông báo từ Service và dừng lại 1 chút
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
            _screenManager.SwitchTo(ScreenType.Battle, result.Team);
        }
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
        
        while (Console.KeyAvailable) Console.ReadKey(true); // Xóa buffer phím
        Console.ResetColor();
        Console.WriteLine("\n(Press any key to continue...)");
        Console.ReadKey(true);
    }

    public void Shutdown() { }
}