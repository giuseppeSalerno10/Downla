using Downla.Managers;
using Downla.Models.FileModels;
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

        public Task<byte[]> DownloadSegment(string url, IM3U8ReaderService? reader = null, CancellationToken ct = default)
        {
            if (reader == null)
            {
                reader = new M3U8ReaderService();
            }

            return _manager.DownloadSegment(url, reader, ct);

        }
        public Task<DownloadInfosModel> DownloadVideo(string url, int maxConnections, string downloadPath, long maxPacketSize, string fileName, IM3U8ReaderService? reader = null, CancellationToken ct = default)
        {
            if (reader == null)
            {
                reader = new M3U8ReaderService();
            }

            return _manager.DownloadVideo(url, maxConnections, downloadPath, maxPacketSize, fileName, reader, ct);
        }
        public Task<M3U8VideoModel> GetVideoMetadata(string url, IM3U8ReaderService? reader = null, CancellationToken ct = default)
        {
            if (reader == null)
            {
                reader = new M3U8ReaderService();
            }

            return _manager.GetVideoMetadata(url, reader, ct);
        }
    }
}
