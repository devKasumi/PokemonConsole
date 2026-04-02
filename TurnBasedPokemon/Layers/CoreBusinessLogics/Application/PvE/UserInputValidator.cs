using System;
using System.Text.RegularExpressions;

public static class UserInputValidator
{
    private static readonly Regex LoginMenuRegex = new Regex("^[1-3]$");
    private static readonly Regex BattleMenuRegex = new Regex("^[1-4]$");
    private static readonly Regex MainMenuRegex = new Regex("^[1-5]$");
    // private static readonly Regex InventoryMenuRegex = new Regex("^[1-3]$");
    private static readonly Regex InventoryMenuRegex = LoginMenuRegex;
    private static readonly Regex PokemonCenterMenuRegex = LoginMenuRegex;
    private static readonly Regex CommandMenuRegex = BattleMenuRegex;

    public static bool ValidateLoginMenuInput(string input)
        => LoginMenuRegex.IsMatch(input);

    public static bool ValidateMainMenuInput(string input)
        => MainMenuRegex.IsMatch(input);

    public static bool ValidateBattleMenuInput(string input)
        => BattleMenuRegex.IsMatch(input);

    public static bool ValidateInventoryMenuInput(string input)
        => InventoryMenuRegex.IsMatch(input);

    public static bool ValidatePokemonCenterMenuInput(string input)
        => PokemonCenterMenuRegex.IsMatch(input);

    public static bool ValidateCommandMenuInput(string input)
        => CommandMenuRegex.IsMatch(input);
}