using Core.ConfigModels;
using Core.Services;

namespace Core.Managers
{
    internal class JumpscareManager
    {
        private readonly IConfigService _configService;
        private JumpscareModel.JumpscareList _jumpscareList;

        public JumpscareManager(IConfigService configService)
        {
            _configService = configService;
            _jumpscareList = _configService.Load<JumpscareModel.JumpscareList>(_configService.JumpscareConfigFilePath) ?? new JumpscareModel.JumpscareList { Jumpscares = [] };
        }

        public IReadOnlyList<JumpscareModel.Jumpscare> GetAll() => _jumpscareList.Jumpscares;

        public JumpscareModel.Jumpscare? GetByName(string name) => _jumpscareList.Jumpscares.FirstOrDefault(j => j.Name == name);

        public void Add(JumpscareModel.Jumpscare jumpscare)
        {
            if (_jumpscareList.Jumpscares.Any(j => j.Name == jumpscare.Name)) throw new InvalidOperationException($"Jumpscare '{jumpscare.Name}' already exists.");
            _jumpscareList.Jumpscares.Add(jumpscare);
            Save();
        }

        public void Remove(string name)
        {
            var jumpscare = GetByName(name) ?? throw new KeyNotFoundException($"Jumpscare '{name}' not found.");
            _jumpscareList.Jumpscares.Remove(jumpscare);
            Save();
        }

        public void Update(JumpscareModel.Jumpscare updated)
        {
            var existing = GetByName(updated.Name) ?? throw new KeyNotFoundException($"Jumpscare '{updated.Name}' not found");

            existing.AssetPath = updated.AssetPath;
            existing.FrameAmount = updated.FrameAmount;
            existing.FrameFrequency = updated.FrameFrequency;
            Save();
        }


        private void Save() => _configService.Save(_configService.JumpscareConfigFilePath, _jumpscareList);
    }
}
