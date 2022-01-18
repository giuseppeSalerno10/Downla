using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Core
{
    internal class ConnectionInfo
    {
        public Task<HttpResponseMessage> Task { get; set; }
        public int ConnectionIndex { get; set; }
    }
}
