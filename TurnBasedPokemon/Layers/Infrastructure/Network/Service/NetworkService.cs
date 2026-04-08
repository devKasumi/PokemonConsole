using Microsoft.AspNetCore.SignalR.Client;
using PokemonEntity;
using System;
using System.Threading.Tasks;

public class NetworkService : INetworkService
{
    private HubConnection _connection;
    
    public event Action<string, string>? OnMatchFound;
    public event Action? OnWaiting;
    public event Action<string>? OnMatchmakingRejected;
    
    public event Action<PvPTurnResult>? OnTurnResultReceived;

    /// <summary>
    /// Caches the most recent TurnResult so PvPScreen can read it on initialization
    /// (solves race condition where server sends initial state before screen subscribes).
    /// </summary>
    public PvPTurnResult? LastTurnResult { get; private set; }
    
    public NetworkService()
    {
        // Address of your PokemonServer (must match server's launchSettings.json port)
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5237/battlehub")
            .WithAutomaticReconnect()
            .Build();

        // Listen for match-making messages from Server
        _connection.On<string, string>("MatchFound", (roomId, opponentName) => {
            OnMatchFound?.Invoke(roomId, opponentName);
        });

        _connection.On("WaitingForOpponent", () => {
            OnWaiting?.Invoke();
        });

        _connection.On<string>("MatchmakingRejected", (reason) => {
            OnMatchmakingRejected?.Invoke(reason);
        });

        // UPDATE 2: Listen for the strongly-typed PvPTurnResult object
        _connection.On<PvPTurnResult>("ReceiveTurnResult", (result) => {
            LastTurnResult = result;
            OnTurnResultReceived?.Invoke(result);
        });
    }

    public async Task<bool> Connect()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("[Network] Client connected to server.");
                return true;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Network] Failed to connect. Server is not running.");
                Console.ResetColor();
                return false;
            }
        }
        return true; // Already connected
    }

    public async Task FindMatch(string username, List<Pokemon> party)
    {
        await _connection.InvokeAsync("FindMatch", username, party);
    }

    // UPDATE 3: Use the BattleActionRequest packet to send the move
    public async Task SubmitMoveAsync(string roomId, string playerName, int moveIndex, Pokemon attackerPokemon)
    {
        var request = new BattleActionRequest 
        { 
            RoomId = roomId, 
            PlayerName = playerName, 
            MoveIndex = moveIndex,
            AttackerPokemon = attackerPokemon
        };
        
        // Match the exact method name "SubmitMove" on the BattleHub server
        await _connection.InvokeAsync("SubmitMove", request);
    }

    public async Task CancelMatchmakingAsync()
    {
        await _connection.InvokeAsync("CancelMatchmaking");
    }
}