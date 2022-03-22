using Tekla.Structures;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Tekla.Technology.Akit.UserScript
{
    /// <summary>
    /// Tekla Structures required class
    /// </summary>
    public class Script
    {
        /// <summary>
        /// Name of external model application to launch
        /// </summary>
        const string ApplicationName = "SpreadsheetReinforcement.exe";

        /// <summary>
        /// Main code to launch external application
        /// </summary>
        /// <param name="akit">Internal dialog kit</param>
        public static void Run(IScript akit)
        {
            LaunchExternalApplication.Run(ApplicationName, akit);
        }
    }

    public static class LaunchExternalApplication
    {
        /// <summary> Subfolder to search for executable in </summary>
        private const string ApplicationSubPath = "applications\\Tekla\\Model";
        private const string EnvironmentsSubPath = "Environments\\common\\extensions";

        /// <summary>
        /// Main code that launches external application
        /// </summary>
        /// <param name="applicationName">Exact name of application file</param>
        /// <param name="akit">A-kit to run commands from</param>
        public static void Run(string applicationName, IScript akit)
        {
            //Get root path for Tekla Structures bin folder
            var xsbin = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSBIN", ref xsbin);
            if (string.IsNullOrEmpty(xsbin))
            {
                Tekla.Structures.Model.Operations.Operation.DisplayPrompt("XSBIN Variable not found, failed to launch application.");
                return;
            }

            //Check common extensions folder first
            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);
            var envDir = new DirectoryInfo(Path.Combine(xsDataDir, EnvironmentsSubPath));
            if (!string.IsNullOrEmpty(xsDataDir) && envDir.Exists)
            {
                var exeFiles = envDir.GetFiles(applicationName, SearchOption.AllDirectories);
                if (exeFiles.Count() > 0)
                {
                    StartExtension(exeFiles[0].FullName);
                    return;
                }
            }

            //Check that application directory root exists
            var appDir = new DirectoryInfo(Path.Combine(xsbin, ApplicationSubPath));
            if (!appDir.Exists)
            {
                Tekla.Structures.Model.Operations.Operation.DisplayPrompt(appDir.FullName + " directory does not exist, failed to launch application");
                return;
            }

            //Check that file exists before starting
            var appFiles = appDir.GetFiles(applicationName, SearchOption.AllDirectories);
            if (!appFiles.Any())
            {
                Tekla.Structures.Model.Operations.Operation.DisplayPrompt(appDir.FullName + " file does not exist, failed to launch application");
                return;
            }
            StartExtension(appFiles[0].FullName);
        }

        /// <summary>
        /// Start external application
        /// </summary>
        /// <param name="filePath">File full path</param>
        private static void StartExtension(string filePath)
        {
            var externalApplication = new Process { StartInfo = { FileName = filePath } };
            if (externalApplication.Start()) Tekla.Structures.Model.Operations.Operation.DisplayPrompt(filePath + " successfully started.");
            else Tekla.Structures.Model.Operations.Operation.DisplayPrompt(filePath + " unable to be started.");
        }
    }
}