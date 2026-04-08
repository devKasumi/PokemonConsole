public interface IAuthenService
{
    bool Register(RegisterRequest request);
    bool Login(LoginRequest request);
    bool LoadProgress(string username);
    void SaveProgress();
    void SetChampion();
}
