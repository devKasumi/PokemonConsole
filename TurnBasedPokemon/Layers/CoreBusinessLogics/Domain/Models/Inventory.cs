public class Inventory
{
    // Dictionary: Key is the Item object, Value is the quantity
    public Dictionary<Item, int> Items { get; private set; } = new();

    public void AddItem(Item item, int amount = 1)
    {
        if (Items.ContainsKey(item))
            Items[item] += amount;
        else
            Items.Add(item, amount);
    }

    public bool RemoveItem(Item item)
    {
        if (Items.ContainsKey(item) && Items[item] > 0)
        {
            Items[item]--;
            if (Items[item] <= 0) Items.Remove(item);
            return true;
        }
        return false;
    }
}