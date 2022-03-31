using Microsoft.VisualStudio.TestTools.UnitTesting;
using YarnNinja.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;
using System.IO;

namespace YarnNinja.Common.Tests
{
    [TestClass()]
    public class YarnApplicationTests
    {
        private YarnApplication _yarnApp;


        [TestMethod()]
        [DataRow(@"C:\Repos\YarnNinja\Samples\application_1639352826059_8646.log", DisplayName = "Valid Tez Yarn Application")]
        [DataRow(@"C:\Repos\YarnNinja\Samples\application_1647350095798_0001.log", DisplayName = "Valid Mapreduce Yarn Application")]
        public void YarnApplication_TezOrMapReduce_ReturnYarnObject(string fileName)
        {
            string log = File.ReadAllText(fileName);
            _yarnApp = new YarnApplication(log);

            Assert.IsNotNull(_yarnApp);
        }

        [TestMethod()]
        [DataRow(@"C:\Repos\YarnNinja\Samples\FakeFile.log", DisplayName = "Valid Mapreduce Yarn Application")]
        public void YarnApplication_InvalidFileFormat_NotAYarnApp(string fileName)
        {
            string log = File.ReadAllText(fileName);
            try
            {
                _yarnApp = new YarnApplication(log);
                Assert.Fail(); // If it gets to this line, no exception was thrown
            }
            catch (InvalidYarnFileFormat ex)
            {
            }
            catch (Exception ex) {
                Assert.Fail();
            }
        }
    }
}