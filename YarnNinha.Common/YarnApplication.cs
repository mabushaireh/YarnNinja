using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarnNinja.Common
{
    public enum YarnApplicationStatus
    {
        SUCCEEDED
    }
    public class YarnApplication
    {
        public YarnApplicationHeader Header { get; set; }
        public List<YarnApplicationContainer> Containers { get; set; }
    }
}
