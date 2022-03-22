namespace SpreadsheetReinforcement.View
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using Tools;
    using Services;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closing += OnClosing;
            GeneralEvents.HideMainForm += HideForm;
            GeneralEvents.ShowMainForm += ShowForm;
        }

        private void StopProgress(object sender, StringArg e)
        {
            Dispatcher.BeginInvoke(new Action(() => IsEnabled = true));
        }

        private void StartProgress(object sender, StringArg e)
        {
            Dispatcher.BeginInvoke(new Action(() => IsEnabled = false));
        }

        private void ShowForm(object sender, StringArg e)
        {
            Dispatcher.BeginInvoke(new Action(() => WindowState = WindowState.Normal));
        }

        private void HideForm(object sender, StringArg e)
        {
            Dispatcher.BeginInvoke(new Action(() => WindowState = WindowState.Minimized));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            //Save user settings
            Properties.Settings.Default.LastMainPositionTop = Top;
            Properties.Settings.Default.LastMainPositionLeft = Left;
            Properties.Settings.Default.LastMainSizeHeight = Height;
            Properties.Settings.Default.LastMainSizeWidth = Width;
            Properties.Settings.Default.Save();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get user settings
            Top = Properties.Settings.Default.LastMainPositionTop;
            Left = Properties.Settings.Default.LastMainPositionLeft;
            Height = Properties.Settings.Default.LastMainSizeHeight;
            Width = Properties.Settings.Default.LastMainSizeWidth;
        }
    }
}
