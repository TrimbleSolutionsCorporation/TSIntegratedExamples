namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tekla.Structures;

    /// <summary>
    /// Tekla model class extensions
    /// </summary>
    public static class TxModel
    {
        /// <summary> Tekla Core null int value </summary>
        public const int NullIntegerValue = -2147483648;

        /// <summary> Tekla Core null double value </summary>
        public const double NullDoubleValue = -2147483648.0;

        /// <summary> Default path seperator </summary>
        public static readonly char[] FolderPathSeparators = { ';' };

        /// <summary>
        /// Gets Tekla firm folder directory
        /// </summary>
        public static DirectoryInfo FirmFolder
        {
            get
            {
                var tempString = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_FIRM", ref tempString);
                if (string.IsNullOrEmpty(tempString)) return null;
                var tDir = new DirectoryInfo(tempString);
                return tDir.Exists ? tDir : null;
            }
        }

        /// <summary>
        /// Gets the project folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> TemplateFolders
        {
            get
            {
                var xsProject = TxAdvancedOptions.XsTemplateDirectory;
                while (xsProject.Contains(@"\\")) xsProject = xsProject.Replace(@"\\", @"\");
                var set1 = xsProject.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);

                var xsProject2 = TxAdvancedOptions.XsTemplateDirectorySystem;
                while (xsProject2.Contains(@"\\")) xsProject2 = xsProject2.Replace(@"\\", @"\");
                var set2 = xsProject2.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);
                return set1.Concat(set2);
            }
        }

        /// <summary>
        /// Get standard properties directories
        /// </summary>
        public static List<DirectoryInfo> StandardPropertyDirectories
        {
            get
            {
                var propertyDirectories =
                    Tekla.Structures.Dialog.UIControls.EnvironmentFiles.GetStandardPropertyFileDirectories();
                return propertyDirectories.Select(dir => new DirectoryInfo(dir)).Where(tDir => tDir.Exists).ToList();
            }
        }

        /// <summary>
        /// Returns if imperial units are being used
        /// </summary>
        public static bool IsImperial
        {
            get
            {
                var stringTemp = TxAdvancedOptions.XsImperial;
                if(stringTemp.Trim().ToUpper() == "TRUE") return true;
                return false;
            }
        }
    }
}