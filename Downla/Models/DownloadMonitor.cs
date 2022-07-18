using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Downla.Models
{
    public class DownloadMonitor : INotifyPropertyChanged
    {
        public async Task EnsureDownloadCompletion()
        {
            await DownloadTask;
            await WriteTask;

            DownloadTask.Dispose();
            WriteTask.Dispose();
        }

        public DownloadStatuses Status
        {
            get { return status; }
            internal set
            {
                if (value != status) 
                {
                    status = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DownloadMonitorInfos Infos { get; internal set; } = new();
        public List<Exception> Exceptions { get; internal set; } = new();
        internal Task WriteTask { get; set; } = null!;
        internal Task DownloadTask { get; set; } = null!;

        #region Fields
        private DownloadStatuses status;
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class DownloadMonitorInfos : INotifyPropertyChanged
    {
        public int Percentage 
        { 
            get => percentage;
            private set 
            {
                if (percentage != value)
                {
                    percentage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int TotalPackets 
        { 
            get => totalPackets;
            internal set 
            {
                if (totalPackets != value)
                {
                    totalPackets = value;
                    NotifyPropertyChanged();
                }
            } 
        }
        public int ActiveConnections 
        {
            get => activeConnections;
            internal set 
            {
                if (activeConnections != value)
                {
                    activeConnections = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public long FileSize 
        { 
            get => fileSize;
            internal set
            {
                if (fileSize != value)
                {
                    fileSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public long PacketSize 
        { 
            get => packetSize;
            internal set
            {
                if (packetSize != value)
                {
                    packetSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public long CurrentSize 
        { 
            get => currentSize;
            internal set
            {
                if (currentSize != value)
                {
                    currentSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string FileName 
        { 
            get => fileName ?? throw new Exception("fileName is null");
            internal set
            {
                if (fileName != value)
                {
                    fileName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string FileDirectory
        {
            get => fileDirectory ?? throw new Exception("fileDirectory is null");
            internal set
            {
                if (fileDirectory != value)
                {
                    fileDirectory = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int DownloadedPackets
        {
            get => downloadedPackets;
            internal set
            {
                if (downloadedPackets != value)
                {
                    downloadedPackets = value;
                    Percentage = TotalPackets == 0 ? 0 : downloadedPackets * 100 / TotalPackets;
                    NotifyPropertyChanged();
                }
            }
        }

        #region Fields
        private int downloadedPackets;
        private string? fileDirectory;
        private int percentage;
        private int totalPackets;
        private int activeConnections;
        private long fileSize;
        private long packetSize;
        private long currentSize;
        private string? fileName;
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}