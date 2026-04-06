using System.Collections.Concurrent;
using PokemonServer.Models;

namespace PokemonServer.Services
{
    public class MatchmakingService
    {
        // Thread-safe queue for players waiting for a match
        private readonly ConcurrentQueue<WaitingPlayer> _waitingQueue = new();
        
        // Dictionary to store active battle rooms (Key: RoomId, Value: BattleRoom)
        private readonly ConcurrentDictionary<string, BattleRoom> _activeRooms = new();

        /// <summary>
        /// Attempts to add a player to the queue and match them if possible.
        /// </summary>
        public BattleRoom? JoinQueue(string connectionId, string username)
        {
            var newPlayer = new WaitingPlayer { ConnectionId = connectionId, Username = username };
            _waitingQueue.Enqueue(newPlayer);

            Console.WriteLine($"[Matchmaking] Player {username} joined the queue. Total waiting: {_waitingQueue.Count}");

            // If there are at least 2 players waiting, create a match!
            if (_waitingQueue.Count >= 2)
            {
                if (_waitingQueue.TryDequeue(out var player1) && _waitingQueue.TryDequeue(out var player2))
                {
                    var room = new BattleRoom
                    {
                        Player1 = player1,
                        Player2 = player2
                    };

                    _activeRooms.TryAdd(room.RoomId, room);
                    Console.WriteLine($"[Matchmaking] Match found! Room: {room.RoomId} ({player1.Username} vs {player2.Username})");
                    
                    return room;
                }
            }

            return null; // Not enough players yet, keep waiting
        }

        public BattleRoom? GetRoom(string roomId)
        {
            _activeRooms.TryGetValue(roomId, out var room);
            return room;
        }
    }
}