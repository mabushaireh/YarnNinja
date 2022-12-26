using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarnNinja.Common
{
    public class YarnApplicationLogLine
    {
        public int LineNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public TraceLevel TraceLevel { get; set; }
        public string Function { get; set; }
        public string Module { get; set; }
        public string Msg { get; set; }

        public bool IsSyslogLine { get { return (this.Timestamp == DateTime.MinValue? false : true); } }
    }
}
