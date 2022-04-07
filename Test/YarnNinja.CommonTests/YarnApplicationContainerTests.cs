using Microsoft.VisualStudio.TestTools.UnitTesting;
using YarnNinja.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace YarnNinja.CommonTests
{
    [TestClass()]
    public class YarnApplicationContainerTests : BaseCommonTest
    {
        private YarnApplicationContainer? _masterApp;

        [TestMethod()]
        [DataRow("container-localizer-syslog", 2, DisplayName = "Correct Container Logs By Type container-localizer-syslog")]
        [DataRow("dag_1639352826059_8646_1.dot", 42, DisplayName = "Correct Container Logs By Type dag_1639352826059_8646_1.dot")]
        [DataRow("dag_1639352826059_8646_2.dot", 12, DisplayName = "Correct Container Logs By Type dag_1639352826059_8646_2.dot")]
        [DataRow("dag_1639352826059_8646_3.dot", 50, DisplayName = "Correct Container Logs By dag_1639352826059_8646_3.dot")]
        [DataRow("directory.info", 189, DisplayName = "Correct Container Logs By directory.info")]
        [DataRow("launch_container.sh", 56, DisplayName = "Correct Container Logs By launch_container.sh")]
        [DataRow("prelaunch.out", 4, DisplayName = "Correct Container Logs By prelaunch.out")]
        [DataRow("stderr", 5, DisplayName = "Correct Container Logs By stderr")]
        [DataRow("stdout", 468, DisplayName = "Correct Container Logs By stdout")]
        [DataRow("syslog", 96, DisplayName = "Correct Container Logs By syslog")]
        [DataRow("syslog_dag_1639352826059_8646_1", 11699, DisplayName = "Correct Container Logs By syslog_dag_1639352826059_8646_1")]
        [DataRow("syslog_dag_1639352826059_8646_1_post", 22, DisplayName = "Correct Container Logs By syslog_dag_1639352826059_8646_1_post")]
        [DataRow("syslog_dag_1639352826059_8646_2", 144, DisplayName = "Correct Container Logs By syslog_dag_1639352826059_8646_2")]
        [DataRow("syslog_dag_1639352826059_8646_2_post", 24, DisplayName = "Correct Container Logs By syslog_dag_1639352826059_8646_2_post")]
        [DataRow("syslog_dag_1639352826059_8646_3", 70, DisplayName = "Correct Container Logs By syslog_dag_1639352826059_8646_3")]
        public void Tez_GetLogsByTypeTest_ReturnCorrectLineCounts(string logType, int expectedLogCount)
        {

            _masterApp = GetActiveYarnApp(YarnApplicationType.Tez)?.ApplicationMaster;

            Assert.AreEqual(expected: expectedLogCount, actual: _masterApp?.GetLogsByType(logType).Count);
        }


        [TestMethod()]
        [DataRow(LogType.directory_info, 189, DisplayName = "Correct Container Logs By Type directory_info")]
        [DataRow(LogType.launch_container_sh, 56, DisplayName = "Correct Container Logs By Type launch_container_sh")]
        [DataRow(LogType.prelaunch_err, 0, DisplayName = "Correct Container Logs By Type prelaunch_err")]
        [DataRow(LogType.stderr, 5, DisplayName = "Correct Container Logs By stderr")]
        [DataRow(LogType.stdout, 468, DisplayName = "Correct Container Logs By stdout")]
        [DataRow(LogType.syslog, 12057, DisplayName = "Correct Container Logs By syslog")]
        [DataRow(LogType.prelaunch_out, 4, DisplayName = "Correct Container Logs By prelaunch.out")]
        [DataRow(LogType.DAG, 104, DisplayName = "Correct Container Logs By DAG")]
        [DataRow(LogType.Unknown, 0, DisplayName = "Correct Container Logs By Unknown")]
        public void Tez_GetLogsByBaseTypeTest_ReturnCorrectLineCounts(LogType logType, int expectedLogCount)
        {
            _masterApp = GetActiveYarnApp(YarnApplicationType.Tez)?.ApplicationMaster;

            Assert.AreEqual(expected: expectedLogCount, actual: _masterApp?.GetLogsByBaseType(logType).Count);
        }
    }
}