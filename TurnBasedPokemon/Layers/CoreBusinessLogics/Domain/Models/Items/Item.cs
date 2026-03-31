public enum ItemCategory { Healing, Capture, Utility }

public abstract class Item
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemCategory Category { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }

    // Abstract method: Each item type will define its own logic
    public abstract bool Use(Pokemon target);
}