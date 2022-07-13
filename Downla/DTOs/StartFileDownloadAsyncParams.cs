using Downla.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.DTOs
{
    public class StartFileDownloadAsyncParams
    {
        public Uri Uri { get; set; } = null!;
        public int MaxConnections { get; set; }
        public long MaxPacketSize { get; set; }
        public string DownloadPath { get; set; } = null!;
        public Dictionary<string, string>? Headers { get; set; }
        public CancellationToken CancellationToken { get; set; }


        public OnDownlaEventDelegate? OnStatusChange { get; set; }
        public OnDownlaEventDelegate? OnPacketDownloaded { get; set; }
        public int SleepTime { get; internal set; }
    }
}
