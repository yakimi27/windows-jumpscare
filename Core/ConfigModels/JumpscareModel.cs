namespace Core.ConfigModels
{
    internal class JumpscareModel
    {
        public class JumpscareList
        {
            public List<Jumpscare> Jumpscares { get; set; }
        }
        public class Jumpscare
        {
            public string Name { get; set; }
            public string AssetPath { get; set; }
            public string FrameAmount { get; set; }
            public byte FrameFrequency { get; set; }
        }
    }
}
