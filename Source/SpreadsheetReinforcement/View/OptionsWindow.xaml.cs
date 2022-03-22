namespace SpreadsheetReinforcement.View
{
    using System;
    using ViewModel;

    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        public OptionsWindow(VmMainWindow parentViewModel)
        {
            if (parentViewModel == null) throw new ArgumentNullException("parentViewModel");
            InitializeComponent();
            DataContext = parentViewModel;
        }
    }
}
