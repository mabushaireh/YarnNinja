using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YarnNinja.Common
{
    public enum LogType
    {
        container_localizer_syslog,
        directory_info,
        launch_container_sh,
        prelaunch_err,
        stderr,
        stdout,
        syslog,
        prelaunch_out,
    }
    public class YarnApplicationContainerLog
    {
        public LogType YarnLogType { get; set; }
        public string LogText { get; set; }

        public YarnApplicationContainerLog(Match m)
        {
            this.LogText = m.Groups[3].Captures[0].Value;

            var logTypes = m.Groups[4].Captures[0].Value;

            switch (logTypes)
            {
                case "container-localizer-syslog":
                    YarnLogType = LogType.container_localizer_syslog;
                    break;
                case "directory.info":
                    YarnLogType = LogType.directory_info;
                    break;
                case "launch_container":
                    YarnLogType = LogType.launch_container_sh;
                    break;
                case "prelaunch.err":
                    YarnLogType = LogType.prelaunch_err;
                    break;
                case "prelaunch.out":
                    YarnLogType = LogType.prelaunch_out;
                    break;
                case "stderr":
                    YarnLogType = LogType.stderr;
                    break;
                case "stdout":
                    YarnLogType = LogType.stdout;
                    break;
                case "syslog":
                default:
                    YarnLogType = LogType.syslog;
                    break;
            }


        }
    }
}
