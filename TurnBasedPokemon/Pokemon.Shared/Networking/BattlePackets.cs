using PokemonEntity;

// Data sent from Client -> Server when a player chooses an attack
public class BattleActionRequest
{
    public string RoomId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int MoveIndex { get; set; } // E.g., 0 for Tackle, 1 for Ember
    public Pokemon AttackerPokemon { get; set; } = new();
    // Set server-side only — used for deduplication (ConnectionId is always unique)
    public string ConnectionId { get; set; } = string.Empty;
}

// Data sent from Server -> Both Clients after calculating the turn
public class BattleTurnResult
{
    public List<string> CombatLogs { get; set; } = new();
    public int Player1Hp { get; set; }
    public int Player2Hp { get; set; }
    public bool IsGameOver { get; set; }
    public string WinnerName { get; set; } = string.Empty;
}