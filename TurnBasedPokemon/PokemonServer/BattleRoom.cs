using System;

namespace PokemonServer.Models
{
    public class WaitingPlayer
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class BattleRoom
    {
        private static int _roomCounter = 0;
        public string RoomId { get; set; } = $"BATTLE-{Interlocked.Increment(ref _roomCounter):D4}";
        public WaitingPlayer Player1 { get; set; } = new WaitingPlayer();
        public WaitingPlayer Player2 { get; set; } = new WaitingPlayer();
        
        // We will add more states here later (e.g., Turn limits, current HP)
        public bool IsActive { get; set; } = true;
    }
}