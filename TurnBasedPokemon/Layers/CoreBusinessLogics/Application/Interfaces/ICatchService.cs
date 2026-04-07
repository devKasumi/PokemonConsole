using PokemonEntity;

public interface ICatchService
{
    public CatchResult ExecuteCapture(CaptureItem item, Pokemon target);
}