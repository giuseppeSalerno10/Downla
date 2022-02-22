
namespace Downla
{
    public interface IDownlaClient
    {
        DownloadInfosModel DownloadInfos { get; set; }
        string DownloadPath { get; set; }
        int MaxConnections { get; set; }
        long MaxPacketSize { get; set; }

        void Dispose();
        DownloadInfosModel StartDownloadAsync(Uri uri, CancellationToken ct);
        DownloadInfosModel StartDownload(Uri uri, string authorizationHeader, CancellationToken ct);
        Task AwaitDownloadCompletation(CancellationToken ct);
    }
}