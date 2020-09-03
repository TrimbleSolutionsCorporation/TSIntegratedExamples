namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Tekla.Structures;
    using Tekla.Structures.Drawing;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.Operations;

    /// <summary>
    /// Macro builder.
    /// </summary>
    public sealed class DynamicMacroBuilder
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DynamicMacroBuilder()
        {
            this._Macro = new StringBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="script">Script text.</param>
        public DynamicMacroBuilder(string script)
        {
            this._Macro = new StringBuilder(script);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Activates a field on a dialog.
        /// </summary>
        /// <param name="dialog">Dialog name.</param>
        /// <param name="field">Field name.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder Activate(string dialog, string field)
        {
            return AppendMethodCall("Activate", dialog, field);
        }

        /// <summary>
        /// Invokes a callback.
        /// </summary>
        /// <param name="callback">Callback name.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder Callback(string callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            return Callback(callback, "");
        }

        /// <summary>
        /// Invokes a callback.
        /// </summary>
        /// <param name="callback">Callback name.</param>
        /// <param name="parameter">Callback parameter.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder Callback(string callback, string parameter)
        {
            return Callback(callback, parameter, "main_frame");
        }

        /// <summary>
        /// Invokes a callback.
        /// </summary>
        /// <param name="callback">Callback name.</param>
        /// <param name="parameter">Callback parameter.</param>
        /// <param name="frame">Target frame.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder Callback(string callback, string parameter, string frame)
        {
            return AppendMethodCall("Callback", callback, parameter, frame);
        }

        /// <summary>
        /// Checks or unchecks a field.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="value">Check value.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder CheckValue(string name, int value)
        {
            return AppendMethodCall("CheckValue", name, value);
        }

        /// <summary>
        /// Starts a command.
        /// </summary>
        /// <param name="command">Command name.</param>
        /// <param name="parameter">Command parameter.</param>
        /// <param name="frame">Target frame.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder CommandStart(string command, string parameter, string frame)
        {
            return AppendMethodCall("CommandStart", command, parameter, frame);
        }

        /// <summary>
        /// Ends a command.
        /// </summary>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder CommandEnd()
        {
            return AppendMethodCall("CommandEnd");
        }

        /// <summary>
        /// Performs file selection.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder FileSelection(params string[] items)
        {
            this._Macro.Append("akit.FileSelection(");

            for (int Index = 0; Index < items.Length; Index++)
            {
                if (Index > 0)
                {
                    this._Macro.Append(", ");
                }

                this._Macro.Append('"').Append(items[Index]).Append('"');
            }

            this._Macro.AppendLine(");");
            return this;
        }

        /// <summary>
        /// Selects items from a list field.
        /// </summary>
        /// <param name="dialog">Dialog name.</param>
        /// <param name="field">Field name.</param>
        /// <param name="items">List items.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder ListSelect(string dialog, string field, params string[] items)
        {
            this._Macro.AppendFormat(@"akit.ListSelect(""{0}"", ""{1}""", dialog, field);

            for (int Index = 0; Index < items.Length; Index++)
            {
                this._Macro.Append(", ").Append('"').Append(items[Index]).Append('"');
            }

            this._Macro.AppendLine(");");
            return this;
        }

        /// <summary>
        /// Invokes a modal dialog.
        /// </summary>
        /// <param name="value">Modal dialog value.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder ModalDialog(int value)
        {
            return AppendMethodCall("ModalDialog", value);
        }

        /// <summary>
        /// Simulates a mouse button down event.
        /// </summary>
        /// <param name="frame">Frame name.</param>
        /// <param name="subframe">Subframe name.</param>
        /// <param name="x">Mouse X position.</param>
        /// <param name="y">Mouse Y position.</param>
        /// <param name="modifier">Mouse button modifiers.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder MouseDown(string frame, string subframe, int x, int y, int modifier)
        {
            return AppendMethodCall("MouseDown", frame, subframe, x, y, modifier);
        }

        /// <summary>
        /// Simulates a mouse button up event.
        /// </summary>
        /// <param name="frame">Frame name.</param>
        /// <param name="subframe">Subframe name.</param>
        /// <param name="x">Mouse X position.</param>
        /// <param name="y">Mouse Y position.</param>
        /// <param name="modifier">Mouse button modifiers.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder MouseUp(string frame, string subframe, int x, int y, int modifier)
        {
            return AppendMethodCall("MouseUp", frame, subframe, x, y, modifier);
        }

        /// <summary>
        /// Pushes a button.
        /// </summary>
        /// <param name="button">Button name.</param>
        /// <param name="frame">Frame name.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder PushButton(string button, string frame)
        {
            return AppendMethodCall("PushButton", button, frame);
        }

        /// <summary>
        /// Changes the active tab page.
        /// </summary>
        /// <param name="dialog">Dialog name.</param>
        /// <param name="field">Field name.</param>
        /// <param name="item">Tab page.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder TabChange(string dialog, string field, string item)
        {
            return AppendMethodCall("TabChange", dialog, field, item);
        }

        /// <summary>
        /// Selects items on a table field.
        /// </summary>
        /// <param name="dialog">Dialog name.</param>
        /// <param name="field">Field name.</param>
        /// <param name="items">Table items.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder TableSelect(string dialog, string field, params int[] items)
        {
            this._Macro.AppendFormat(@"akit.TableSelect(""{0}"", ""{1}""", dialog, field);

            for (int Index = 0; Index < items.Length; Index++)
            {
                this._Macro.Append(", ").Append(items[Index]);
            }

            this._Macro.AppendLine(");");
            return this;
        }

        /// <summary>
        /// Selects items in a tree field.
        /// </summary>
        /// <param name="dialog">Dialog name,</param>
        /// <param name="field">Field name.</param>
        /// <param name="rowstring">Tree row string.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder TreeSelect(string dialog, string field, string rowstring)
        {
            return AppendMethodCall("TreeSelect", dialog, field, rowstring);
        }

        /// <summary>
        /// Changes a field value.
        /// </summary>
        /// <param name="dialog">Dialog name.</param>
        /// <param name="field">Field name.</param>
        /// <param name="data">Field value.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        public DynamicMacroBuilder ValueChange(string dialog, string field, string data)
        {
            return AppendMethodCall("ValueChange", dialog, field, data);
        }

        /// <summary>
        /// Runs the constructed macro.
        /// </summary>
        public void Run()
        {
            try
            {
                string Name = GetMacroFileName();
                string MacrosPath = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref MacrosPath);
                if (MacrosPath.Contains(";"))
                {
                    var paths = MacrosPath.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                        .OrderBy(f => f.Length);
                    MacrosPath = paths.FirstOrDefault();
                }

                File.WriteAllText(
                    Path.Combine(MacrosPath, Name),
                    "namespace Tekla.Technology.Akit.UserScript {" +
                    "public class Script {" +
                    "public static void Run(Tekla.Technology.Akit.IScript akit) {" +
                    this._Macro.ToString() +
                    "}" +
                    "}" +
                    "}"
                );

                RunMacro("..\\" + Name);
            }
            catch (IOException Ex)
            {
                Debug.WriteLine(Ex);
            }
        }

        /// <summary>
        /// Executes a macro in the view.
        /// </summary>
        /// <param name="macroName">Macro name.</param>
        public void RunMacro(string macroName)
        {
            var CurrentModel = new Model();
            var DrawingHandler = new DrawingHandler();

            if (DrawingHandler.GetActiveDrawing() != null) macroName = @"..\drawings\" + macroName;

            if (CurrentModel.GetConnectionStatus())
            {
                if (!Path.HasExtension(macroName)) macroName += ".cs";

                while (Operation.IsMacroRunning()) Thread.Sleep(100);

                Operation.RunMacro(macroName);
            }
        }

        /// <summary>
        /// Returns the constructed macro text.
        /// </summary>
        /// <returns>Macro text.</returns>
        public override string ToString()
        {
            return this._Macro.ToString();
        }

        /// <summary>
        /// Appends a method call to macro.
        /// </summary>
        /// <param name="method">Method to call.</param>
        /// <param name="arguments">Argument list.</param>
        /// <returns>Reference to self for fluent interface pattern.</returns>
        private DynamicMacroBuilder AppendMethodCall(string method, params object[] arguments)
        {
            this._Macro.Append("akit.").Append(method).Append('(');

            for (int Index = 0; Index < arguments.Length; Index++)
            {
                if (Index > 0)
                {
                    this._Macro.Append(", ");
                }

                if (arguments[Index] is string)
                {
                    this._Macro.Append('"').Append(arguments[Index]).Append('"');
                }
                else
                {
                    this._Macro.Append(arguments[Index]);
                }
            }

            this._Macro.AppendLine(");");
            return this;
        }

        /// <summary>
        /// Gets the generated macro file name.
        /// </summary>
        /// <returns>Generated macro file name.</returns>
        private static string GetMacroFileName()
        {
            lock (Random)
            {
                if (_TempFileIndex < 0)
                {
                    _TempFileIndex = Random.Next(0, MaxTempFiles);
                }
                else
                {
                    _TempFileIndex = (_TempFileIndex + 1) % MaxTempFiles;
                }

                return string.Format(FileNameFormat, _TempFileIndex);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Macro text.
        /// </summary>
        private readonly StringBuilder _Macro;

        /// <summary>
        /// Temporary file index.
        /// </summary>
        private static int _TempFileIndex = -1;

        /// <summary>
        /// Random number source.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Maximum number of temporary files to use.
        /// </summary>
        private const int MaxTempFiles = 32;

        /// <summary>
        /// File name format.
        /// </summary>
        private const string FileNameFormat = "macro_{0:00}.cs";

        #endregion
    }
}