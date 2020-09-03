namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;

    /// <summary>
    /// Extensions class for Line in Geometry3d
    /// </summary>
    public static class TxLine
    {
        ///<summary>
        /// Prints points and line information to logger
        ///</summary>
        ///<param name="ln">Geometry line to print</param>
        ///<param name="headerText">Header text to add</param>
        public static void Print(this Line ln, string headerText)
        {
            if (ln == null) return;
            if (string.IsNullOrEmpty(headerText)) headerText = "Line Print";
            Debug.WriteLine("======================" + headerText + "===================");
            Debug.WriteLine("Origin Point: ");
            Debug.WriteLine("X value: " + Math.Round(ln.Origin.X, 5));
            Debug.WriteLine("Y value: " + Math.Round(ln.Origin.Y, 5));
            Debug.WriteLine("Z value: " + Math.Round(ln.Origin.Z, 5));
            Debug.WriteLine("Direction Vector: ");
            Debug.WriteLine("X value: " + Math.Round(ln.Direction.X, 5));
            Debug.WriteLine("Y value: " + Math.Round(ln.Direction.Y, 5));
            Debug.WriteLine("Z value: " + Math.Round(ln.Direction.Z, 5));
            Debug.WriteLine("===================================================");
            Debug.WriteLine(System.Environment.NewLine);
        }
    }
}