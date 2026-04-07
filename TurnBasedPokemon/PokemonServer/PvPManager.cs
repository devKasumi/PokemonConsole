// PokemonServer/Services/PvPManager.cs
using PokemonEntity; // Shared models
using System.Collections.Concurrent;

public class PvPBattleState
{
    public string RoomId { get; set; }
    public PlayerState Player1 { get; set; }
    public PlayerState Player2 { get; set; }
    public Dictionary<string, string> PendingMoves { get; } = new();

    public class PlayerState
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Pokemon ActivePokemon { get; set; } // Current HP, Stats
    }
}

public class PvPManager
{
    private readonly ConcurrentDictionary<string, PvPBattleState> _activeBattles = new();

    public void CreateBattle(string roomId, PvPBattleState state) => _activeBattles[roomId] = state;

    // public BattleResult ProcessTurn(string roomId)
    // {
    //     var battle = _activeBattles[roomId];
    //     // 1. Authoritative Damage Calculation
    //     // Compare Speed -> Execute Moves -> Calculate HP loss
    //     // (Use the exact same formula from your local BattleService here)
        
    //     var result = new BattleResult {
    //         // Fill with damage dealt, fainted status, etc.
    //     };
        
    //     battle.PendingMoves.Clear();
    //     return result;
    // }
}