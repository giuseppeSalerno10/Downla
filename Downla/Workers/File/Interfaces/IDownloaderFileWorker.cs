﻿using Downla.Models;
using Downla.Models.FileModels;

namespace Downla.Workers.File.Interfaces
{
    public interface IDownloaderFileWorker
    {
        Task StartThread(DownloadMonitor context, Uri uri, Dictionary<string,string>? Headers, int maxConnections, int sleepTime, CustomSortedList<IndexedItem<byte[]>> completedConnections, SemaphoreSlim downloadSemaphore, CancellationTokenSource downlaCts);
    }
}