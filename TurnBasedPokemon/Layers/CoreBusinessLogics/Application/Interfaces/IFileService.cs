public interface IFileService
{
    void Save<T>(string fileName, T data);
    T? Load<T>(string fileName);
    bool Exists(string fileName);
    void Delete(string fileName);
}