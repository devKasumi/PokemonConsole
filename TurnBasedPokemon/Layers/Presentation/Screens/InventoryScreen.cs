using Screens;

public class InventoryContext
{
    public Player? Player { get; set; }
    public Pokemon? Enemy { get; set; }

    public InventoryContext(Player player, Pokemon pokemon)
    {
        Player = player;
        Enemy = pokemon;
    }
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
            Console.ReadKey(true);
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
        if (_player == null) return;

        switch (input)
        {
            case "1": // Capture Pocket
                OpenPocket(ItemCategory.Capture);
                break;
            case "2": // Healing Pocket
                OpenPocket(ItemCategory.Healing);
                break;
            case "3": 
                _screenManager.SwitchTo(ScreenType.Battle); 
                break;
        } 
    }

    private void OpenPocket(ItemCategory category)
    {
        int catKey = (int)category;
        Dictionary<string, List<Item>> pocket = _player.Inventory[catKey];

        if (pocket.Count == 0)
        {
            Console.WriteLine($"\nYour {category} pocket is empty!");
            Thread.Sleep(1000);
            return;
        }

        // Convert dictionary to a list to have numeric indexes for selection
        var itemGroups = pocket.ToList();

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"--- {category.ToString().ToUpper()} POCKET ---");
            
            for (int i = 0; i < itemGroups.Count; i++)
            {
                // Show: 1. Poke Ball (x5)
                Console.WriteLine($"{i + 1}. {itemGroups[i].Key} (x{itemGroups[i].Value.Count})");
            }
            Console.WriteLine("0. Back");
            Console.Write("\nSelect an item to use: ");

            string input = Console.ReadLine() ?? "";
            if (input == "0") break;

            if (int.TryParse(input, out int index) && index > 0 && index <= itemGroups.Count)
            {
                // Get the list of instances for the selected name
                var selectedInstances = itemGroups[index - 1].Value;

                if (selectedInstances.Count > 0)
                {
                    // Pick the first available instance in the list
                    Item itemToUse = selectedInstances[0];
                    UseItem(itemToUse);
                    
                    // After using an item (and potentially switching screens), we exit the loop
                    break; 
                }
            }
            else
            {
                Console.WriteLine("Invalid selection!");
                Thread.Sleep(800);
            }
        }
    }

    private void UseItem(Item item)
    {
        // 1. Logic cho CaptureItem (PokeBall, GreatBall...)
        if (item is CaptureItem ball)
        {
            Console.WriteLine("Use pokemon ball !!!!");
            
            Console.WriteLine("\n[DEBUG] --- POKEMON ---");
            Console.WriteLine($"Name: {_targetPokemon.Specie?.Name}");
            Console.WriteLine($"HP: {_targetPokemon.CurrentHP} / {_targetPokemon.MaxHP}");
            Console.WriteLine($"CatchRate: {_targetPokemon.Specie?.CatchRate}");
            Console.WriteLine($"Status: {_targetPokemon.Status}");
            Console.WriteLine("---------------------------------\n");
            // ------------------------------------

            Console.ReadKey(true);


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
                
                _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
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