using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;
using YarnNinja.Common.Core;

namespace YarnNinja.CommonTests
{
    [TestClass()]
    public class BaseCommonTest
    {
        private const string tezLogFileName = @"./Samples/application_1639352826059_8646.log";
        private const string mapreduceLogFileName = "./Samples/application_1647350095798_0001.log";

        private static YarnApplication? _tezYarnApp;
        private static YarnApplication? _mapreduceYarnApp;

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
                        var mapredicelog = File.ReadAllText(mapreduceLogFileName);

                        _mapreduceYarnApp = new(mapredicelog);
                    }
                    return _mapreduceYarnApp;
                case YarnApplicationType.Spark:
                    return null;
                default:
                    return null;
            }
        }

    }
}
