using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

public class MySqlStoryService : IStoryService
{
    private readonly string _connection;
    private readonly GameSession _gameSession;
    private readonly PokemonSpawner _pokemonSpawner;
    private readonly ItemSpawner _itemSpawner;
    private readonly GameData _gameData; 

    public MySqlStoryService(GameSession session, PokemonSpawner pokeSpawner, ItemSpawner itemSpawner, string connection)
    {
        _gameSession = session;
        _pokemonSpawner = pokeSpawner;
        _itemSpawner = itemSpawner;
        _connection = connection;
        _gameData = LoadDataFromDb();
    }

    /// <summary>
    /// Fetches all story data (Phases, Locations, NPCs) into memory during startup.
    /// </summary>
    private GameData LoadDataFromDb()
    {
        var gameData = new GameData { Phases = new List<PhaseData>() };
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        
        try
        {
            using var conn = new MySqlConnection(_connection); 
            conn.Open();

            // 1. Fetch Phases
            string sqlPhase = "SELECT * FROM game_phases ORDER BY PhaseId ASC";
            using (var cmdP = new MySqlCommand(sqlPhase, conn))
            using (var rP = cmdP.ExecuteReader()) 
            {
                while (rP.Read()) 
                {
                    gameData.Phases.Add(new PhaseData 
                    { 
                        PhaseId = rP.GetInt32("PhaseId"), 
                        Name = rP.GetString("Name"), 
                        Locations = new List<LocationData>() 
                    });
                }
            }

            // 2. Fetch Locations and map them to their parent Phase
            string sqlLoc = "SELECT * FROM locations ORDER BY Id ASC";
            using (var cmdL = new MySqlCommand(sqlLoc, conn))
            using (var rL = cmdL.ExecuteReader()) 
            {
                while (rL.Read()) 
                {
                    var phase = gameData.Phases.FirstOrDefault(p => p.PhaseId == rL.GetInt32("PhaseId"));
                    if (phase != null) 
                    {
                        phase.Locations.Add(new LocationData 
                        {
                            Id = rL.GetInt32("Id"), 
                            Name = rL.GetString("Name"), 
                            MinLevel = rL.GetInt32("MinLevel"), 
                            MaxLevel = rL.GetInt32("MaxLevel"),
                            WildPokemons = JsonSerializer.Deserialize<List<string>>(rL.GetString("WildPokemons"), options) ?? new(),
                            Narratives = JsonSerializer.Deserialize<List<string>>(rL.GetString("Narratives"), options) ?? new(),
                            NPCs = JsonSerializer.Deserialize<List<NpcData>>(rL.GetString("NPCs"), options) ?? new()
                        });
                    }
                }
            }
            Console.WriteLine($"[SqlStoryService] Successfully loaded {gameData.Phases.Count} phases from DB.");
            return gameData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Database Error in StoryService: {ex.Message}");
            return new GameData(); 
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

    // --- EXPLORATION LOGIC ---

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

    public string HealTeamAtCenter()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0) return "You don't have any Pokemon to heal!";
        
        foreach (var p in team) p.Heal(p.MaxHP);
        return "Nurse Joy: We've restored your Pokemon to full health!";
    }

    public (bool Success, string Message, Pokemon? WildPokemon, Item? FoundItem) ExploreWildArea()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0 || team.All(p => p.CurrentHP <= 0))
        {
            return (false, "It's too dangerous! All your Pokemon have fainted. Go to the Center!", null, null);
        }

        var loc = GetCurrentLocation();
        Random r = new Random();
        
        // 1. Roll Item
        Item? item = _itemSpawner.RollForRandomItem();
        if (item != null) _gameSession.Player.AddItem(item);

        // 2. Roll Pokemon (60% chance)
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

    // --- BATTLE LOGIC ---

    public (bool Success, string Message, NpcData? Leader, List<Pokemon>? Team) TryChallengeGym()
    {
        var team = _gameSession.Player.PokemonTeam;
        if (team.Count == 0 || team.All(p => p.CurrentHP <= 0))
            return (false, "Your team is not in condition to battle a Gym Leader!", null, null);

        var loc = GetCurrentLocation();
        var leader = loc.NPCs.FirstOrDefault(n => n.Role == "Gym Leader" || n.Role == "Champion" || n.Role == "Elite Four");
        
        if (leader == null)
            return (false, "There is no Gym Leader in this area.", null, null);

        if (_gameSession.Player.Badges.Count < leader.RequiredBadge)
            return (false, "You can not challenge the gym leader right now, please collect enough badges.", null, null);

        var bossTeam = new List<Pokemon>();
        foreach (var pData in leader.PokemonTeam)
        {
            var p = _pokemonSpawner.SpawnPokemon(pData.Name, pData.Level);
            if (p != null) bossTeam.Add(p);
        }

        _gameSession.CurrentEnemyTeam = bossTeam;
        _gameSession.CurrentEnemyTeamIndex = 0;
        _gameSession.IsTrainerBattle = true;

        return (true, $"You challenge {leader.Role} {leader.Name}!", leader, bossTeam);
    }

    public (bool Success, string Message, List<Pokemon>? Team) TryChallengeTrainer(NpcData trainer)
    {
        var playerTeam = _gameSession.Player.PokemonTeam;

        if (playerTeam.Count == 0 || playerTeam.All(p => p.CurrentHP <= 0))
            return (false, "Your Pokemon are too weak to battle! Go to the Center first.", null);

        if (trainer.PokemonTeam == null || trainer.PokemonTeam.Count == 0)
            return (false, $"{trainer.Name} doesn't have any Pokemon to battle.", null);

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
            return (false, "You already have a partner Pokemon!");

        var starter = _pokemonSpawner.SpawnPokemon(starterName, 5);
        if (starter == null)
            return (false, $"Error: Could not find data for {starterName}.");

        player.PokemonTeam.Add(starter);
        player.CurrentPokemon = starter;

        // Give 5 Poke Balls to start
        for (int i = 0; i < 5; i++)
        {
            var ball = _itemSpawner.SpawnItem("Poke Ball");
            if (ball != null) player.AddItem(ball);
        }

        return (true, $"Congratulations! {starterName} is now your partner!");
    }
}