using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokemonServer.Services;

namespace PokemonServer.Hubs
{
    public class BattleHub : Hub
    {
        private readonly MatchmakingService _matchmakingService;

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