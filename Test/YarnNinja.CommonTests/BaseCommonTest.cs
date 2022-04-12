using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;

namespace YarnNinja.CommonTests
{
    [TestClass()]
    public class BaseCommonTest
    {
        private const string tezLogFileName = @"./Samples/application_1639352826059_8646.log";
        private const string mapreduceLogFileName = "./Samples/application_1647350095798_0001.log";
        private const string sparkLogFileName = "./Samples/1648899966078_0027.log";

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
                        var tezlog = File.ReadAllText(tezLogFileName);
                        _tezYarnApp = new (tezlog);

                    }
                    return _tezYarnApp;
                case YarnApplicationType.MapReduce:
                    if (_mapreduceYarnApp == null)
                    {
                        var mapreducelog = File.ReadAllText(mapreduceLogFileName);

                        _mapreduceYarnApp = new(mapreducelog);
                    }
                    return _mapreduceYarnApp;
                case YarnApplicationType.Spark:
                    if (_sparkYarnApp == null)
                    {
                        var sparklog = File.ReadAllText(sparkLogFileName);

                        _sparkYarnApp = new(sparklog);
                    }
                    return _sparkYarnApp;
                default:
                    return null;
            }
        }

    }
}
