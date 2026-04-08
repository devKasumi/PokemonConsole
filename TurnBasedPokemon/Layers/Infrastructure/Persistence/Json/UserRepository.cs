// Persistence/Repositories/UserRepository.cs
using Application.RepoInterfaces;

public class UserRepository : IUserRepository
{
    private readonly string _filePath = "users.json";
    private readonly IFileService _fileService;

    public UserRepository(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Retrieves all users from the persistent storage.
    /// </summary>
    public List<User> GetAll()
    {
        return _fileService.Load<List<User>>(_filePath) ?? new List<User>();
    }

    public User? GetByUsername(string username)
    {
        return GetAll().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Saves the user progress. Updates existing users or adds new ones.
    /// </summary>
    public void Save(User user)
    {
        List<User> allUsers = GetAll();
        int index = allUsers.FindIndex(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase));
        
        if (index >= 0)
            allUsers[index] = user;
        else
            allUsers.Add(user);

        _fileService.Save(_filePath, allUsers);
    }
}