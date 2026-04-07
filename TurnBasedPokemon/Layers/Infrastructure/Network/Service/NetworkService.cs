using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

public class NetworkService
{
    private HubConnection _connection;
    public event Action<string, string>? OnMatchFound;
    public event Action? OnWaiting;
    public event Action<string>? OnTurnResultReceived;

    public NetworkService()
    {
        // Address of your PokemonServer
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/battlehub")
            .WithAutomaticReconnect()
            .Build();

        // Listen for messages from Server
        _connection.On<string, string>("MatchFound", (roomId, opponentName) => {
            OnMatchFound?.Invoke(roomId, opponentName);
        });

        _connection.On("WaitingForOpponent", () => {
            OnWaiting?.Invoke();
        });

        // Listen for the calculated result from the Server
        _connection.On<string>("ReceiveTurnResult", (resultJson) => {
            OnTurnResultReceived?.Invoke(resultJson);
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

    // Method to send your move to the Hub
    public async Task SendBattleMove(string roomId, string moveName)
    {
        await _connection.InvokeAsync("SendMove", roomId, moveName);
    }
}