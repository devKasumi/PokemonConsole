public class GameSession
{
    public Player Player { get; set; }

    public void OnGymLeaderDefeated(string badgeName)
    {
        Player.AddBadge(badgeName);
        Console.WriteLine($"Congratulations! You've earned the {badgeName} badge!");

        // Logic to check if player move to the next phase
        if (Player.Badges.Count == 2) Player.CurrentPhase = 2;
        if (Player.Badges.Count == 5) Player.CurrentPhase = 3;
        if (Player.Badges.Count == 8) Player.CurrentPhase = 4;
    }
}