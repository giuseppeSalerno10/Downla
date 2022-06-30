using Downla.Models;
using Downla.Services;

namespace Downla.Interfaces
{
    public interface IDownlaClient
    {
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        DownloadMonitor StartFileDownload(Uri uri, string? authorizationHeader = null, CancellationToken ct = default);
        DownloadMonitor StartM3U8Download(Uri uri, string fileName, int startConnectionDelay, CancellationToken ct = default);
    }
}