namespace SpreadsheetReinforcement.Tools
{
    /// <summary>
    /// Creates new temporary macro and runs preset methods
    /// </summary>
    public static class ModelCommands
    {
        /// <summary>
        /// Saves Model
        /// </summary>
        public static void SaveModel()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileSave", null);
        }

        /// <summary>
        /// Saves Model and Creates Backup
        /// </summary>
        public static void SaveModelAndBackup()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileSaveAndBackup", null);
        }

        /// <summary>
        /// Auto saves model
        /// </summary>
        public static void AutoSave()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileAutoSave", null);
        }

        /// <summary>
        /// Fits work area by all parts in model
        /// </summary>
        /// <param name="byParts">Fit by parts (true) or by entire model (false)</param>
        public static void FitWorkArea(bool byParts = false)
        {
            var akit = new DynamicMacroBuilder();
            akit.Callback(byParts ? "acmd_fit_workarea_by_parts" : "acmd_fit_workarea", "", "main_frame");
            akit.Run();
        }

        /// <summary>
        /// Zooms to selected object in model
        /// </summary>
        public static void ZoomToSelected()
        {
            var macro1 = new DynamicMacroBuilder();
            macro1.Callback("acmdZoomToSelected", "", "main_frame");
            macro1.Run();
        }

        /// <summary>
        /// Turns on select assembly switch
        /// </summary>
        public static void SwitchOnAssembly()
        {
            var macro1 = new DynamicMacroBuilder();
            macro1.ValueChange("main_frame", "sel_custom_objects", "0");
            macro1.ValueChange("main_frame", "select_assemblies", "1");
            macro1.Run();
        }

        /// <summary>
        /// Turns off select assembly switch
        /// </summary>
        public static void SwitchOffAssembly()
        {
            var macro1 = new DynamicMacroBuilder();
            macro1.ValueChange("main_frame", "sel_custom_objects", "1");
            macro1.ValueChange("main_frame", "select_assemblies", "0");
            macro1.Run();
        }


        /// <summary>
        /// Run full numbering
        /// </summary>
        public static void RunFullNumbering()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FullNumbering", null);
        }

        /// <summary>
        /// Run correct database
        /// </summary>
        public static void CorrectDatabase()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("CorrectDatabase", null);
        }

        /// <summary>
        /// Run correct library
        /// </summary>
        public static void CorrectLibrary()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("CorrectXslibDatabase", null);
        }

        /// <summary>
        /// Run diagnose model attributes
        /// </summary>
        public static void DiagnoseAttributes()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("DiagnoseAttributeDefinitions", null);
        }

        /// <summary>
        /// Run find distant objects
        /// </summary>
        public static void FindDistantObjects()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FindDistantObjects", null);
        }

        /// <summary>
        /// Run open customize dialog
        /// </summary>
        public static void OpenCustomizeDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("Customize", null);
        }

        /// <summary>
        /// Open options settings dialog
        /// </summary>
        public static void OpenOptionSettingsDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("OptionSettings", null);
        }

        /// <summary>
        /// Open environment variables dialog
        /// </summary>
        public static void OpenEnvironmentVariablesDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("EnvironmentVariables", null);
        }

        /// <summary>
        /// Toggle snap reference
        /// </summary>
        public static void ToggleSnapReference()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSnapReference", null);
        }

        /// <summary>
        /// Toggle snap geometry
        /// </summary>
        public static void ToggleSnapGeometry()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSnapGeometry", null);
        }

        /// <summary>
        /// Toggle snap Free
        /// </summary>
        public static void ToggleSnapFree()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSnapFree", null);
        }

        /// <summary>
        /// Run inquire center of gravity
        /// </summary>
        public static void InquireCenterOfGravity()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("InquireCenterOfGravity", null);
        }

        /// <summary>
        /// Open about tekla structures dialog
        /// </summary>
        public static void OpenAboutTeklaStructuresDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("HelpAboutTeklaStructures", null);
        }

        /// <summary>
        /// Open release notes
        /// </summary>
        public static void OpenReleaseNotesDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ReleaseNotes", null);
        }

        /// <summary>
        /// Open help contents
        /// </summary>
        public static void OpenHelpContentsDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("HelpContents", null);
        }

        /// <summary>
        /// Open file open dialog
        /// </summary>
        public static void OpenFileDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileOpen", null);
        }

        /// <summary>
        /// Open create new model dialog
        /// </summary>
        public static void OpenCreateNewModelDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileNew", null);
        }

        /// <summary>
        /// Open file save as dialog
        /// </summary>
        public static void OpenModelFileSaveAsDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileSaveAs", null);
        }

        /// <summary>
        /// Open plot dialog
        /// </summary>
        public static void OpenPlotDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FilePlot", null);
        }


        /// <summary>
        /// Open create reference object dialog
        /// </summary>
        public static void OpenCreateReferenceObject()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ReferenceModelAndProperties", null);
        }

        /// <summary>
        /// Open reference model list dialog
        /// </summary>
        public static void OpenReferenceObjectList()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("DisplayReferenceObjectListDialog", null);
        }

        /// <summary>
        /// Run numbering for selected objects only
        /// </summary>
        public static void NumberingForSelectedObjects()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("NumberingForSelectedObjects", null);
        }

        /// <summary>
        /// Open create drawings dialog
        /// </summary>
        public static void OpenCreateDrawingsDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("CreateDrawings", null);
        }

        /// <summary>
        /// Create cast unit drawing for selected objects
        /// </summary>
        public static void CreateCastUnitDrawingForSelected()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("CastUnitDrawing", null);
        }

        /// <summary>
        /// Create assembly drawing for selected objects
        /// </summary>
        public static void CreateAssemblyDrawingForSelected()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("AssemblyDrawing", null);
        }

        /// <summary>
        /// Open report dialog
        /// </summary>
        public static void OpenReportDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("Report", null);
        }

        /// <summary>
        /// Open component catalog dialog
        /// </summary>
        public static void OpenDisplayComponentCatalogDialog()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("DisplayComponentCatalog", null);
        }

        /// <summary>
        /// Changes selected joint to conceptual type
        /// </summary>
        public static void ChangeSelectedJointTypeToConceptual()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ChangeJointTypeToConceptual", null);
        }

        /// <summary>
        /// Changes selected joint to detailing type
        /// </summary>
        public static void ChangeSelectedJointTypeToDetail()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ChangeJointTypeToDetail", null);
        }

        /// <summary>
        /// Explode selected joint
        /// </summary>
        public static void ExplodeSelectedJoint()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ExplodeJoint", null);
        }

        /// <summary>
        /// Toggle select all
        /// </summary>
        public static void ToggleSelectAll()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectAll", null);
        }

        /// <summary>
        /// Toggle select connections
        /// </summary>
        public static void ToggleSelectConnections()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectConnections", null);
        }

        /// <summary>
        /// Toggle select parts
        /// </summary>
        public static void ToggleSelectParts()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectParts", null);
        }

        /// <summary>
        /// Toggle select points
        /// </summary>
        public static void ToggleSelectPoints()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectPoints", null);
        }

        /// <summary>
        /// Toggle select welds
        /// </summary>
        public static void ToggleSelectWeldings()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectWeldings", null);
        }

        /// <summary>
        /// Toggle select cuts
        /// </summary>
        public static void ToggleSelectCuts()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectCuts", null);
        }

        /// <summary>
        /// Toggle select views
        /// </summary>
        public static void ToggleSelectViews()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectViews", null);
        }

        /// <summary>
        /// Toggle select bolts
        /// </summary>
        public static void ToggleSelectBolts()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectBolts", null);
        }

        /// <summary>
        /// Toggle select single bolts
        /// </summary>
        public static void ToggleSelectSingleBolts()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectSingleBolts", null);
        }

        /// <summary>
        /// Toggle select components
        /// </summary>
        public static void ToggleSelectComponents()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectComponents", null);
        }

        /// <summary>
        /// Toggle select loads
        /// </summary>
        public static void ToggleSelectLoads()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectLoads", null);
        }

        /// <summary>
        /// Does nothing, no idea what this is
        /// </summary>
        public static void ToggleSelectObjects()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectObjects", null);
        }

        /// <summary>
        /// Selects "Objects in components" or 2nd selection switch in set of 5 tree types
        /// </summary>
        public static void ToggleSelectObjectsInConnections()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectObjectsInConnections", null);
        }

        /// <summary>
        /// Selects "Objects" or 1st selection switch in set of 5 tree types
        /// </summary>
        public static void ToggleSelectCustomObjects()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectCustomObjects", null);
        }

        /// <summary>
        /// Selects "Assemblies" or 3rd selection switch in set of 5 tree types
        /// </summary>
        public static void ToggleSelectAssemblies()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectAssemblies", null);
        }

        /// <summary>
        /// Selects "Objects in Assemblies" or 4th selection switch in set of 5 tree types
        /// </summary>
        public static void ToggleSelectObjectsInAssemblies()
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("ToggleSelectObjectsInAssemblies", null);
        }
    }
}