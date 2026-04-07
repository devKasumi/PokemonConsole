using System.Text.Json.Serialization;

public class PokemonSpecies
{
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("Types")]
    public List<ElementType> Types { get; set; }

    public Stats BaseStats { get; set; }

    public List<PokemonMove> MoveSet { get; set; }

    public int CatchRate { get; set; }

    public int BaseExp { get; set; } 

    public int? EvolutionId { get; set; }

    public int? EvolutionLevel { get; set; }
}