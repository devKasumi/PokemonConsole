using System.Collections.Generic;
using PokemonEntity;

public class PvPBattleState
{
    public string RoomId { get; set; } = string.Empty;
    public string Player1Name { get; set; } = string.Empty;
    public string Player2Name { get; set; } = string.Empty;
    public List<Pokemon> Player1Party { get; set; } = new();
    public List<Pokemon> Player2Party { get; set; } = new();
    public int Player1ActiveIndex { get; set; } = 0;
    public int Player2ActiveIndex { get; set; } = 0;
    public bool IsGameOver { get; set; } = false;
    public string? WinnerName { get; set; }

    public Pokemon Player1Active => Player1Party.Count > Player1ActiveIndex ? Player1Party[Player1ActiveIndex] : null;
    public Pokemon Player2Active => Player2Party.Count > Player2ActiveIndex ? Player2Party[Player2ActiveIndex] : null;
}