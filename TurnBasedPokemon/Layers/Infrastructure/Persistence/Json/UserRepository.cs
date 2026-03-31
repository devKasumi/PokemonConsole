using System.Text.Json;

public class UserRepository : IUserRepository
{
    private readonly string _filePath = "users.json";

    private List<User> LoadAll()
    {
        if (!File.Exists(_filePath)) return new List<User>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }

    public User GetByUsername(string username)
    {
        return LoadAll().FirstOrDefault(u => u.Username == username);
    }

    public void Save(User user)
    {
        List<User> users = LoadAll();
        users.Add(user);
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}