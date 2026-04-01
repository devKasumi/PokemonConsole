public enum ItemCategory { Healing, Capture, Utility }

public abstract class Item
{
    // Match with "Id" in JSON
    public int Id { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // In JSON it is "Type", so we map it here
    public ItemCategory Category { get; set; }
    
    public int Price { get; set; }

    public abstract bool Use(Pokemon target);
}