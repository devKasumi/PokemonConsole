using System;
using System.Collections.Generic;
using System.Linq;

public enum PokemonStatus { None, Asleep, Frozen, Paralyzed, Burned, Poisoned }

public class Pokemon
{
    public PokemonSpecies Specie { get; set; }
    public int Level { get; set; }
    public int TotalExp { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public List<PokemonMove> Moves { get; set; }
    public bool IsFainted { get; set; }
    public PokemonStatus Status { get; set; } = PokemonStatus.None;

    public Pokemon() { }

    public Pokemon(PokemonSpecies pokemonSpecie, int level, int currentHp, int maxHp)
    {
        Specie = pokemonSpecie;
        Level = level;
        CurrentHP = currentHp;
        MaxHP = maxHp;
        Moves = new List<PokemonMove>();
        IsFainted = (currentHp <= 0);
    }

    public void UpdateMoveSet(List<PokemonMove> newMoves)
    {
        // Clear the current active moves to prevent indexing mismatches between UI and Logic
        this.Moves.Clear();
        this.Moves.AddRange(newMoves);
    }

    public void ReplaceMove(int index, PokemonMove newMove)
    {
        // FIX: You were modifying Specie.MoveSet (The Template/DNA).
        // You should modify this.Moves (The individual Pokemon's active moves).
        if (index >= 0 && index < Moves.Count)
        {
            Moves[index] = newMove;
        }
        else
        {
            Console.WriteLine("Invalid move index.");
        }
    }

    public void Heal(int hpAmount)
    {
        // Restore HP and remove fainted status if HP becomes positive
        if (IsFainted && hpAmount > 0) IsFainted = false;
        
        CurrentHP += hpAmount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
    }

    public void GainExperience(int amount)
    {
        TotalExp += amount;
    }
    

    public void LevelUp()
    {
        Level++;

        // Calculate stat gains based on a portion of current base stats
        int hpGain = (int)(Specie.BaseStats.HP / 50.0) + 1;
        int attackGain = (int)(Specie.BaseStats.Attack / 50.0) + 1;
        int defenseGain = (int)(Specie.BaseStats.Defense / 50.0) + 1;
        int spAttackGain = (int)(Specie.BaseStats.SpAttack / 50.0) + 1;
        int spDefenseGain = (int)(Specie.BaseStats.SpDefense / 50.0) + 1;
        int speedGain = (int)(Specie.BaseStats.Speed / 50.0) + 1;

        // Apply gains to the internal species instance
        // (This works because the Spawner clones the species template)
        Specie.BaseStats.HP += hpGain;
        Specie.BaseStats.Attack += attackGain;
        Specie.BaseStats.Defense += defenseGain;
        Specie.BaseStats.SpAttack += spAttackGain;
        Specie.BaseStats.SpDefense += spDefenseGain;
        Specie.BaseStats.Speed += speedGain;

        // Update current vitals
        MaxHP = Specie.BaseStats.HP;
        CurrentHP += hpGain;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n★ {Specie.Name} leveled up to Level {Level}!");
        Console.WriteLine($"Stats increased! (+{hpGain} HP, +{attackGain} Atk, +{defenseGain} Def, +{spAttackGain} SpAtk, +{spDefenseGain} SpDef, +{speedGain} Sp)");
        Console.ResetColor();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            OnFaint();
        }
        else
        {
            Console.WriteLine($"{Specie.Name} took {damage} damage!");
        }
    }

    private void OnFaint()
    {
        Console.WriteLine($"{Specie.Name} has fainted!");
        IsFainted = true;
    }

    public bool CanEvolve()
    {
        // Check if species has a valid evolution ID and target level met
        // Using EvolutionId != 0 assuming 0 means "No Evolution"
        return Specie.EvolutionId != 0 && Level >= Specie.EvolutionLevel;
    }

    public void Evolve(Pokemon evolvedTemplate)
    {
        if (evolvedTemplate == null) return;

        // 1. Maintain health percentage during transformation
        float hpRatio = (float)this.CurrentHP / this.MaxHP;

        // 2. Overwrite species data and base stats with the evolved form
        this.Specie = evolvedTemplate.Specie; 
        this.MaxHP = evolvedTemplate.MaxHP;
        
        // 3. Recalculate current HP based on ratio + evolution bonus
        this.CurrentHP = (int)(MaxHP * hpRatio) + 20;
        if (this.CurrentHP > MaxHP) this.CurrentHP = MaxHP;

        // 4. Inherit moves from the evolved template
        this.Moves = evolvedTemplate.Moves;
        
        // 5. Ensure fainted status is cleared if health was restored
        if (this.CurrentHP > 0) IsFainted = false;
    }
}