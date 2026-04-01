public class ItemDTO
{
    // Ensure these match the JSON keys exactly
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    
    // Specific fields
    public int? HealAmount { get; set; }
    public double? CatchRateMultiplier { get; set; }
}