using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public class M3U8Video
    {
        public string Version { get; internal set; } = null!;
        public Uri Uri { get; internal set; } = null!;
        public string PlainString { get; internal set; } = null!;
        public M3U8VideoInfo? Infos { get; internal set; }
        public M3U8Playlist[] Playlists { get; internal set; } = null!;
    }
    public class M3U8VideoInfo
    {
        public string Subtitles { get; internal set; } = null!;
    }
}
