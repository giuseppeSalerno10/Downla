using Downla.Models.FileModels;

namespace Downla.Interfaces
{
    public interface IDownlaClient
    {
        string DownloadPath { get; set; }
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        DownloadInfosModel StartFileDownload(Uri uri, string? authorizationHeader = null, CancellationToken ct = default);
    }
}