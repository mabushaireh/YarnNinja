using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace YarnNinja.Common
{
    public enum YarnApplicationStatus
    {
        SUCCEEDED,
        FAILED,
        KILLED,
        CANCELED,
        UNKNOWN
    }

    public enum TraceLevel
    {
        INFO,
        WARN,
        ERROR,
        DEBUG,
        UNKNOWN
    }

    public class ContainerProccessedArgs
    {
        public ContainerProccessedArgs(string containerId, int totalContainerLogs, int currentContainerLogsCount) 
        { ContainerId = containerId; TotalContainerLogs = totalContainerLogs; CurrentContainerLogsCount = currentContainerLogsCount; }
        public string ContainerId { get; } // readonly
        public int TotalContainerLogs { get;  }
        public int CurrentContainerLogsCount { get; }
    }

    public class YarnApplication
    {
        

        internal const string applicationIdPattern = "application_\\d+_\\d{4,}";
        internal const string containerLogPattern = "Container: (.*?) on (.*?)LogAggregationType:.*?LogContents:(.*?)End of LogType:(.*?)[\\*]+";
        const string applicationExsitStausTezPattern = "Unregistering application from RM, exitStatus=(.*), exitMessage=(.*) stats:submittedDAGs=(\\d*), successfulDAGs=(\\d*), failedDAGs=(\\d*), killedDAGs=(\\d*)";
        const string applicationExsitStausMapredPattern = "Final Stats: PendingReds:(\\d*) ScheduledMaps:(\\d*) ScheduledReds:(\\d*) AssignedMaps:(\\d*) AssignedReds:(\\d*) CompletedMaps:(\\d*) CompletedReds:(\\d*) ContAlloc:(\\d*) ContRel:(\\d*) HostLocal:(\\d*) RackLocal:(\\d*)";
        const string userPattern = "export USER=\"(.*?)\"";
        const string queueNameTezPattern = ".*queueName=(.*)";
        const string queueNameMapredPattern = ".*queue: (.*)";

        public delegate void ContainerProccessedHandler(object sender, ContainerProccessedArgs e);
        public event ContainerProccessedHandler ContainerProccessed;
        private int totalContainerLogs = 0;
        private int currentContainerLogsCount = 0;

        public string ShortId
        {
            get
            {
                return Header.Id.Substring(Header.Id.Length - 4);
            }
        }

        public string YarnLog { get; }
        public YarnApplicationHeader Header { get; set; }
        public List<YarnApplicationContainer> Containers { get; set; }

        private YarnApplicationContainer _applicationMaster;
        public YarnApplicationContainer ApplicationMaster
        {
            get
            {
                if (_applicationMaster is null)
                {
                    var minOrder = this.Containers.Min(p => p.Order);
                    var appMaster = this.Containers.Where(p => p.Order == minOrder).FirstOrDefault();
                    if (appMaster != null)
                        this._applicationMaster = appMaster;
                    else
                        throw new InvalidYarnFileFormat("Unable to find the application Master!");
                }
                return this._applicationMaster;
            }
        }

        private List<string>? _workerNodes = null;
        public List<string>? WorkerNodes
        {
            get
            {
                if (_workerNodes is null)
                {
                    _workerNodes = new List<string>();
                    foreach (var item in Containers)
                    {
                        if (!_workerNodes.Any(p => p.Equals(item.WorkerNode)))
                        {
                            _workerNodes.Add(item.WorkerNode);
                        }
                    }
                }
                return _workerNodes;
            }
        }

        [ExcludeFromCodeCoverage]
        private YarnApplication() { }


        public YarnApplication(string yarnLog)
        {
            this.YarnLog = yarnLog;

            Header = new YarnApplicationHeader();

            if (Regex.IsMatch(this.YarnLog, applicationIdPattern))
            {
                Header.Id = Regex.Match(this.YarnLog, applicationIdPattern).Value;
            }
            else
            {
                throw new InvalidYarnFileFormat("Unable to find the yarn application ID!");
            }

            var isTez = yarnLog.Contains("./tezlib");
            var isSpark = false;
            var isMapred = false;

            if (!isTez)
                isSpark = yarnLog.Contains("__spark_conf__");

            if (!isTez && !isSpark)
                isMapred = yarnLog.Contains("./mr-framework");
            

            Header.Type = (isTez ? YarnApplicationType.Tez : (isMapred ? YarnApplicationType.MapReduce : (isSpark ? YarnApplicationType.Spark : throw new InvalidYarnFileFormat("Not Support Yarn App Format!"))));


           // ParseContainersAsync().Wait();

        }


        public async Task ParseContainersAsync()
        {
            this.Containers = new List<YarnApplicationContainer>();
            Regex r = new(containerLogPattern, RegexOptions.Singleline);

            //Estimate the number of containers logs
            //FIXME: find a better way to estimate number of containers, without proccesing all the file with regex
            totalContainerLogs = (int) (this.YarnLog.Length / 15000);

            Match m = r.Match(this.YarnLog);
            while (m.Success)
            {
                ++currentContainerLogsCount;
                Group containerId = m.Groups[1];
                Group workernode = m.Groups[2];

                CaptureCollection containerc = containerId.Captures;
                CaptureCollection workernodec = workernode.Captures;
                var id = containerc[0].Value.Trim();


                var container = this.Containers.FirstOrDefault(p => p.Id.Equals(id));


                if (container is null)
                {
                    container = new YarnApplicationContainer(this.Header.Type) { Id = id, WorkerNode = workernodec[0].Value.Trim() };

                    this.Containers.Add(container);
                }

                var LogText = m.Groups[3].Captures[0].Value.Trim();

                var logType = m.Groups[4].Captures[0].Value.Trim();

                container.Logs.Add(new YarnApplicationContainerLog(this.Header.Type, LogText, logType));


                m = m.NextMatch();
                this.ContainerProccessed?.Invoke(this, new ContainerProccessedArgs(id, totalContainerLogs, currentContainerLogsCount));
            }
            this.ContainerProccessed?.Invoke(this, new ContainerProccessedArgs("", currentContainerLogsCount, currentContainerLogsCount));


            if (this.ApplicationMaster is not null)
                await ParseHeaderInfoAsync();
        }

        private async Task ParseHeaderInfoAsync()
        {
            // Set start and finish for the application as the app Master Start and Finish
            this.Header.Start = this.ApplicationMaster.Start;
            this.Header.Finish = this.ApplicationMaster.Finish;

            //Get User
            var allLunchLogs = this.ApplicationMaster.GetLogsByBaseType(LogType.launch_container_sh);
            Regex r = new(userPattern, RegexOptions.Singleline);
            var exportuserCommand = allLunchLogs.Where(P => P.Msg.StartsWith("export USER=")).FirstOrDefault();

            if (exportuserCommand is not null)
            {
                Match m = r.Match(exportuserCommand.Msg);

                while (m.Success)
                {
                    Group user = m.Groups[1];
                    this.Header.User = user.Captures[0].Value.Trim();
                    break;
                }
            }

            // Get application status, Queue name and user
            var allSysLogs = this.ApplicationMaster.GetLogsByBaseType(LogType.syslog);



            // Get Queue Name
            if (this.Header.Type == YarnApplicationType.Tez)
            {
                var dagSubmitted = allSysLogs.Where(p =>
                                    p.Function.StartsWith("IPC Server handler") &&
                                    p.Msg.Contains("[Event:DAG_SUBMITTED]")
                    ).FirstOrDefault();

                r = new Regex(queueNameTezPattern, RegexOptions.Singleline);

                if (dagSubmitted is not null)
                {
                    Match m = r.Match(dagSubmitted.Msg);

                    while (m.Success)
                    {
                        Group user = m.Groups[1];
                        this.Header.QueueName = user.Captures[0].Value.Trim();
                        break;
                    }
                }
            }
            else if (this.Header.Type == YarnApplicationType.MapReduce)
            {
                var rmCommunicator = allSysLogs.Where(p =>
                                    p.Function.Equals("main") &&
                                    p.Msg.Contains("queue:")
                    ).FirstOrDefault();

                r = new Regex(queueNameMapredPattern, RegexOptions.Singleline);

                if (rmCommunicator is not null)
                {
                    Match m = r.Match(rmCommunicator.Msg);

                    while (m.Success)
                    {
                        Group user = m.Groups[1];
                        this.Header.QueueName = user.Captures[0].Value.Trim();
                        break;
                    }
                }

            }
            else
            {
                // FIXME: not sure yet how to get the queue name from the log. couldnt find it yet
                this.Header.QueueName = "NA";

            }

            if (Header.Type == YarnApplicationType.Tez)
            {
                var shutdownLogs = allSysLogs.Where(p => p.Function.Equals("AMShutdownThread")).ToList();

                r = new Regex(applicationExsitStausTezPattern, RegexOptions.Singleline);

                foreach (var log in shutdownLogs)
                {


                    Match m = r.Match(log.Msg);

                    while (m.Success)
                    {
                        Group statusg = m.Groups[1];
                        Group msgg = m.Groups[2];
                        Group submittedDagsg = m.Groups[3];
                        Group successfulDagsg = m.Groups[4];
                        Group failedDagsg = m.Groups[5];
                        Group killedDagsg = m.Groups[6];

                        this.Header.Status = statusg.Captures[0].Value.Trim() switch
                        {
                            "SUCCEEDED" => YarnApplicationStatus.SUCCEEDED,
                            _ => YarnApplicationStatus.UNKNOWN,
                        };

                        this.Header.Msg = msgg.Captures[0].Value.Trim();
                        this.Header.SubmittedDags = int.Parse(submittedDagsg.Captures[0].Value.Trim());
                        this.Header.SuccessfullDags = int.Parse(successfulDagsg.Captures[0].Value.Trim());
                        this.Header.FailedDags = int.Parse(failedDagsg.Captures[0].Value.Trim());
                        this.Header.KilledDags = int.Parse(killedDagsg.Captures[0].Value.Trim());
                        break;
                    }
                }
            }
            else if(Header.Type == YarnApplicationType.MapReduce)
            {
                var shutdownLogs = allSysLogs.Where(p => p.TraceLevel == TraceLevel.INFO 
                && p.Function.StartsWith("Thread") 
                && p.Module.Equals("org.apache.hadoop.mapreduce.v2.app.rm.RMContainerAllocator")
                && p.Msg.StartsWith("Final Stats:")).ToList();

                r = new Regex(applicationExsitStausMapredPattern, RegexOptions.Singleline);

                foreach (var log in shutdownLogs)
                {


                    Match m = r.Match(log.Msg);

                    while (m.Success)
                    {
                        Group pendingRedsg = m.Groups[1];
                        Group scheduledMaps = m.Groups[2];
                        Group scheduledRedsg = m.Groups[3];
                        Group assignedMapsg = m.Groups[4];
                        Group assignedRedsg = m.Groups[5];
                        Group completedMapsg = m.Groups[6];
                        Group completedRedsg = m.Groups[7];
                        Group contAllocg = m.Groups[8];
                        Group contRelg = m.Groups[9];
                        Group hostLocalg = m.Groups[10];
                        Group rackLocalg = m.Groups[11];

                        this.Header.CompletedMappers = int.Parse(completedMapsg.Captures[0].Value.Trim());
                        this.Header.CompletedReducers = int.Parse(completedRedsg.Captures[0].Value.Trim());


                        break;
                    }
                }
            }


            //Get containers status:
            if (this.Header.Type == YarnApplicationType.Tez)
            {
                //var allDagLogs = this.ApplicationMaster.GetLogsByBaseType(LogType.DAG);
                var conatinerStatus = allSysLogs.Where(p => p.TraceLevel == TraceLevel.INFO && p.Function.StartsWith("Dispatcher thread") && p.Module.Contains("container.AMContainerImpl")).ToList();
                r = new Regex("Container (.*) exited with diagnostics set to Container (.*), exitCode=(.*)\\. (\\[(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}.\\d{3})\\])?([\\s\\S\\r\\n.]*)", RegexOptions.Multiline);

                foreach (var line in conatinerStatus)
                {
                    var m = r.Match(line.Msg);

                    if (m.Success)
                    {
                        Group ContainerIdg = m.Groups[1];
                        Group statusg = m.Groups[2];
                        Group statusCodeg = m.Groups[3];
                        Group statusTimeg = m.Groups[5];
                        Group StatusMessageg = m.Groups[6];

                        var container = this.Containers.Where(p => p.Id.Equals(ContainerIdg.Captures[0].Value.Trim())).FirstOrDefault();

                        if (container != null)
                        {
                            container.Status = statusg.Captures[0].Value.Trim();
                            container.StatusCode = statusCodeg.Captures[0].Value.Trim();
                            if (statusTimeg!=null && statusTimeg.Captures.Count >0 && !string.IsNullOrEmpty(statusTimeg.Captures[0].Value))
                                container.StatusTime = DateTime.ParseExact(statusTimeg.Captures[0].Value.Trim(), "yyyy-MM-dd HH:mm:ss.fff", null);

                            container.StatusMessage = StatusMessageg.Captures[0].Value.Trim();
                        }
                    }

                }
            }
           
            
        }
    }
}
