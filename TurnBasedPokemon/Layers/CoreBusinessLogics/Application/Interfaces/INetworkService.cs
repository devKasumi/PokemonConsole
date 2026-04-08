using PokemonEntity;
using System;
using System.Threading.Tasks;

public interface INetworkService
{
    event Action<string, string>? OnMatchFound;
    event Action? OnWaiting;
    event Action<string>? OnMatchmakingRejected;
    event Action<PvPTurnResult>? OnTurnResultReceived;
    PvPTurnResult? LastTurnResult { get; }

    Task<bool> Connect();
    Task FindMatch(string username, List<Pokemon> party);
    Task SubmitMoveAsync(string roomId, string playerName, int moveIndex, Pokemon attackerPokemon);
    Task CancelMatchmakingAsync();
}
