using PokemonEntity;

public interface IBattleService
{
    public BattleTurnResult ExecutePlayerTurn(PokemonMove move);

    public BattleTurnResult ExecuteEnemyTurn();

    public ProcessExpResult ProcessVictory();

    public bool CanSwitchPokemon(Pokemon target);
}