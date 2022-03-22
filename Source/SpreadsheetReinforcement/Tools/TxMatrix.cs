namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    /// <summary>
    /// Extension of internal Matrix
    /// </summary>
    public static class TxMatrix
    {
        /// <summary>
        /// Prints a transformation plane To Global
        /// </summary>
        /// <param name="p">Plane</param>
        /// <param name="header">Header Text</param>
        public static void PrintTransformationPlaneToGlobal(this TransformationPlane p, string header)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            Debug.WriteLine(header);
            Debug.WriteLine("");
            Debug.WriteLine((p.TransformationMatrixToGlobal).ToString());
            Debug.WriteLine("");
        }

        /// <summary>
        /// Transforms coordinate system using matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static CoordinateSystem Transform(this Matrix m, CoordinateSystem cs)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (cs == null) throw new ArgumentNullException(nameof(cs));

            var origin = new Point(cs.Origin);
            var xAxis = new Vector(cs.AxisX);
            var yAxis = new Vector(cs.AxisY);

            var newOrigin = m.Transform(origin);
            var newXaxis = Transform(m, xAxis);
            var newYaxis = Transform(m, yAxis);
            return new CoordinateSystem(newOrigin, newXaxis, newYaxis);
        }

        /// <summary>
        /// Transforms vector by matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector Transform(this Matrix m, Vector v)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (v == null) throw new ArgumentNullException(nameof(v));

            var p1 = m.Transform(new Point());
            var p2 = m.Transform(new Point(v.X, v.Y, v.Z));
            return new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        }

        /// <summary>
        /// Prints a transformation plane To Local
        /// </summary>
        /// <param name="p">Plane</param>
        /// <param name="header">Header Text</param>
        public static void PrintTransformationPlaneToLocal(this TransformationPlane p, string header)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));

            Debug.WriteLine(header);
            Debug.WriteLine("");
            Debug.WriteLine((p.TransformationMatrixToLocal).ToString());
            Debug.WriteLine("");
        }

        /// <summary>
        /// Print matrix for debugging
        /// </summary>
        /// <param name="m">Matrix</param>
        /// <param name="header">Header Text</param>
        public static void PrintMatrix(this Matrix m, string header)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Debug.WriteLine(header);
            Debug.WriteLine("");
            Debug.WriteLine(m.ToString());
            Debug.WriteLine("");
        }
    }
}