using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using YarnNinja.Common;
using YarnNinja.Common.Utils;

namespace YarnNinja.CommonTests
{
    [TestClass()]
    public class YarnApplicationTests : BaseCommonTest
    {

        [TestMethod()]
        [DataRow(YarnApplicationType.Tez, DisplayName = "Valid Tez Yarn Application")]
        [DataRow(YarnApplicationType.MapReduce, DisplayName = "Valid Mapreduce Yarn Application")]
        [DataRow(YarnApplicationType.Spark, DisplayName = "Valid Spark Yarn Application")]
        public void YarnApplication_TezOrMapReduce_ReturnYarnObject(YarnApplicationType expectedAppType)
        {

            if (GetActiveYarnApp(expectedAppType) == null)
            {
                Assert.Fail("Failed to Parse App");
            }

            Assert.AreEqual(expected: expectedAppType, actual: GetActiveYarnApp(expectedAppType)?.Header.Type);
        }

        [TestMethod()]
        [DataRow("FakeFile.log", DisplayName = "InvalidFaileFormatException Expected")]
        public void YarnApplication_InvalidFileFormat_NotAYarnApp(string fileName)
        {
            YarnApplication? _yarnApp = null;
            try
            {

                var log = File.ReadAllText(@"./Samples/" + fileName);
                _yarnApp = new YarnApplication(log);
                _yarnApp.ParseContainersAsync();

                Assert.Fail(); // If it gets to this line, no exception was thrown
            }
            catch (InvalidYarnFileFormat)
            {
                Assert.IsNull(_yarnApp);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }


        [TestMethod()]
        [DataRow(YarnApplicationType.Tez, 10, DisplayName = "Correct Workers for Tez")]
        [DataRow(YarnApplicationType.Spark, 3, DisplayName = "Correct Workers for Spark")]
        public void YarnApplicationTest_ReturnWorkers(YarnApplicationType appType, int expectedWorkers)
        {
            Assert.IsTrue(GetActiveYarnApp(appType)?.WorkerNodes?.Count == expectedWorkers);
        }

        

        [TestMethod()]
        [DataRow(YarnApplicationType.Tez,
            "application_1639352826059_8646",
            "2022-03-14 11:22:56,847",
            "2022-03-14 11:33:43,392",
            367,
            "hive",
            "default",
            YarnApplicationStatus.SUCCEEDED,
            DisplayName = "Correct Header for Tez")]
        [DataRow(YarnApplicationType.Spark,
            "application_1648899966078_0027",
            "2022-04-04 11:37:27,000",
            "2022-04-04 11:49:50,000",
            31,
            "spark",
            "NA",
            YarnApplicationStatus.SUCCEEDED,
            DisplayName = "Correct Header for Spark")]
        public void YarnApplicationTest_ReturnCorrectHeaderInfo(YarnApplicationType appType,
            string appId,
            string start,
            string finish,
            int numOfContainers,
            string user,
            string queue,
            YarnApplicationStatus appStatus)
        {
            var startDate = DateTime.ParseExact(start, DateTimeUtils.AppDateTimeFormat, null);
            var finsihDate = DateTime.ParseExact(finish, DateTimeUtils.AppDateTimeFormat, null);
            var duration = finsihDate - startDate;

            Assert.AreEqual(expected: appId, actual: GetActiveYarnApp(appType)?.Header?.Id, "Application Id is not correct!");
            Assert.AreEqual(expected: startDate, actual: GetActiveYarnApp(appType)?.Header?.Start, "Start is not correct!");
            Assert.AreEqual(expected: finsihDate, actual: GetActiveYarnApp(appType)?.Header?.Finish, "Finish is not correct!");
            Assert.AreEqual(expected: duration, actual: GetActiveYarnApp(appType)?.Header?.Duration, "Duration is not correct!");
            Assert.AreEqual(expected: numOfContainers, actual: GetActiveYarnApp(appType)?.Containers.Count, "Container Count is not correct!");
            Assert.AreEqual(expected: appStatus, actual: GetActiveYarnApp(appType)?.Header.Status, "Status Count is not correct!");
            Assert.AreEqual(expected: user, actual: GetActiveYarnApp(appType)?.Header.User, "User Count is not correct!");
            Assert.AreEqual(expected: queue, actual: GetActiveYarnApp(appType)?.Header.QueueName, "QueueName Count is not correct!");
        }



        [TestMethod()]
        [DataRow(YarnApplicationType.Tez,
            3, 2, 0, 0,
            DisplayName = "Correct Dags for Tez")]
        public void YarnApplicationTest_ReturnCorrectDAGCount(YarnApplicationType appType,
            int dagCount,
            int succDags,
            int failsDags,
            int killedDags)
        {
            Assert.AreEqual(expected: dagCount, actual: GetActiveYarnApp(appType)?.Header?.SubmittedDags, "Submitted Dags Id not correct!");
            Assert.AreEqual(expected: succDags, actual: GetActiveYarnApp(appType)?.Header?.SuccessfullDags, "Successfull Dags is not correct!");
            Assert.AreEqual(expected: failsDags, actual: GetActiveYarnApp(appType)?.Header?.FailedDags, "Failed Dags is not correct!");
            Assert.AreEqual(expected: killedDags, actual: GetActiveYarnApp(appType)?.Header?.KilledDags, "Killed Dags is not correct!");
        }
    }
}