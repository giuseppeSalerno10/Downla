using Downla.Models;
using Downla.Services;

namespace Downla.Interfaces
{
    public interface IDownlaClient
    {
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }
        string DownloadPath { get; set; }

        event OnDownlaEventDelegate? OnStatusChange;
        event OnDownlaEventDelegate? OnPacketDownloaded;

        Task<DownloadMonitor> StartFileDownloadAsync(Uri uri, int sleepTime, string? authorizationHeader = null, CancellationToken ct = default);
        Task<DownloadMonitor> StartM3U8DownloadAsync(Uri uri, string fileName, int sleepTime, CancellationToken ct = default);
    }
}