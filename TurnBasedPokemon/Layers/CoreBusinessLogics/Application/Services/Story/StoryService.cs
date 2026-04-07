using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using PokemonEntity;

public class StoryService : IStoryService
{
    private readonly GameSession _gameSession;
    private readonly PokemonSpawner _pokemonSpawner;
    private readonly ItemSpawner _itemSpawner;
    private GameData _gameData;

    public StoryService(GameSession session, PokemonSpawner pokeSpawner, ItemSpawner itemSpawner)
    {
        _gameSession = session;
        _pokemonSpawner = pokeSpawner;
        _itemSpawner = itemSpawner;
        _gameData = LoadData();
    }

    private GameData LoadData()
    {
        if (!File.Exists("story.json")) return new GameData();

        string json = File.ReadAllText("story.json");
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        try 
        {
            return JsonSerializer.Deserialize<GameData>(json, options) ?? new GameData();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[!] JSON Error: {ex.Message}");
            throw; 
        }
    }

    public void InitializeStory(string command)
    {
        if (command == "NewGame")
        {
            var p = _gameSession.Player;
            p.CurrentPhase = 1;
            p.CurrentLocation = _gameData.Phases.FirstOrDefault()?.Locations.FirstOrDefault()?.Name ?? "Pallet Town";
            _gameSession.HasPlayedNarrative = false;
            p.PokemonTeam.Clear();
            p.Badges.Clear();
        }
    }

    public LocationData GetCurrentLocation()
    {
        var phase = _gameData.Phases.FirstOrDefault(p => p.PhaseId == _gameSession.Player?.CurrentPhase);
        return phase?.Locations.FirstOrDefault(l => l.Name == _gameSession.Player?.CurrentLocation) ?? new LocationData();
    }

    public bool ShouldPlayNarrative() => !_gameSession.HasPlayedNarrative;
    public void MarkNarrativeAsPlayed() => _gameSession.HasPlayedNarrative = true;

    // --- LOGIC MOVE LOGIC ---
    public string Move(bool forward)
    {
        var player = _gameSession.Player;
        var currentPhase = _gameData.Phases.FirstOrDefault(p => p.PhaseId == player.CurrentPhase);
        if (currentPhase == null) return "System Error: Cannot find current phase data.";

        int index = currentPhase.Locations.FindIndex(l => l.Name == player.CurrentLocation);

        if (forward)
        {
            if (index < currentPhase.Locations.Count - 1)
            {
                player.CurrentLocation = currentPhase.Locations[index + 1].Name;
                _gameSession.HasPlayedNarrative = false;
                return "You moved forward to the next area.";
            }
            if (player.CurrentPhase < _gameData.Phases.Count)
            {
                player.CurrentPhase++;
                player.CurrentLocation = _gameData.Phases.First(p => p.PhaseId == player.CurrentPhase).Locations[0].Name;
                _gameSession.HasPlayedNarrative = false;
                return "You advanced to a new Phase of your journey!";
            }
            _gameSession.IsTrainerBattle = false;
            Console.ReadKey(true);
            return "You cannot go any further in this direction.";
        }
        else // Backward
        {
            if (index > 0)
            {
                player.CurrentLocation = currentPhase.Locations[index - 1].Name;
                _gameSession.HasPlayedNarrative = false;
                return "You returned to the previous area.";
            }
            if (player.CurrentPhase > 1)
            {
                player.CurrentPhase--;
                player.CurrentLocation = _gameData.Phases.First(p => p.PhaseId == player.CurrentPhase).Locations.Last().Name;
                _gameSession.HasPlayedNarrative = false;
                return "You traveled back to the previous Phase.";
            }
            _gameSession.IsTrainerBattle = false;
            Console.ReadKey(true);
            return "You are already at the beginning of your journey.";
        }
    }

