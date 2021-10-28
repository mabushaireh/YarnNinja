using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YarnNinja.Common.Utils
{
    public static class YarnLogParser
    {
        const string applicationIdPattern = "application_\\d+_\\d{4,}";
        const string containerIdPattern = "Container: (container_[^\\s]*)";
        const string containerLogPattern = "Container: (.*?) on (.*?)_30050_(.*?)End of LogType:(.*?)[\\*]+";
        const string workerNodePattern = "Container: (.*) on (.*)_30050";
        const string appExitStatusPattern = "[AMShutdownThread] |rm.YarnTaskSchedulerService|: Unregistering application from RM, exitStatus=.*,";



        public static async Task<YarnApplicationHeader> GetHeaderAsync(string yarnLogText)
        {
            var header = new YarnApplicationHeader();

            if (Regex.IsMatch(yarnLogText, applicationIdPattern))
            {
                header.Id = Regex.Match(yarnLogText, applicationIdPattern).Value;
            }

            return header;
        }

        public static async Task<YarnApplication> Parse(string yarnLogText)
        {
            var app = new YarnApplication();

            app.Header = await GetHeaderAsync(yarnLogText);
            app.Containers = await GetContainers(yarnLogText);
            return app;
        }

        private static async Task<List<YarnApplicationContainer>> GetContainers(string yarnLogText)
        {
            var containers = new List<YarnApplicationContainer>();
            Regex r = new Regex(containerLogPattern, RegexOptions.Singleline);

            Match m = r.Match(yarnLogText);
            while (m.Success)
            {
                Group containerId = m.Groups[1];
                Group workernode = m.Groups[2];

                CaptureCollection containerc = containerId.Captures;
                CaptureCollection workernodec = workernode.Captures;
                var id = containerc[0].Value;


                var container = containers.FirstOrDefault(p => p.Id.Equals(id));


                if (container is null)
                {
                    container = new YarnApplicationContainer { Id = id, WorkerNode = workernodec[0].Value };
                    containers.Add(container);
                }

                container.Logs.Add(new YarnApplicationContainerLog(m));


                m = m.NextMatch();
            }
            return containers;
        }
    }

}
