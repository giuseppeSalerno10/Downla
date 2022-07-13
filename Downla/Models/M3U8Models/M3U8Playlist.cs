using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public record M3U8Playlist
    {
        public Uri Uri { get; internal set; } = null!;
        public M3U8PlaylistSegment[] Segments { get; internal set; } = null!;
        public M3U8PlaylistHeader Header { get; internal set; } = new();
        public string PlainString { get; internal set; } = null!;
        public long Bandwidth { get; internal set; }
        public int Resolution { get; internal set; }
    }
    public class M3U8PlaylistHeader
    {
        public string Version { get; internal set; } = null!;
        public string IsCacheAllowed { get; internal set; } = null!;
        public string MediaSequence { get; internal set; } = null!;
        public string PlaylistType { get; internal set; } = null!;
        public string TargetDuration { get; internal set; } = null!;
    }
    public class M3U8PlaylistSegment
    {
        public Uri Uri { get; internal set; } = null!;
    }
}
