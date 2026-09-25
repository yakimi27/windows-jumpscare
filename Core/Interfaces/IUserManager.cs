using System.Runtime.Versioning;

namespace Core.Interfaces
{
    public interface IUserManager
    {
        event Action<string>? JumpscareChanged;
        int GetJumpscareChance();
        string GetSelectedJumpscare();

        [SupportedOSPlatform("windows")]
        bool IsAutostartEnabled();
        void SetJumpscareChance(int chance);
        void SetSelectedJumpscare(string jumpscare);

        [SupportedOSPlatform("windows")]
        void SetAutostart(bool enable);
        void Reset();
    }
}
