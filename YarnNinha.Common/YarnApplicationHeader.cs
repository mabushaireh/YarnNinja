using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common.Core;

namespace YarnNinja.Common
{
    public class YarnApplicationHeader
    {
        public string Id { get; set; }
        public YarnApplicationType Type { get; set; }
        public YarnApplicationStatus Status {get;set;}
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}
