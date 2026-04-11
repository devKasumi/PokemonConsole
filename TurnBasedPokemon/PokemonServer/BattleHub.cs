using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using PokemonServer.Services;
using PokemonEntity;

namespace PokemonServer.Hubs
{
    public class BattleHub : Hub
    {
        private readonly MatchmakingService _matchmakingService;
        private readonly PvPManager _pvpManager;
        private readonly IServiceProvider _serviceProvider;

        public BattleHub(MatchmakingService matchmakingService, PvPManager pvpManager, IServiceProvider serviceProvider)
        {
            _matchmakingService = matchmakingService;
            _pvpManager = pvpManager;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Called by the Client when they want to find a PvP match.
        /// </summary>
        public async Task FindMatch(string username, List<Pokemon> party)
        {
            string currentConnectionId = Context.ConnectionId;
            
            // Store the player's party temporarily for battle initialization
            _matchmakingService.StoreParty(currentConnectionId, party);

            // Try to put the player in a room
            var (room, rejected) = _matchmakingService.JoinQueue(currentConnectionId, username);

            if (rejected)
            {
                await Clients.Caller.SendAsync("MatchmakingRejected", "This account is already in the matchmaking queue.");
                return;
            }

            if (room != null)
            {
                // Retrieve both players' parties
                var party1 = _matchmakingService.GetParty(room.Player1.ConnectionId);
                var party2 = _matchmakingService.GetParty(room.Player2.ConnectionId);

                if (party1.Count > 0 && party2.Count > 0)
                {
                    _pvpManager.InitializeBattle(room.RoomId, room.Player1.Username, party1, room.Player2.Username, party2);
                    Console.WriteLine($"[PvP] Battle initialized for room {room.RoomId}");
                }
                else
                {
                    Console.WriteLine($"[WARN] One or both parties are empty for room {room.RoomId}.");
                }

                // Match Found! Add both players to a SignalR Group using the RoomId
                await Groups.AddToGroupAsync(room.Player1.ConnectionId, room.RoomId);
                await Groups.AddToGroupAsync(room.Player2.ConnectionId, room.RoomId);

                // Send initial battle state so clients can render before first move
                var state = _pvpManager.GetBattleState(room.RoomId);
                if (state != null)
                {
                    var initialResult = PvPTurnResult.Continue(
                        state.Player1Active?.CurrentHP ?? 0,
                        state.Player2Active?.CurrentHP ?? 0,
                        new List<string> { "Battle started! Choose your move." },
                        state.Player1Active != null ? new PvPMonState
                        {
                            Name = state.Player1Active.Specie.Name,
                            CurrentHP = state.Player1Active.CurrentHP,
                            MaxHP = state.Player1Active.MaxHP,
                            IsFainted = state.Player1Active.IsFainted,
                            Moves = state.Player1Active.Moves.Select(m => new PvPMoveState
                            {
                                Name = m.Name, Type = m.Type.ToString(), Power = m.Power, PP = m.PP
                            }).ToList()
                        } : null,
                        state.Player2Active != null ? new PvPMonState
                        {
                            Name = state.Player2Active.Specie.Name,
                            CurrentHP = state.Player2Active.CurrentHP,
                            MaxHP = state.Player2Active.MaxHP,
                            IsFainted = state.Player2Active.IsFainted,
                            Moves = state.Player2Active.Moves.Select(m => new PvPMoveState
                            {
                                Name = m.Name, Type = m.Type.ToString(), Power = m.Power, PP = m.PP
                            }).ToList()
                        } : null
                    );

                    // Send initial state to both players
                    await Clients.Client(room.Player1.ConnectionId).SendAsync("ReceiveTurnResult", initialResult);
                    // For Player2, swap the active Pokémon perspective
                    var p2Result = PvPTurnResult.Continue(
                        initialResult.Player2Hp, initialResult.Player1Hp,
                        initialResult.CombatLogs,
                        initialResult.Player2Active, initialResult.Player1Active
                    );
                    await Clients.Client(room.Player2.ConnectionId).SendAsync("ReceiveTurnResult", p2Result);
                }

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

        // Track turn timers: when the first player submits, start a 60s countdown for the second
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _turnTimers = new();

        public async Task SubmitMove(BattleActionRequest request)
        {
            // Resolve player identity server-side from ConnectionId (not client-sent name)
            var room = _matchmakingService.GetRoom(request.RoomId);
            if (room != null)
            {
                var state = _pvpManager.GetBattleState(request.RoomId);
                if (state != null)
                {
                    if (Context.ConnectionId == room.Player1.ConnectionId)
                        request.PlayerName = state.Player1Name;
                    else if (Context.ConnectionId == room.Player2.ConnectionId)
                        request.PlayerName = state.Player2Name;
                }
            }

            Console.WriteLine($"[PvP] SubmitMove from {request.PlayerName} (conn: {Context.ConnectionId}) in room {request.RoomId}, move: {request.MoveIndex}");

            _pvpManager.RegisterMove(request, Context.ConnectionId);

            if (_pvpManager.IsRoomReady(request.RoomId))
            {
                // Cancel the timeout timer since both moves are in
                if (_turnTimers.TryRemove(request.RoomId, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }

                var result = _pvpManager.ProcessTurn(request.RoomId);

                if (room != null)
                {
                    // Send perspective-correct results to each player
                    // Player1 sees result as-is (Player1Active = their own)
                    await Clients.Client(room.Player1.ConnectionId).SendAsync("ReceiveTurnResult", result);

                    // Player2 gets swapped perspective (their Pokémon = Player1Active)
                    PvPTurnResult p2Result;
                    if (result.IsGameOver)
                    {
                        p2Result = PvPTurnResult.GameOver(
                            result.WinnerName!, result.CombatLogs,
                            result.Player2Active, result.Player1Active
                        );
                    }
                    else
                    {
                        p2Result = PvPTurnResult.Continue(
                            result.Player2Hp, result.Player1Hp,
                            result.CombatLogs,
                            result.Player2Active, result.Player1Active
                        );
                    }
                    await Clients.Client(room.Player2.ConnectionId).SendAsync("ReceiveTurnResult", p2Result);
                }
                else
                {
                    // Fallback: room not found, send to group (shouldn't happen)
                    await Clients.Group(request.RoomId).SendAsync("ReceiveTurnResult", result);
                }
            }
            else
            {
                // First player submitted — start 60s timer for the opponent
                StartTurnTimer(request.RoomId);
                await Clients.Caller.SendAsync("WaitingForOpponentMove");
            }
        }

        private void StartTurnTimer(string roomId)
        {
            // Only start one timer per turn
            if (_turnTimers.ContainsKey(roomId)) return;

            var cts = new CancellationTokenSource();
            _turnTimers[roomId] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), cts.Token);
                }
                catch (TaskCanceledException)
                {
                    return; // Both players submitted in time
                }

                // Timeout: find who hasn't submitted and auto-skip them
                _turnTimers.TryRemove(roomId, out _);

                var state = _pvpManager.GetBattleState(roomId);
                if (state == null || state.IsGameOver) return;

                // Register a skip move (MoveIndex = -1) for the missing player
                var missingPlayer = _pvpManager.GetMissingPlayer(roomId);
                if (missingPlayer != null)
                {
                    Console.WriteLine($"[PvP] Timeout! {missingPlayer.Value.playerName} skipped their turn in room {roomId}");
                    _pvpManager.RegisterMove(new BattleActionRequest
                    {
                        RoomId = roomId,
                        PlayerName = missingPlayer.Value.playerName,
                        MoveIndex = -1
                    }, missingPlayer.Value.connectionId);

                    if (_pvpManager.IsRoomReady(roomId))
                    {
                        var result = _pvpManager.ProcessTurn(roomId);
                        var room = _matchmakingService.GetRoom(roomId);

                        if (room != null)
                        {
                            var hubContext = _serviceProvider.GetRequiredService<IHubContext<BattleHub>>();
                            await hubContext.Clients.Client(room.Player1.ConnectionId).SendAsync("ReceiveTurnResult", result);

                            PvPTurnResult p2Result;
                            if (result.IsGameOver)
                            {
                                p2Result = PvPTurnResult.GameOver(
                                    result.WinnerName!, result.CombatLogs,
                                    result.Player2Active, result.Player1Active
                                );
                            }
                            else
                            {
                                p2Result = PvPTurnResult.Continue(
                                    result.Player2Hp, result.Player1Hp,
                                    result.CombatLogs,
                                    result.Player2Active, result.Player1Active
                                );
                            }
                            await hubContext.Clients.Client(room.Player2.ConnectionId).SendAsync("ReceiveTurnResult", p2Result);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Called by the client when they cancel matchmaking (press Q).
        /// </summary>
        public async Task CancelMatchmaking()
        {
            _matchmakingService.LeaveQueue(Context.ConnectionId);
            Console.WriteLine($"[Matchmaking] Player cancelled matchmaking: {Context.ConnectionId}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles when a player disconnects unexpectedly (e.g., closing the console, network loss).
        /// </summary>
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            string disconnectedId = Context.ConnectionId;
            Console.WriteLine($"[Network] Client disconnected: {disconnectedId}");

            // 1. Clean up: remove from queue if they were just waiting
            _matchmakingService.LeaveQueue(disconnectedId);

            // 2. Check if the player was in the middle of an active battle
            var activeRoom = _matchmakingService.GetRoomByConnectionId(disconnectedId);
            
            if (activeRoom != null)
            {
                Console.WriteLine($"[PvP] Player dropped from active room {activeRoom.RoomId}. Granting default win.");

                // Calculate the default win result
                var disconnectResult = _pvpManager.HandlePlayerDisconnect(
                    activeRoom.RoomId, 
                    disconnectedId, 
                    activeRoom.Player1.ConnectionId, 
                    activeRoom.Player2.ConnectionId
                );

                if (disconnectResult != null)
                {
                    // Find the Connection ID of the player who survived
                    string remainingConnId = (activeRoom.Player1.ConnectionId == disconnectedId) 
                        ? activeRoom.Player2.ConnectionId 
                        : activeRoom.Player1.ConnectionId;

                    // Send the "You Win" screen to the remaining player
                    await Clients.Client(remainingConnId).SendAsync("ReceiveTurnResult", disconnectResult);
                }

                // 3. Remove the room completely from server memory
                _matchmakingService.RemoveRoom(activeRoom.RoomId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}