namespace Core.ConfigModels
{
    public class UserModel
    {
        public ushort JumpscareChance { get; set; } = 65535; // max 65535
        public string SelectedJumpscare { get; set; } = "Withered Foxy";
    }
}
