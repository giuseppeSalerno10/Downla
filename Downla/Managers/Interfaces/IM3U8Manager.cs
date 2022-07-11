using Downla.DTOs;
using Downla.Models;
using Downla.Models.M3U8Models;

namespace Downla.Managers
{
    public interface IM3U8Manager
    {
        Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct);
        Task<M3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct);
        Task<DownloadMonitor> StartDownloadVideoAsync(StartM3U8DownloadAsyncParams downladParams);
    }
}