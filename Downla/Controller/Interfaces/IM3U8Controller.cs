using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Controller.Interfaces
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegmentAsync(Uri url, CancellationToken ct = default);
        DownloadMonitor StartDownloadVideoAsync(Uri url, int maxConnections, string fileName, int startConnectionDelay, CancellationToken ct = default);
        Task<M3U8Video> GetVideoMetadataAsync(Uri url, CancellationToken ct = default);
    }
}