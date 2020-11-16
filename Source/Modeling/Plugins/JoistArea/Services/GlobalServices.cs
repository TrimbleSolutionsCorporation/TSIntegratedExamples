namespace JoistArea.Services
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using View;

    /// <summary>
    /// Services class for common methods
    /// </summary>
    public static class GlobalServices
    {
        private static Tekla.Structures.Dialog.Localization _localization;

        /// <summary>
        /// Opens log file for this feature
        /// </summary>
        public static void LogRequested()
        {
            //todo: open log file?
        }

        public static string GetTranslated(string name)
        {
            if (string.IsNullOrEmpty(name) || _localization == null) return name;
            return _localization.GetText(name);
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
            if (ex != null)
            {
                Trace.WriteLine(ex.GetType().Name + ": " + ex.Message + ex.StackTrace + ex.InnerException);
#if DEBUG
                MessageBox.Show(ex.InnerException + ex.Message + ex.StackTrace);
#endif
            }
        }

        /// <summary>
        /// Opens TUA article for this feature
        /// </summary>
        public static void HelpRequested()
        {
            try
            {
                var helpUrl = Constants.HelpFileName;
                Tekla.Structures.Dialog.HelpViewer.DisplayHelpTopic(helpUrl);
            }
            catch (Exception ex)
            {
                LogException(ex, true);
            }
        }

        /// <summary>
        /// Set localization static service method
        /// </summary>
        /// <param name="tsdLoc"></param>
        public static void SetLocalization(Tekla.Structures.Dialog.Localization tsdLoc)
        {
            if (tsdLoc == null) throw new ArgumentNullException(nameof(tsdLoc));
            _localization = tsdLoc;
        }
    }
}