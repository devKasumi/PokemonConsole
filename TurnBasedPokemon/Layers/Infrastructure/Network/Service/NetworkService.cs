using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

public class NetworkService
{
    private HubConnection _connection;
    
    public event Action<string, string>? OnMatchFound;
    public event Action? OnWaiting;
    
    // UPDATE 1: Change from Action<string> to Action<BattleTurnResult>
    public event Action<PvPTurnResult>? OnTurnResultReceived;
    
    public NetworkService()
    {
        // Address of your PokemonServer
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/battlehub")
            .WithAutomaticReconnect()
            .Build();

        // Listen for match-making messages from Server
        _connection.On<string, string>("MatchFound", (roomId, opponentName) => {
            OnMatchFound?.Invoke(roomId, opponentName);
        });

        _connection.On("WaitingForOpponent", () => {
            OnWaiting?.Invoke();
        });

        // UPDATE 2: Listen for the strongly-typed PvPTurnResult object
        _connection.On<PvPTurnResult>("ReceiveTurnResult", (result) => {
            OnTurnResultReceived?.Invoke(result);
        });
    }

    public async Task Connect()
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync();
    }

    public async Task FindMatch(string username)
    {
        await _connection.InvokeAsync("FindMatch", username);
    }

    // UPDATE 3: Use the BattleActionRequest packet to send the move
    public async Task SubmitMoveAsync(string roomId, string playerName, int moveIndex)
    {
        var request = new BattleActionRequest 
        { 
            RoomId = roomId, 
            PlayerName = playerName, 
            MoveIndex = moveIndex 
        };
        
        // Match the exact method name "SubmitMove" on the BattleHub server
        await _connection.InvokeAsync("SubmitMove", request);
    }
}