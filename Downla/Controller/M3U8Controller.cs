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

        public Task<byte[]> DownloadSegment(Uri uri, CancellationToken ct = default)
        {
            return _manager.DownloadSegment(uri, ct);

        }
        public DownlaDownload DownloadVideo(Uri uri, int maxConnections, long maxPacketSize, string fileName, CancellationToken ct = default)
        {
            return _manager.DownloadVideo(uri, maxConnections, maxPacketSize, fileName, ct);
        }
        public Task<DownlaM3U8Video> GetVideoMetadata(Uri uri, CancellationToken ct = default)
        {
            return _manager.GetVideoMetadata(uri, ct);
        }
    }
}
