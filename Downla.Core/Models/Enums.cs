using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Core
{
    public enum DownloadStatuses
    {
        Downloading = 1,
        Completed = 2,
        Faulted = 3,
        Canceled = 4,
    }
}
