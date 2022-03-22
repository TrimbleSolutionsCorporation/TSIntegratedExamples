namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Log class the captures the Trace/Debug to file
    /// </summary>
    public class LogListener : TraceListener
    {
        private const string TraceLogFileSuffix = "_Log.txt";
        private static TextWriterTraceListener _listener;
        private static string _logFileRootPath;
        private static string _logFileName;
        private static LogListener _instance;

        /// <summary>
        /// Singleton method if used grabs static instance
        /// Creates new instance if not initialized before now
        /// </summary>
        public static LogListener Instance
        {
            get { return _instance ?? (_instance = new LogListener()); }
        }

        /// <summary>
        /// Root application name, do not use suffix, .xml will be added on to the end automatically
        /// Can set manually with Initialize(ApplicationRootName) override, or it will try to
        /// 1st use the EntryAssembly Name, if null then try to use the CallingAssembly Name
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (Assembly.GetEntryAssembly() == null) return Assembly.GetCallingAssembly().GetName().Name;
                return Assembly.GetEntryAssembly()?.GetName().Name;
            }
        }

        /// <summary>
        /// Get a DirectoryInfo instance of the executing assembly .../application data/application name/
        /// </summary>
        public static DirectoryInfo ApplicationDataFolder
        {
            get
            {
                var di = new DirectoryInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ApplicationName));
                if (!di.Exists) di.Create();
                return di;
            }
        }

        /// <summary>
        /// Version of executing assembly
        /// </summary>
        public static string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        /// <summary> Eventhandler for write occured </summary>
        public event EventHandler<StringArg> WriteOccured;

        /// <summary>
        /// Sends message that write has occured
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            if (WriteOccured != null) WriteOccured.BeginInvoke(null, new StringArg(message), null, null);
        }

        /// <summary> Eventhandler for writeline occured </summary>
        public event EventHandler<StringArg> WriteLineOccured;

        /// <summary>
        /// Sends message that writeline has occured
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            if (WriteLineOccured != null) WriteLineOccured.BeginInvoke(null, new StringArg(message), null, null);
        }

        /// <summary>
        /// Directory folder that is used for log file location, based off application data folder unless overridden
        /// </summary>
        private static string LogFileRootPath
        {
            get { return !string.IsNullOrEmpty(_logFileRootPath) ? _logFileRootPath : ApplicationDataFolder.FullName; }
        }

        /// <summary>
        /// Short file name of application log file with suffix
        /// </summary>
        public string TraceLogFileName
        {
            get { return !string.IsNullOrEmpty(_logFileName) ? _logFileName : ApplicationName + TraceLogFileSuffix; }
        }

        /// <summary>
        /// Gets time of last build
        /// </summary>
        /// <returns>Time</returns>
        public static DateTime RetrieveLinkerTimestamp()
        {
            var filePath = Assembly.GetCallingAssembly().Location;
            const int cPeHeaderOffset = 60;
            const int cLinkerTimestampOffset = 8;
            var b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null) s.Close();
            }

            var i = BitConverter.ToInt32(b, cPeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(b, i + cLinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            //dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            dt = dt.AddHours(TimeZoneInfo.Local.GetUtcOffset(dt).Hours);
            return dt;
        }

        /// <summary>
        /// Actual log file
        /// </summary>
        private FileInfo TraceLogFile
        {
            get { return new FileInfo(Path.Combine(LogFileRootPath, TraceLogFileName)); }
        }

        /// <summary>
        /// Uses new process to start text based log file
        /// </summary>
        public void OpenFileInSeparateProcess()
        {
            if (_instance == null || _listener == null) return;
            _listener.Flush();
            Process.Start(TraceLogFile.FullName);
        }

        /// <summary>
        /// Initialize a textwriter trace listener with default log file name
        /// </summary>
        public LogListener()
        {
            // Second step: create some trace listeners
            _listener = new TextWriterTraceListener(TraceLogFile.FullName);
            _instance = this;

            // Configuring trace listeners
            _listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.Callstack;

            // Adding our trace listeners
            Trace.Listeners.Add(_listener);
            Trace.AutoFlush = true;

            WriteStartupInfo();
        }

        /// <summary>
        /// Initialize a textwriter trace listener with custom log file name
        /// </summary>
        public LogListener(string customLogFilePath)
        {
            // Second step: create some trace listeners
            _logFileRootPath = customLogFilePath;
            _listener = new TextWriterTraceListener(TraceLogFile.FullName);
            _instance = this;

            // Configuring trace listeners
            _listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.Callstack;

            // Adding our trace listeners
            Trace.Listeners.Add(_listener);
            Trace.AutoFlush = true;

            WriteStartupInfo();
        }

        /// <summary>
        /// Initialize a textwriter trace listener with custom log file name
        /// </summary>
        public LogListener(FileInfo logfile)
        {
            // Second step: create some trace listeners
            _logFileRootPath = logfile.DirectoryName;
            _logFileName = logfile.Name;
            _listener = new TextWriterTraceListener(TraceLogFile.FullName);
            _instance = this;

            // Configuring trace listeners
            _listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.Callstack;

            // Adding our trace listeners
            Trace.Listeners.Add(_listener);
            Trace.AutoFlush = true;

            WriteStartupInfo();
        }


        /// <summary>
        /// Writes starting data to log file
        /// </summary>
        private void WriteStartupInfo()
        {
            Trace.WriteLine("Log file started: " + DateTime.Now);
            Trace.WriteLine("Trace file Initialized at: " + TraceLogFile.FullName);
            Trace.WriteLine("Assembly build name: " + ApplicationName);
            Trace.WriteLine("Assembly version: " + AssemblyVersion);
            Trace.WriteLine("Assembly build date: " + RetrieveLinkerTimestamp());
            Trace.WriteLine("-------------------------------------------------------------------");
        }

        /// <summary>
        /// Writes closing data to log file and closes base listener
        /// </summary>
        public void CloseLog()
        {
            Trace.WriteLine("Log file closed: " + DateTime.Now);
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine(Environment.NewLine);
            _listener.Close();
        }
    }

    /// <summary>The string event arguments</summary>
    public class StringArg : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="StringArg"/> class.</summary>
        /// <param name="stringValue">The string of the string event arguments.</param>
        public StringArg(string stringValue)
        {
            StringValue = stringValue;
        }

        /// <summary>Gets the string of the string event arguments.</summary>
        public string StringValue { get; private set; }
    }
}