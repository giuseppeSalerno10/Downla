using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Core
{
    public class DownloadInfoes
    {
        public DownloadStatuses Status { get; set; }
        public int TotalParts { get; set; }
        public int ActiveParts { get; set; }
        public int CompletedParts { get; set; }
        public long FileSize { get; set; }
        public long CurrentSize { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;

        internal string AdditionalInformations { get; set; } = string.Empty;
    }
}
