namespace Core.ConfigModels
{
    public class UserModel
    {
        public int JumpscareChance { get; set; } = 1000000; // max 1000000
        public string SelectedJumpscare { get; set; } = "Withered Foxy";
        public bool IsAutostartEnabled { get; set; } = false;
    }
}
