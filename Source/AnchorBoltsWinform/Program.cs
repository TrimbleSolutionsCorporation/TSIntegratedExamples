namespace AnchorBoltsWinform
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using View;
    using ViewModel;
    using Tekla.Structures;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                bool createdNew;
                using(new Mutex(true, AppStrings.ApplicationName, out createdNew))
                {
                    if(createdNew)
                    {
                        if(CheckConnection() && TeklaStructures.Connect())
                        {
                            //Initialize Tekla events
                            TeklaStructures.Connect();
                            TeklaStructures.Closed += TeklaStructuresExited;
                            //TeklaStructures.Model.Loaded += TeklaStructuresExited;

                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);

                            var diaMain = new DiaMain();
                            diaMain.Closed += MainWindowClosed;
                            diaMain.Show(TeklaStructures.MainWindow);
                            Application.Run(diaMain);
                        }
                    }
                    else
                    {
                        var window = FindWindow(null, Assembly.GetExecutingAssembly().GetName().Name);
                        ShowWindow(window, 9);
                        SetForegroundWindow(window);
                    }
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace;
                Trace.WriteLine(msg);
                MessageBox.Show(msg);
            }
        }

        /// <summary>
        /// Check connection to Tekla and sees if a model is open,
        /// Shows message box when no connection or model open
        /// </summary>
        /// <returns>Is Tekla Structures running, model is open, and connection successful?</returns>
        public static bool CheckConnection()
        {
            if(new Tekla.Structures.Model.Model().GetConnectionStatus())
            {
                //check that model is opened
                if(string.IsNullOrEmpty(new Tekla.Structures.Model.Model().GetInfo().ModelPath))
                {
                    MessageBox.Show(AppStrings.ModelNotLoaded);
                    return false;
                }
                Debug.WriteLine("Connected to TeklaStructures: " + TeklaStructuresInfo.GetCurrentProgramVersion());
                return true;
            }
            MessageBox.Show(AppStrings.TeklaNotConnected);
            return false;
        }

        private static void MainWindowClosed(object sender, EventArgs eventArgs)
        {
            TeklaStructures.Disconnect();
            ShutDown(null, null);
        }

        private static void ShutDown(object sender, EventArgs e)
        {
            //Shut down the application
            Application.Exit();
        }

        private static void TeklaStructuresExited(object sender, EventArgs e)
        {
            ShutDown(null, EventArgs.Empty);
        }

        /// <summary>Finds the window.</summary>
        /// <param name="className">The class name.</param>
        /// <param name="windowName">The window name.</param>
        /// <returns>Returns a pointer to the window.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);

        /// <summary>Sets the window the specified window's show state.</summary>
        /// <param name="windowHandle">Handle to the window that should be shown.</param>
        /// <param name="value">Control value indicating how the window is to be shown.</param>
        /// <returns>Return a nonzero value if the window was shown.</returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr windowHandle, int value);

        /// <summary>Sets the window into foreground and activates it.</summary>
        /// <param name="windowHandle">Handle to the window that should be activated and brought to the foreground.</param>
        /// <returns>Return a nonzero value if the window was brought to the foreground.</returns>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr windowHandle);
    }
}
