namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;

    /// <summary>
    /// Extension of internal Matrix
    /// </summary>
    public static class TxMatrix
    {
        /// <summary>
        /// Transforms vector by matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector Transform(this Matrix m, Vector v)
        {
            if (m == null || v == null) throw new ArgumentNullException();
            var p1 = m.Transform(new Point());
            var p2 = m.Transform(new Point(v.X, v.Y, v.Z));
            return new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        }

        /// <summary>
        /// Transforms coordinate sytem using matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static CoordinateSystem Transform(this Matrix m, CoordinateSystem cs)
        {
            if (m == null || cs == null)
                throw new ArgumentNullException();
            var origin = new Point(cs.Origin);
            var xAxis = new Vector(cs.AxisX);
            var yAxis = new Vector(cs.AxisY);

            var newOrigin = m.Transform(origin);
            var newXaxis = Transform(m, xAxis);
            var newYaxis = Transform(m, yAxis);
            return new CoordinateSystem(newOrigin, newXaxis, newYaxis);
        }

    }
}
