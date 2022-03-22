namespace SpreadsheetReinforcement.ViewModel
{
    using System;
    using Data;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using ModelLogic;
    using Services;
    using Tekla.Structures.Catalogs;
    using Tools;
    using View;
    using Tekla.Structures.Dialog;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.Operations;

    public class VmMainWindow : SmartBindingBase
    {
        private List<string> _allSelectionFilters;
        private const string RelativePathString = ".\\";

        public SpreadsheetResultData CurrentDesignResult
        {
            get { return GetDynamicValue<SpreadsheetResultData>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Default root folder in model sub-folder </summary>
        private static DirectoryInfo DefaultRootFolder
        {
            get
            {
                var dir = new DirectoryInfo(Path.Combine(new Model().GetInfo().ModelPath,
                    GlobalConstants.DefaultRootFolderName));
                if (!dir.Exists) dir.Create();
                return dir;
            }
        }

        /// <summary> List of all selection filters </summary>
        public List<string> AllSelectionFilters
        {
            get
            {
                if (_allSelectionFilters != null) return _allSelectionFilters;
                _allSelectionFilters = TxSettings.GetTeklaSettingFiles("SObjGrp");
                return _allSelectionFilters;
            }
        }

        /// <summary> Excel table file read results </summary>
        public ObservableCollection<SpreadsheetResultData> DesignResults
        {
            get { return GetDynamicValue<ObservableCollection<SpreadsheetResultData>>(); }
            set { SetDynamicValue(value); }
        }

        public List<string> RebarGradeOptions
        {
            get { return GetDynamicValue<List<string>>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Topmost setting - binds to windows property </summary>
        public bool TopMost
        {
            get { return Properties.Settings.Default.TopMost; }
            set
            {
                Properties.Settings.Default.TopMost = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(TopMost));
            }
        }

        public bool SelectionValid
        {
            get
            {
                if (CurrentSetting == null) return false;
                if (string.IsNullOrEmpty(CurrentSetting.PillarFilter)) return false;
                if (string.IsNullOrEmpty(CurrentSetting.FootingFilter)) return false;
                if (string.IsNullOrEmpty(CurrentSetting.ImportFilePath)) return false;
                return true;
            }
        }

        /// <summary> Main window title </summary>
        public string Title => GlobalConstants.ExtensionDisplayName;

        /// <summary> Status text for user to see in status field </summary>
        public string StatusText
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary>
        /// Flag for when application is busy operating
        /// </summary>
        public bool IsBusy
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public bool IsValidSpreadsheetData
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public ProgressHelper ProgressProxy
        {
            get { return GetDynamicValue<ProgressHelper>(); }
            set { SetDynamicValue(value); }
        }

        public IEnumerable<SavedSetting.BindingNameType> BindingTypeOptions =>
            EnumTools.EnumToList<SavedSetting.BindingNameType>();

        /// <summary>
        /// New view model instance: Required for xaml DataContext setting
        /// </summary>
        public VmMainWindow()
        {
            IsValidSpreadsheetData = false;
            DesignResults = new ObservableCollection<SpreadsheetResultData>();
            RebarGradeOptions = new List<string>();
            StatusText = GlobalConstants.InterfaceInitialized;
            CurrentSetting = new SavedSetting();
            IsBusy = false;
            ProgressProxy = new ProgressHelper();
        }

        /// <summary>
        /// Updates status text to window for user to see
        /// </summary>
        /// <param name="sender">Not needed, null</param>
        /// <param name="e">String arg message with text</param>
        private void NewStatusMessageDel(object sender, StringArg e)
        {
            StatusText = e.StringValue;
        }

        public ICommand OnLoadedCommand => new CommandWrapper(OnLoaded);

        private void OnLoaded(object parameter)
        {
            GeneralEvents.NewStatusMessage += NewStatusMessageDel;
            SavedSettingsChanged += NewSavedSettingsChangedDel;
            SavedSettingDatabase.Instance.Initialize(".srset");
            LoadDefaultSettings();
            PopulateRebarGrades();
        }

        private void PopulateRebarGrades()
        {
            var result = new List<string>();
            var catalog = new CatalogHandler();
            var rebarItemEnum = catalog.GetRebarItems();
            while (rebarItemEnum.MoveNext())
            {
                var rbi = rebarItemEnum.Current as RebarItem;
                if(!result.Contains(rbi.Grade)) result.Add(rbi.Grade);
            }

            RebarGradeOptions = result;
        }


        /// <summary>
        /// Call to validate and load design results from file
        /// </summary>
        public ICommand RefreshFileCommand => new CommandWrapper(RefreshFile, CanRefreshFile);

        private async void RefreshFile(object parameter)
        {
            GeneralEvents.SendNewStatusMessage(new StringArg("Started reading Excel file..."));
            IsBusy = true;
            ProgressProxy.IsProgressIndeterminate = true;

            await System.Threading.Tasks.Task.Run(() =>
            {
                DesignResults = MainLogic.GetFileResults(CurrentSetting);
            });

            GeneralEvents.SendNewStatusMessage(new StringArg("Finished reading Excel file."));

            Application.Current.Dispatcher.Invoke(() =>
                {
                    IsBusy = false;
                    IsValidSpreadsheetData = true;
                    RaisePropertyChanged(nameof(SelectionValid));
                }
            );
        }

        private bool CanRefreshFile(object parameter)
        {
            if (!SelectionValid) return false;
            if (IsBusy) return false;
            return true;
        }

        /// <summary>
        /// Opens log file
        /// </summary>
        public ICommand LogRequestedCommand => new CommandWrapper(OpenLog, CanOpenLog);

        private void OpenLog(object parameter)
        {
            LogListener.Instance.OpenFileInSeparateProcess();
        }

        private bool CanOpenLog(object parameter)
        {
            return !IsBusy;
        }


        /// <summary>
        /// Calls help requested url to launch
        /// </summary>
        public ICommand HelpRequestedCommand => new CommandWrapper(OpenHelp, CanOpenHelp);

        private void OpenHelp(object parameter)
        {
            HelpViewer.DisplayHelpTopic(GlobalConstants.HelpShortUrl);
        }

        private bool CanOpenHelp(object parameter)
        {
            return !IsBusy;
        }


        /// <summary>
        /// Activates file picker dialog
        /// </summary>
        public ICommand SelectFileCommand => new CommandWrapper(SelectFile, CanSelectFile);

        private void SelectFile(object parameter)
        {
            DirectoryInfo rootDir = null;
            if (!string.IsNullOrEmpty(CurrentSetting.ImportFilePath))
            {
                var oldFile = new FileInfo(CurrentSetting.ImportFilePath);
                if (oldFile.Exists && oldFile.DirectoryName != null) rootDir = new DirectoryInfo(oldFile.DirectoryName);
            }
            else rootDir = new DirectoryInfo(DefaultRootFolder.FullName);


            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xlsx",
                Filter = "Spreadsheet Files (*.xlsx)|*.xlsx|Spreadsheet Files (*.xls)|*.xls",
                CheckFileExists = true,
            };
            if (rootDir != null) dialog.InitialDirectory = rootDir.FullName;
            var result = dialog.ShowDialog();
            if (result == true && File.Exists(dialog.FileName))
            {
                var file = new FileInfo(dialog.FileName);
                CurrentSetting.ImportFilePath = file.FullName;
            }
        }

        private bool CanSelectFile(object parameter)
        {
            return CurrentSetting != null && !IsBusy;
        }


        /// <summary>
        /// UI Call to main logic to export selected objects
        /// </summary>
        public ICommand CreateFromSelectedCommand => new CommandWrapper(CreateFromSelected, CanCreateFromSelected);

        private async void CreateFromSelected(object parameter)
        {
            if (ShowNumberingWarningIfNeeded() != MessageBoxResult.OK) return;
            GeneralEvents.SendNewStatusMessage(new StringArg("Create From Selected started..."));
            IsBusy = true;

            await System.Threading.Tasks.Task.Run(() =>
            {
                MainLogic.ImportForSelected(new SavedSetting(CurrentSetting), DesignResults, ProgressProxy);
            });

            GeneralEvents.SendNewStatusMessage(new StringArg("Create From Selected finished."));
            
            Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressProxy.ProgressText = string.Empty;
                    IsBusy = false;
                }
            );
        }

        private bool CanCreateFromSelected(object parameter)
        {
            return SelectionValid && !IsBusy && IsValidSpreadsheetData;
        }


        /// <summary>
        /// UI Call to main logic to export all objects
        /// </summary>
        public ICommand CreateFromAllCommand => new CommandWrapper(CreateFromAll, CanCreateFromAll);

        private async void CreateFromAll(object parameter)
        {
            if (ShowNumberingWarningIfNeeded() != MessageBoxResult.OK) return;
            GeneralEvents.SendNewStatusMessage(new StringArg("Create From All started..."));
            IsBusy = true;

            await System.Threading.Tasks.Task.Run(() =>
            {
                MainLogic.ImportForAll(new SavedSetting(CurrentSetting), DesignResults, ProgressProxy);
            });

            GeneralEvents.SendNewStatusMessage(new StringArg("Create From All finished."));

            Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressProxy.ProgressText = string.Empty;
                    IsBusy = false;
                }
            );
        }

        private bool CanCreateFromAll(object parameter)
        {
            return SelectionValid && !IsBusy && IsValidSpreadsheetData;
        }


        /// <summary>
        /// Opens current set export folder
        /// </summary>
        public ICommand OpenExcelFileCommand => new CommandWrapper(OpenExcelFile, CanOpenExcelFile);

        private void OpenExcelFile(object parameter)
        {
            Process.Start(CurrentSetting.ImportFilePath);
        }

        private bool CanOpenExcelFile(object parameter)
        {
            return CurrentSetting != null && !string.IsNullOrEmpty(CurrentSetting.ImportFilePath);
        }


        /// <summary>
        /// Opens options dialog
        /// </summary>
        public ICommand OpenOptionsWindowCommand => new CommandWrapper(OpenOptionsWindow, CanOpenOptionsWindow);

        private void OpenOptionsWindow(object parameter)
        {
            var dialog = new OptionsWindow(this)
            {
                Owner = Application.Current.MainWindow, WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog();
        }

        private bool CanOpenOptionsWindow(object parameter)
        {
            return !IsBusy;
        }


        /// <summary>
        /// UI Call to main logic to export selected objects
        /// </summary>
        public ICommand SelectionChangedCommand => new CommandWrapper(SelectionChanged);

        private void SelectionChanged(object parameter)
        {
            if (CurrentDesignResult == null) return;
            //todo: Use this to trigger actions based on grid row selection changed if needed
        }


        /// <summary>
        /// /Updates/validates if export buttons should be grayed out
        /// </summary>
        public ICommand UpdateValidSelectionFlagCommand => new CommandWrapper(UpdateValidSelectionFlag);

        private void UpdateValidSelectionFlag(object parameter)
        {
            RaisePropertyChanged(nameof(SelectionValid));
        }


        /// <summary>
        /// Prompts user numbering not up to date warning
        /// </summary>
        /// <returns>Returns user choice for continue</returns>
        private MessageBoxResult ShowNumberingWarningIfNeeded()
        {
            if (CurrentSetting.BindingName != SavedSetting.BindingNameType.Mark) return MessageBoxResult.OK;
            if (Operation.IsNumberingUpToDateAll()) return MessageBoxResult.OK;

            MessageBox.Show(GlobalConstants.NumberingWarning,
                GlobalConstants.ExtensionDisplayName + " : " + GlobalConstants.Warning,
                MessageBoxButton.OK, MessageBoxImage.Stop);
            return MessageBoxResult.Cancel;
        }

        /// <summary>
        /// Gets the adjusted long path accounting for shortened relative path occurence
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLongOutputPath(string path)
        {
            if (path.StartsWith(RelativePathString))
                path = Path.Combine(DefaultRootFolder.FullName, path.Replace(RelativePathString, string.Empty));
            return path;
        }

        /// <summary>
        /// Gets the adjusted short path accounting for shortened relative path occurence
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetShortOutputPath(string path)
        {
            var shitString = DefaultRootFolder.FullName;
            if (path.StartsWith(shitString)) path = RelativePathString + path.Replace(shitString, string.Empty);
            return path;
        }

        private void NewSavedSettingsChangedDel(object sender, EventArgs e)
        {
            RefreshFile(null);
        }

        #region SaveLoad Settings Region

        public EventHandler SavedSettingsChanged;

        private string _saveLoadText;
        public string SaveLoadText
        {
            get { return _saveLoadText; }
            set { SetValue(ref _saveLoadText, value); }
        }

        /// <summary> Current program options setting for controls to bind to </summary>
        public SavedSetting CurrentSetting
        {
            get { return GetDynamicValue<SavedSetting>(); }
            set
            {
                SetDynamicValue(value);
                RaisePropertyChanged(nameof(SelectionValid));
            }
        }


        /// <summary>
        /// Tries to load standard or first deserialized SaveLoad setting from self caching database
        /// </summary>
        private void LoadDefaultSettings()
        {
            try
            {
                //Get deserialized default file
                var defaultSetting = SavedSettingDatabase.Instance.GetDefaultSetting();
                CurrentSetting = defaultSetting != null ? new SavedSetting(defaultSetting) : new SavedSetting();
                SaveLoadText = CurrentSetting.SaveName;

                //SavedSettingsChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Catches combobox index changed and loads copy of data from deserialized file
        /// </summary>
        public ICommand SelectedSavedSettingChangedCommand => new CommandWrapper(SwitchSettings, CanSwitchSettings);

        private void SwitchSettings(object argument)
        {
            try
            {
                if (string.IsNullOrEmpty(SaveLoadText)) return;
                var fromCacheSettings = SavedSettingDatabase.Instance.GetSettingByName(SaveLoadText);
                if (fromCacheSettings == null) return;
                CurrentSetting = new SavedSetting(fromCacheSettings);
                SaveLoadText = CurrentSetting.SaveName;
                SavedSettingsChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private bool CanSwitchSettings(object argument)
        {
            return CurrentSetting != null && !string.IsNullOrEmpty(SaveLoadText);
        }


        /// <summary>
        /// Saves copy of current settings to disk with new name, overwrites if same name
        /// Also adds to current deserialized data file cache
        /// </summary>
        public ICommand SaveCurrentSettingsCommand => new CommandWrapper(SaveCurrentSettings, CanSaveCurrentSettings);

        private void SaveCurrentSettings(object argument)
        {
            try
            {
                if (string.IsNullOrEmpty(SaveLoadText)) return;
                var setting = new SavedSetting(CurrentSetting) {SaveName = SaveLoadText};
                SavedSettingDatabase.Instance.SaveSetting(setting);
                SaveLoadText = setting.SaveName;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private bool CanSaveCurrentSettings(object argument)
        {
            return CurrentSetting != null && !string.IsNullOrEmpty(SaveLoadText);
        }

        #endregion
    }
}