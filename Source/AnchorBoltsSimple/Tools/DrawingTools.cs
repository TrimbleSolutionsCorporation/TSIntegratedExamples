namespace AnchorBoltsSimple.Tools
{
    using System.Collections.Generic;
    using Tekla.Structures.Drawing;

    /// <summary>
    /// Drawing level extensions methods 
    /// </summary>
    public static class DrawingTools
    {
        /// <summary> Drawing handler instance </summary>
        public static DrawingHandler Handler { get { return new DrawingHandler(); } }

        /// <summary> Active (currently open) drawing </summary>
        public static Drawing ActiveDrawing { get { return Handler.GetActiveDrawing(); } }

        /// <summary>
        /// Gets all drawing objects currently selected from drawing
        /// </summary>
        public static List<DrawingObject> SelectedDrawingObjects
        {
            get
            {
                var result = new List<DrawingObject>();
                var selectedEnum = Handler.GetDrawingObjectSelector().GetSelected();
                while (selectedEnum.MoveNext()) result.Add(selectedEnum.Current);
                return result;
            }
        }

        /// <summary>
        /// Gets list of all drawings
        /// </summary>
        public static List<Drawing> AllDrawings
        {
            get
            {
                var result = new List<Drawing>();
                var dwgs = Handler.GetDrawings();
                while (dwgs.MoveNext()) result.Add(dwgs.Current);
                return result;
            }
        }
    }
}
