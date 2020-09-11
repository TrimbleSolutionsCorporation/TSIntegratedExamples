namespace SpreadsheetReinforcement.View
{
    using Services;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using Tools;
    using Tekla.Structures;
    using Tekla.Structures.Model;
    using Assembly = System.Reflection.Assembly;

    public sealed partial class App
    {
        /// <summary>This string should not be changed between versions unless the application is really renamed. </summary>
        private const string DisplayApplicationName = "Spreadsheet Reinforcement";
        private static string ApplicationName
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                return string.Format("{0}_{1}.{2}", assm.GetName().Name, assm.GetName().Version.Major, assm.GetName().Version.Minor);
            }
        }
        private const string UnauthorizedMessage = "This program requires Engineering configuration or above to run.";
        private static LogListener _mainLog;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            try
            {
                bool createdNew;
                using (new Mutex(true, ApplicationName, out createdNew))
                {
                    if (createdNew)
                    {
                        if (ConnectWithDialog() && CheckTeklaConfiguration())
                        {
                            //Initialize Logging
                            _mainLog = new LogListener();
                            _mainLog.WriteLineOccured += ListerWriteLineOccured;
                            Trace.Listeners.Add(_mainLog);
                            PrintSystemInformation();

                            //Initialize Events
                            TeklaStructures.Connect();
                            TeklaStructures.Closed += TeklaStructuresExited;

                            //Start main dialog for this application
                            var mainWindow = new MainWindow();
                            mainWindow.Closed += MainWindowClosed;
                            mainWindow.ShowDialog();
                        }
                    }
                    //Application already open, exit
                    Trace.WriteLine(
                        $"No more than one instance of {ApplicationName} can run simultaneously, closing down new instance.");
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace;
                Trace.WriteLine(msg);
                MessageBox.Show(msg);
            }
        }

        private static void PrintSystemInformation()
        {
            Trace.WriteLine("System Info:");
            var username = System.Environment.UserName;
            Trace.WriteLine($"User logged on: {username}");
            Trace.WriteLine($"Machine Name: {Environment.MachineName}");
            Trace.WriteLine($"Is 64 bit OS: {Environment.Is64BitOperatingSystem}");
            Trace.WriteLine($"OS Version: {Environment.OSVersion}");
            Trace.WriteLine($"Processor Count: {Environment.ProcessorCount}\n");
        }

        private static bool CheckTeklaConfiguration()
        {
            switch (ModuleManager.Configuration)
            {
                case ModuleManager.ProgramConfigurationEnum.CONFIGURATION_DRAFTER:
                    MessageBox.Show(UnauthorizedMessage);
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check connection to Tekla and sees if a model is open
        /// Shows message box when no connection or model open
        /// </summary>
        /// <returns>Is TS running, model is open, and connection successful?</returns>
        private static bool ConnectWithDialog()
        {
            //Create new model connection without creating channel.
            var tModel = new Model();

            if (tModel.GetConnectionStatus())
            {
                //check that model is opened
                if (string.IsNullOrEmpty(tModel.GetInfo().ModelPath))
                {
                    MessageBox.Show("Model is not loaded in TeklaStructures, you must load a model first.");
                    return false;
                }
                Debug.WriteLine("Connected to TeklaStructures: " + TeklaStructuresInfo.GetCurrentProgramVersion());
                return true;
            }
            MessageBox.Show("Cannot connect to TeklaStructures process, Tekla Structures must first be running.");
            return false;
        }

        private static void TeklaStructuresExited(object sender, EventArgs eventArgs)
        {
            //Forced shutdown, TS already closed now
            ShutDown();
        }

        private static void MainWindowClosed(object sender, EventArgs eventArgs)
        {
            TeklaStructures.Disconnect();
            ShutDown();
        }

        private static void ShutDown()
        {
            //Stop logging
            _mainLog?.CloseLog();

            //Force end application
            System.Environment.Exit(0);
        }

        private void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            var nl = Environment.NewLine;
            var msg =
                $"Unhandled exception: Message {ex.Message + nl}, InnerMessage{ex.InnerException + nl}, StackTrace{ex.StackTrace}";
            Trace.WriteLine(msg);
            MessageBox.Show(msg);
        }

        private static void ListerWriteLineOccured(object sender, StringArg e)
        {
            GeneralEvents.SendNewStatusMessage(e);
        }
    }
}
