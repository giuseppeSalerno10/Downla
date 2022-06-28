using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public record DownlaM3U8Playlist
    {
        public Uri Uri { get; set; } = null!;
        public DownlaM3U8PlaylistSegment[] Segments { get; set; } = null!;
        public DownlaM3U8PlaylistHeader Header { get; set; } = new();

    }
    public class DownlaM3U8PlaylistHeader
    {
        public string Version { get; set; } = null!;
        public string IsCacheAllowed { get; set; } = null!;
        public string MediaSequence { get; set; } = null!;
        public string PlaylistType { get; set; } = null!;
        public string TargetDuration { get; set; } = null!;
    }
    public class DownlaM3U8PlaylistSegment
    {
        public Uri Uri { get; set; } = null!;
    }
}
