using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.RepoInterfaces;

public class JsonStoryRepository : IStoryRepository
{
    private readonly string _filePath;

    public JsonStoryRepository(string filePath = "story.json")
    {
        _filePath = filePath;
    }

    public GameData LoadStoryData()
    {
        if (!File.Exists(_filePath)) return new GameData();

        string json = File.ReadAllText(_filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        try
        {
            return JsonSerializer.Deserialize<GameData>(json, options) ?? new GameData();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[!] JSON Error: {ex.Message}");
            throw;
        }
    }
}
