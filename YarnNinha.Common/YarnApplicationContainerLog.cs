using System.Text.RegularExpressions;
using YarnNinja.Common.Core;

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

        internal const string yarnLogLineMapredPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) (\\w*) \\[(.*)\\] (.*?): (.*)";
        internal const string yarnLogLineMapredSplitPattern = "\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} \\w* \\[.*\\] .*?: ";

        private List<YarnApplicationLogLine> logLines;
        public string YarnLogType { get; set; }
        public LogType BaseLogType
        {
            get
            {
                switch (YarnLogType)
                {
                    case "container-localizer-syslog":
                        return LogType.syslog;
                    case "directory.info":
                        return LogType.directory_info;
                    case "launch_container.sh":
                        return LogType.launch_container_sh;
                    case "prelaunch.err":
                        return LogType.prelaunch_err;
                    case "prelaunch.out":
                        return LogType.prelaunch_out;
                    case "stderr":
                        return LogType.stderr;
                    case "stdout":
                        return LogType.stdout;
                    case "syslog":
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

        public List<YarnApplicationLogLine> LogLines
        {
            get
            {
                if (logLines is null)
                {
                    logLines = new List<YarnApplicationLogLine>();
                    ParseLogsAsync().Wait();
                }
                return logLines;

            }
        }

        private readonly YarnApplicationType yarnApplicationType;

        public Core.YarnApplicationType YarnApplicationType
        {
            get { return this.yarnApplicationType; }
            private set { }
        }


        private async Task ParseLogsAsync()
        {
            if (this.BaseLogType != LogType.syslog)
            {
                var result = LogText.Split(new[] { '\r', '\n' });
                foreach (var line in result)
                {
                    var logLine = new YarnApplicationLogLine
                    {
                        Msg = line
                    };
                    this.logLines.Add(logLine);
                }
            }
            else
            {
                Regex r = null;
                Regex l = null;
                if (this.YarnApplicationType == Core.YarnApplicationType.Tez)
                {
                    r = new Regex(yarnLogLineTezPattern, RegexOptions.Singleline);
                    l = new Regex(yarnLogLineTezSplitPattern, RegexOptions.None);
                }

                else if (this.YarnApplicationType == Core.YarnApplicationType.MapReduce)
                {
                    r = new Regex(yarnLogLineMapredPattern, RegexOptions.None);
                    l = new Regex(yarnLogLineMapredSplitPattern, RegexOptions.None);
                }
                else
                    throw new Exception("Spark Logs is not implemeneted yet!");

                //FIXME: added new line at the end to make split works
                var txt = LogText + Environment.NewLine;

                var lines = l.Split(txt);
                //Remove empty lines
                lines = lines.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                for (int i =0; i < lines.Length; i += 2)
                {

                    Match m = r.Match(lines[i]);
                    if (m.Success)
                    {
                        Group timestampg = m.Groups["TimeStamp"];
                        Group traceLevelg = m.Groups["TraceLevel"];
                        Group functiong = m.Groups["Function"];
                        Group moduleg = m.Groups["Module"];

                        var logLine = new YarnApplicationLogLine
                        {
                            Timestamp = DateTime.ParseExact(timestampg.Captures[0].Value.Trim(), "yyyy-MM-dd HH:mm:ss,fff", null),
                            Function = functiong.Captures[0].Value.Trim(),
                            Module = moduleg.Captures[0].Value.Trim(),
                            Msg = lines[i + 1].Trim()
                    };

                        var traceLevelstr = traceLevelg.Captures[0].Value.Trim();
                        logLine.TraceLevel = traceLevelstr switch
                        {
                            "INFO" => TraceLevel.INFO,
                            "WARN" => TraceLevel.WARN,
                            "ERROR" => TraceLevel.ERROR,
                            "DEBUG" => TraceLevel.DEBUG,
                            _ => TraceLevel.UNKNOWN,
                        };
                        this.logLines.Add(logLine);
                    }

                }
            }

        }

        private YarnApplicationContainerLog() { }

        public YarnApplicationContainerLog(YarnApplicationType applicationType, string logText, string logType)
        {

            this.LogText = logText;
            this.yarnApplicationType = applicationType;
            YarnLogType = logType;
        }
    }
}
