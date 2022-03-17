using YarnNinja.Common.Core;

namespace YarnNinja.Common
{
    public class YarnApplicationContainer
    {
        private int countMappers = -1;
        private int countReducers = -1;

        
        public int CountMappers
        {
            get 
            {
                if (yarnApplicationType != YarnApplicationType.MapReduce) {
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



        private YarnApplicationType yarnApplicationType = YarnApplicationType.Tez;
        public string ApplicationType { get; private set; }

        public string Id { get; set; }

        private DateTime start = DateTime.MinValue;
        public DateTime Start
        {
            get
            {
                if (start == DateTime.MinValue)
                {
                    // parse for container start time
                    var allLogs = GetLogsByBaseType(LogType.syslog);

                    this.start = allLogs.Min(p => p.Timestamp);
                    this.finish = allLogs.Max(p => p.Timestamp);
                }

                return this.start;
            }
        }

        private DateTime finish = DateTime.MaxValue;
        public DateTime Finish
        {
            get
            {
                if (finish == DateTime.MaxValue)
                {
                    // parse for container start time
                    var allLogs = GetLogsByBaseType(LogType.syslog);

                    this.start = allLogs.Min(p => p.Timestamp);
                    this.finish = allLogs.Max(p => p.Timestamp);
                }

                return this.finish;
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

        public string Status { get; set; }

        public List<YarnApplicationContainerLog> Logs { get; set; } = new List<YarnApplicationContainerLog>();

        public TimeSpan Duration
        {
            get
            {
                return Finish - Start;
            }
        }



        public YarnApplicationContainer(YarnApplicationType applicationType) { this.yarnApplicationType = yarnApplicationType; }

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