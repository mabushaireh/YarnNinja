using System.Diagnostics.CodeAnalysis;

namespace YarnNinja.Common
{
    public class YarnApplicationContainer
    {
        
        

        public string Id { get; set; }

        private int countMappers = -1;

        [ExcludeFromCodeCoverage]
        public int CountMappers
        {
            get
            {
                if (yarnApplicationType != YarnApplicationType.MapReduce)
                {
                    return -1;
                }
                if (countMappers < 0)
                {
                    // get all syslog lines
                    var allSyslogLine = GetLogsByBaseType(LogType.syslog);


                    var filterTasks = allSyslogLine.Where(p => p.TraceLevel == TraceLevel.INFO && p.Function.Equals("main") && p.Equals("org.apache.hadoop.metrics2.impl.MetricsSystemImpl") && p.Msg.EndsWith("metrics system started")).ToList();
                    countMappers = filterTasks.Where(p => p.Msg.StartsWith("MapTask")).Count();
                    countReducers = filterTasks.Where(p => p.Msg.StartsWith("ReduceTask")).Count();
                }
                return countMappers;
            }
            private set { }
        }
        
        private int countReducers = -1;

        [ExcludeFromCodeCoverage]
        public int CountReducers
        {
            get
            {
                if (yarnApplicationType != YarnApplicationType.MapReduce)
                {
                    return -1;
                }
                if (countReducers < 0)
                {
                    // get all syslog lines
                    var allSyslogLine = GetLogsByBaseType(LogType.syslog);

                    var filterTasks = allSyslogLine.Where(p => p.TraceLevel == TraceLevel.INFO && p.Function.Equals("main") && p.Equals("org.apache.hadoop.metrics2.impl.MetricsSystemImpl") && p.Msg.EndsWith("metrics system started")).ToList();
                    countMappers = filterTasks.Where(p => p.Msg.StartsWith("MapTask")).Count();
                    countReducers = filterTasks.Where(p => p.Msg.StartsWith("ReduceTask")).Count();
                }
                return countReducers;
            }
            private set { }
        }

        private readonly YarnApplicationType yarnApplicationType;
        public string ApplicationType { get; private set; }

        private DateTime start = DateTime.MinValue;
        public DateTime Start
        {
            get
            {
                List<YarnApplicationLogLine> allLogs;
                if (start == DateTime.MinValue)
                {
                    if (yarnApplicationType == YarnApplicationType.Tez || yarnApplicationType == YarnApplicationType.MapReduce)
                        // parse for container start time
                        allLogs = GetLogsByBaseType(LogType.syslog);
                    {
                        allLogs = GetLogsByBaseType(LogType.stderr);
                        allLogs.AddRange(GetLogsByBaseType(LogType.stdout));
                    }

                    if (allLogs.Count > 0)
                    {
                        this.start = allLogs.Min(p => p.Timestamp);
                        this.finish = allLogs.Max(p => p.Timestamp);
                    }
                    
                }

                return this.start;
            }
        }

        private DateTime finish = DateTime.MaxValue;
        internal string Status;

        public DateTime Finish
        {
            get
            {
                List<YarnApplicationLogLine> allLogs;

                if (finish == DateTime.MaxValue)
                {
                    if (yarnApplicationType == YarnApplicationType.Tez || yarnApplicationType == YarnApplicationType.MapReduce)
                        // parse for container start time
                        allLogs = GetLogsByBaseType(LogType.syslog);
                    else
                    {
                        allLogs = GetLogsByBaseType(LogType.stderr);
                        allLogs.AddRange(GetLogsByBaseType(LogType.stdout));
                    }
                        


                    if (allLogs.Count > 0)
                    {
                        this.start = allLogs.Min(p => p.Timestamp);
                        this.finish = allLogs.Max(p => p.Timestamp);
                    }
                        
                }

                return this.finish;
            }
        }

        public string StatusMessage { get; set; }


        public TimeSpan Duration
        {
            get
            {
                return Finish - Start;
            }
        }


        public int Order
        {
            get
            {
                return int.Parse(Id.Substring(Id.Length - 6, 6));
            }
        }

        public string WorkerNode { get; set; }

        public List<YarnApplicationContainerLog> Logs { get; set; } = new List<YarnApplicationContainerLog>();
        public string StatusCode { get; internal set; }
        public DateTime StatusTime { get; internal set; }

        public YarnApplicationContainer(YarnApplicationType applicationType) { this.yarnApplicationType = applicationType; }

        [ExcludeFromCodeCoverage]
        private YarnApplicationContainer() { }

        public List<YarnApplicationLogLine> GetLogsByType(string logType)
        {
            List<YarnApplicationContainerLog> logs = this.Logs.Where(p => p.YarnLogType == logType).ToList();
            // Now merage all Loglines
            var allLogs = new List<YarnApplicationLogLine>();
            foreach (var log in logs)
            {
                allLogs.AddRange(log.LogLines);
            }

            return allLogs;
        }

        public List<YarnApplicationLogLine> GetLogsByBaseType(LogType logType)
        {
            List<YarnApplicationContainerLog> logs = this.Logs.Where(p => p.BaseLogType == logType).ToList();
            // Now merage all Loglines
            var allLogs = new List<YarnApplicationLogLine>();
            foreach (var log in logs)
            {
                allLogs.AddRange(log.LogLines);
            }

            return allLogs;
        }

        
    }
}