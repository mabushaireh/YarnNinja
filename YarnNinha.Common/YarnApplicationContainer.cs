namespace YarnNinja.Common
{
    public class YarnApplicationContainer
    {

        public string Id { get; set; }
        private DateTime start = DateTime.MinValue;
        private DateTime finish = DateTime.MaxValue;


        public int Order
        {
            get
            {
                return int.Parse(Id.Substring(Id.Length - 6, 6));
            }
        }

        public List<YarnApplicationLogLine> GetLogsByType(LogType logType)
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

        public string WorkerNode { get; set; }
        public DateTime Start
        {
            get {
                if (start == DateTime.MinValue)
                {
                    // parse for container start time
                    var allLogs = GetLogsByType(LogType.syslog);

                    this.start = allLogs.Min(p => p.Timestamp);
                    this.finish = allLogs.Max(p => p.Timestamp);
                }

                return this.start;
            }
        }
        public string Status { get; set; }
        public List<YarnApplicationContainerLog> Logs { get; set; } = new List<YarnApplicationContainerLog>();
        public DateTime Finish { get
            {
                if (finish == DateTime.MaxValue)
                {
                    // parse for container start time
                    var allLogs = GetLogsByType(LogType.syslog);

                    this.start = allLogs.Min(p => p.Timestamp);
                    this.finish = allLogs.Max(p => p.Timestamp);
                }

                return this.finish;
            } }

        public TimeSpan Duration
        {
            get
            {
                return Finish - Start;
            }
        }
    }
}