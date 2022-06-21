using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public record M3U8Playlist
    {
        public string Url { get; set; } = null!;
        public M3U8PlaylistSegment[] Segments { get; set; } = null!;
        public M3U8PlaylistHeader Header { get; set; } = new();

    }
}
