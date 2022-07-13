﻿using Downla.Models;
using Downla.Models.FileModels;

namespace Downla.Workers.File.Interfaces
{
    public interface IDownloaderFileWorker
    {
        Task StartThread(DownloadMonitor context, Uri uri, string? authorizationHeader, int maxConnections, OnDownlaEventDelegate? onPacketDownload, CustomSortedList<IndexedItem<HttpResponseMessage>> completedConnections, SemaphoreSlim downloadSemaphore, CancellationTokenSource downlaCts);
    }
}