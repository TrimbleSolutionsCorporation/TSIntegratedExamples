namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Class extensions for CoordinateSystem
    /// </summary>
    public static class TxCoordinateSystem
    {
        /// <summary>
        /// Uses cross product to derive local Z axis of part
        /// </summary>
        /// <returns>New local z direction unit vector</returns>
        public static Vector GetAxisZ(this CoordinateSystem cs)
        {
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            return Vector.Cross(cs.AxisX, cs.AxisY).GetNormal();
        }

        /// <summary>
        /// Normalizes a Coordinate system
        /// </summary>
        /// <param name="coord">Coordinate system to normalize</param>
        /// <param name="roundDecimals">Decimal places to round to</param>
        /// <returns>New normalized coordinate system</returns>
        public static CoordinateSystem Normalize(this CoordinateSystem coord, int roundDecimals = 4)
        {
            if (coord == null) return new CoordinateSystem();

            var x = coord.AxisX.GetNormal();
            var y = coord.AxisY.GetNormal();
            var newX = new Vector(Math.Round(x.X, roundDecimals), Math.Round(x.Y, roundDecimals),
                Math.Round(x.Z, roundDecimals));
            var newY = new Vector(Math.Round(y.X, roundDecimals), Math.Round(y.Y, roundDecimals),
                Math.Round(y.Z, roundDecimals));
            return new CoordinateSystem(coord.Origin, newX, newY);
        }

        /// <summary>
        /// Prints debug information for coordinate system
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="header"></param>
        public static void Print(this CoordinateSystem coord, string header)
        {
            if (coord == null) throw new ArgumentNullException(nameof(coord));
            try
            {
                var vectorZ = Vector.Cross(coord.AxisX, coord.AxisY);
                Trace.WriteLine(@"========================================================");
                Trace.WriteLine(header);
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Axis X:\t{0}\t\t{1}\t\t{2}", Math.Round(coord.AxisX.X, 4),
                    Math.Round(coord.AxisX.Y, 4), Math.Round(coord.AxisX.Z, 4)));
                Trace.WriteLine(string.Format("Axis Y:\t{0}\t\t{1}\t\t{2}", Math.Round(coord.AxisY.X, 4),
                    Math.Round(coord.AxisY.Y, 4), Math.Round(coord.AxisY.Z, 4)));
                Trace.WriteLine(string.Format("Axis Z:\t{0}\t\t{1}\t\t{2}", Math.Round(vectorZ.X, 4),
                    Math.Round(vectorZ.Y, 4), Math.Round(vectorZ.Z, 4)));
                Trace.WriteLine(string.Format("Coord :\t{0}\t\t{1}\t\t{2}", Math.Round(coord.Origin.X, 4),
                    Math.Round(coord.Origin.Y, 4), Math.Round(coord.Origin.Z, 4)));
                Trace.WriteLine("");
            }
            catch (Exception ex)
            {
                var fileInfo = new StackFrame(true);
                Trace.WriteLine(string.Format("File : {0}\nLine : {1}\nException : {2}\nTrace : {3}",
                    fileInfo.GetFileName(), fileInfo.GetFileLineNumber(), ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Paints coordinate system in the model with temporary graphics
        /// </summary>
        /// <param name="cs"></param>
        public static void PaintCoordinateSystem(this CoordinateSystem cs)
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
        /// Gets rotated 3d coordinate system
        /// </summary>
        /// <param name="inputCs">Starting coordinate system</param>
        /// <param name="xValue">X rotation value</param>
        /// <param name="yValue">Y rotation value</param>
        /// <param name="zValue">Z rotation value</param>
        /// <returns>New rotated coordinate system</returns>
        public static CoordinateSystem GetRotatedCoordinateSystem(this CoordinateSystem inputCs, double xValue,
            double yValue, double zValue)
        {
            if (inputCs == null) throw new ArgumentNullException(nameof(inputCs));

            //Get rotation from current based on input
            var rotationX = MatrixFactory.Rotate(xValue * Math.PI / 180, inputCs.AxisX);
            var rotationY = MatrixFactory.Rotate(yValue * Math.PI / 180, inputCs.AxisY);
            var rotationZ = MatrixFactory.Rotate(zValue * Math.PI / 180, Vector.Cross(inputCs.AxisX, inputCs.AxisY));
            var rotation3D = rotationX * rotationY * rotationZ;

            //Get rotated system from original
            return new CoordinateSystem
            {
                Origin = inputCs.Origin,
                AxisX = new Vector(rotation3D.Transform(new Point(inputCs.AxisX))),
                AxisY = new Vector(rotation3D.Transform(new Point(inputCs.AxisY)))
            };
        }

        /// <summary>
        /// Gets new translated coordinate system by a vector
        /// </summary>
        /// <param name="cs">Original coordinate system</param>
        /// <param name="v">Translation vector</param>
        /// <returns>New transformed coordinate system</returns>
        public static CoordinateSystem Transform(this CoordinateSystem cs, Vector v)
        {
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            if (v == null) throw new ArgumentNullException(nameof(v));

            var origin = new Point(cs.Origin);
            TxPoint.Translate(origin, v);

            var p1 = new Point();
            var p2 = new Point(cs.AxisX);
            TxPoint.Translate(p1, v);
            TxPoint.Translate(p2, v);
            var axisX = new Vector(p2 - p1);

            var p3 = new Point();
            var p4 = new Point(cs.AxisY);
            TxPoint.Translate(p3, v);
            TxPoint.Translate(p4, v);
            var axisY = new Vector(p4 - p3);

            return new CoordinateSystem(origin, axisX, axisY);
        }
    }
}