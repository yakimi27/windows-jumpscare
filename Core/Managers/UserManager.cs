using Core.ConfigModels;
using Core.Services;

namespace Core.Managers
{
    internal class UserManager
    {
        private readonly IConfigService _configService;
        private UserModel _userModel;

        public UserManager(IConfigService configService)
        {
            _configService = configService;
            _userModel = _configService.Load<UserModel>(_configService.UserConfigFilePath) ?? new UserModel();
        }

        public ushort GetJumpscareChance() => _userModel.JumpscareChance;

        public void SetJumpscareChance(ushort chance)
        {
            _userModel.JumpscareChance = chance;
            Save();
        }

        public void Reset()
        {
            _userModel = new UserModel();
            Save();
        }

        private void Save() => _configService.Save(_configService.UserConfigFilePath, _userModel);
    }
}
