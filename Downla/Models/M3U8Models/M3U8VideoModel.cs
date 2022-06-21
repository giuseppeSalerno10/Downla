using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Models.M3U8Models
{
    public record M3U8VideoModel
    {
        public string Version { get; set; } = null!;
        public string Url { get; set; } = null!;
        public M3U8VideoInfo? Infos { get; set; }
        public M3U8Playlist[] Playlists { get; set; } = null!;
    }
}
