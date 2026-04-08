using PokemonEntity;
using Application.RepoInterfaces;

// File: Application/Services/PokemonSpawner.cs
public class PokemonSpawner
{
    // private readonly PokedexRepository _pokedexRepo;
    private readonly IPokedexRepository _pokedexRepo;

    public PokemonSpawner(IPokedexRepository pokedexRepo)
    {
        _pokedexRepo = pokedexRepo;
    }

    public Pokemon SpawnPokemon(string name, int level)
    {
        // 1. Retrieve the species template from the repository
        PokemonSpecies? originalSpecie = _pokedexRepo.GetSpecies(name);
        if (originalSpecie == null) return null;

        // 2. Clone the template to ensure this instance doesn't modify the global Pokedex data
        PokemonSpecies instanceSpecie = CloneSpecie(originalSpecie);

        // 3. Select up to 4 most recently learned moves based on the current level
        List<PokemonMove> movesForLevel = instanceSpecie.MoveSet
            .Where(m => m.LevelLearned <= level)
            .OrderByDescending(m => m.LevelLearned)
            .Take(4)
            .ToList();

        // 4. Calculate dynamic stats using a standard scaling formula
        int calculatedHP = originalSpecie.BaseStats.HP + (int)(originalSpecie.BaseStats.HP / 50.0 * level);
        int calculatedAtk = originalSpecie.BaseStats.Attack + (int)(originalSpecie.BaseStats.Attack / 50.0 * level);
        int calculatedDef = originalSpecie.BaseStats.Defense + (int)(originalSpecie.BaseStats.Defense / 50.0 * level);
        int calculatedSpAtk = originalSpecie.BaseStats.SpAttack + (int)(originalSpecie.BaseStats.SpAttack / 50.0 * level);
        int calculatedSpDef = originalSpecie.BaseStats.SpDefense + (int)(originalSpecie.BaseStats.SpDefense / 50.0 * level);
        int calculatedSpeed = originalSpecie.BaseStats.Speed + (int)(originalSpecie.BaseStats.Speed / 50.0 * level);

        // 5. Apply the calculated stats to the cloned species instance
        instanceSpecie.BaseStats = new Stats {
            HP = calculatedHP,
            Attack = calculatedAtk,
            Defense = calculatedDef,
            SpAttack = calculatedSpAtk,
            SpDefense = calculatedSpDef,
            Speed = calculatedSpeed
        };

        // 6. Instantiate the Pokemon and assign its learned moves
        Pokemon pokemon = new Pokemon(instanceSpecie, level, calculatedHP, calculatedHP);
        
        // Ensure the Moves list is properly synchronized with the moves identified above
        pokemon.UpdateMoveSet(movesForLevel);

        return pokemon;
    }

    public Pokemon SpawnPokemonById(int evolutionId, int currentLevel)
    {
        // 1. Get the new species from your repository
        PokemonSpecies? nextStage = _pokedexRepo.GetSpeciesById(evolutionId);
        if (nextStage == null) return null;

        // 2. Calculate new stats based on the new species' base stats
        int newMaxHP = nextStage.BaseStats.HP + (int)(nextStage.BaseStats.HP / 50.0 * currentLevel) + currentLevel + 10;
        
        var evolvedStats = new Stats {
            HP = newMaxHP,
            Attack = nextStage.BaseStats.Attack + (int)(nextStage.BaseStats.Attack / 50.0 * currentLevel) + 5,
            Defense = nextStage.BaseStats.Defense + (int)(nextStage.BaseStats.Defense / 50.0 * currentLevel) + 5,
            SpAttack = nextStage.BaseStats.SpAttack + (int)(nextStage.BaseStats.SpAttack / 50.0 * currentLevel) + 5,
            SpDefense = nextStage.BaseStats.SpDefense + (int)(nextStage.BaseStats.SpDefense / 50.0 * currentLevel) + 5,
            Speed = nextStage.BaseStats.Speed + (int)(nextStage.BaseStats.Speed / 50.0 * currentLevel) + 5
        };

        // 3. Clone the species to avoid modifying the global template
        PokemonSpecies instanceSpecie = CloneSpecie(nextStage);
        instanceSpecie.BaseStats = evolvedStats;

        // --- ADDED MOVE LOGIC START ---
        // 4. Select moves appropriate for the current level from the new species' moveset
        List<PokemonMove> movesForLevel = instanceSpecie.MoveSet
            .Where(m => m.LevelLearned <= currentLevel)
            .OrderByDescending(m => m.LevelLearned)
            .Take(4)
            .ToList();
        // --- ADDED MOVE LOGIC END ---

        // 5. Create the Pokemon object
        Pokemon pokemon = new Pokemon(instanceSpecie, currentLevel, newMaxHP, newMaxHP);
        
        // 6. Assign the moves to the new instance
        pokemon.UpdateMoveSet(movesForLevel);

        return pokemon;
    }

    private PokemonSpecies CloneSpecie(PokemonSpecies original)
    {
        // Create a brand new instance in memory
        return new PokemonSpecies
        {
            Id = original.Id,
            Name = original.Name,
            Types = new List<ElementType>(original.Types),
            CatchRate = original.CatchRate,
            BaseExp = original.BaseExp,
            EvolutionId = original.EvolutionId,
            EvolutionLevel = original.EvolutionLevel,
            
            // IMPORTANT: Create a NEW Stats object, don't just point to the old one
            BaseStats = new Stats
            {
                HP = original.BaseStats.HP,
                Attack = original.BaseStats.Attack,
                Defense = original.BaseStats.Defense,
                SpAttack = original.BaseStats.SpAttack,
                SpDefense = original.BaseStats.SpDefense,
                Speed = original.BaseStats.Speed
            },
            
            // Copy the move set list
            MoveSet = new List<PokemonMove>(original.MoveSet)
        };
    }
}