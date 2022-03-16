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
    }
    public class YarnApplicationContainerLog
    {
        internal const string yarnLogLineTezPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) \\[(\\w*)\\] \\[(.*)\\] \\|(.*)\\|: (.*)";
        internal const string yarnLogLineMapredPattern = "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3}) (\\w*) \\[(\\w)*\\] (.*): (.*)";

        private List<YarnApplicationLogLine> logLines = null;
        public LogType YarnLogType { get; set; }
        public string LogText { get; set; }

        public List<YarnApplicationLogLine> LogLines
        {
            get
            {
                if (logLines is null || logLines.Count ==0)
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
            if (this.YarnLogType == LogType.launch_container_sh || this.YarnLogType == LogType.directory_info)
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
                if (this.YarnApplicationType == Core.YarnApplicationType.Tez)
                    r = new Regex(yarnLogLineTezPattern, RegexOptions.None);
                else if (this.YarnApplicationType == Core.YarnApplicationType.MapReduce)
                    r = new Regex(yarnLogLineMapredPattern, RegexOptions.None);
                else
                    throw new Exception("Spark Logs is not implemeneted yet!");

                Match m = r.Match(LogText);
                while (m.Success)
                {
                    Group timestampg = m.Groups[1];
                    Group traceLevelg = m.Groups[2];
                    Group functiong = m.Groups[3];
                    Group moduleg = m.Groups[4];
                    Group msgg = m.Groups[5];


                    var logLine = new YarnApplicationLogLine
                    {
                        Timestamp = DateTime.ParseExact(timestampg.Captures[0].Value.Trim(), "yyyy-MM-dd HH:mm:ss,fff", null),
                        Function = functiong.Captures[0].Value.Trim(),
                        Module = moduleg.Captures[0].Value.Trim(),
                        Msg = msgg.Captures[0].Value.Trim()

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

            switch (logType)
            {
                case "container-localizer-syslog":
                    YarnLogType = LogType.container_localizer_syslog;
                    break;
                case "directory.info":
                    YarnLogType = LogType.directory_info;
                    break;
                case "launch_container.sh":
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
                    YarnLogType = LogType.syslog;
                    break;
                default:
                    if (logType.StartsWith("dag_")) YarnLogType = LogType.DAG;
                    else if (logType.StartsWith("syslog_dag")) YarnLogType = LogType.syslog;
                    break;
            }
        }
    }
}
