using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Controller.Interfaces
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegmentAsync(Uri url, CancellationToken ct = default);
        DownlaDownload StartDownloadVideoAsync(Uri url, int maxConnections, long maxPacketSize, string fileName, CancellationToken ct = default);
        Task<DownlaM3U8Video> GetVideoMetadataAsync(Uri url, CancellationToken ct = default);
    }
}