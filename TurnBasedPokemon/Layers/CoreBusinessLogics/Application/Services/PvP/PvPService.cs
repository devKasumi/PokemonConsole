using PokemonEntity;

public class PvPService : IPvPService
{
    private readonly GameSession _gameSession;
    private readonly INetworkService _networkService;

    public event Action<string, string>? OnMatchFound;
    public event Action<string>? OnMatchmakingRejected;

    public PvPService(GameSession session, INetworkService networkService)
    {
        _gameSession = session;
        _networkService = networkService;
    }

    public bool CanAccessPvP()
    {
        return _gameSession.Player != null && _gameSession.Player.IsChampion;
    }

    public bool Connect()
    {
        return _networkService.Connect().GetAwaiter().GetResult();
    }

    public void FindMatch()
    {
        // Forward events from NetworkService
        _networkService.OnMatchFound += (roomId, opponent) => OnMatchFound?.Invoke(roomId, opponent);
        _networkService.OnMatchmakingRejected += (reason) => OnMatchmakingRejected?.Invoke(reason);

        _networkService.FindMatch(
            _gameSession.CurrentUser!.Username,
            _gameSession.Player!.PokemonTeam
        ).GetAwaiter().GetResult();
    }

    public void CancelMatchmaking()
    {
        _networkService.CancelMatchmakingAsync().GetAwaiter().GetResult();
    }
}
