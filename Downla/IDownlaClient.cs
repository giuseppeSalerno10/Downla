
namespace Downla
{
    public interface IDownlaClient
    {
        DownloadInfosModel DownloadInfos { get; set; }
        string DownloadPath { get; set; }
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        void Dispose();
        DownloadInfosModel DownloadAsync(Uri uri, CancellationToken ct);
        DownloadInfosModel DownloadAsync(Uri uri, string authorizationHeader, CancellationToken ct);
        void EnsureDownload(CancellationToken ct);
    }
}