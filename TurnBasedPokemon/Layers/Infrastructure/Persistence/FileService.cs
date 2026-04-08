using System.Text.Json;
using System.Text.Json.Serialization;

public class FileService : IFileService
{
    private readonly JsonSerializerOptions _options;

    public FileService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            // 1. IMPORTANT: Help read/write Enum under text (Grass, Fire...) instead of numbers (0, 1...)
            Converters = { new JsonStringEnumConverter() },
            
            // 2. Prevent infinite loops when serializing objects with circular references (e.g., User -> PokemonTeam -> Pokemon -> back to User)
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            
            // 3. Ignore null values to reduce file size and avoid cluttering JSON with empty fields
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public void Save<T>(string fileName, T data)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(fileName, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileService] Error saving {fileName}: {ex.Message}");
        }
    }

    public T? Load<T>(string fileName)
    {
        if (!Exists(fileName)) return default;

        try
        {
            string jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<T>(jsonString, _options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileService] Error loading {fileName}: {ex.Message}");
            return default;
        }
    }

    public bool Exists(string fileName) => File.Exists(fileName);

    public void Delete(string fileName) 
    {
        if (Exists(fileName)) File.Delete(fileName);
    }
}