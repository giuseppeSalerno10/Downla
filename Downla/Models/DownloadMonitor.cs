namespace Downla.Models
{
    public class DownloadMonitor
    {
        public DownloadStatuses Status { get; set; }
        public int Percentage { get => Infos.TotalPackets == 0 ? 0 : Infos.DownloadedPackets * 100 / Infos.TotalPackets; }
        public DownloadMonitorInfos Infos { get; set; } = new();
        public List<Exception> Exceptions { get; } = new();
    }

    public class DownloadMonitorInfos
    {
        public int TotalPackets { get; set; }
        public int ActiveConnections { get; set; }
        public int DownloadedPackets { get; set; }
        public long FileSize { get; set; }
        public long CurrentSize { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;
    }
}