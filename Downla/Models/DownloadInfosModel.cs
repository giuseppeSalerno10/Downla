namespace Downla
{
    public class DownloadInfosModel
    {
        private Task? downloadTask;
        private Exception? exception;

        public DownloadStatuses Status { get; set; }
        public int TotalPackets { get; set; }
        public int ActiveConnections { get; set; }
        public int DownloadedPackets { get; set; }
        public long FileSize { get; set; }
        public long CurrentSize { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;
        public List<Exception> Exceptions { get; } = new List<Exception>();

        internal Task DownloadTask
        {
            get => downloadTask ?? throw new ArgumentNullException("DownloadTask Is Null");
            set => downloadTask = value;
        }

        /// <summary>
        /// Await for download completion.
        /// Throw an exception if the operation is faulted.
        /// </summary>
        /// <exception cref="Exception">Generic Exception</exception>
        public async Task EnsureDownloadCompletion(CancellationToken ct = default)
        {
            await DownloadTask.WaitAsync(ct);
            
            DownloadTask.Dispose();
        }
    }
}