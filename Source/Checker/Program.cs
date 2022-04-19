namespace Checker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Tekla.Structures.Model;

    public class Program
    {
        /// <summary>
        /// Checks for Tekla Structures connection
        /// </summary>
        /// <param name="args">List of strings: tsApplicationName, majorNumber, minorNumber</param>
        static void Main(string[] args )
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(args));
            }

            try
            {


            string[] argStrings = args[0].Split(' ');
            string tsApplicationName = argStrings[0];
            int majorNumber = Int32.Parse(argStrings[1]);
            int minorNumber = Int32.Parse(argStrings[2]);

            var tsProcess = new List<Process>();

            foreach (var p in Process.GetProcessesByName(tsApplicationName))
            {
                if (p.MainModule?.FileVersionInfo == null) continue;
                var vInfo = p.MainModule.FileVersionInfo;
                if (vInfo.FileMajorPart == majorNumber &&
                    vInfo.FileMinorPart == minorNumber)
                {
                    tsProcess.Add(p);
                }
            }

            var isConnected = false;
            if (tsProcess.Any())
            {
                var tModel = new Model();
                isConnected = tModel.GetConnectionStatus();
            }
            
            Environment.ExitCode = isConnected ? (int)ConnectionCode.Success : (int)ConnectionCode.Failure;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        enum ConnectionCode 
        {
            Success = 0,
            Failure = 1,
        }
    }

}
