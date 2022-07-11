namespace Downla.Models
{
    public class DownloadMonitor
    {
        private DownloadStatuses status;
        public DownloadStatuses Status
        {
            get { return status; }
            internal set
            {
                if (OnStatusChange != null) { OnStatusChange.Invoke(value, Infos, Exceptions); }
                status = value;
            }
        }

        public event OnDownlaEventDelegate? OnStatusChange;

        public DownloadMonitorInfos Infos { get; internal set; } = new();
        public List<Exception> Exceptions { get; internal set; } = new();

        internal Task WriteTask { get; set; } = null!;
        internal Task DownloadTask { get; set; } = null!;


        public async Task EnsureDownload()
        {
            await DownloadTask;
            await WriteTask;

            DownloadTask.Dispose();
            WriteTask.Dispose();
        }
    }

    public class DownloadMonitorInfos
    {
        public int Percentage { get => TotalPackets == 0 ? 0 : DownloadedPackets * 100 / TotalPackets; }
        public int TotalPackets { get; internal set; }
        public int ActiveConnections { get; internal set; }
        public int DownloadedPackets { get; internal set; }
        public long FileSize { get; internal set; }
        public long PacketSize { get; internal set; }
        public long CurrentSize { get; internal set; }
        public string FileName { get; internal set; } = string.Empty;
        public string FileDirectory { get; internal set; } = string.Empty;
    }

    public delegate void OnDownlaEventDelegate(DownloadStatuses status, DownloadMonitorInfos infos, IEnumerable<Exception> exceptions);
}