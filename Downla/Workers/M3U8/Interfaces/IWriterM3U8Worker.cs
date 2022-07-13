using Downla.Models;
using Downla.Models.FileModels;

namespace Downla.Workers.File.Interfaces
{
    public interface IWriterM3U8Worker
    {
        Task StartThread(DownloadMonitor context, int gcFactor, SemaphoreSlim downloadSemaphore, CustomSortedList<IndexedItem<byte[]>> completedConnections, CancellationTokenSource downlaCts);
    }
}