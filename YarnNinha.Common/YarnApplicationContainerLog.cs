using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using YarnNinja.Common.Utils;

namespace YarnNinja.Common
{
    public enum LogType
    {
        directory_info,
        launch_container_sh,
        prelaunch_err,
        stderr,
        stdout,
        syslog,
        prelaunch_out,
        DAG,
        Unknown,
    }
    public class YarnApplicationContainerLog
    {
        internal const string yarnLogLineTezPattern = "(?<TimeStamp>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) (?:\\[(?<TraceLevel>.*)\\]|(?<TraceLevel>.*)) \\[(?<Function>.*)\\] \\|?(?<Module>.*?)\\|?:";
        internal const string yarnLogLineTezSplitPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} (?:\\[.*\\]|.*) \\[.*\\] \\|?.*?\\|?:)";

        internal const string yarnLogLineSparkPattern = "(?<TimeStamp>\\d{2}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2}) (?<TraceLevel>.*) (?<Module>.*) \\[(?<Function>.*)]:";
        internal const string yarnLogLineSparkSplitPattern = @"(\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} .* .* \[.*]:)";

        internal const string yarnLogLineMapredPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) (\\w*) \\[(.*)\\] (.*?): (.*)";
        internal const string yarnLogLineMapredSplitPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} \\w* \\[.*\\] .*?: )";

        public string YarnLogType { get; set; }
        public YarnApplicationContainer YarnContainer { get; private set; }

        public LogType BaseLogType
        {
            get
            {
                // Handle senario when log type name has something like this: directory.info.This log file belongs to a running container (container_e03_1653473347542_0049_01_000001) and so may not be complete.
                switch (YarnLogType)
                {
                    case string s when s.StartsWith("container-localizer-syslog"):
                        return LogType.syslog;
                    case string s when s.StartsWith("directory.info"):
                        return LogType.directory_info;
                    case string s when s.StartsWith("launch_container.sh"):
                        return LogType.launch_container_sh;
                    case string s when s.StartsWith("prelaunch.err"):
                        return LogType.prelaunch_err;
                    case string s when s.StartsWith("prelaunch.out"):
                        return LogType.prelaunch_out;
                    case string s when s.StartsWith("stderr"):
                        return LogType.stderr;
                    case string s when s.StartsWith("stdout"):
                        return LogType.stdout;
                    case string s when s.StartsWith("syslog"):
                        return LogType.syslog;
                    default:
                        if (YarnLogType.StartsWith("dag_"))
                            return LogType.DAG;
                        else if (YarnLogType.StartsWith("syslog_"))
                            return LogType.syslog;
                        else
                            return LogType.Unknown;
                }
            }
            private set { }
        }
        public string LogText { get; set; }

        public List<YarnApplicationLogLine> LogLines { get; set; }

        private readonly YarnApplicationType yarnApplicationType;

        public YarnApplicationType YarnApplicationType
        {
            get { return this.yarnApplicationType; }
            private set { }
        }


        public async Task ParseLogsAsync(YarnLogFileReader logFileReader)
        {
            // Check if log type is directy info and this is not the first container and containers exists
            //if (this.BaseLogType == LogType.directory_info && this.YarnContainer.YarnApplication.DirectoryInfoLogs is not null)
            //{
            //    this.LogLines = this.YarnContainer.YarnApplication.DirectoryInfoLogs;

            //    // skip till end of container log type Directory
            //    while (!logFileReader.EndOfFile)
            //    {
            //        var line = logFileReader.ReadLine();

            //        if (YarnParserHelper.IsContainerLogContentEnd(line))
            //        {
            //            break;
            //        }
            //    }
            //}

            //Loop till end of container lines
            this.LogLines = new List<YarnApplicationLogLine>();

            while (!logFileReader.EndOfFile)
            {
                var line = logFileReader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (YarnParserHelper.IsContainerLogContentEnd(line))
                {
                    //if (this.BaseLogType == LogType.directory_info && this.YarnContainer.YarnApplication.DirectoryInfoLogs is null)
                    //{
                    //    this.YarnContainer.YarnApplication.DirectoryInfoLogs = this.LogLines;
                    //}

                    break;
                }

                //Check if line starts with datetime stamp
                var traceDatetime = DateTime.MinValue;
                var logLine = new YarnApplicationLogLine();

                string lineWithoutDate = line;
                if (YarnParserHelper.TryWithDateTime(line, out traceDatetime, out lineWithoutDate))
                {
                    logLine.Timestamp = traceDatetime;
                    lineWithoutDate = lineWithoutDate;
                    string msg = "";
                    string module = "";
                    string function = "";
                    string traceLevel = "";

                    if (YarnParserHelper.TryLineParse(lineWithoutDate, out msg, out function, out traceLevel, out module))
                    {

                        logLine.TraceLevel = traceLevel switch
                        {
                            "INFO" => TraceLevel.INFO,
                            "WARN" => TraceLevel.WARN,
                            "ERROR" => TraceLevel.ERROR,
                            "DEBUG" => TraceLevel.DEBUG,
                            _ => TraceLevel.UNKNOWN,
                        };
                        logLine.Msg = msg;
                        logLine.Module = module;
                        logLine.Function = function;
                    }
                }
                else
                {
                    logLine.TraceLevel = TraceLevel.UNKNOWN;
                    logLine.Msg = line;

                }

                logLine.LineNumber = logFileReader.CurrentLineNumber;
                this.LogLines.Add(logLine);
            }
        }


        private YarnApplicationContainerLog() { }

        public YarnApplicationContainerLog(string logType, YarnApplicationContainer container)
        {
            YarnLogType = logType;
            YarnContainer = container;
        }
    }
}
