namespace SpreadsheetReinforcement.View
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using Services;
    using Tools;
    using Tekla.Structures;
    using Assembly = System.Reflection.Assembly;

    public sealed partial class App
    {
        private static LogListener _mainLog;
        private const string DisplayApplicationName = "Spreadsheet Reinforcement";

        private static string ApplicationName
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                return $"{assm.GetName().Name}_{assm.GetName().Version.Major}.{assm.GetName().Version.Minor}";
            }
        }

        public void ApplicationStartup(object sender, StartupEventArgs e)
        {
            try
            {
                using(new Mutex(true, ApplicationName, out var createdNew))
                {
                    if(createdNew)
                    {
                        if(TxModel.ConnectWithDialog())
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
                    Trace.WriteLine($"No more than one instance of {ApplicationName} can run simultaneously, closing down new instance.");
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace;
                Trace.WriteLine(msg);
                MessageBox.Show(msg);
            }
        }

        private void TeklaStructuresExited(object sender, EventArgs e)
        {
            //Forced shutdown, TS already closed now
            CustomShutDown();
        }

        private void MainWindowClosed(object sender, EventArgs eventArgs)
        {
            TeklaStructures.Disconnect();
            CustomShutDown();
        }
        private void CustomShutDown()
        {
            //Shut down logging, then app
            _mainLog?.CloseLog();

            //Force end application
            Environment.Exit(0);
        }

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

        private void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            var nl = Environment.NewLine;
            var msg = $"Unhandled exception: Message {ex.Message + nl}, InnerMessage{ex.InnerException + nl}, StackTrace{ex.StackTrace}";
            Trace.WriteLine(msg);
            MessageBox.Show(msg);
        }

        private static void ListerWriteLineOccured(object sender, StringArg e)
        {
            GeneralEvents.SendNewStatusMessage(e);
        }
    }
}
