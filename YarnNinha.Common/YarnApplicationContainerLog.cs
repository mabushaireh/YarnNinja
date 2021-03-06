using System.Text.RegularExpressions;

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
        internal const string yarnLogLineMapredSplitPattern = "\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} \\w* \\[.*\\] .*?: ";

        private List<YarnApplicationLogLine> logLines;
        public string YarnLogType { get; set; }
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

        public YarnApplicationType YarnApplicationType
        {
            get { return this.yarnApplicationType; }
            private set { }
        }


        private async Task ParseLogsAsync()
        {
            Regex r = null;
            Regex l = null;
            string timestampForamt = "";

            if (this.yarnApplicationType != YarnApplicationType.Spark)
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
                    return;
                }
                else
                {

                    if (this.YarnApplicationType == YarnApplicationType.Tez)
                    {
                        
                        r = new Regex(yarnLogLineTezPattern, RegexOptions.Singleline);
                        l = new Regex(yarnLogLineTezSplitPattern, RegexOptions.None);
                        timestampForamt = "yyyy-MM-dd HH:mm:ss,fff";
                    }

                    else if (this.YarnApplicationType == YarnApplicationType.MapReduce)
                    {
                        r = new Regex(yarnLogLineMapredPattern, RegexOptions.None);
                        l = new Regex(yarnLogLineMapredSplitPattern, RegexOptions.None);
                        timestampForamt = "yyyy-MM-dd HH:mm:ss,fff";
                    }
                }
            }
            else
            {
                if (this.BaseLogType != LogType.stdout && this.BaseLogType != LogType.stderr)
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
                    return;
                }
                else
                {
                    r = new Regex(yarnLogLineSparkPattern, RegexOptions.Singleline);
                    l = new Regex(yarnLogLineSparkSplitPattern, RegexOptions.None);
                    timestampForamt = "yy/MM/dd HH:mm:ss";
                }
            }


            //FIXME: added new line at the end to make split works
            var txt = LogText + Environment.NewLine;

            var lines = l.Split(txt);
            //Remove empty lines
            lines = lines.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            

            for (int i = 0; i < lines.Length;)
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
                        Timestamp = DateTime.ParseExact(timestampg.Captures[0].Value.Trim(), timestampForamt, null),
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
                    i += 2;
                }
                else 
                {
                    //remove first lines dosest start with datatime stamp
                    i++;
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
