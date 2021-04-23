namespace SpreadsheetReinforcement.View
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using Tools;
    using Tekla.Structures;
    using Tekla.Structures.Model;
    using Assembly = System.Reflection.Assembly;
    using Services;

    public sealed partial class App : Application
    {
        private const string DisplayApplicationName = "Spreadsheet Reinforcement";

        private static string ApplicationNameAndVersion
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                return $"{DisplayApplicationName}_{assm.GetName().Version.Major}.{assm.GetName().Version.Minor}";
            }
        }

        private const string UnauthorizedMessage = "This program requires Engineering configuration or above to run.";
        private static LogListener _mainLog;
        private static Mutex _mut;

        /// <summary>
        /// On application startup initialize LogListener and register events
        /// Checks if Tekla Structures is running and configuration meets minimum requirements
        /// </summary>
        /// <param name="e">Startup event args</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            _mut = new Mutex(true, ApplicationNameAndVersion, out var createdNew);
            if (!createdNew) WarnMoreThanOneAndTriggerShutdown();
            else if (ConnectWithDialog() && CheckTeklaConfiguration())
            {
                //Initialize Logging
                _mainLog = new LogListener();
                _mainLog.WriteLineOccured += ListerWriteLineOccured;
                Trace.Listeners.Add(_mainLog);
                PrintSystemInformation();

                //Initialize Events
                TeklaStructures.Connect();
                TeklaStructures.Closed += TeklaStructuresExited;
            }
            base.OnStartup(e);
        }

        /// <summary>
        /// On application exit dispose of local variables and un-register events
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                //Un-register Events, close log, release mutex
                TeklaStructures.Closed -= TeklaStructuresExited;
                TeklaStructures.Disconnect();
                _mut?.ReleaseMutex();
                _mainLog?.CloseLog();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
            }
            base.OnExit(e);
        }

        /// <summary>
        /// Logs message to trace logger and messageBox to user, then triggers ShutDown procedure
        /// Used for: Application already open, exit
        /// </summary>
        private void WarnMoreThanOneAndTriggerShutdown()
        {
            var msg = $"No more than one instance of {ApplicationNameAndVersion} can run simultaneously, closing down new instance.";
            Trace.WriteLine(msg);
            MessageBox.Show(msg, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TriggerAppShutDown();
        }

        /// <summary>
        /// Prints basic system information to Trace logger
        /// </summary>
        private static void PrintSystemInformation()
        {
            Trace.WriteLine("System Info:");
            var username = Environment.UserName;
            Trace.WriteLine($"User logged on: {username}");
            Trace.WriteLine($"Machine Name: {Environment.MachineName}");
            Trace.WriteLine($"Is 64 bit OS: {Environment.Is64BitOperatingSystem}");
            Trace.WriteLine($"OS Version: {Environment.OSVersion}");
            Trace.WriteLine($"Processor Count: {Environment.ProcessorCount}\n");
        }

        /// <summary>
        /// Checks Tekla Structures active configuration if meets minimum requirements for extension to run properly
        /// </summary>
        /// <returns>True if acceptable configuration to run in</returns>
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

        /// <summary>
        /// Tekla Structures event exit delegate
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="eventArgs">Event args</param>
        private static void TeklaStructuresExited(object sender, EventArgs eventArgs)
        {
            //Forced shutdown, TS already closed now
            Trace.WriteLine($"Tekla Structures exited, closing {ApplicationNameAndVersion}.", "TeklaStructuresExited");
            TriggerAppShutDown();
        }

        /// <summary>
        /// Method to trigger current application shutdown procedures
        /// </summary>
        private static void TriggerAppShutDown()
        {
            Application.Current?.Shutdown();
        }

        /// <summary>
        /// Unhandled exception event delegate to log exception information
        /// </summary>
        /// <param name="sender">Object Sender</param>
        /// <param name="e">Unhandled event arg</param>
        private void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            var nl = Environment.NewLine;
            var msg = $"Unhandled exception: Message {ex.Message + nl}, InnerMessage{ex.InnerException + nl}, StackTrace{ex.StackTrace}";
            Trace.WriteLine(msg);
            MessageBox.Show(msg, $"{ApplicationNameAndVersion} Exception encountered.");
        }

        /// <summary>
        /// Log event delegate to send message for message handler
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">String event arg</param>
        private static void ListerWriteLineOccured(object sender, StringArg e)
        {
            GeneralEvents.SendNewStatusMessage(e);
        }
    }
}
