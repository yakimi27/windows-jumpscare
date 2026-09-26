using Core.ConfigModels;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Core.Services
{
    public class ConfigService : IConfigService
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
            catch (FileNotFoundException ex)
            {
                Trace.TraceError($"Config file not found at '{filePath}': {ex}");
                Debug.WriteLine($"Config file not found at '{filePath}': {ex}");
                return new T();
            }
            catch (IOException ex)
            {
                Trace.TraceError($"IO error reading config file at '{filePath}': {ex}");
                Debug.WriteLine($"IO error reading config file at '{filePath}': {ex}");
                return new T();
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.TraceError($"Access denied reading config file at '{filePath}': {ex}");
                Debug.WriteLine($"Access denied reading config file at '{filePath}': {ex}");
                return new T();
            }
            catch (JsonException ex)
            {
                Trace.TraceError($"JSON deserialization error in config file at '{filePath}': {ex}");
                Debug.WriteLine($"JSON deserialization error in config file at '{filePath}': {ex}");
                return new T();
            }
        }

        public void Save<T>(string filePath, T data)
        {
            var tempPath = filePath + ".tmp";
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, filePath, overwrite: true);
            }
            catch (IOException ex)
            {
                Trace.TraceError($"IO error writing config file at '{filePath}': {ex}");
                Debug.WriteLine($"IO error writing config file at '{filePath}': {ex}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.TraceError($"Access denied writing config file at '{filePath}': {ex}");
                Debug.WriteLine($"Access denied writing config file at '{filePath}': {ex}");
            }
            catch (JsonException ex)
            {
                Trace.TraceError($"JSON serialization error in config file at '{filePath}': {ex}");
                Debug.WriteLine($"JSON serialization error in config file at '{filePath}': {ex}");
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                    // ignore errors during temp file cleanup
                }
            }
        }

        private void InitializeConfigDirectory()
        {
            Directory.CreateDirectory(_appDirectory);

            if (!File.Exists(_userConfigFilePath))
                Save(_userConfigFilePath, new UserModel());

            if (!File.Exists(_jumpscareConfigFilePath))
                Save(_jumpscareConfigFilePath, new JumpscareModel.JumpscareList
                {
                    Jumpscares = [
                        new () {Name = "Withered Foxy", AssetsPath = "assets/withered_foxy", FrameAmount = 15, FrameFrequency= 60},
                        new() {Name = "Withered Freddy", AssetsPath = "assets/withered_freddy", FrameAmount = 36, FrameFrequency= 50}
                        ]
                });
        }
    }
}
