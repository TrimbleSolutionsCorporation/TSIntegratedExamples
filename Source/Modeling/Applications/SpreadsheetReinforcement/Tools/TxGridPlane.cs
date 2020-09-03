namespace SpreadsheetReinforcement.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    /// <summary>
    /// Extensions class for grid plane
    /// </summary>
    public static class TxGridPlane
    {
        /// <summary>
        /// Gets first point along grid line
        /// </summary>
        /// <param name="gp">Grid plane</param>
        /// <returns>new Point</returns>
        public static Point GetFirstPoint(this GridPlane gp)
        {
            if (gp == null) throw new ArgumentNullException(nameof(gp));

            var origin = gp.Plane.Origin;
            var vx = gp.Plane.AxisX;
            return new Point(origin.X, origin.Y, vx.Z);
        }

        /// <summary>
        /// Gets second point along grid line
        /// </summary>
        /// <param name="gp">Grid plane</param>
        /// <returns>new Point</returns>
        public static Point GetSecondPoint(this GridPlane gp)
        {
            if (gp == null) throw new ArgumentNullException(nameof(gp));

            var tempPoint = GetFirstPoint(gp);
            var vx = gp.Plane.AxisX;
            tempPoint.Translate(vx.X, vx.Y, vx.Z);
            return tempPoint;
        }

        /// <summary>
        /// Determines if Grid plane is along elevation=0 in model
        /// </summary>
        /// <param name="gp">Grid Plane</param>
        /// <returns>True if along zero elevation</returns>
        public static bool IsZeroElevation(this GridPlane gp)
        {
            if (gp == null) throw new ArgumentNullException(nameof(gp));

            var upAngle = Math.Abs(gp.Plane.AxisY.GetAngleBetween(new Vector(0, 0, 1)) * 180 / Math.PI);
            if (upAngle < 5 || upAngle > 350) return true;
            return false;
        }

        /// <summary>
        /// Returns LineSegment from grid plane, ignores extensions
        /// </summary>
        /// <param name="gp">Model GridPlane</param>
        /// <returns>New LineSegment</returns>
        public static LineSegment GetLineSegment(this GridPlane gp)
        {
            if (gp == null) throw new ArgumentNullException(nameof(gp));
            return new LineSegment(GetFirstPoint(gp), GetSecondPoint(gp));
        }

        /// <summary>
        /// Gets intersection point of two different grid planes in 3d space
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ap"></param>
        /// <param name="tolerance">Distance tolerance in comparison of points</param>
        /// <returns></returns>
        public static Point Intersect(this GridPlane cp, GridPlane ap,
            double tolerance = GeometryConstants.DISTANCE_EPSILON)
        {
            if (cp == null) throw new ArgumentNullException(nameof(cp));
            if (ap == null) throw new ArgumentNullException(nameof(ap));

            var intersectLine = cp.GetLineSegment().IntersectLineSegment(ap.GetLineSegment(), tolerance);
            if (intersectLine == null) return null;
            return intersectLine.Length() > tolerance ? null : intersectLine.Point1;
        }
    }
}