    // --- LOGIC HEALING LOGIC ---
    public string HealTeamAtCenter()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0) return "You don't have any Pokemon to heal!";
        
        foreach (var p in team) p.Heal(p.MaxHP);
        return "Nurse Joy: We've restored your Pokemon to full health!";
    }

    // --- LOGIC WILD AREA ---
    public (bool Success, string Message, Pokemon? WildPokemon, Item? FoundItem) ExploreWildArea()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0 || team.All(p => p.CurrentHP <= 0))
        {
            return (false, "It's too dangerous! All your Pokemon have fainted. Go to the Center!", null, null);
        }

        var loc = GetCurrentLocation();
        Random r = new Random();
        
        // 1. Pick Item randomly
        Item? item = _itemSpawner.RollForRandomItem();
        if (item != null) _gameSession.Player.AddItem(item);

        // 2. 60% encounter pokemon
        if (loc.WildPokemons != null && loc.WildPokemons.Count > 0 && r.Next(1, 101) <= 60)
        {
            string pName = loc.WildPokemons[r.Next(loc.WildPokemons.Count)];
            int level = r.Next(loc.MinLevel, loc.MaxLevel + 1);
            Pokemon wildP = _pokemonSpawner.SpawnPokemon(pName, level);

            return (true, $"A wild {pName} jumped out of the tall grass!", wildP, item);
        }

        _gameSession.IsTrainerBattle = false;

        return (false, "You searched the grass but only found peace and quiet.", null, item);
    }

    // --- LOGIC GYM BATTLE ---
    public (bool Success, string Message, NpcData? Leader, List<Pokemon>? Team) TryChallengeGym()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0 || team.All(p => p.CurrentHP <= 0))
            return (false, "Your team is not in condition to battle a Gym Leader!", null, null);

        var loc = GetCurrentLocation();
        var leader = loc.NPCs.FirstOrDefault(n => n.Role == "Gym Leader" || n.Role == "Champion" || n.Role == "Elite Four");
        
        if (leader == null)
            return (false, "There is no Gym Leader in this area.", null, null);

        if (_gameSession.Player.Badges.Count != leader.RequiredBadge)
            return (false, "You can not challenge gymleader right now, please collect enough badges.", null, null);

        // Init boss's team
        var bossTeam = new List<Pokemon>();
        foreach (var pData in leader.PokemonTeam)
        {
            var p = _pokemonSpawner.SpawnPokemon(pData.Name, pData.Level);
            if (p != null) bossTeam.Add(p);
        }

        _gameSession.CurrentEnemyTeam = bossTeam;
        _gameSession.CurrentEnemyTeamIndex = 0;
        _gameSession.IsTrainerBattle = true;

        return (true, $"You challenge Gym Leader {leader.Name}!", leader, bossTeam);
    }

    public (bool Success, string Message, List<Pokemon>? Team) TryChallengePokemonLeague(NpcData boss)
    {
        var playerTeam = _gameSession.Player.PokemonTeam;

        // 1. Check if the player's team is healthy
        if (playerTeam.Count == 0 || playerTeam.All(p => p.CurrentHP <= 0))
        {
            return (false, "Your Pokemon are too weak to battle! Please visit the Pokemon Center first.", null);
        }

        // 2. Check for required badges (Only applies to Gym Leaders, Elite Four, Champion)
        if (_gameSession.Player.Badges.Count < boss.RequiredBadge)
        {
            return (false, $"You need at least {boss.RequiredBadge} badges to challenge {boss.Name}!", null);
        }

        // 3. Ensure the boss actually has a team configured
        if (boss.PokemonTeam == null || boss.PokemonTeam.Count == 0)
        {
            return (false, $"{boss.Name} doesn't have any Pokemon configured for battle.", null);
        }

        // 4. Generate the boss's team from the database/spawner
        var bossTeam = new List<Pokemon>();
        foreach (var pData in boss.PokemonTeam)
        {
            var p = _pokemonSpawner.SpawnPokemon(pData.Name, pData.Level);
            if (p != null) bossTeam.Add(p);
        }

        // 5. Save the state to GameSession for the BattleScreen to use
        _gameSession.CurrentEnemyTeam = bossTeam;
        _gameSession.CurrentEnemyTeamIndex = 0;
        _gameSession.IsTrainerBattle = true;

        return (true, $"You are challenged by {boss.Role} {boss.Name}!", bossTeam);
    }

    public (bool Success, string Message, List<Pokemon>? Team) TryChallengeTrainer(NpcData trainer)
    {
        var playerTeam = _gameSession.Player.PokemonTeam;

        if (playerTeam.Count == 0 || playerTeam.All(p => p.CurrentHP <= 0))
        {
            return (false, "Your Pokemon are too weak to battle! Go to the Center first.", null);
        }

        if (trainer.PokemonTeam == null || trainer.PokemonTeam.Count == 0)
        {
            return (false, $"{trainer.Name} doesn't have any Pokemon to battle.", null);
        }

        var enemyTeam = new List<Pokemon>();
        foreach (var pData in trainer.PokemonTeam)
        {
            var p = _pokemonSpawner.SpawnPokemon(pData.Name, pData.Level);
            if (p != null) enemyTeam.Add(p);
        }

        return (true, $"{trainer.Name} wants to battle!", enemyTeam);
    }

    public (bool Success, string Message) GiveStarter(string starterName)
    {
        var player = _gameSession.Player;

        if (player.PokemonTeam.Count > 0)
        {
            return (false, "You already have a partner Pokemon!");
        }

        var starter = _pokemonSpawner.SpawnPokemon(starterName, 5);
        
        if (starter == null)
        {
            return (false, $"Error: Could not find data for {starterName}.");
        }

        player.PokemonTeam.Add(starter);
        player.CurrentPokemon = starter;

        for (int i = 0; i < 5; i++)
        {
            var ball = _itemSpawner.SpawnItem("Poke Ball");
            if (ball != null) player.AddItem(ball);
        }

        return (true, $"Congratulations! {starterName} is now your partner!");
    }
}