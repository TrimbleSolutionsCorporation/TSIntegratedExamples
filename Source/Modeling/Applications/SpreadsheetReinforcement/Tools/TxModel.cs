namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using Tekla.Structures;
    using Tekla.Structures.Dialog;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;
    using Point = Tekla.Structures.Geometry3d.Point;
    using Vector = Tekla.Structures.Geometry3d.Vector;

    /// <summary>
    /// Tekla model class extensions
    /// </summary>
    public static class TxModel
    {
        /// <summary> Tekla Core null int value </summary>
        public const int NullIntegerValue = -2147483648;

        /// <summary> Tekla Core null double value </summary>
        public const double NullDoubleValue = -2147483648.0;

        /// <summary> Default path seperator </summary>
        public static readonly char[] FolderPathSeparators = {';'};

        /// <summary> Model attributes subfolder name </summary>
        public const string AttributeSubFolderName = "attributes";

        /// <summary> Model uda name for offset x value </summary>
        public const string ProjectOffsetX = "PROJ_DATUM_X";

        /// <summary> Model uda name for offset y value </summary>
        public const string ProjectOffsetY = "PROJ_DATUM_Y";

        /// <summary> Model uda name for offset z value </summary>
        public const string ProjectOffsetZ = "PROJ_DATUM_Z";

        /// <summary>
        /// Checks that Tekla is running, a connection can be made, and a Model is open
        /// </summary>
        public static bool IsConnected =>
            IsRunning && new Model().GetConnectionStatus() && !string.IsNullOrEmpty(ModelFolder);

        /// <summary>
        /// Gets a boolean value indicating whether the application is running.
        /// </summary>
        /// <value>
        /// Indicates whether the application is running.
        /// </value>
        public static bool IsRunning => MainWindowHandle != IntPtr.Zero;

        /// <value>
        /// Main window handle.
        /// </value>
        public static IntPtr MainWindowHandle => MainWindow.Frame != null ? MainWindow.Frame.Handle : IntPtr.Zero;

        /// <summary>
        /// Model root folder
        /// </summary>
        public static string ModelFolder => new Model().GetInfo().ModelPath;

        /// <summary>
        /// Model root folder
        /// </summary>
        public static List<DirectoryInfo> StandardPropertyDirectories
        {
            get
            {
                var propertyDirectories =
                    Tekla.Structures.Dialog.UIControls.EnvironmentFiles.GetStandardPropertyFileDirectories();
                return propertyDirectories.Select(dir => new DirectoryInfo(dir)).Where(tDir => tDir.Exists).ToList();
            }
        }

        /// <summary>
        /// Gets Tekla firm folder directory
        /// </summary>
        public static DirectoryInfo FirmFolder
        {
            get
            {
                var tempString = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_FIRM", ref tempString);
                if (string.IsNullOrEmpty(tempString)) return null;
                var tDir = new DirectoryInfo(tempString);
                return tDir.Exists ? tDir : null;
            }
        }

        /// <summary>
        /// Gets the project folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> TemplateFolders
        {
            get
            {
                var xsProject = TxAdvancedOptions.XsTemplateDirectory;
                while (xsProject.Contains(@"\\")) xsProject = xsProject.Replace(@"\\", @"\");
                var set1 = xsProject.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);

                var xsProject2 = TxAdvancedOptions.XsTemplateDirectorySystem;
                while (xsProject2.Contains(@"\\")) xsProject2 = xsProject2.Replace(@"\\", @"\");
                var set2 = xsProject2.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);
                return set1.Concat(set2);
            }
        }

        /// <summary>
        /// Gets dx, dy, and dz for global offset uda's
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Vector GlobalOffsetValues(this Model model)
        {
            double dx = 0.0, dy = 0.0, dz = 0.0;
            new Model().GetProjectInfo().GetUserProperty(ProjectOffsetX, ref dx);
            new Model().GetProjectInfo().GetUserProperty(ProjectOffsetY, ref dy);
            new Model().GetProjectInfo().GetUserProperty(ProjectOffsetZ, ref dz);
            return new Vector(dx, dy, dz);
        }


        /// <summary>
        /// Check connection to Tekla and sees if a model is open
        /// Shows message box when no connection or model open
        /// </summary>
        /// <returns>Is TS running, model is open, and connection successful?</returns>
        public static bool ConnectWithDialog()
        {
            //Create new model connection without creating channel.
            var tModel = new Model();

            if (tModel.GetConnectionStatus())
            {
                //check that model is opened
                if (string.IsNullOrEmpty(tModel.GetInfo().ModelPath))
                {
                    MessageBox.Show("Model is not loaded in TeklaStructures, you must load a model first.");
                    return false;
                }

                Debug.WriteLine("Connected to TeklaStructures: " + TeklaStructuresInfo.GetCurrentProgramVersion());
                return true;
            }

            MessageBox.Show("Cannot connect to TeklaStructures process, Tekla Structures must first be running.");
            return false;
        }

        /// <summary>
        /// Gets the Current NumberFormatInfo for installed culture
        /// </summary>
        public static NumberFormatInfo CurrentNumberFormat => CultureInfo.CurrentCulture.NumberFormat;

        /// <summary>
        /// Gets current Number decimal separator
        /// </summary>
        public static string NumberDecimalSeparator => CurrentNumberFormat.NumberDecimalSeparator;

        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <value>
        /// Application version string.
        /// </value>
        public static string Version => TeklaStructuresInfo.GetCurrentProgramVersion();

        /// <summary>
        /// Returns if imperial units are being used
        /// </summary>
        public static bool IsImperial
        {
            get
            {
                var stringTemp = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_IMPERIAL", ref stringTemp);
                if (stringTemp.Trim().ToUpper() == "TRUE") return true;
                return false;
            }
        }

        /// <summary>
        /// Gets the project folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> ProjectFolders
        {
            get
            {
                var xsProject = TxAdvancedOptions.XsProject;
                while (xsProject.Contains(@"\\")) xsProject = xsProject.Replace(@"\\", @"\");
                return xsProject.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Gets the company folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> CompanyFolders
        {
            get
            {
                var xsFirm = TxAdvancedOptions.XsFirm;
                while (xsFirm.Contains(@"\\")) xsFirm = xsFirm.Replace(@"\\", @"\");
                return xsFirm.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Gets the cloning template model folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> CloningTemplateModelFolders
        {
            get
            {
                var xsCloningDir = TxAdvancedOptions.XsCloningTemplateDir;
                foreach (var location in xsCloningDir.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    foreach (var path in Directory.GetDirectories(location))
                    {
                        yield return path;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the system folders.
        /// </summary>
        /// <value>
        /// Enumerable collection of paths.
        /// </value>
        public static IEnumerable<string> SystemFolders
        {
            get
            {
                var xsDir = TxAdvancedOptions.XsSystem;
                while (xsDir.Contains(@"\\")) xsDir = xsDir.Replace(@"\\", @"\");
                return xsDir.Split(FolderPathSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        /// <summary>
        /// Gets the full Model Path
        /// </summary>
        public static string ModelPath => new Model().GetInfo().ModelPath;

        /// <summary>
        /// Model attributes folder
        /// </summary>
        public static string ModelAttributesFolder => Path.Combine(ModelFolder, AttributeSubFolderName);

        /// <summary>
        /// Gets list of currently selected model objects
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ModelObject> GetSelectedObjects(this Model model)
        {
            var result = new List<ModelObject>();
            var selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            var objs = selector.GetSelectedObjects();
            while (objs.MoveNext()) result.Add(objs.Current);
            return result;
        }

        /// <summary>
        /// Gets a list of all phases current available from model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<Phase> GetAllPhases(this Model model)
        {
            var phases = model.GetPhases();
            return phases.Cast<Phase>().ToList();
        }

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// True if the value is set to the default.
        /// </returns>
        public static bool IsDefaultValue(this int value)
        {
            return value == NullIntegerValue;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// True if the value is set to the default.
        /// </returns>
        public static bool IsDefaultValue(this double value)
        {
            return Math.Abs(value - NullDoubleValue) < GeometryConstants.DISTANCE_EPSILON;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value (empty string).
        /// 
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// True if the value is set to the default.
        /// </returns>
        public static bool IsDefaultValue(this string value)
        {
            return value == "";
        }

        /// <summary>
        /// Gets the current model coordinate system transformation plane
        /// </summary>
        /// <param name="tModel"></param>
        /// <returns></returns>
        public static TransformationPlane GetWorkPlane(this Model tModel)
        {
            return tModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
        }

        /// <summary>
        /// Changes the model workplane
        /// </summary>
        /// <param name="tModel"></param>
        /// <returns></returns>
        public static bool SetWorkPlane(this Model tModel)
        {
            return tModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
        }

        /// <summary>
        /// Changes the model workplane
        /// </summary>
        /// <param name="tModel"></param>
        /// <param name="tCoordinateSystem"></param>
        /// <returns></returns>
        public static bool SetWorkPlane(this Model tModel, CoordinateSystem tCoordinateSystem)
        {
            if (tCoordinateSystem == null) return false;
            return tModel.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(new TransformationPlane(tCoordinateSystem));
        }

        /// <summary>
        /// Changes the model workplane
        /// </summary>
        /// <param name="tModel"></param>
        /// <param name="tTransformationPlane"></param>
        /// <returns></returns>
        public static bool SetWorkPlane(this Model tModel, TransformationPlane tTransformationPlane)
        {
            if (tTransformationPlane == null) return false;
            return tModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(tTransformationPlane);
        }

        /// <summary>
        /// Changes the workplane to model object coordinate system
        /// </summary>
        /// <param name="tModel"></param>
        /// <param name="mo"></param>
        /// <returns></returns>
        public static bool SetWorkPlane(this Model tModel, ModelObject mo)
        {
            if (mo == null) return false;
            return tModel.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(new TransformationPlane(mo.GetCoordinateSystem()));
        }

        /// <summary>
        /// Sets the current work plane to Global
        /// </summary>
        public static void SetWorkPlaneToGlobal(this Model tModel)
        {
            new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
        }

        /// <summary>
        /// Paints current WorkPlane in model with temporary graphics
        /// </summary>
        /// <param name="tModel"></param>
        public static void DrawWorkPlane(this Model tModel)
        {
            DrawCoordinateSystem(new CoordinateSystem());
        }

        /// <summary>
        /// Paints specific coordinate system from model object
        /// </summary>
        /// <param name="mo"></param>
        public static void DrawCoordinateSystem(this ModelObject mo)
        {
            DrawCoordinateSystem(mo.GetCoordinateSystem());
        }

        /// <summary>
        /// Gets all views for model that are visible
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<View> GetVisibleViews(this Model model)
        {
            var views = new List<View>();
            var vEnum = ViewHandler.GetVisibleViews();
            while (vEnum.MoveNext())
            {
                var v = vEnum.Current;
                views.Add(v);
            }

            return views;
        }

        /// <summary>
        /// Gets currently selected View from Model
        /// </summary>
        public static View GetSelectedView(this Model model)
        {
            View v = null;
            var vEnum = ViewHandler.GetSelectedViews();
            if (vEnum.Count <= 0) return v;
            while (vEnum.MoveNext())
            {
                v = vEnum.Current;
                if (v != null) break;
            }

            return v;
        }

        /// <summary>
        /// Gets global up vector based on current workplane
        /// </summary>
        /// <param name="tModel">Current model</param>
        /// <returns>Transformed up vector</returns>
        public static Vector GetGlobalUpVector(this Model tModel)
        {
            var zGlobal = new Vector(0, 0, 1);
            var transCoord = MatrixFactory.FromCoordinateSystem(new CoordinateSystem());
            return zGlobal.Multiplier(transCoord).GetNormal();
        }

        /// <summary>
        /// Paints current model coordinate system
        /// </summary>
        /// <param name="cs"></param>
        public static void DrawCoordinateSystem(this CoordinateSystem cs)
        {
            //Local variables
            const double vScalar = 500.0;
            if (cs == null) cs = new CoordinateSystem();
            var graphicsDrawer = new GraphicsDrawer();

            //Color constants
            var red = new Color(1, 0, 0);
            var green = new Color(0, 1, 0);
            var blue = new Color(0, 0, 1);

            //Initialize axis points for arrows at origin
            var endX = new Point(cs.Origin);
            var endY = new Point(cs.Origin);
            var endZ = new Point(cs.Origin);

            //Move axis end points for arrows along vector scaled
            TxPoint.Translate(endX, cs.AxisX.GetNormal() * vScalar);
            TxPoint.Translate(endY, cs.AxisY.GetNormal() * vScalar);
            TxPoint.Translate(endZ, Vector.Cross(cs.AxisX, cs.AxisY).GetNormal() * vScalar);

            //Draw result lines from origin
            graphicsDrawer.DrawLineSegment(cs.Origin, endX, red);
            graphicsDrawer.DrawLineSegment(cs.Origin, endY, green);
            graphicsDrawer.DrawLineSegment(cs.Origin, endZ, blue);
        }

        /// <summary>
        /// Saves current model through UI virtual call
        /// </summary>
        public static void SaveModel(this Model tModel)
        {
            Tekla.Structures.ModelInternal.Operation.dotStartAction("FileSave", null);
        }

        /// <summary>
        /// Waits for macro running
        /// </summary>
        /// <param name="waitCount">How many cycles of 100ms to wait before returning anyway</param>
        /// <returns>returns when done waiting</returns>
        public static bool WaitForMacro(int waitCount)
        {
            while (Tekla.Structures.Model.Operations.Operation.IsMacroRunning() && waitCount > 0)
            {
                waitCount--;
                Thread.Sleep(100);
            }

            return waitCount != 0;
        }

        /// <summary>
        /// Runs macro from model operations class
        /// </summary>
        /// <param name="macroName"></param>
        /// <returns></returns>
        public static bool RunMacro(string macroName)
        {
            if (string.IsNullOrEmpty(macroName)) return false;
            return Tekla.Structures.Model.Operations.Operation.RunMacro(macroName);
        }

        /// <summary>
        /// Runs Macro using operation class
        /// </summary>
        /// <param name="tModel">Tekla Model</param>
        /// <param name="macroName">Macro name</param>
        /// <returns>True if successful</returns>
        public static bool RunMacro(this Model tModel, string macroName)
        {
            return RunMacro(macroName);
        }

        /// <summary>
        /// Waits for any macro currently running using operation class
        /// </summary>
        /// <param name="tModel">Tekla Model</param>
        /// <param name="waitCount"></param>
        /// <returns>True if successful</returns>
        public static bool WaitForMacro(this Model tModel, int waitCount)
        {
            return WaitForMacro(waitCount);
        }
    }
}