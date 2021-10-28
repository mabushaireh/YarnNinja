using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common.Core;

namespace YarnNinja.App.Console
{
    public static class ConsoleUtil
    {
       

        public static void DisplayMessage(string msg, YarnNinja.Common.Core.TraceLevel level) {
            var currentColor = System.Console.ForegroundColor;
            
            switch (level)
            {
                case YarnNinja.Common.Core.TraceLevel.DEBUG:
                    System.Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case YarnNinja.Common.Core.TraceLevel.INFO:
                    System.Console.ForegroundColor = ConsoleColor.White;
                    break;
                case YarnNinja.Common.Core.TraceLevel.WARNING:
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case YarnNinja.Common.Core.TraceLevel.ERROR:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case YarnNinja.Common.Core.TraceLevel.FATAL:
                    System.Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                
                default:
                    break;
            }
            StackTrace stackTrace = new StackTrace();
            string method = stackTrace.GetFrame(1).GetMethod().Name;
            string className = stackTrace.GetFrame(1).GetMethod().DeclaringType.FullName;

            string levelPrefix = level.ToString();

            System.Console.WriteLine($"[{DateTime.UtcNow.ToString()}][{className}][{method}][{levelPrefix}]: {msg}");
            System.Console.ForegroundColor = currentColor;
        }

        internal static string? ReadLine(string msg)
        {
            DisplayMessage(msg);
            return System.Console.ReadLine();
        }

        public static void DisplayMessage(string msg)
        {
            var currentColor = System.Console.ForegroundColor;

            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine($"{msg}");

            System.Console.ForegroundColor = currentColor;

        }
    }
}
