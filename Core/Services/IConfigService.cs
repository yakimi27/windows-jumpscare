namespace Core.Services
{
    internal interface IConfigService
    {
        string UserConfigFilePath { get; }
        string JumpscareConfigFilePath { get; }

        T? Load<T>(string filePath) where T : new();
        void Save<T>(string filePath, T data);
    }
}
