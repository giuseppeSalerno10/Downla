using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Controller.Interfaces
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegmentAsync(Uri url, CancellationToken ct = default);
        Task StartDownloadVideoAsync(Uri url, int maxConnections, string downloadPath, string fileName, int startConnectionDelay, out DownloadMonitor downloadMonitor, CancellationToken ct = default);
        Task<M3U8Video> GetVideoMetadataAsync(Uri url, CancellationToken ct = default);
    }
}