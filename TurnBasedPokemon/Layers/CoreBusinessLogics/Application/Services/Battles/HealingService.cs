public class HealingService : IHealingService
{
    private readonly GameSession _gameSession;

    public HealingService(GameSession session) 
    { 
        _gameSession = session; 
    }

    public bool ExecuteHealing(HealingItem potion)
    {
        var player = _gameSession.Player;
        var target = player?.CurrentPokemon;

        if (player == null || target == null) return false;

        if (target.CurrentHP >= target.MaxHP)
        {
            return false;
        }

        target.Heal(potion.HealAmount);

        player.RemoveItem(potion);

        return true;
    }
}