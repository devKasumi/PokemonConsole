using System.Text.Json.Serialization;

public enum ItemCategory { Healing, Capture, General, Utility }

// STRATEGY PATTERN: Base Item class with JSON Polymorphism support
[JsonDerivedType(typeof(CaptureItem), typeDiscriminator: "capture")]
[JsonDerivedType(typeof(HealingItem), typeDiscriminator: "healing")]
[JsonDerivedType(typeof(GeneralItem), typeDiscriminator: "general")]
public abstract class Item
{
    // Match with "Id" in JSON
    public int Id { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // In JSON it is "Type", so we map it here
    public ItemCategory Category { get; set; }
    
    public int Price { get; set; }
}