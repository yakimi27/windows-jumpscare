namespace Core.Interfaces
{
    public interface IUserManager
    {
        ushort GetJumpscareChance();
        string GetSelectedJumpscare();
        void SetJumpscareChance(ushort chance);
        void SetSelectedJumpscare(string jumpscare);
        void Reset();
    }
}
