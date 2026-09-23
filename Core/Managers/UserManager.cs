using Core.ConfigModels;
using Core.Interfaces;
using Core.Services;

namespace Core.Managers
{
    public class UserManager : IUserManager
    {
        public event Action<string>? JumpscareChanged;
        private readonly IConfigService _configService;
        private UserModel _userModel;

        public UserManager(IConfigService configService)
        {
            _configService = configService;
            _userModel = _configService.Load<UserModel>(_configService.UserConfigFilePath) ?? new UserModel();
        }

        public ushort GetJumpscareChance() => _userModel.JumpscareChance;
        public string GetSelectedJumpscare() => _userModel.SelectedJumpscare;

        public void SetJumpscareChance(ushort chance)
        {
            _userModel.JumpscareChance = chance;
            Save();
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
