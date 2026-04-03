using Screens;

public class InventoryScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameSession _gameSession;
    private readonly ICatchService _catchService;
    private readonly IHealingService _healingService;

    public InventoryScreen(ScreenManager screenManager,
                           GameSession session,
                           ICatchService catchService,
                           IHealingService healingService)
    {
        _screenManager = screenManager;
        _gameSession = session;
        _catchService = catchService;
        _healingService = healingService;
    }

    public void Initialize(object? data = null) => Console.Clear();

    public void Update()
    {
        Console.Clear();
        RenderText();
    }

    public void RenderText()
    {
        Console.Clear();
        Menu.InventoryMenu(); 
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine(" 1. POCKET: CAPTURE");
        Console.WriteLine(" 2. POCKET: HEALING");
        Console.WriteLine(" 3. BACK TO BATTLE");
        Console.WriteLine(new string('═', 30));
        Console.Write(" Select category: ");

        string input = Console.ReadLine() ?? string.Empty;

        switch (input)
        {
            case "1": OpenPocket(ItemCategory.Capture); break;
            case "2": OpenPocket(ItemCategory.Healing); break;
            case "3": _screenManager.SwitchTo(ScreenType.Battle); break;
            default:
                Console.WriteLine(" Invalid choice!");
                Thread.Sleep(600);
                break;
        }
    }

    private void OpenPocket(ItemCategory category)
    {
        // Get Inventory directly from player in GameSession
        var inventory = _gameSession.Player?.Inventory;
        if (inventory == null || !inventory.ContainsKey((int)category)) return;

        var pocket = inventory[(int)category];

        if (pocket.Count == 0)
        {
            Console.WriteLine($"\n [!] Your {category} pocket is empty!");
            Thread.Sleep(800);
            return;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== {category.ToString().ToUpper()} POCKET ===");
            var itemGroups = pocket.ToList();

            for (int i = 0; i < itemGroups.Count; i++)
            {
                Console.WriteLine($" {i + 1}. {itemGroups[i].Key} (x{itemGroups[i].Value.Count})");
            }
            Console.WriteLine(" 0. Back");
            Console.Write("\n Select item to use: ");

            string input = Console.ReadLine() ?? "";
            if (input == "0") break;

            if (int.TryParse(input, out int index) && index > 0 && index <= itemGroups.Count)
            {
                var selectedInstances = itemGroups[index - 1].Value;
                Item itemToUse = selectedInstances[0];
                
                if (UseItem(itemToUse)) break; // If success -> break;
            }
        }
    }

    private bool UseItem(Item item)
    {
        // 1. Handle capture item
        if (item is CaptureItem ball)
        {
            var target = _gameSession.CurrentEnemyPokemon;
            if (target == null)
            {
                Console.WriteLine(" [!] There is no wild Pokemon to catch!");
                Thread.Sleep(1000);
                return false;
            }

            // Call CatchService to handle the logic (...)
            // bool success = _catchService.AttemptCatch(_gameSession.Player, target, ball);
            CatchResult result = _catchService.ExecuteCapture(ball, target);
            bool success = result.IsCaught;

            if (success)
            {
                Console.WriteLine($"\n Gotcha! {target.Specie.Name} was caught!");
                Thread.Sleep(1500);
                _screenManager.SwitchTo(ScreenType.Story, "MainStoryMenu");
            }
            else
            {
                Console.WriteLine($"\n Oh no! The Pokemon broke free!");
                Thread.Sleep(1500);
                _screenManager.SwitchTo(ScreenType.Battle);
            }
            return true;
        }

        // 2. handle healing item
        if (item is HealingItem potion)
        {
            var activePokeName = _gameSession.Player?.CurrentPokemon?.Specie.Name;

            if (_healingService.ExecuteHealing(potion))
            {
                Console.WriteLine($"\n {activePokeName} recovered {potion.HealAmount} HP!");
                Thread.Sleep(1000);
                
                _screenManager.SwitchTo(ScreenType.Battle);
                return true;
            }
            else
            {
                Console.WriteLine($"\n [!] {activePokeName} is already at full health!");
                Thread.Sleep(1000);
                return false;
            }
        }

        return false;
    }

    public void Shutdown() { }
}