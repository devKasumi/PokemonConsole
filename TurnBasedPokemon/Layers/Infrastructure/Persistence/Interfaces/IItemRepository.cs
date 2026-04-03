public interface IItemRepository
{
    void LoadData();
    Item? GetItemByName(string name);
    Item? GetItemById(int id);
    List<Item> GetAllItems();
}