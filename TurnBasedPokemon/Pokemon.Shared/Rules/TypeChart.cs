using System.Net.NetworkInformation;

public class TypeChart
{
    private static readonly Dictionary<ElementType, List<ElementType>> Weaknesses = new Dictionary<ElementType, List<ElementType>>
    {
        { ElementType.Fire,         new List<ElementType> { ElementType.Water, ElementType.Ground, ElementType.Rock } },
        { ElementType.Water,        new List<ElementType> { ElementType.Electric, ElementType.Grass } },
        { ElementType.Grass,        new List<ElementType> { ElementType.Fire, ElementType.Ice, ElementType.Poison, ElementType.Flying, ElementType.Bug } },
        { ElementType.Electric,     new List<ElementType> { ElementType.Ground } },
        { ElementType.Psychic,      new List<ElementType> { ElementType.Bug, ElementType.Ghost, ElementType.Dark } },
        { ElementType.Ice,          new List<ElementType> { ElementType.Fire, ElementType.Fighting, ElementType.Rock, ElementType.Steel } },
        { ElementType.Dragon,       new List<ElementType> { ElementType.Ice, ElementType.Dragon, ElementType.Fairy } },
        { ElementType.Dark,         new List<ElementType> { ElementType.Fighting, ElementType.Bug, ElementType.Fairy } },
        { ElementType.Fairy,        new List<ElementType> { ElementType.Poison, ElementType.Steel } },
        // Normal has no weaknesses
    };

    public static bool IsEffective(ElementType attack, ElementType defense)
    {
        return Weaknesses.ContainsKey(defense) && Weaknesses[defense].Contains(attack);
    }

    public static float GetEffectivenessMultiplier(ElementType attack, ElementType defense)
    {
        if (IsEffective(attack, defense))
            return 2.0f; // Super effective
        else if (IsEffective(defense, attack))
            return 0.5f; // Not very effective
        else
            return 1.0f; // Normal effectiveness
    }
}