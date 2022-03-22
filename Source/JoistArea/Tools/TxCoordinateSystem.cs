namespace JoistArea.Tools
{
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
            return Vector.Cross(cs.AxisX, cs.AxisY).GetNormal();
        }

        /// <summary>
        /// Paints coordinate system in the model with temporary graphics
        /// </summary>
        /// <param name="cs"></param>
        [Conditional("DEBUG")]
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
            endX.Translate(cs.AxisX.GetNormal() * vScalar);
            endY.Translate(cs.AxisY.GetNormal() * vScalar);
            endZ.Translate(Vector.Cross(cs.AxisX, cs.AxisY).GetNormal() * vScalar);

            //Draw result lines from origin
            graphicsDrawer.DrawLineSegment(cs.Origin, endX, red);
            graphicsDrawer.DrawLineSegment(cs.Origin, endY, green);
            graphicsDrawer.DrawLineSegment(cs.Origin, endZ, blue);
        }

        /// <summary>
        /// Gets new translated coordinate system by a vector
        /// </summary>
        /// <param name="cs">Original coordinate system</param>
        /// <param name="v">Translation vector</param>
        /// <returns>New transformed coordinate system</returns>
        public static CoordinateSystem Transform(this CoordinateSystem cs, Vector v)
        {
            var origin = new Point(cs.Origin);
            origin.Translate(v);

            var p1 = new Point();
            var p2 = new Point(cs.AxisX);
            p1.Translate(v);
            p2.Translate(v);
            var axisX = new Vector(p2 - p1);

            var p3 = new Point();
            var p4 = new Point(cs.AxisY);
            p3.Translate(v);
            p4.Translate(v);
            var axisY = new Vector(p4 - p3);

            return new CoordinateSystem(origin, axisX, axisY);
        }

    }
}
