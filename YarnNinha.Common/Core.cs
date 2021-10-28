using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarnNinja.Common.Core
{
    public enum TraceLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        FATAL
    }

    public enum YarnApplicationType
    {
        Tez,
        MapReduce,
        Spark
    }
}
