using Screens;

public class InventoryContext
{
    public Player? Player { get; set; }
    public Pokemon? Enemy { get; set; }
}

public class InventoryScreen : IScreen
{
    private Player _player;
    private Pokemon? _targetPokemon;
    private ScreenManager _screenManager;

    public InventoryScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public void Initialize(object? data)
    {
        if (data is InventoryContext context)
        {
            _player = context.Player;
            _targetPokemon = context.Enemy;
        }
        else
        {
            Console.WriteLine("Invalid data passed to InventoryScreen!");
            _screenManager.SwitchTo(ScreenType.Story);
        }
    }

    public void RenderText()
    {
        Menu.InventoryMenu();
        
        Console.Write("Select an item: ");
        string input = Console.ReadLine() ?? string.Empty;

        if (UserInputValidator.ValidateInventoryMenuInput(input))
        {
            HandleSelection(input);
        }
        else
        {
            Console.WriteLine("Invalid input! Please try again.");
            Thread.Sleep(1000);
        }
    }

    public void Update()
    {
        RenderText();
        // string input = Console.ReadLine() ?? "";
        // HandleSelection(input);
    }

    public void Shutdown()
    {
        
    }

    private void HandleSelection(string input)
    {
        if (input == "3")
        {
            _screenManager.SwitchTo(ScreenType.Battle); 
            return;
        }

        if (_player.Inventory.Count != 0)
        {
            var groupedItems = _player.GetGroupedItems().ToList();
            if (int.TryParse(input, out int index) && index > 0 && index <= groupedItems.Count)
            {
                Item selectedItem = groupedItems[index - 1].Item;
                UseItem(selectedItem);
            }
        }
        else
        {
            Console.WriteLine("Your inventory is empty!");
            Thread.Sleep(1000);
        }
    }

    private void UseItem(Item item)
    {
        // 1. Logic cho CaptureItem (PokeBall, GreatBall...)
        if (item is CaptureItem ball)
        {
            if (_targetPokemon == null)
            {
                Console.WriteLine("You can't use that here!");
                Thread.Sleep(1000);
                return;
            }

            bool success = ball.Use(_targetPokemon);
            
            if (success)
            {
                _player.RemoveItem(item);
                _player.AddPokemon(_targetPokemon);
                
                _screenManager.SwitchTo(ScreenType.Story);
            }
            else
            {
                _player.RemoveItem(item);
                _screenManager.SwitchTo(ScreenType.Battle);
            }
        }
        // 2. Logic cho HealingItem (Potion...)
        else if (item is HealingItem potion)
        {
            _player.CurrentPokemon.Heal(potion.HealAmount);
            _player.RemoveItem(item);
            Thread.Sleep(1000);
        }
    }
}