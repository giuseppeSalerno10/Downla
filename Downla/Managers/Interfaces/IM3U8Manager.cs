using Downla.Models.FileModels;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Managers
{
    public interface IM3U8Manager
    {
        Task<byte[]> DownloadSegment(string url, IM3U8ReaderService reader, CancellationToken ct);
        Task<DownloadInfosModel> DownloadVideo(string url, int maxConnections, string downloadPath, long maxPacketSize, string fileName, IM3U8ReaderService reader, CancellationToken ct);
        Task<M3U8VideoModel> GetVideoMetadata(string url, IM3U8ReaderService reader, CancellationToken ct);
    }
}