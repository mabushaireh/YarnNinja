using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    public class YarnApplication
    {
        internal const string applicationIdPattern = "application_\\d+_\\d{4,}";
        internal const string containerLogPattern = "Container: (.*?) on (.*?)_30050_(.*?)End of LogType:(.*?)[\\*]+";
        const string applicationExsitStausPattern = "Unregistering application from RM, exitStatus=(.*), exitMessage=(.*) stats:submittedDAGs=(\\d*), successfulDAGs=(\\d*), failedDAGs=(\\d*), killedDAGs=(\\d*)";
        const string userPattern = "export USER=\"(.*?)\"";
        const string queueNamePattern = ".*queueName=(.*)";
        

        public string YarnLog { get; }
        public YarnApplicationHeader Header { get; set; }
        public List<YarnApplicationContainer> Containers { get; set; }

        private YarnApplicationContainer? _applicationMaster = null;
        public YarnApplicationContainer? ApplicationMaster
        {
            get
            {
                if (_applicationMaster is null)
                {
                    var minOrder = this.Containers.Min(p => p.Order);

                    this._applicationMaster = this.Containers.Where(p => p.Order == minOrder).FirstOrDefault();
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


        private YarnApplication() { }


        public YarnApplication(string yarnLog)
        {
            this.YarnLog = yarnLog;

            Header = new YarnApplicationHeader();

            if (Regex.IsMatch(this.YarnLog, applicationIdPattern))
            {
                Header.Id = Regex.Match(this.YarnLog, applicationIdPattern).Value;
            }

            var isTez = yarnLog.Contains("./tezlib");
            var isMapred = false;
            if (!isTez)
                isMapred = yarnLog.Contains("./mr-framework");

            Header.Type = (isTez ? Core.YarnApplicationType.Tez : (isMapred ? Core.YarnApplicationType.MapReduce : Core.YarnApplicationType.Spark));


            ParseContainersAsync().Wait();
        }


        private async Task ParseContainersAsync()
        {
            this.Containers = new List<YarnApplicationContainer>();
            Regex r = new Regex(containerLogPattern, RegexOptions.Singleline);



            Match m = r.Match(this.YarnLog);
            while (m.Success)
            {
                Group containerId = m.Groups[1];
                Group workernode = m.Groups[2];

                CaptureCollection containerc = containerId.Captures;
                CaptureCollection workernodec = workernode.Captures;
                var id = containerc[0].Value.Trim();


                var container = this.Containers.FirstOrDefault(p => p.Id.Equals(id));


                if (container is null)
                {
                    container = new YarnApplicationContainer (this.Header.Type) { Id = id, WorkerNode = workernodec[0].Value.Trim() };

                    this.Containers.Add(container);
                }

                var LogText = m.Groups[3].Captures[0].Value.Trim();

                var logType = m.Groups[4].Captures[0].Value.Trim();

                container.Logs.Add(new YarnApplicationContainerLog(this.Header.Type, LogText, logType));


                m = m.NextMatch();
            }

            if (this.ApplicationMaster is not null)
                await ParseHeaderInfoAsync();
        }

        private async Task ParseHeaderInfoAsync()
        {
            // Get Application Type:
            var allDirectoryInfoLogs = this.ApplicationMaster.GetLogsByType(LogType.directory_info);


            // Set start and finish for the application as the app Master Start and Finish
            this.Header.Start = this.ApplicationMaster.Start;
            this.Header.Finish = this.ApplicationMaster.Finish;


            
            
            //Get User
            var allLunchLogs = this.ApplicationMaster.GetLogsByType(LogType.launch_container_sh);
            Regex r = new Regex(userPattern, RegexOptions.Singleline);
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

            // Get application status
            var allSysLogs = this.ApplicationMaster.GetLogsByType(LogType.syslog);
           


            // Get Queue Name
            var dagSubmitted = allSysLogs.Where(p =>
                                p.Function.StartsWith("IPC Server handler") &&
                                p.Msg.Contains("[Event:DAG_SUBMITTED]")
                ).FirstOrDefault();

            r = new Regex(queueNamePattern, RegexOptions.Singleline);

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

            r = new Regex(userPattern, RegexOptions.Singleline);

            var shutdownLogs = allSysLogs.Where(p => p.Function.Equals("AMShutdownThread")).ToList();

            r = new Regex(applicationExsitStausPattern, RegexOptions.Singleline);

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

                    switch (statusg.Captures[0].Value.Trim())
                    {
                        case "SUCCEEDED":
                            this.Header.Status = YarnApplicationStatus.SUCCEEDED;
                            break;
                        default:
                            this.Header.Status = YarnApplicationStatus.UNKNOWN;
                            break;
                    }

                    this.Header.Msg = msgg.Captures[0].Value.Trim();
                    this.Header.SubmittedDags = int.Parse(submittedDagsg.Captures[0].Value.Trim());
                    this.Header.SuccessfullDags = int.Parse(successfulDagsg.Captures[0].Value.Trim());
                    this.Header.FailedDags = int.Parse(failedDagsg.Captures[0].Value.Trim());
                    this.Header.KilledDags = int.Parse(killedDagsg.Captures[0].Value.Trim());
                    break;
                }
            }


        }
    }
}
