using Downla.DTOs;
using Downla.Models;
using Downla.Models.M3U8Models;

namespace Downla.Controller.Interfaces
{
    public interface IM3U8Controller
    {
        Task<byte[]> DownloadSegmentAsync(Uri uri, CancellationToken ct = default);
        Task<M3U8Video> GetVideoMetadataAsync(Uri uri, CancellationToken ct = default);
        Task<DownloadMonitor> StartDownloadVideoAsync(StartM3U8DownloadAsyncParams downloadParams);
    }
}