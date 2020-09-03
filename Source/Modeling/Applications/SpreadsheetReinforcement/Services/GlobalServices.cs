namespace SpreadsheetReinforcement.Services
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    /// <summary>
    /// Services class for common methods
    /// </summary>
    public static class GlobalServices
    {
        /// <summary>
        /// Opens log file for this feature
        /// </summary>
        public static void LogRequested()
        {
            //todo: open log file?
        }

        /// <summary>
        /// Gets resource string from all current application resources by name
        /// </summary>
        /// <param name="resourceKeyName">String resource key name</param>
        /// <returns></returns>
        public static string GetResourceString(string resourceKeyName)
        {
            if (string.IsNullOrEmpty(resourceKeyName)) return string.Empty;
            return Application.Current.Resources[resourceKeyName] as string;
        }

        /// <summary>
        /// Logs exception to trace logger and DisplayPrompt if in Debug mode
        /// </summary>
        /// <param name="ex">Exception to get logging information from</param>
        /// <param name="displayPrompt">True if to show in TS main window display on Debug</param>
        public static void LogException(Exception ex, bool displayPrompt)
        {
            if (ex == null) return;
            Trace.WriteLine(ex.GetType().Name + ": " + ex.Message + ex.StackTrace + ex.InnerException);
            if (displayPrompt)
            {
                Tekla.Structures.Model.Operations.Operation.DisplayPrompt(
                    "[DEBUG] " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Logs exception to trace logger and DisplayPrompt if in Debug mode
        /// </summary>
        /// <param name="msg">String message to print to debug listener</param>
        /// <param name="ex">Exception to get logging information from</param>
        public static void LogException(string msg, Exception ex = null)
        {
            if (ex != null) Trace.WriteLine(ex.GetType().Name + ": " + ex.Message + ex.StackTrace + ex.InnerException);
            Debug.WriteLine(msg);
        }

        /// <summary>
        /// Logs exception to trace logger and DisplayPrompt if in Debug mode
        /// </summary>
        /// <param name="ex">Exception to get logging information from</param>
        public static void LogException(Exception ex = null)
        {
            if (ex != null) Trace.WriteLine(ex.GetType().Name + ": " + ex.Message + ex.StackTrace + ex.InnerException);
        }

        /// <summary>
        /// Opens TUA article for this feature
        /// </summary>
        public static void HelpRequested()
        {
            try
            {
                var helpUrl = (string) Application.Current.Resources["StatusSubmittal.HelpFileUrl"];
                Tekla.Structures.Dialog.HelpViewer.DisplayHelpTopic(helpUrl);
            }
            catch (Exception ex)
            {
                LogException(ex, true);
            }
        }
    }
}