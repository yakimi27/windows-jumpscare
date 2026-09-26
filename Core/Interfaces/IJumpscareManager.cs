using Core.ConfigModels;

namespace Core.Interfaces
{
    public interface IJumpscareManager
    {
        IReadOnlyList<JumpscareModel.Jumpscare> GetAll();
        JumpscareModel.Jumpscare? GetByName(string name);
        JumpscareModel.Jumpscare? GetFirstValidJumpscare();
        string? GetFirstValidJumpscareName();
        void Add(JumpscareModel.Jumpscare jumpscare);
        void Remove(string name);
        void Update(JumpscareModel.Jumpscare updated);
    }
}
