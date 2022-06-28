using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public class DownlaM3U8Video
    {
        public string Version { get; set; } = null!;
        public Uri Uri { get; set; } = null!;
        public DownlaM3U8VideoInfo? Infos { get; set; }
        public DownlaM3U8Playlist[] Playlists { get; set; } = null!;
    }
    public class DownlaM3U8VideoInfo
    {
        public string Subtitles { get; set; } = null!;
    }
}
