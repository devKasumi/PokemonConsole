public record CatchResult
{
    public bool IsCaught { get; init; }
    public int Shakes { get; init; }
    public string DisplayMessage { get; init; } = string.Empty;
    public ConsoleColor MessageColor { get; init; } = ConsoleColor.White;

    public static CatchResult Success(int shakes, string pokemonName) => new()
    {
        IsCaught = true,
        Shakes = shakes,
        DisplayMessage = $"Gotcha! {pokemonName} was caught!",
        MessageColor = ConsoleColor.Green
    };

    public static CatchResult Failure(int shakes, string pokemonName) => new()
    {
        IsCaught = false,
        Shakes = shakes,
        DisplayMessage = shakes switch
        {
            0 => $"Oh no! The ball missed the {pokemonName}!",
            3 => $"Aww! It appeared to be caught! {pokemonName} broke free!",
            _ => $"Oh no! The {pokemonName} broke free!"
        },
        MessageColor = ConsoleColor.Red
    };
}