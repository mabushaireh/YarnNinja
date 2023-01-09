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
        [DataRow(YarnApplicationType.Tez, 367, 10, DisplayName = "Tez Retrun Correct Container Counts")]
        [DataRow(YarnApplicationType.MapReduce, 3,2, DisplayName = "MapReduce Retrun Correct Container Counts")]
        [DataRow(YarnApplicationType.Spark, 31, 3, DisplayName = "Spark Return Retrun Correct Container Counts")]
        public void YarnApplication_ReturnCorrectConainerWorkerNodeCounts(YarnApplicationType expectedAppType, int expetedContainerCount, int expetedWorkerCount)
        {

            if (GetActiveYarnApp(expectedAppType) == null)
            {
                Assert.Fail("Failed to Parse App");
            }

            Assert.AreEqual(expected: expetedContainerCount, actual: GetActiveYarnApp(expectedAppType)?.Containers.Count,"Wrong Container Count");
            Assert.AreEqual(expected: expetedWorkerCount, actual: GetActiveYarnApp(expectedAppType)?.WorkerNodes.Count, "Wrong Workernodes Count");

        }

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
        [DataRow("FakeFile.log", DisplayName = "InvalidYarnFileFormat Expected")]
        public void YarnApplication_InvalidFileFormat_NotAYarnApp(string fileName)
        {
            YarnApplication? _yarnApp = null;
            try
            {
                var file = new YarnLogFileReader();

                file.OpenFile(@"./Samples/" + fileName);
                
                _yarnApp = new YarnApplication(file);
                _yarnApp.ParseContainersAsync();

                //Assert.Fail();
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
        [DataRow("C:\\Users\\maabusha.MIDDLEEAST.000\\Downloads\\application_1671046156402_3288.log\\application_1671046156402_3288.log", DisplayName = "Big File Parse Expected")]
        public void YarnApplication_BigFileParse(string filePath)
        {
            YarnApplication? _yarnApp = null;
            try
            {
                var file = new YarnLogFileReader();

                file.OpenFile(filePath);

                _yarnApp = new YarnApplication(file);
                _yarnApp.ParseContainersAsync();

                //Assert.Fail();
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
        [DataRow(YarnApplicationType.MapReduce, 2, DisplayName = "Correct Workers for Mapreduce")]
        public void YarnApplicationTest_ReturnWorkers(YarnApplicationType appType, int? expectedWorkers)
        {
            Assert.AreEqual<int?>(GetActiveYarnApp(appType)?.WorkerNodes?.Count , expectedWorkers, "Number of workers is incorrect");
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
        [DataRow(YarnApplicationType.MapReduce,
            "application_1647184687608_0039",
            "2022-03-14 12:19:57,522",
            "2022-03-14 12:20:25,643",
            3,
            "hive",
            "default",
            YarnApplicationStatus.SUCCEEDED,
            DisplayName = "Correct Header for MapReduce")]
        public void YarnApplicationTest_ReturnCorrectHeaderInfo(YarnApplicationType appType,
            string appId,
            string start,
            string finish,
            int numOfContainers,
            string user,
            string queue,
            YarnApplicationStatus appStatus)
        {
            var startDate = DateTime.ParseExact(start, DateTimeUtils.AppDateTimeFormat1, null);
            var finsihDate = DateTime.ParseExact(finish, DateTimeUtils.AppDateTimeFormat1, null);
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

        [TestMethod()]
        [DataRow(YarnApplicationType.MapReduce,
            1, 1, 0, 0,
            DisplayName = "Correct Mappersand Reducers for mapreduce")]
        public void YarnApplicationTest_ReturnCorrectMappersReducedsCount(YarnApplicationType appType,
            int completedMappers,
            int completedReducers,
            int failedMappers,
            int failedReducers)
        {
            Assert.AreEqual(expected: completedMappers, actual: GetActiveYarnApp(appType)?.Header?.CompletedMappers, "Completed Mappers not correct!");
            Assert.AreEqual(expected: completedReducers, actual: GetActiveYarnApp(appType)?.Header?.CompletedReducers, "Completed Reduces is not correct!");
            Assert.AreEqual(expected: failedMappers, actual: GetActiveYarnApp(appType)?.Header?.FailedMappers, "Failed Mappers is not correct!");
            Assert.AreEqual(expected: failedReducers, actual: GetActiveYarnApp(appType)?.Header?.FailedReducers, "Failed Reduces is not correct!");
        }
    }
}