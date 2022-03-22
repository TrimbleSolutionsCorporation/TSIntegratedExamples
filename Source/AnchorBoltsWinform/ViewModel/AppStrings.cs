namespace AnchorBoltsWinform.ViewModel
{
    using System.Reflection;

    /// <summary>
    /// Application level strings and general shared properties
    /// </summary>
    public static class AppStrings
    {
        /// <summary> Model not loaded warning string </summary>
        public const string ModelNotLoaded = "Model is not loaded in TeklaStructures, you must load a model first.";

        /// <summary> Tekla Structures not connected warning string </summary>
        public const string TeklaNotConnected = "Cannot connect to TeklaStructures process, Tekla Structures must first be running.";

        /// <summary> Help file document name </summary>
        public const string HelpFileName = "AnchorBoltsWinform.pdf";

        /// <summary>
        /// Application name from assembly reflection with version number added for uniqueness
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                return string.Format("{0}_{1}.{2}", assm.GetName().Name, assm.GetName().Version.Major, assm.GetName().Version.Minor);
            }
        }
    }
}