namespace Core.Interfaces
{
    public interface IUserManager
    {
        event Action<string>? JumpscareChanged;
        ushort GetJumpscareChance();
        string GetSelectedJumpscare();
        void SetJumpscareChance(ushort chance);
        void SetSelectedJumpscare(string jumpscare);
        void Reset();
    }
}
