using System.Text.Json;

namespace Core.Services
{
    internal class ConfigService : IConfigService
    {
        private const string AppFolderName = "yakimi27/WindowsJumpscare";
        private const string UserConfigFileName = "userConfig.json";
        private const string JumpscareConfigFileName = "jumpscareConfig.json";

        private readonly string _appDataPath;
        private readonly string _appDirectory;
        private readonly string _userConfigFilePath;
        private readonly string _jumpscareConfigFilePath;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public ConfigService()
        {
            _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _appDirectory = Path.Combine(_appDataPath, AppFolderName);
            _userConfigFilePath = Path.Combine(_appDirectory, UserConfigFileName);
            _jumpscareConfigFilePath = Path.Combine(_appDirectory, JumpscareConfigFileName);

            InitializeConfigDirectory();
        }

        public string UserConfigFilePath => _userConfigFilePath;
        public string JumpscareConfigFilePath => _jumpscareConfigFilePath;

        public T? Load<T>(string filePath) where T : new()
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
            catch (JsonException)
            {
                return new T();
            }
        }

        public void Save<T>(string filePath, T data)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(filePath, json);
        }

        private void InitializeConfigDirectory()
        {
            Directory.CreateDirectory(_appDirectory);

            if (!File.Exists(_userConfigFilePath))
            {
                File.WriteAllText(_userConfigFilePath, "{}");
            }

            if (!File.Exists(_jumpscareConfigFilePath))
            {
                File.WriteAllText(_jumpscareConfigFilePath, "{}");
            }
        }
    }
}
