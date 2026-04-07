using PokemonEntity;

namespace Screens
{
    public interface IScreen
    {
        // Allow for getting data 
        void Initialize(object? data = null);

        void Update();

        void Shutdown();
    }
}