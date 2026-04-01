public class ItemDTO
{
    public string ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Healing" or "Capture"
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    
    // Fields for specific types (nullable so they don't crash if missing)
    public int? HealAmount { get; set; }
    public double? CatchRateMultiplier { get; set; }
}