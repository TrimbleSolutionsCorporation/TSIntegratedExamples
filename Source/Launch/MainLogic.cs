﻿namespace Listener
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public static class MainLogic
    {
        private const string TsApplicationName = "TeklaStructures";
        private const string BinFolder = @"C:\Program Files\Tekla Structures\2022.0 Daily\bin";
        private const string ModelName = @"C:\TeklaStructuresModels\2022_Headless\2022_Headless.db1";
        private const int majorNumber = 2022;
        private const int minorNumber = 0;

        public static List<Process> GetTeklaProcess()
        {
            var result = new List<Process>();
            foreach (var p in Process.GetProcessesByName(TsApplicationName))
            {
                if (p.MainModule?.FileVersionInfo == null) continue;
                var vInfo = p.MainModule.FileVersionInfo;
                if (vInfo.FileMajorPart == majorNumber &&
                    vInfo.FileMinorPart == minorNumber)
                {
                    result.Add(p);
                }
            }
            return result;
        }

        public static Process StartTeklaStructures()
        {
            var appPath = Path.Combine(BinFolder, TsApplicationName + ".exe");
            var args = $"-I \"C:\\Bypass.ini\" \"{ModelName}\"";
            var tsProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = args
                }
            };
            tsProcess.Start();
            tsProcess.WaitForInputIdle();

            //Force wait for open window handle
            while (tsProcess.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(100);
            }
            return tsProcess;
        }
    }
}
