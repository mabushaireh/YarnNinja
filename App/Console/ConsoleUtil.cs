using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;

namespace YarnNinja.App.Console
{
    public static class ConsoleUtil
    {
       

       

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
