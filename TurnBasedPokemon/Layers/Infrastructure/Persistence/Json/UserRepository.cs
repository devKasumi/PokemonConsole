using System.Text.Json;
using System.Text.Json.Serialization;

public class UserRepository : IUserRepository
{
    private readonly string _filePath = "users.json";
    private readonly JsonSerializerOptions _options;

    public UserRepository()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // Important: This handles the polymorphism ($type) automatically
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public List<User> GetAll()
    {
        if (!File.Exists(_filePath)) return new List<User>();
        
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json, _options) ?? new List<User>();
        }
        catch { return new List<User>(); }
    }

    public User? GetByUsername(string username)
    {
        return GetAll().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void Save(User user)
    {
        List<User> allUsers = GetAll();
        
        // Find if user exists to update, otherwise add new
        int index = allUsers.FindIndex(u => u.Username == user.Username);
        
        if (index >= 0)
            allUsers[index] = user; // Update existing (Save Game)
        else
            allUsers.Add(user);    // Add new (Register)

        string json = JsonSerializer.Serialize(allUsers, _options);
        File.WriteAllText(_filePath, json);
    }
}