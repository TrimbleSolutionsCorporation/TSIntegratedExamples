namespace Listener.View
{
    using System.Windows;
    using ViewModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            this.InitializeComponent();
            this._vm = this.DataContext as MainViewModel;
        }

        private void TempLoadEventDel(object sender, RoutedEventArgs e)
        {
            this._vm.OnLoadedCommand.Execute(sender);
        }
    }
}
