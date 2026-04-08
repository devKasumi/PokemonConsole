using PokemonEntity;

public class GameData 
{ 
    public List<PhaseData> Phases { get; set; } = new();
}

public class PhaseData 
{ 
    public int PhaseId { get; set; }
    public string Name { get; set; }
    public List<LocationData> Locations { get; set; } = new();
}

public class LocationData 
{ 
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> Narratives { get; set; } = new();
    public List<NpcData> NPCs { get; set; } = new();

    public List<string> WildPokemons { get; set; } = new();
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
}

public class NpcData 
{ 
    public string Name { get; set; }
    public string Role { get; set; } 
    public List<string> Dialogue { get; set; } = new();
    public string Action { get; set; }
    public int RequiredBadge { get; set; }
    public List<NpcPokemon> PokemonTeam { get; set; } = new();
    public string ActionParam { get; set; } 
}

public class NpcPokemon
{
    public string Name { get; set; }
    public int Level { get; set; }
    public List<PokemonMove> Moves { get; set; } = new();
}
