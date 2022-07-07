namespace Downla.Models
{
    public class DownloadMonitor
    {
        private DownloadStatuses status;
        public DownloadStatuses Status
        {
            get { return status; }
            set
            {
                if (OnStatusChange != null) { OnStatusChange.Invoke(value, Infos, Exceptions); }
                status = value;
            }
        }

        public event OnDownlaEventDelegate? OnStatusChange;

        public DownloadMonitorInfos Infos { get; set; } = new();
        public List<Exception> Exceptions { get; } = new();
    }

    public class DownloadMonitorInfos
    {
        public int Percentage { get => TotalPackets == 0 ? 0 : DownloadedPackets * 100 / TotalPackets; }
        public int TotalPackets { get; set; }
        public int ActiveConnections { get; set; }
        public int DownloadedPackets { get; set; }
        public long FileSize { get; set; }
        public long CurrentSize { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;
    }

    public delegate void OnDownlaEventDelegate(DownloadStatuses status, DownloadMonitorInfos infos, IEnumerable<Exception> exceptions);
}