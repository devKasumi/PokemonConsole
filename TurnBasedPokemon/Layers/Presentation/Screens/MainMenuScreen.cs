using Screens;

public class MainMenuScreen : IScreen
{
    private readonly ScreenManager _screenManager;

    public MainMenuScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public void Initialize(object? data = null)
    {
        Console.Clear();
    }

    public void Update()
    {
        Menu.MainMenu();
        string input = Console.ReadLine() ?? string.Empty;

        if (!UserInputValidator.ValidateMainMenuInput(input))
        {
            Console.Clear();
            Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
            return;
        }

        switch (input)
        {
            case "1" : HandleStartNewGame(); break;
            case "2" : HandleLoadGame(); break;
            case "3" : HandlePvP(); break;
            case "4" : HandleSaveGame(); break;
            case "5" : HandleBackToLogin(); break;
        }
    }

    private void HandleStartNewGame()
    {
        _screenManager.SwitchTo(ScreenType.Story, "NewGame");
    }

    private void HandleLoadGame()
    {
        _screenManager.SwitchTo(ScreenType.Story);
    }

    private void HandlePvP()
    {
        // TODO: Implement PvP mode
    }

    private void HandleSaveGame()
    {
        // TODO: Implement save game logic
    }

    private void HandleBackToLogin()
    {
        _screenManager.SwitchTo(ScreenType.Login);
    }

    public void Shutdown()
    {
        // Cleanup resources if needed
    }
}