using System.Text.Json;

public class FileService : IFileService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

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