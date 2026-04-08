using PokemonEntity;

public interface IPvPService
{
    bool CanAccessPvP();
    bool Connect();
    void FindMatch();
    void CancelMatchmaking();

    void SubscribeTurnResult(Action<PvPTurnResult> handler);
    void UnsubscribeTurnResult(Action<PvPTurnResult> handler);
    PvPTurnResult? LastTurnResult { get; }
    void SubmitMove(string roomId, int moveIndex);
    bool RecordMatchResult(string winnerName);

    event Action<string, string>? OnMatchFound;
    event Action<string>? OnMatchmakingRejected;
}
