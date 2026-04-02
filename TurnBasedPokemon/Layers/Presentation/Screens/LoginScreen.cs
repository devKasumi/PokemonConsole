using System;
using Screens;

public class LoginScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly AuthenService _authenService;

    public LoginScreen(ScreenManager screenManager,
                       GameSession session,
                       AuthenService authenService)
    {
        _screenManager = screenManager;
        _gameSession = session;
        _authenService = authenService;
    }

    public void Initialize(object? data = null)
    {
        Console.Clear();
    }

    public void Update()
    {
        Menu.LoginMenu();
        string input = Console.ReadLine() ?? string.Empty;

        if (!UserInputValidator.ValidateLoginMenuInput(input))
        {
            Console.Clear();
            Console.WriteLine("Invalid input. Please enter a number between 1 and 3.");
            return;
        }

        switch (input)
        {
            case "1" : HandleLogin(); break;
            case "2" : HandleRegister(); break;
            case "3" : Shutdown(); break;
        }
    }

    private void HandleLogin()
    {
        Console.Write("Username: ");
        string username = Console.ReadLine() ?? string.Empty;
        Console.Write("Password: ");
        string password = Console.ReadLine() ?? string.Empty;
        var user = _authenService.Login(new LoginRequest(username, password));
        if (user != null)
        {
            Console.Clear();
            Console.WriteLine("Login successful!");
            _screenManager.SwitchTo(ScreenType.MainMenu);
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Login failed. Please check your credentials and try again.");
            _screenManager.SwitchTo(ScreenType.Login);
        }
    }

    private void HandleRegister()
    {
        Console.Write("Choose a username: ");
        string username = Console.ReadLine() ?? string.Empty;
        Console.Write("Choose a password: ");
        string password = Console.ReadLine() ?? string.Empty;
        bool success = _authenService.Register(new RegisterRequest(username, password));
        if (success)
        {
            Console.Clear();
            Console.WriteLine("Registration successful! You can now log in.");
            _screenManager.SwitchTo(ScreenType.Login);
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Registration failed. Username may already be taken.");
            _screenManager.SwitchTo(ScreenType.Login);
        }
    }

    public void Shutdown()
    {
        // Clean up resources if needed
        // _screenManager.SwitchTo(ScreenType.Exit);
        _screenManager.ExitGame();
    }
}