using Downla.Models;
using Downla.Services;

namespace Downla.Interfaces
{
    public interface IDownlaClient
    {
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        DownlaDownload StartFileDownload(Uri uri, string? authorizationHeader = null, CancellationToken ct = default);
        DownlaDownload StartM3U8Download(Uri uri, string fileName, int startConnectionDelay, CancellationToken ct = default);
    }
}