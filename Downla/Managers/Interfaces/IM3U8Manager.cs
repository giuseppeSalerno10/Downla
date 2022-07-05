using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Managers
{
    public interface IM3U8Manager
    {
        Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct);
        Task StartDownloadVideoAsync(Uri uri, int maxConnections, string downloadPath, string fileName, int sleepTime, out DownloadMonitor downloadMonitor, CancellationToken ct);
        Task<M3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct);
    }
}