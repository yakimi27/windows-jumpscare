using Core.ConfigModels;
using Core.Interfaces;
using Core.Services;
using System.Runtime.Versioning;

namespace Core.Managers
{
    public class UserManager : IUserManager
    {
        public event Action<string>? JumpscareChanged;
        public event Action<int>? JumpscareChanceChanged;
        private readonly IConfigService _configService;
        private UserModel _userModel;

        private const int DefaultJumpscareChance = 749635;

        public UserManager(IConfigService configService)
        {
            _configService = configService;
            _userModel = _configService.Load<UserModel>(_configService.UserConfigFilePath) ?? new UserModel();
            if (_userModel.JumpscareChance <= 0)
            {
                _userModel.JumpscareChance = DefaultJumpscareChance;
                Save();
            }
        }

        public int GetJumpscareChance() => _userModel.JumpscareChance;
        public string GetSelectedJumpscare() => _userModel.SelectedJumpscare;
        [SupportedOSPlatform("windows")]
        public bool IsAutostartEnabled()
        {
            bool enabled = AutostartManager.IsAutostartEnabled();
            if (_userModel.IsAutostartEnabled != enabled)
            {
                _userModel.IsAutostartEnabled = enabled;
                Save();
            }
            return enabled;
        }

        [SupportedOSPlatform("windows")]
        public void SetAutostart(bool enable)
        {
            _userModel.IsAutostartEnabled = enable;
            Save();
            AutostartManager.SetAutostart(enable);
        }

        public void SetJumpscareChance(int chance)
        {
            if (chance <= 0)
            {
                chance = DefaultJumpscareChance;
            }
            _userModel.JumpscareChance = chance;
            Save();
            JumpscareChanceChanged?.Invoke(chance);
        }

        public void SetSelectedJumpscare(string jumpscare)
        {
            if (_userModel.SelectedJumpscare != jumpscare)
            {
                _userModel.SelectedJumpscare = jumpscare;
                Save();
                JumpscareChanged?.Invoke(jumpscare);
            }
       }

        public void Reset()
        {
            _userModel = new UserModel();
            Save();
        }

        private void Save() => _configService.Save(_configService.UserConfigFilePath, _userModel);
    }
}
