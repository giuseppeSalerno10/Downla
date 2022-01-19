namespace Downla
{
    public class DownloadInfoes
    {
        private Task? downloadTask;

        public DownloadStatuses Status { get; set; }
        public int TotalPackets { get; set; }
        public int ActiveConnections { get; set; }
        public int DownloadedPackets { get; set; }
        public long FileSize { get; set; }
        public long CurrentSize { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;

        internal string AdditionalInformations { get; set; } = string.Empty;
        internal Task DownloadTask
        {
            get => downloadTask ?? throw new ArgumentNullException("AdditionalInformations Is Null");
            set => downloadTask = value;
        }
    }
}