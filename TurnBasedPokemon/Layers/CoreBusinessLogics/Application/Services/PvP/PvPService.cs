using PokemonEntity;

public class PvPService : IPvPService
{
    private readonly GameSession _gameSession;
    private readonly INetworkService _networkService;
    private readonly IAuthenService _authenService;

    public event Action<string, string>? OnMatchFound;
    public event Action<string>? OnMatchmakingRejected;

    public PvPTurnResult? LastTurnResult => _networkService.LastTurnResult;

    public PvPService(GameSession session, INetworkService networkService, IAuthenService authenService)
    {
        _gameSession = session;
        _networkService = networkService;
        _authenService = authenService;
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

    public void SubscribeTurnResult(Action<PvPTurnResult> handler)
    {
        _networkService.OnTurnResultReceived += handler;
    }

    public void UnsubscribeTurnResult(Action<PvPTurnResult> handler)
    {
        _networkService.OnTurnResultReceived -= handler;
    }

    public void SubmitMove(string roomId, int moveIndex)
    {
        var username = _gameSession.CurrentUser!.Username;
        var pokemon = _gameSession.Player!.PokemonTeam[0];
        _ = _networkService.SubmitMoveAsync(roomId, username, moveIndex, pokemon);
    }

    /// <summary>
    /// Records win/loss and saves progress. Returns true if the current player won.
    /// </summary>
    public bool RecordMatchResult(string winnerName)
    {
        if (_gameSession.Player == null || _gameSession.CurrentUser == null) return false;

        bool isWin = _gameSession.CurrentUser.Username == winnerName;
        if (isWin)
            _gameSession.Player.PvPWins++;
        else
            _gameSession.Player.PvPLosses++;

        _authenService.SaveProgress();
        return isWin;
    }
}
