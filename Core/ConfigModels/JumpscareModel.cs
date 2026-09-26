namespace Core.ConfigModels
{
    public class JumpscareModel
    {
        public class JumpscareList
        {
            public List<Jumpscare> Jumpscares { get; set; } = [];
        }
        public class Jumpscare
        {
            public string Name { get; set; } = string.Empty;
            public string AssetsPath { get; set; } = string.Empty;
            public byte FrameAmount { get; set; }
            public byte FrameFrequency { get; set; }
        }
    }
}
