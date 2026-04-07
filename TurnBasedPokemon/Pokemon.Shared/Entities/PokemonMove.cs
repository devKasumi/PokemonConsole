public class PokemonMove
{
    public string Name { get; set; }
    public ElementType Type { get; set; }
    public int Power { get; set; }
    public int PP { get; set; }
    public MoveType MoveType { get; set; }
    public int LevelLearned { get; set; }

    public PokemonMove() { }

    public PokemonMove(string name, ElementType type, int power, int pp, MoveType moveType)
    {
        Name = name;
        Type = type;
        Power = power;
        PP = pp;
        MoveType = moveType;
    }
}