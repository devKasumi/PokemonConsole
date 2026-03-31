public class HealingItem : Item
{
    public int HealAmount { get; set; }

    public override bool Use(Pokemon target)
    {
        if (target.CurrentHP >= target.MaxHP) 
        {
            Console.WriteLine($"{target.Specie.Name} is already at full health!");
            return false; // Item not consumed
        }

        target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + HealAmount);
        Console.WriteLine($"Restored {HealAmount} HP to {target.Specie.Name}!");
        return true; // Item consumed
    }
}