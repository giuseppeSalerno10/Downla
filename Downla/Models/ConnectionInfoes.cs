using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Core
{
    internal class ConnectionInfoes
    {
        #pragma warning disable CS8618
        public Task<HttpResponseMessage> Task { get; set; }
        #pragma warning restore CS8618
        
        public int Index { get; set; }
    }
}
