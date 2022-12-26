using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YarnNinja.Common.Utils
{
    public static class YarnParserHelper
    {
        private const string YarnContainerLogBegin = "Container: (.*?) on (.*)";
        private const string YarnContainerLogEnd = "[\\*]+";
        private const string LogAggregationType = "LogAggregationType: (.*)";
        private const string LineSeperator = "(=+)";
        private const string LogType = "LogType:(.*)";
        private const string ContainerLogConetentEnd = "End of LogType:(.*)";
        private const string ApplicationIdPattern = "application_\\d+_\\d{4,}";
        private const string AppDateTimePattern1 = "(?<TimeStamp>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3})"; //yyyy-MM-dd HH:mm:ss,fff
        private const string AppDateTimePattern2 = "(?<TimeStamp>\\d{2}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2})"; //yy-MM-dd HH:mm:ss
        private const string yarnLogLinePattern1 = "(?:\\[(?<TraceLevel>.*)\\]|(?<TraceLevel>.*)) \\[(?<Function>.*)\\] \\|?(?<Module>.*?)\\|?:(?<Msg>.*)";
        private const string yarnLogLinePattern2 = "(?<TraceLevel>.*) (?<Module>.*) \\[(?<Function>.*)]:(?<Msg>.*)";
        private const string yarnLogLinePattern3 = "(?<TraceLevel>\\w*) \\[(?<Function>.*)\\] (?<Module>.*?): (?<Msg>.*)";


        public static bool TryContainerLogBegin(string line, out string containerName, out string workerName)
        {
            containerName = "";
            workerName = "";
            Regex r = new(YarnContainerLogBegin, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                Group containerId = m.Groups[1];
                Group workernode = m.Groups[2];

                CaptureCollection containerc = containerId.Captures;
                CaptureCollection workernodec = workernode.Captures;
                containerName = containerc[0].Value.Trim();
                workerName = workernodec[0].Value.Trim();

                return true;
            }

            return false;
        }

        public static bool TryApplicationId(string line, out string applicationId)
        {
            applicationId = "";
            Regex r = new(ApplicationIdPattern, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                Group applicationIdg = m.Groups[0];

                CaptureCollection applicationIdgc = applicationIdg.Captures;
                applicationId = applicationIdgc[0].Value.Trim();

                return true;
            }

            return false;
        }


        public static bool IsContainerLogEnd(string line)
        {

            Regex r = new(YarnContainerLogEnd, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

        public static bool IsLogAggregationType(string line)
        {
            Regex r = new(LogAggregationType, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

        public static bool IsLineSeperator(string line)
        {
            Regex r = new(LineSeperator, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

        public static bool TryContainerLogLogType(string line, out string logType)
        {
            logType = "";
            Regex r = new(LogType, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                Group logTypeg = m.Groups[1];

                CaptureCollection containerc = logTypeg.Captures;

                logType = containerc[0].Value.Trim();
                return true;
            }

            return false;
        }

        public static bool IsContainerLogContentEnd(string line)
        {
            Regex r = new(ContainerLogConetentEnd, RegexOptions.Singleline);
            Match m = r.Match(line);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

        public static bool TryApplicationType(string line, out YarnApplicationType applicationType)
        {
            applicationType = YarnApplicationType.NA;

            if (line.Contains("./tezlib"))
            {
                applicationType = YarnApplicationType.Tez;
                return true;
            }

            if (line.Contains("./__spark_conf__"))
            {
                applicationType = YarnApplicationType.Spark;
                return true;
            }

            if (line.Contains("/mr-framework"))
            {
                applicationType = YarnApplicationType.MapReduce;
                return true;
            }

            return false;
        }

        public static bool TryWithDateTime(string line, out DateTime correctDate, out string lineWithoutDate)
        {
            correctDate = DateTime.MinValue;
            lineWithoutDate = line;
            Regex r = new(AppDateTimePattern1, RegexOptions.Singleline);
            Match m = r.Match(line.Trim());


            if (m.Success)
            {
                string datetime = m.Groups["TimeStamp"].Captures[0].Value.Trim();
                correctDate = DateTime.ParseExact(datetime, DateTimeUtils.AppDateTimeFormat1, null);
                lineWithoutDate = lineWithoutDate.Replace(datetime, "");
                return true;
            }

            r = new(AppDateTimePattern2, RegexOptions.Singleline);
            m = r.Match(line);

            if (m.Success)
            {
                string datetime = m.Groups["TimeStamp"].Captures[0].Value.Trim();
                correctDate = DateTime.ParseExact(datetime, DateTimeUtils.AppDateTimeFormat2, null);
                lineWithoutDate = lineWithoutDate.Replace(datetime, ""); return true;
            }

            return false;
        }

        public static bool TryLineParse(string line, out string msg, out string function, out string traceLevel, out string module)
        {
            msg = line;
            function = "";
            module = "";
            traceLevel = "";

            Regex r = new(yarnLogLinePattern1, RegexOptions.Singleline);
            Match m = r.Match(line.Trim());
            if (m.Success)
            {
                Group traceLevelg = m.Groups["TraceLevel"];
                Group functiong = m.Groups["Function"];
                Group moduleg = m.Groups["Module"];
                Group msgg = m.Groups["Msg"];

                function = functiong.Captures[0].Value.Trim();
                module = moduleg.Captures[0].Value.Trim();
                traceLevel = traceLevelg.Captures[0].Value.Trim();
                msg = msgg.Captures[0].Value.Trim();
                return true;

            }


            r = new(yarnLogLinePattern2, RegexOptions.Singleline);
            m = r.Match(line);
            if (m.Success)
            {
                Group traceLevelg = m.Groups["TraceLevel"];
                Group functiong = m.Groups["Function"];
                Group moduleg = m.Groups["Module"];
                Group msgg = m.Groups["Msg"];

                function = functiong.Captures[0].Value.Trim();
                module = moduleg.Captures[0].Value.Trim();
                traceLevel = traceLevelg.Captures[0].Value.Trim();
                msg = msgg.Captures[0].Value.Trim();
                return true;
            }

            r = new(yarnLogLinePattern3, RegexOptions.Singleline);
            m = r.Match(line);
            if (m.Success)
            {
                Group traceLevelg = m.Groups["TraceLevel"];
                Group functiong = m.Groups["Function"];
                Group moduleg = m.Groups["Module"];
                Group msgg = m.Groups["Msg"];

                function = functiong.Captures[0].Value.Trim();
                module = moduleg.Captures[0].Value.Trim();
                traceLevel = traceLevelg.Captures[0].Value.Trim();
                msg = msgg.Captures[0].Value.Trim();
                return true;

            }

            return false;
        }
            
    }
}
