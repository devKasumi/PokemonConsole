using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokemonServer.Services;
using System.Collections.Concurrent;

namespace PokemonServer.Hubs
{
    public class BattleHub : Hub
    {
        private readonly MatchmakingService _matchmakingService;
        // Dictionary to temporarily hold moves: Key = RoomId, Value = List of (ConnectionId, MoveName)
        private static readonly ConcurrentDictionary<string, List<(string PlayerId, string Move)>> _pendingMoves = new();

        public BattleHub(MatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        /// <summary>
        /// Called by the Client when they want to find a PvP match.
        /// </summary>
        public async Task FindMatch(string username)
        {
            string currentConnectionId = Context.ConnectionId;
            
            // Try to put the player in a room
            var room = _matchmakingService.JoinQueue(currentConnectionId, username);

            if (room != null)
            {
                // Match Found! Add both players to a SignalR Group using the RoomId
                await Groups.AddToGroupAsync(room.Player1.ConnectionId, room.RoomId);
                await Groups.AddToGroupAsync(room.Player2.ConnectionId, room.RoomId);

                // Broadcast to Player 1 that they found Player 2
                await Clients.Client(room.Player1.ConnectionId).SendAsync("MatchFound", room.RoomId, room.Player2.Username);
                
                // Broadcast to Player 2 that they found Player 1
                await Clients.Client(room.Player2.ConnectionId).SendAsync("MatchFound", room.RoomId, room.Player1.Username);
            }
            else
            {
                // Still waiting. Tell the caller to hang tight.
                await Clients.Caller.SendAsync("WaitingForOpponent");
            }
        }

        public async Task SendMove(string roomId, string moveName)
        {
            var moves = _pendingMoves.GetOrAdd(roomId, _ => new List<(string, string)>());
            
            lock (moves)
            {
                moves.Add((Context.ConnectionId, moveName));
            }

            if (moves.Count == 2)
            {
                // Logic: Both players have moved! 
                // 1. Calculate damage/speed here (Authoritative Server)
                // 2. For now, we'll just send a "Result Packet" back to the Group
                string resultData = $"Player {moves[0].PlayerId} used {moves[0].Move}, Player {moves[1].PlayerId} used {moves[1].Move}!";
                
                await Clients.Group(roomId).SendAsync("ReceiveTurnResult", resultData);
                
                // Clear moves for the next turn
                _pendingMoves.TryRemove(roomId, out _);
            }
            else
            {
                // Tell the player to wait for the opponent
                await Clients.Caller.SendAsync("WaitingForOpponentMove");
            }
        }

        /// <summary>
        /// Handles when a player disconnects unexpectedly (e.g., closing the console).
        /// </summary>
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            Console.WriteLine($"[Network] Client disconnected: {Context.ConnectionId}");
            // TODO: Handle surrendering if they were in an active BattleRoom
            await base.OnDisconnectedAsync(exception);
        }
    }
}