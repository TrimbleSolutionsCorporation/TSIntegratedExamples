namespace JoistArea.View
{
    using System;
    using System.Windows;
    using Logic;
    using ViewModel;

    /// <summary>
    /// Interaction logic for MainPluginWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // define event
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Loaded += OnLoadedDel;
            GlobalServices.SetLocalization(Localization);
        }

        private void OnLoadedDel(object sender, RoutedEventArgs e)
        {
            try
            {
                //Todo: update any dialog combo boxes from attributes?
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void ApplyClicked(object sender, EventArgs e)
        {
            Apply();
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void GetClicked(object sender, EventArgs e)
        {
            Get();
        }

        private void ModifyClicked(object sender, EventArgs e)
        {
            Modify();
        }

        private void OkClicked(object sender, EventArgs e)
        {
            Apply();
            Close();
        }

        private void OnOffClicked(object sender, EventArgs e)
        {
            ToggleSelection();
        }

        private void PartMaterial_SelectClicked(object sender, EventArgs e)
        {
            PartMaterialCatalog.SelectedMaterial = _viewModel.Material;
        }

        private void PartMaterial_SelectDone(object sender, EventArgs e)
        {
            _viewModel.Material = PartMaterialCatalog.SelectedMaterial;
        }

        private void JoistProfile_SelectClicked(object sender, EventArgs e)
        {
            JoistProfileCatalog.SelectedProfile = _viewModel.JoistProfile;
        }

        private void JoistProfile_SelectionDone(object sender, EventArgs e)
        {
            _viewModel.JoistProfile = JoistProfileCatalog.SelectedProfile;
        }
    }
}
