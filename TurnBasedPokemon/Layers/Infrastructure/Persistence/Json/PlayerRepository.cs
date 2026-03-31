using System.Text.Json;

public class PlayerRepository
{
    private readonly string _filePath = "user.json";

    public Player Load()
    {
        try 
        {
            if (!File.Exists(_filePath)) return null;

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Player>(json);
        }
        catch (Exception)
        {
            
            return null; 
        }
    }

    public void Save(Player player)
    {
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(player, options);
        File.WriteAllText(_filePath, json);
    }
}