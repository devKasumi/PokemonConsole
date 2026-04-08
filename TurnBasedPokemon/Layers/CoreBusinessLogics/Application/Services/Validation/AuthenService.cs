using BCrypt.Net;
using Application.RepoInterfaces;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);

public class AuthenService : IAuthenService
{
    private readonly IUserRepository _userRepository;
    private readonly GameSession _gameSession;

    public AuthenService(IUserRepository userRepository, GameSession session)
    {
        _userRepository = userRepository;
        _gameSession = session;
    }

    public bool Register(RegisterRequest request)
    {
        if (_userRepository.GetByUsername(request.Username) != null)
            return false; // User already exists

        // Hash the password before saving
        // HashPassword will automatically generate a salt and include it in the hash
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        User newUserName = new User
        {
            Username = request.Username,
            Password = hashedPassword
        };

        _userRepository.Save(newUserName);
        return true;
    }

    public bool Login(LoginRequest request)
    {
        User? user = _userRepository.GetByUsername(request.Username);
        
        if (user == null) return false;

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        
        if (isPasswordValid)
        {
            _gameSession.CurrentUser = user; 
            return true;
        }

        return false;
    }

    public bool LoadProgress(string username)
    {
        var loadedUser = _userRepository.GetByUsername(username);
        
        if (loadedUser != null)
        {
            _gameSession.CurrentUser = loadedUser;
            return true;
        }
        
        return false;
    }

    public void SaveProgress()
    {
        if (_gameSession.CurrentUser != null)
            _userRepository.Save(_gameSession.CurrentUser);
    }

    public void SetChampion()
    {
        if (_gameSession.Player != null)
            _gameSession.Player.IsChampion = true;
    }
}