public interface IUserRepository {
    User GetByUsername(string username);
    void Save(User user);
}