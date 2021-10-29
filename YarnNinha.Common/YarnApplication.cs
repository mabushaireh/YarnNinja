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
        //const string containerIdPattern = "Container: (container_[^\\s]*)";
        internal const string containerLogPattern = "Container: (.*?) on (.*?)_30050_(.*?)End of LogType:(.*?)[\\*]+";
        //const string workerNodePattern = "Container: (.*) on (.*)_30050";
        //const string appExitStatusPattern = "[AMShutdownThread] |rm.YarnTaskSchedulerService|: Unregistering application from RM, exitStatus=.*,";
        const string applicationExsitStausPattern = "Unregistering application from RM, exitStatus=(.*), exitMessage=(.*) stats:submittedDAGs=(\\d*), successfulDAGs=(\\d*), failedDAGs=(\\d*), killedDAGs=(\\d*)";

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
            YarnLog = yarnLog;

            Header = new YarnApplicationHeader();

            if (Regex.IsMatch(this.YarnLog, applicationIdPattern))
            {
                Header.Id = Regex.Match(this.YarnLog, applicationIdPattern).Value;
            }

            GetContainersAsync(this.YarnLog);
        }


        private async Task GetContainersAsync(string yarnLogText)
        {
            this.Containers = new List<YarnApplicationContainer>();
            Regex r = new Regex(containerLogPattern, RegexOptions.Singleline);

            Match m = r.Match(yarnLogText);
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
                    container = new YarnApplicationContainer { Id = id, WorkerNode = workernodec[0].Value.Trim() };
                    this.Containers.Add(container);
                }

                container.Logs.Add(new YarnApplicationContainerLog(m));


                m = m.NextMatch();
            }

            if (this.ApplicationMaster is not null)
                await ParesApplicationMasterAsync(this.ApplicationMaster);
        }

        private async Task ParesApplicationMasterAsync(YarnApplicationContainer appMaster)
        {
            //TODO: get app Start Time ==> first date one the syslog ??? couldnt find a better way
            // Get Syslog logs
            List<YarnApplicationContainerLog> syslog = appMaster.Logs.Where(p => p.YarnLogType == LogType.syslog).ToList();

            // Now merage all Loglines
            var allLogs = new List<YarnApplicationLogLine>();
            foreach (var log in syslog)
            {
                allLogs.AddRange(log.LogLines);
            }

            this.Header.Start = allLogs.Min(p => p.Timestamp);
            this.Header.Finish = allLogs.Max(p => p.Timestamp);

            // Get application status
            var shutdownLogs = allLogs.Where(p => p.Function.Equals("AMShutdownThread")).ToList();

            Regex r = new Regex(applicationExsitStausPattern, RegexOptions.Singleline);

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

                    m = m.NextMatch();
                }
            }


        }
    }
}
