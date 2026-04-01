public abstract class Trainer
{
    public string Name { get; set; }
    public List<Pokemon> Team { get; set; } = new();
    public bool IsDefeated { get; set; }
}

public class NPC : Trainer
{
    public NPCRole Role { get; set; }   // Gym Leader, Elite Four, Champion, Rival, etc.
    public string ChallengeQuote { get; set; }   // A unique quote that the NPC says when challenged to a battle
    public string DefeatQuote { get; set; }      // A unique quote that the NPC says when defeated in battle  
    public int RequiredBadge { get; set; }  
}

public enum NPCRole
{
    GymLeader,
    EliteFour,
    Champion,
    Rival,
    Other
}