namespace AnchorBoltsWinform.Tools
{
    using System.Collections.Generic;
    using Tekla.Structures.Dialog.UIControls;

    /// <summary>
    /// Settings class to get Tekla files
    /// </summary>
    public static class TxSettings
    {
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
    }
}