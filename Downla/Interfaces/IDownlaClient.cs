
namespace Downla
{
    public interface IDownlaClient
    {
        string DownloadPath { get; set; }
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        DownloadInfosModel StartDownload(Uri uri, CancellationToken ct);
        DownloadInfosModel StartDownload(Uri uri, string authorizationHeader, CancellationToken ct);
    }
}