using System.Text.RegularExpressions;
using YarnNinja.Common.Core;

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
        DAG,
        Unknown,
    }
    public class YarnApplicationContainerLog
    {
        internal const string yarnLogLineTezPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) \\[(\\w*)\\] \\[(.*)\\] \\|(.*)\\|: (.*)";
        internal const string yarnLogLineTezSplitPattern = "\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} \\[\\w*\\] \\[.*\\] \\|.*\\|: ";

        internal const string yarnLogLineMapredPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) (\\w*) \\[(.*)\\] (.*?): (.*)";
        internal const string yarnLogLineMapredSplitPattern = "\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} \\w* \\[.*\\] .*?: ";

        private List<YarnApplicationLogLine> logLines = null;
        public string YarnLogType { get; set; }
        public LogType BaseLogType
        {
            get
            {
                switch (YarnLogType)
                {
                    case "container-localizer-syslog":
                        return LogType.container_localizer_syslog;
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
                        else if (YarnLogType.StartsWith("syslog_dag"))
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
                if (logLines is null || logLines.Count == 0)
                {
                    logLines = new List<YarnApplicationLogLine>();
                    ParseLogsAsync().Wait();
                }
                return logLines;

            }
        }

        private YarnApplicationType yarnApplicationType = YarnApplicationType.Tez;

        public Core.YarnApplicationType YarnApplicationType
        {
            get { return this.yarnApplicationType; }
            private set { }
        }


        private async Task ParseLogsAsync()
        {
            if (this.BaseLogType == LogType.launch_container_sh || this.BaseLogType == LogType.directory_info)
            {
                var result = LogText.Split(new[] { '\r', '\n' });
                foreach (var line in result)
                {
                    var logLine = new YarnApplicationLogLine();

                    logLine.Msg = line;
                    this.logLines.Add(logLine);
                }
            }
            else
            {
                Regex r = null;
                Regex l = null;
                if (this.YarnApplicationType == Core.YarnApplicationType.Tez)
                {
                    r = new Regex(yarnLogLineTezPattern, RegexOptions.None);
                    l = new Regex(yarnLogLineTezSplitPattern, RegexOptions.None);
                }

                else if (this.YarnApplicationType == Core.YarnApplicationType.MapReduce)
                {
                    r = new Regex(yarnLogLineMapredPattern, RegexOptions.None);
                    l = new Regex(yarnLogLineMapredSplitPattern, RegexOptions.None);
                }
                else
                    throw new Exception("Spark Logs is not implemeneted yet!");

                var lines = l.Split(LogText);
                //Remove first match because we know it doent count
                lines = lines.Where((source, index) => index != 0).ToArray();
                var i = 0;
                Match m = r.Match(LogText);
                while (m.Success)
                {
                    Group timestampg = m.Groups[1];
                    Group traceLevelg = m.Groups[2];
                    Group functiong = m.Groups[3];
                    Group moduleg = m.Groups[4];
                    Group msgg = m.Groups[5];

                    if (!lines[i].StartsWith(msgg.Captures[0].Value.Trim())) {
                        throw new Exception("Parse lines failed!");
                    }
                    var logLine = new YarnApplicationLogLine
                    {
                        Timestamp = DateTime.ParseExact(timestampg.Captures[0].Value.Trim(), "yyyy-MM-dd HH:mm:ss,fff", null),
                        Function = functiong.Captures[0].Value.Trim(),
                        Module = moduleg.Captures[0].Value.Trim(),
                        Msg = lines[i++]

                    };

                    var traceLevelstr = traceLevelg.Captures[0].Value.Trim();
                    switch (traceLevelstr)
                    {
                        case "INFO":
                            logLine.TraceLevel = TraceLevel.INFO;
                            break;
                        case "WARN":
                            logLine.TraceLevel = TraceLevel.WARN;
                            break;
                        case "ERROR":
                            logLine.TraceLevel = TraceLevel.ERROR;
                            break;
                        case "DEBUG":
                            logLine.TraceLevel = TraceLevel.DEBUG;
                            break;
                        default:
                            logLine.TraceLevel = TraceLevel.UNKNOWN;
                            break;
                    }
                    this.logLines.Add(logLine);
                    m = m.NextMatch();
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
