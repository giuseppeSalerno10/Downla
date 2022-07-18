using Downla.Models;
using Downla.Models.FileModels;
using Downla.Models.M3U8Models;

namespace Downla.Workers.File.Interfaces
{
    public interface IDownloaderM3U8Worker
    {
        Task StartThread(DownloadMonitor context, M3U8Playlist playlist, int maxConnections, int sleepTime, CustomSortedList<IndexedItem<byte[]>> completedConnections, SemaphoreSlim downloadSemaphore, CancellationTokenSource downlaCts);
    }
}