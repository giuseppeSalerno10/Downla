using Downla.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.DTOs
{
    public class StartM3U8DownloadAsyncParams
    {
        public Uri Uri { get; set; } = null!;
        public int MaxConnections { get; set; }
        public long MaxPacketSize { get; set; }
        public string DownloadPath { get; set; } = null!;
        public CancellationToken CancellationToken { get; set; }
        public string FileName { get; internal set; } = null!;
        public int SleepTime { get; internal set; }
    }
}
