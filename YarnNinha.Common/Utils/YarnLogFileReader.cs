using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace YarnNinja.Common.Utils
{
    public class YarnLogFileReader
    {
        private StreamReader sr;
        private long proccessedBytes = 0;
        private long totalBytes = 0;
        public event EventHandler ProgressEventHandler;
        private int PreviousProgress = 0;

        public void OpenFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            totalBytes = fi.Length ;
            if (fi.Exists)
            {
                sr = new(filePath);

            }

        }

        public string ReadLine()
        {
            string line;
            if (EndOfFile == false)
            {
                line = sr.ReadLine();
                proccessedBytes += getLineByeteCount(line);

                if ((int)ProgressPrecent > PreviousProgress) 
                {
                    PreviousProgress = (int)ProgressPrecent;
                    ProgressEventHandler?.Invoke(this, new EventArgs());
                }
                return line;
            }
            throw new Exception("End of File");
        }

        public bool EndOfFile { 
            get { return sr.EndOfStream;  }
        }

        public long FileSize
        {
            get { return totalBytes; }
        }

        public long ProccessedBytes
        {
            get { return proccessedBytes; }
        }

        public double ProgressPrecent
        {
            get { return (this.EndOfFile? 100 : (((double)proccessedBytes)/ (double)totalBytes) * 100); }
        }

        public void CloseFile()
        {
            this.sr.Close();
            this.sr.Dispose();
        }

        private int getLineByeteCount(string line)
        {
            Encoding encoding = Encoding.UTF8; // Or whatever
            return encoding.GetByteCount(line);
        }
    }
}
