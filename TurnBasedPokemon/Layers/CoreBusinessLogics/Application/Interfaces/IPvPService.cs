using PokemonEntity;

public interface IPvPService
{
    bool CanAccessPvP();
    bool Connect();
    void FindMatch();
    void CancelMatchmaking();

    event Action<string, string>? OnMatchFound;
    event Action<string>? OnMatchmakingRejected;
}
