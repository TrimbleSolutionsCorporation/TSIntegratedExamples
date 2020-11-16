namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tekla.Structures;
    using Tekla.Structures.Dialog.UIControls;

    /// <summary>
    /// Settings class to get Tekla files
    /// </summary>
    public static class TxSettings
    {
        /// <summary>
        /// Firm folder location
        /// </summary>
        public static DirectoryInfo FirmFolder
        {
            get
            {
                var tempString = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_FIRM", ref tempString);
                if (string.IsNullOrEmpty(tempString)) return null;
                var tDir = new DirectoryInfo(tempString);
                return !tDir.Exists ? null : tDir;
            }
        }

        /// <summary>
        /// All standard property directories
        /// </summary>
        public static List<DirectoryInfo> StandardPropertyDirectories
        {
            get
            {
                var propertyDirectories = EnvironmentFiles.GetStandardPropertyFileDirectories();
                return propertyDirectories.Select(dir => new DirectoryInfo(dir)).Where(tDir => tDir.Exists).ToList();
            }
        }

        /// <summary>
        /// Gets list of files that have file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <returns>List of file names</returns>
        public static List<string> GetTeklaSettingFiles(string fileExtension)
        {
            var propertyDirectories = EnvironmentFiles.GetStandardPropertyFileDirectories();
            var availableFiles = EnvironmentFiles.GetMultiDirectoryFileList(propertyDirectories, fileExtension);
            return availableFiles;
        }

        /// <summary>
        /// Gets file from Tekla settings folders
        /// </summary>
        /// <param name="fileName">Name and extension of file to get.  E.g. standard.prt</param>
        /// <returns></returns>
        public static FileInfo GetTeklaSettingFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException();
            var results = new List<FileInfo>();
            var propertyDirectories = EnvironmentFiles.GetStandardPropertyFileDirectories();
            foreach (var str in propertyDirectories)
            {
                var dir = new DirectoryInfo(str);
                if(!dir.Exists)continue;
                results.AddRange(dir.GetFiles(fileName, SearchOption.AllDirectories));
            }
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Gets string file name based on component number.  E.g. returns p_141
        /// </summary>
        /// <param name="componentNumber">Component number</param>
        /// <returns>String file name</returns>
        public static string GetSettingExtensionForComponent(string componentNumber)
        {
            return string.IsNullOrEmpty(componentNumber) ? string.Empty : string.Format("CMUTools.{0}.View.DiaMain.xml", componentNumber);
        }

        /// <summary>
        /// File name including extension, but not full path
        /// </summary>
        /// <param name="fullName">Example: standard.vi</param>
        /// <returns>Fileinfo where it actually exists, null if none found</returns>
        public static FileInfo GetFileByName(string fullName)
        {
            if (!fullName.Contains(".")) throw new ApplicationException("GetFileByName argument fullName did not contain '.', must contain extension");
            var shortName = fullName.StringLeft(".");
            var extension = fullName.StringRight(".");
            if (string.IsNullOrEmpty(shortName) || string.IsNullOrEmpty(extension)) throw new ApplicationException("GetFileByName failed to parse argument fullName into parts.");

            //Check that attribute file exists, set to something viable if possible
            return TxModel.StandardPropertyDirectories.SelectMany(di => di.GetFiles(fullName, SearchOption.AllDirectories)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the string to the right of seperator
        /// </summary>
        /// <param name="tString">String to parse</param>
        /// <param name="seperator">String seperator</param>
        /// <returns>Remainder of string to right of seperator</returns>
        public static string StringRight(this string tString, string seperator)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(tString) || string.IsNullOrEmpty(seperator)) return result;
            var index = tString.LastIndexOf(seperator, StringComparison.Ordinal);
            return index < 1 ? result : tString.Substring(index + 1, tString.Length - index - 1);
        }

        /// <summary>
        /// Gets the string to the left of seperator
        /// </summary>
        /// <param name="tString">String to parse</param>
        /// <param name="seperator">String seperator</param>
        /// <returns>Remainder of string to left of seperator</returns>
        public static string StringLeft(this string tString, string seperator)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(tString) || string.IsNullOrEmpty(seperator)) return result;
            var index = tString.LastIndexOf(seperator, StringComparison.Ordinal);
            return index < 1 ? result : tString.Substring(0, index);
        }
    }
}
