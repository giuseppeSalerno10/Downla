using Downla.Models;
using Downla.Services;

namespace Downla.Interfaces
{
    public interface IDownlaClient
    {
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }
        string DownloadPath { get; set; }

        Task StartFileDownloadAsync(Uri uri, out DownloadMonitor downloadMonitor, string? authorizationHeader = null, CancellationToken ct = default);
        Task StartM3U8DownloadAsync(Uri uri, string fileName, int startConnectionDelay, out DownloadMonitor downloadMonitor, CancellationToken ct = default);
    }
}