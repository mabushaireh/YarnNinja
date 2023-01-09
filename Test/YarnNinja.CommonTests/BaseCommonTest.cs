using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;
using YarnNinja.Common.Utils;

namespace YarnNinja.CommonTests
{
    [TestClass()]
    public class BaseCommonTest
    {
        protected const string tezLogFileName = @"./Samples/application_1639352826059_8646_tez.log";
        protected const string mapreduceLogFileName = "./Samples/application_1647184687608_0039_mapred.log";
        protected const string sparkLogFileName = "./Samples/1648899966078_0027_spark.log";

        private static YarnApplication? _tezYarnApp;
        private static YarnApplication? _mapreduceYarnApp;
        private static YarnApplication? _sparkYarnApp;


        protected static YarnApplication? GetActiveYarnApp(YarnApplicationType yarnType)
        {
            switch (yarnType)
            {
                case YarnApplicationType.Tez:
                    if (_tezYarnApp == null)
                    {
                        var yarnLogReader = new YarnLogFileReader();
                        yarnLogReader.OpenFile(tezLogFileName);
                        _tezYarnApp = new (yarnLogReader);
                        _tezYarnApp.ParseContainersAsync();

                    }
                    return _tezYarnApp;
                case YarnApplicationType.MapReduce:
                    if (_mapreduceYarnApp == null)
                    {
                        var yarnLogReader = new YarnLogFileReader();
                        yarnLogReader.OpenFile(mapreduceLogFileName);
                        _mapreduceYarnApp = new(yarnLogReader);
                        _mapreduceYarnApp.ParseContainersAsync();
                    }
                    return _mapreduceYarnApp;
                case YarnApplicationType.Spark:
                    if (_sparkYarnApp == null)
                    {
                        var yarnLogReader = new YarnLogFileReader();
                        yarnLogReader.OpenFile(sparkLogFileName);
                        _sparkYarnApp = new(yarnLogReader);
                        _sparkYarnApp.ParseContainersAsync();
                    }
                    return _sparkYarnApp;
                default:
                    return null;
            }
        }

    }
}
