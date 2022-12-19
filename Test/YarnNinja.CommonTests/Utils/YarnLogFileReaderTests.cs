using Microsoft.VisualStudio.TestTools.UnitTesting;
using YarnNinja.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.CommonTests;

namespace YarnNinja.Common.Utils.Tests
{
    [TestClass()]
    public class YarnLogFileReaderTests : BaseCommonTest
    {
        [TestMethod()]
        public void OpenFileTest_YarnLogFileReader()
        {
            var file = new YarnLogFileReader();

            file.OpenFile(tezLogFileName);

            Assert.IsFalse(file.EndOfFile, "File Is Empty");
            Assert.AreEqual(46110243, file.FileSize,  "File Size is wrong");
            Assert.AreEqual(0, file.ProccessedBytes, "Proccess Size is wrong");
        }

        [TestMethod()]
        public void ReadLineTestYarnLogFileReaderTests()
        {
            var file = new YarnLogFileReader();

            file.OpenFile(tezLogFileName);

            var line = file.ReadLine();

            Assert.AreNotEqual(string.Empty, line, "line is empty");
            Assert.AreEqual(113, file.ProccessedBytes, "processed bytes is wrong");
            Assert.AreEqual(0.0002450648546788183, file.ProgressPrecent, "processed bytes is wrong");


            Assert.IsFalse(file.EndOfFile, "File Is Empty");
            line = file.ReadLine();
            Assert.AreNotEqual(string.Empty, line, "line is empty");
            Assert.AreEqual(143, file.ProccessedBytes, "processed bytes is wrong");
            Assert.AreEqual(0.0003101263205227524, file.ProgressPrecent, "processed bytes is wrong");

            // Read 160,000 lines
            for (int i = 0; i < 160000; i++)
            {
                line = file.ReadLine();
            }

            Assert.AreNotEqual(string.Empty, line, "line is empty");
            Assert.AreEqual(23237083, file.ProccessedBytes, "processed bytes is wrong");
            Assert.AreEqual(50.39462273057204, file.ProgressPrecent, "processed bytes is wrong");


            //Read till end

            while (!file.EndOfFile)
            {
                file.ReadLine();
            }
            Assert.AreEqual(45450909, file.ProccessedBytes, "processed bytes is wrong");
            Assert.AreEqual(100, file.ProgressPrecent, "processed bytes is wrong");
        }
    }
}