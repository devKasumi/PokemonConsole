using System.Collections.Concurrent;
using PokemonEntity;
using PokemonServer.Models;

namespace PokemonServer.Services
{
    public class MatchmakingService
    {
        // Use a dictionary instead of a queue so we can remove cancelled players
        private readonly ConcurrentDictionary<string, WaitingPlayer> _waitingPlayers = new();
        private readonly object _matchLock = new();
        
        // Dictionary to store active battle rooms (Key: RoomId, Value: BattleRoom)
        private readonly ConcurrentDictionary<string, BattleRoom> _activeRooms = new();

        // Temporary storage for player parties before match is made
        private readonly ConcurrentDictionary<string, List<Pokemon>> _pendingParties = new();

        /// <summary>
        /// Store a player's party temporarily until a match is found.
        /// </summary>
        public void StoreParty(string connectionId, List<Pokemon> party)
        {
            _pendingParties[connectionId] = party;
        }

        /// <summary>
        /// Retrieve and remove a stored party for a connection.
        /// </summary>
        public List<Pokemon> GetParty(string connectionId)
        {
            _pendingParties.TryRemove(connectionId, out var party);
            return party ?? new List<Pokemon>();
        }

        /// <summary>
        /// Attempts to add a player to the queue and match them if possible.
        /// Returns: (room, rejected). If rejected=true, same account is already queued.
        /// </summary>
        public (BattleRoom? room, bool rejected) JoinQueue(string connectionId, string username)
        {
            var newPlayer = new WaitingPlayer { ConnectionId = connectionId, Username = username };

            lock (_matchLock)
            {
                // Prevent the same account from queuing twice
                if (_waitingPlayers.Values.Any(p => p.Username == username))
                {
                    Console.WriteLine($"[Matchmaking] Rejected: {username} is already in the queue.");
                    return (null, true);
                }

                // Find another waiting player (not ourselves, and not the same username)
                var opponent = _waitingPlayers.Values.FirstOrDefault(p => p.ConnectionId != connectionId && p.Username != username);

                if (opponent != null)
                {
                    // Remove the opponent from the waiting list
                    _waitingPlayers.TryRemove(opponent.ConnectionId, out _);

                    var room = new BattleRoom
                    {
                        Player1 = opponent,
                        Player2 = newPlayer
                    };

                    _activeRooms.TryAdd(room.RoomId, room);
                    Console.WriteLine($"[Matchmaking] Match found! Room: {room.RoomId} ({opponent.Username} vs {newPlayer.Username})");
                    return (room, false);
                }
                else
                {
                    // No opponent available — add to waiting list
                    _waitingPlayers[connectionId] = newPlayer;
                    Console.WriteLine($"[Matchmaking] Player {username} joined the queue. Total waiting: {_waitingPlayers.Count}");
                    return (null, false);
                }
            }
        }

        /// <summary>
        /// Remove a player from the matchmaking queue (when they cancel or disconnect).
        /// </summary>
        public void LeaveQueue(string connectionId)
        {
            if (_waitingPlayers.TryRemove(connectionId, out var player))
            {
                Console.WriteLine($"[Matchmaking] Player {player.Username} left the queue.");
            }
            _pendingParties.TryRemove(connectionId, out _);
        }

        public BattleRoom? GetRoom(string roomId)
        {
            _activeRooms.TryGetValue(roomId, out var room);
            return room;
        }
    }
}