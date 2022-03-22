namespace AnchorBoltsWinform.View
{
    using System;
    using System.Linq;
    using System.Windows.Forms;
    using ModelLogic;
    using Tools;
    using ViewModel;
    using Tekla.Structures;
    using Tekla.Structures.Datatype;
    using Tekla.Structures.Dialog;
    using Tekla.Structures.Dialog.UIControls;

    public partial class DiaMain : ApplicationFormBase
    {
        public DiaMain()
        {
            InitializeComponent();
            InitializeForm();
            Dialogs.SetSettings(string.Empty);
           
            KeyPreview = true;
            KeyDown += KeyDownDelegate;
            createApplyCancel1.CancelClicked += CancelClickedDel;
            createApplyCancel1.CreateClicked += Btn_Create_Click;
            createApplyCancel1.ApplyClicked += Btn_Apply_ClickDel;
            Load += OnLoadDel;
        }

        private void OnLoadDel(object sender, EventArgs e)
        {
            var dimSettings = TxSettings.GetTeklaSettingFiles("dim");
            var filterSettings = TxSettings.GetTeklaSettingFiles("SObjGrp");
            if (dimSettings.Any()) cbx_DmSettingsName.Items.AddRange(dimSettings.ToArray());
            if (filterSettings.Any()) cbx_AnFilerName.Items.AddRange(filterSettings.ToArray());
        }

        private void CancelClickedDel(object sender, EventArgs e) { Close(); }

        private void Btn_Create_Click(object sender, EventArgs e)
        {
            Apply();
            var settings = GetUiDataFromForm();
            MainLogic.Run(settings);
        }

        private AppData GetUiDataFromForm()
        {
            var settings = new AppData
                           {
                               AnchorBoltFilerName = cbx_AnFilerName.Text,
                               DimensionSettingsName = cbx_DmSettingsName.Text,
                               DimensionLineOffset = TryConvertDbl(tbx_DimOffset.Text)
                           };
            settings.CheckDefaults();
            return settings;
        }

        private static double TryConvertDbl(string text)
        {
            var dist = Distance.Parse(text);
            return dist.Millimeters;
        }

        private void Btn_Apply_ClickDel(object sender, EventArgs e) { Apply(); }

        private void KeyDownDelegate(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1) HelpViewer.DisplayHelpTopic(AppStrings.HelpFileName);
        }

        protected override void OnLoad(EventArgs e)
        {
            //TxLocalization.Initialize(GlobalConstants.MessagesFileName);
            //Text = TxLocalization.GetTranslated(GlobalConstants.DisplayApplicationName);
            TeklaStructures.Connect();
            InitializeHelpLink();
            TeklaStructures.Drawing.EditorClosed += DrawingEditorClosedDel;
            base.OnLoad(e);
        }

        private void DrawingEditorClosedDel(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)Close);
        }

        private void InitializeHelpLink()
        {
            saveLoad1.HelpFileType = SaveLoad.HelpFileTypeEnum.UserDefined;
            saveLoad1.HelpUrl = string.Empty;
            saveLoad1.UserDefinedHelpFilePath = "";
            saveLoad1.HelpKeyword = AppStrings.HelpFileName;
            saveLoad1.KeyDown += KeyDownDelegate;
        }
    }
}
