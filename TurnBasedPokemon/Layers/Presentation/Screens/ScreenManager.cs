using Screens;
using PokemonEntity;

public class ScreenManager
{
    public ScreenType CurrentScreen { get; set; } = ScreenType.Login;
    private readonly Dictionary<ScreenType, IScreen> _screens = new();
    private bool _isRunning = true;
    private object? _pendingData = null;

    public ScreenManager()
    {
        
    }

    public void RegisterScreen(ScreenType screenType, IScreen screen)
    {
        _screens[screenType] = screen;
    }

    public void ExitGame() => _isRunning = false;

    public void Run()   // Main loop to run the screen manager
    {
        while(_isRunning)
        {
            if (_screens.ContainsKey(CurrentScreen))
            {
                var screen = _screens[CurrentScreen];
                screen.Initialize(_pendingData);
                screen.Update();
            }
            else
            {
                Console.WriteLine($"Screen {CurrentScreen} not found.");
                ExitGame();
            }
        }
    }

    public void SwitchTo(ScreenType screenType, object? data = null)
    {
        if (_screens.ContainsKey(screenType))
        {
            CurrentScreen = screenType;
            _pendingData = data;
        }
        else
        {
            Console.WriteLine($"Screen {screenType} not found.");
        }
    }

}
