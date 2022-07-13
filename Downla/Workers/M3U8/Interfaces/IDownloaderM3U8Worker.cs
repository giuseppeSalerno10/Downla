using Downla.Models;
using Downla.Models.FileModels;

namespace Downla.Workers.File.Interfaces
{
    public interface IDownloaderM3U8Worker
    {
        Task StartThread(DownloadMonitor context, Uri uri, int maxConnections, int sleepTime, OnDownlaEventDelegate? onPacketDownload, CustomSortedList<IndexedItem<byte[]>> completedConnections, SemaphoreSlim downloadSemaphore, CancellationTokenSource downlaCts);
    }
}