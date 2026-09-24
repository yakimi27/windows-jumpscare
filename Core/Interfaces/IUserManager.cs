using System.Runtime.Versioning;

namespace Core.Interfaces
{
    public interface IUserManager
    {
        event Action<string>? JumpscareChanged;
        ushort GetJumpscareChance();
        string GetSelectedJumpscare();

        [SupportedOSPlatform("windows")]
        bool IsAutostartEnabled();
        void SetJumpscareChance(ushort chance);
        void SetSelectedJumpscare(string jumpscare);

        [SupportedOSPlatform("windows")]
        void SetAutostart(bool enable);
        void Reset();
    }
}
