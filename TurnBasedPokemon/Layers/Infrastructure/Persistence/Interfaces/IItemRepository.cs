public interface IItemRepository
{
    Item? GetItemByName(string name);
    Item? GetItemById(int id);
    List<Item> GetAllItems();
}