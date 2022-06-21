using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Controller
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegment(string url, IM3U8ReaderService? reader = null, CancellationToken ct = default);
        Task<DownloadInfosModel> DownloadVideo(string url, int maxConnections, string downloadPath, long maxPacketSize, string fileName, IM3U8ReaderService? reader = null, CancellationToken ct = default);
        Task<M3U8VideoModel> GetVideoMetadata(string url, IM3U8ReaderService? reader = null, CancellationToken ct = default);
    }
}