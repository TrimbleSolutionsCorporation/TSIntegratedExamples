namespace DrawingRectangle.View
{
    using System;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this._viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ApplyClicked(object sender, EventArgs e)
        {
            this.Apply();
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GetClicked(object sender, EventArgs e)
        {
            this.Get();
        }

        private void ModifyClicked(object sender, EventArgs e)
        {
            this.Modify();
        }

        private void OkClicked(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void OnOffClicked(object sender, EventArgs e)
        {
            this.ToggleSelection();
        }
    }
}
