namespace Downla.Models.M3U8Models
{
    public class M3U8PlaylistHeader
    {
        public string Version { get; set; } = null!;
        public string IsCacheAllowed { get; set; } = null!;
        public string MediaSequence { get; set; } = null!;
        public string PlaylistType { get; set; } = null!;
        public string TargetDuration { get; set; } = null!;
    }
}