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


        internal Exception Exception 
        { 
            get => exception ?? throw new ArgumentNullException("Exception Is Null");
            set => exception = value; 
        }
        internal Task DownloadTask
        {
            get => downloadTask ?? throw new ArgumentNullException("DownloadTask Is Null");
            set => downloadTask = value;
        }
    }
}