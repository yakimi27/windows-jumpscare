namespace Core.ConfigModels
{
    internal class UserModel
    {
        public ushort JumpscareChance { get; set; } = 10000; //max65536
        public string SelectedJumpscare { get; set; } = "Withered Foxy";
    }
}
