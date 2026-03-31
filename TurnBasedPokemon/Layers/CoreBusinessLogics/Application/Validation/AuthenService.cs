using BCrypt.Net;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);

public class AuthenService
{
    private readonly IUserRepository _userRepository;

    public AuthenService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
            Password = hashedPassword,
            Level = 1  
        };

        _userRepository.Save(newUserName);
        return true;
    }

    public User Login(LoginRequest request)
    {
        User user = _userRepository.GetByUsername(request.Username);
        if (user == null)
            return null; // User not found

        // Verify the password using BCrypt's Verify method
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid)
            return null; // Invalid password

        return user; // Authentication successful
    }
}