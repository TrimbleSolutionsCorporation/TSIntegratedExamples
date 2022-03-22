namespace SpreadsheetReinforcement.Tools
{
    using System;
    using Tekla.Structures;

    /// <summary>
    /// Class for getting known advanced options from TS
    /// </summary>
    public static class TxAdvancedOptions
    {
        #region Properties

        /// <summary> Value for XS_PARAMETRIC_PROFILE_SEPARATOR </summary>
        public static string XsParametricProfileSeparator
        {
            get { return GetAdvOption("XS_PARAMETRIC_PROFILE_SEPARATOR"); }
        }

        /// <summary> Value for XS_TEMPLATE_DIRECTORY </summary>
        public static string XsTemplateDirectory
        {
            get { return GetAdvOption("XS_TEMPLATE_DIRECTORY"); }
        }

        /// <summary> Value for XS_DIR </summary>
        public static string XsDir
        {
            get { return GetAdvOption("XS_DIR"); }
        }

        /// <summary> Value for XSDATADIR </summary>
        public static string XsDataDir
        {
            get { return GetAdvOption("XSDATADIR"); }
        }

        /// <summary> Value for XS_RUNPATH </summary>
        public static string XsRunPath
        {
            get { return GetAdvOption("XS_RUNPATH"); }
        }

        /// <summary> Value for XSUSERDATADIR </summary>
        public static string XsUserDataDir
        {
            get { return GetAdvOption("XSUSERDATADIR"); }
        }

        /// <summary> Value for XSBIN </summary>
        public static string XsBin
        {
            get { return GetAdvOption("XSBIN"); }
        }

        /// <summary> Value for XS_APPLICATIONS </summary>
        public static string XsApplications
        {
            get { return GetAdvOption("XS_APPLICATIONS"); }
        }

        /// <summary> Value for DXK_FONTPATH </summary>
        public static string DxkFontpath
        {
            get { return GetAdvOption("DXK_FONTPATH"); }
        }

        /// <summary> Value for DXK_SYMBOLPATH </summary>
        public static string DxkSymbolPath
        {
            get { return GetAdvOption("DXK_SYMBOLPATH"); }
        }

        /// <summary> Value for DAK_BMPPATH </summary>
        public static string DakBmpPath
        {
            get { return GetAdvOption("DAK_BMPPATH"); }
        }

        /// <summary> Value for XS_TPLED_DIRECTORY </summary>
        public static string XsTpledDirectory
        {
            get { return GetAdvOption("XS_TPLED_DIRECTORY"); }
        }

        /// <summary> Value for XS_MIGRATION_WIZARD </summary>
        public static string XsMigrationWizard
        {
            get { return GetAdvOption("XS_MIGRATION_WIZARD"); }
        }

        /// <summary> Value for XS_LANGUAGE </summary>
        public static string XsLanguageString
        {
            get { return GetAdvOption("XS_LANGUAGE"); }
        }

        /// <summary> Value for XS_PROJECT </summary>
        public static string XsProject
        {
            get { return GetAdvOption("XS_PROJECT"); }
        }

        /// <summary> Value for XS_FIRM </summary>
        public static string XsFirm
        {
            get { return GetAdvOption("XS_FIRM"); }
        }

        /// <summary> Value for XS_SYSTEM </summary>
        public static string XsSystem
        {
            get { return GetAdvOption("XS_SYSTEM"); }
        }

        /// <summary> Value for XS_IMPERIAL </summary>
        public static string XsImperial
        {
            get { return GetAdvOption("XS_IMPERIAL"); }
        }

        /// <summary> Value for XS_CLONING_TEMPLATE_DIRECTORY </summary>
        public static string XsCloningTemplateDir
        {
            get { return GetAdvOption("XS_CLONING_TEMPLATE_DIRECTORY"); }
        }

        /// <summary> Value for XS_MACRO_DIRECTORY </summary>
        public static string XsMacroDirectory
        {
            get { return GetAdvOption("XS_MACRO_DIRECTORY"); }
        }

        /// <summary> Value for XS_RUNPATH </summary>
        public static string XsRunpath
        {
            get { return GetAdvOption("XS_RUNPATH"); }
        }

        /// <summary> Value for XS_INP </summary>
        public static string XsInp
        {
            get { return GetAdvOption("XS_INP"); }
        }

        /// <summary> Value for XS_PROFDB </summary>
        public static string XsProfDb
        {
            get { return GetAdvOption("XS_PROFDB"); }
        }

        /// <summary> Value for XS_UEL_IMPORT_FOLDER </summary>
        public static string XsUelImportFolder
        {
            get { return GetAdvOption("XS_UEL_IMPORT_FOLDER"); }
        }

        /// <summary> Value for XS_SUPPORT_EMAIL_ADDRESS </summary>
        public static string XsSupportEmailAddress
        {
            get { return GetAdvOption("XS_SUPPORT_EMAIL_ADDRESS"); }
        }

        /// <summary> Value for XS_TPLED_INI </summary>
        public static string XsTpledIni
        {
            get { return GetAdvOption("XS_TPLED_INI"); }
        }

        /// <summary> Value for XS_CLONING_TEMPLATE_DIRECTORY </summary>
        public static string XsCloningTemplateDirectory
        {
            get { return GetAdvOption("XS_CLONING_TEMPLATE_DIRECTORY"); }
        }

        /// <summary> Value for XS_MODEL_TEMPLATE_DIRECTORY </summary>
        public static string XsModelTemplateDirectory
        {
            get { return GetAdvOption("XS_MODEL_TEMPLATE_DIRECTORY"); }
        }

        /// <summary> Value for XS_TEMPLATE_DIRECTORY_SYSTEM </summary>
        public static string XsTemplateDirectorySystem
        {
            get { return GetAdvOption("XS_TEMPLATE_DIRECTORY_SYSTEM"); }
        }

        /// <summary> Value for XS_DEFAULT_BREP_PATH </summary>
        public static string XsDefaultBrepPath
        {
            get { return GetAdvOption("XS_DEFAULT_BREP_PATH"); }
        }

        /// <summary> Value for ASCII_LEGEND_PATH </summary>
        public static string AsciiLegendPath
        {
            get { return GetAdvOption("ASCII_LEGEND_PATH"); }
        }

        /// <summary> Value for XS_REFERENCE_CACHE </summary>
        public static string XsReferenceCache
        {
            get { return GetAdvOption("XS_REFERENCE_CACHE"); }
        }

        #endregion

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