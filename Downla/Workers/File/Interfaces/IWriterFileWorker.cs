using Downla.Models;
using Downla.Models.FileModels;

namespace Downla.Workers.File.Interfaces
{
    public interface IWriterFileWorker
    {
        Task StartThread(DownloadMonitor context, SemaphoreSlim downloadSemaphore, CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections, CancellationTokenSource downlaCts);
    }
}