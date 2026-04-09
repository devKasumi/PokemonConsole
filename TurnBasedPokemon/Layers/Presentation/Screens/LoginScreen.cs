using System;
using Screens;
using PokemonEntity;

public class LoginScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly IAuthenService _authenService;

    public LoginScreen(ScreenManager screenManager,
                       GameSession session,
                       IAuthenService authenService)
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
        Console.Clear();
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
        Menu.PrintHeader("LOGIN TO YOUR ADVENTURE");
        Console.Write("  Enter Username: ");
        string user = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(user))
        {
            Console.WriteLine("Username cannot be empty. Returning to menu...");
            _screenManager.SwitchTo(ScreenType.Login);
            return;
        }

        Console.Write("  Enter Password: ");
        string pass = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine("Password cannot be empty. Returning to menu...");
            _screenManager.SwitchTo(ScreenType.Login);
            return;
        }

        // Calling the Service logic
        bool success = _authenService.Login(new LoginRequest(user, pass));
        if (success)
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
        // Console.Clear();
        Console.WriteLine("--- CREATE NEW ADVENTURE ---");
        Console.Write("Choose a username: ");
        string username = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Username cannot be empty. Returning to menu...");
            _screenManager.SwitchTo(ScreenType.Login);
            return;
        }

        Console.Write("Choose a password: ");
        string password = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Password cannot be empty. Returning to menu...");
            _screenManager.SwitchTo(ScreenType.Login);
            return;
        }

        bool success = _authenService.Register(new RegisterRequest(username, password));

        Console.Clear();
        if (success)
        {
            Console.WriteLine("Registration successful! You can now log in.");
        }
        else
        {
            Console.WriteLine("Registration failed. Username \"" + username + "\" is already taken. Please choose a different username.");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        _screenManager.SwitchTo(ScreenType.Login);
    }

    public void Shutdown()
    {
        // Clean up resources if needed
        _screenManager.ExitGame();
    }
}