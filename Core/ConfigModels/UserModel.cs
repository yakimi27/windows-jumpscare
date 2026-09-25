namespace Core.ConfigModels
{
    public class UserModel
    {
        public int JumpscareChance { get; set; } = Constants.DefaultJumpscareChance;
        public string SelectedJumpscare { get; set; } = "Withered Foxy";
        public bool IsAutostartEnabled { get; set; } = false;
    }
}
