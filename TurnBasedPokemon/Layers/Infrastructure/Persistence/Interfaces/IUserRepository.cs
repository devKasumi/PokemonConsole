public interface IUserRepository
{
    List<User> GetAll();
    User? GetByUsername(string username);
    void Save(User user);
}