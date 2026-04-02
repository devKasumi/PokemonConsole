using System.Text.Json.Serialization;

public enum ItemCategory { Healing, Capture, Utility }

// STRATEGY PATTERN: Base Item class with JSON Polymorphism support
[JsonDerivedType(typeof(CaptureItem), typeDiscriminator: "capture")]
[JsonDerivedType(typeof(HealingItem), typeDiscriminator: "healing")]
public abstract class Item
{
    // Match with "Id" in JSON
    public int Id { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // In JSON it is "Type", so we map it here
    public ItemCategory Category { get; set; }
    
    public int Price { get; set; }

    // public abstract bool Use(Pokemon target);
}