using Downla.Models;
using Downla.Models.M3U8Models;
using Downla.Services;

namespace Downla.Managers
{
    public interface IM3U8Manager
    {
        Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct);
        DownlaDownload StartDownloadVideoAsync(Uri uri, int maxConnections, string fileName, int sleepTime, CancellationToken ct);
        Task<DownlaM3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct);
    }
}