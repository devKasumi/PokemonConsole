public interface IExpService
{
    public ProcessExpResult GrantExperience(Pokemon pokemon, int amount);

    public int CalculateExpGain(Pokemon playerPoke, Pokemon enemyPoke);
}