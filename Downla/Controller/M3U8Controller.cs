using Downla.Controller.Interfaces;
using Downla.Managers;
using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Controller
{
    public class M3U8Controller : IM3U8Controller
    {
        private readonly IM3U8Manager _manager;
        public M3U8Controller(IM3U8Manager manager)
        {
            _manager = manager;
        }

        public Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct = default)
        {
            return _manager.DownloadSegmentAsync(uri, ct);

        }
        public DownlaDownload StartDownloadVideoAsync(Uri uri, int maxConnections, long maxPacketSize, string fileName, CancellationToken ct = default)
        {
            return _manager.StartDownloadVideoAsync(uri, maxConnections, maxPacketSize, fileName, ct);
        }
        public Task<DownlaM3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct = default)
        {
            return _manager.GetVideoMetadataAsync(uri, ct);
        }
    }
}
