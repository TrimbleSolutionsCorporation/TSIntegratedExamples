namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures;

    /// <summary>
    /// Class for getting known advanced options from TS
    /// </summary>
    public static class TxAdvancedOptions
    {
        /// <summary> Value for XS_IMPERIAL </summary>
        public static string XsImperial => GetAdvOption("XS_IMPERIAL");

        /// <summary> Value for XS_SUPPORT_EMAIL_ADDRESS </summary>
        public static string XsSupportEmailAddress => GetAdvOption("XS_SUPPORT_EMAIL_ADDRESS");

        /// <summary> Value for XS_TEMPLATE_DIRECTORY </summary>
        public static string XsTemplateDirectory
        {
            get { return GetAdvOption("XS_TEMPLATE_DIRECTORY"); }
        }

        /// <summary> Value for XS_TEMPLATE_DIRECTORY_SYSTEM </summary>
        public static string XsTemplateDirectorySystem
        {
            get { return GetAdvOption("XS_TEMPLATE_DIRECTORY_SYSTEM"); }
        }

        /// <summary>
        /// Gets Tekla Structures Advanced option for string
        /// </summary>
        /// <param name="udaName">String name to get option for</param>
        /// <returns>String return value</returns>
        private static string GetAdvOption(string udaName)
        {
            if (string.IsNullOrEmpty(udaName)) throw new ArgumentNullException();
            var tempValue = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption(udaName, ref tempValue);
            return tempValue;
        }
    }
}
