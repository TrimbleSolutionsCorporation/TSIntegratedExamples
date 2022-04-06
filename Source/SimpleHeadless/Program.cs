// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Copyright © 2021 Trimble Solutions Corporation. Trimble Solutions Corporation is a Trimble Company">
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SimpleHeadless
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Service;

    public class Program
    {
        private static List<string> _paths;
        private static FileStream filestream;
        private static StreamWriter streamwriter;
        private static int status;
        private static string logFileName = "SimpleHeadless_log.txt";

        private static readonly DirectoryInfo BinDirectory =
            new DirectoryInfo(@"C:\Program Files\Tekla Structures\2022.0 Daily\bin");

        private static readonly DirectoryInfo ModelDirectory =
            new DirectoryInfo(@"C:\TeklaStructuresModels\2022_Headless");

        private static readonly FileInfo EnvironmentFile =
            new FileInfo(@"C:\ProgramData\Trimble\Tekla Structures\2022.0 Daily\Environments\USA\env_US.ini");

        private static readonly FileInfo RoleFile =
            new FileInfo(@"C:\ProgramData\Trimble\Tekla Structures\2022.0 Daily\Environments\USA\Role_Imperial_Steel_Detailing.ini");

        private static string languageStr = "ENGLISH";
        private static string licenseStr = "FULL";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveTsAssembly;
            Redirect();

            try
            {
                using (var tss = new TeklaStructuresService(BinDirectory, languageStr, EnvironmentFile, RoleFile))
                {
                    //Initialize console service with model
                    tss.Initialize(ModelDirectory, licenseStr);

                    //Pause to attach debugger
                    //Console.WriteLine("Attach debugger, then press a key...");
                    //Console.ReadKey();

                    //Check connection to service
                    if (!CheckAndLogConnectionStatus()) return;

                    //Create new beam in memory
                    var beam = new Beam(Beam.BeamTypeEnum.BEAM)
                    {
                        StartPoint = new Point(0, 0, 0),
                        EndPoint = new Point(1000, 0, 0),
                        Material = new Material { MaterialString = "A992" },
                        Profile = new Profile { ProfileString = "W16X40" },
                        Class = "901",
                        Name = "TestBeam",
                        PartNumber = new NumberingSeries("x", 1),
                        AssemblyNumber = new NumberingSeries("X", 1),
                        Position = new Position
                        {
                            Depth = Position.DepthEnum.BEHIND,
                            Plane = Position.PlaneEnum.MIDDLE,
                            Rotation = Position.RotationEnum.TOP
                        }
                    };

                    //Insert new beam and commit changes
                    var flag = beam.Insert();
                    Console.WriteLine($"Beam insert flag: {flag}.");
                    new Model().CommitChanges();

                    //Save model
                    new ModelHandler().Save();

                    //Add log entry and pause
                    Console.WriteLine("End of headless operations...");
                    //Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal exception: {ex.InnerException}");
                Console.WriteLine($"Fatal message: {ex.Message}");
                Console.WriteLine($"Fatal stacktrace: {ex.StackTrace}");
            }
            finally
            {
                streamwriter.Dispose();
                filestream.Dispose();
                OpenLog();
            }
        }

        private static void OpenLog()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
            var fi = new FileInfo(path);
            if (!fi.Exists) return;
            Process.Start(fi.FullName);
        }

        private static bool CheckAndLogConnectionStatus()
        {
            //First check status from API Model
            if (!new Model().GetConnectionStatus())
            {
                Console.WriteLine("Failed to connect to Tekla Structures Service...");
                return false;
            }
            Console.WriteLine("Success: connected to Tekla Structures Service...");

            //Check open model and project data
            var modelPath = new Model().GetInfo().ModelPath;
            if (string.IsNullOrEmpty(modelPath))
            {
                Console.WriteLine($"Error: unable to open model given path: {ModelDirectory}");
                return false;
            }
            Console.WriteLine($"Model path: {modelPath}.");
            Console.WriteLine("ProjectInfo.Name        : " + new Model().GetProjectInfo().Name);
            Console.WriteLine("ProjectInfo.Description : " + new Model().GetProjectInfo().Description);
            return true;
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int device, IntPtr handle);

        private static void Redirect()
        {
            filestream = new FileStream(logFileName, FileMode.Create);
            streamwriter = new StreamWriter(filestream) { AutoFlush = true };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            if (filestream.SafeFileHandle != null)
            {
                var handle = filestream.SafeFileHandle.DangerousGetHandle();
                status = SetStdHandle(-11, handle); // set stdout
                // Check status as needed
                status = SetStdHandle(-12, handle); // set stderr
            }
            // Check status as needed
        }

        private static System.Reflection.Assembly ResolveTsAssembly(object sender, ResolveEventArgs args)
        {
            CacheTsAssemblyPaths();
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";

            //Check in each path and try to find file and load from that location
            foreach (var path in _paths)
            {
                var assemblyPath = Path.Combine(path, assemblyName);
                if (!File.Exists(assemblyPath)) continue;

                Debug.WriteLine($"Info: Assembly resolved from path: {assemblyPath}, loading from this location.");
                return System.Reflection.Assembly.LoadFrom(assemblyPath);
            }

            //Start from root bin directory
            if (!BinDirectory.Exists) return null;

            //Search all directories from bin
            var foundFiles = BinDirectory.GetFiles(assemblyName, SearchOption.AllDirectories);
            if (foundFiles.Length < 1) return null;

            //Return file with shortest path, optimally closest to plugins since there are many duplicates
            var orderedFiles = foundFiles.OrderBy(f => f.FullName.Length).ToList();
            Debug.WriteLine($"Info: Assembly resolved from path: {orderedFiles[0].FullName}, loading from this location.");
            return System.Reflection.Assembly.LoadFrom(orderedFiles[0].FullName);
        }

        private static void CacheTsAssemblyPaths()
        {
            //Cache path only once per session
            _paths = new List<string>();
            if (!BinDirectory.Exists) return;

            //Add paths and print to debug trace
            _paths.Add(BinDirectory.FullName);
            _paths.Add(Path.Combine(BinDirectory.FullName, "plugins"));
            foreach (var path in _paths)
            {
                Debug.WriteLine($"Info: Assembly search path added: {path} added.");
            }
        }
    }
}
