using System;
using System.Collections;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

namespace AnchorBoltsSimple.Tools
{
    /// <summary>
    /// General class extension methods
    /// </summary>
    public static class TxExtensions
    {
        /// <summary>
        /// Check if is parallel to other vector
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="angleTolerance">Tolerance in degree to check parallel</param>
        /// <returns>True if angle is close to 0 or 180</returns>
        public static bool IsParallel(this Vector v1, Vector v2, double angleTolerance = 0.0)
        {
            if (v2 == null) return false;
            if (Math.Abs(angleTolerance) < 0.001) angleTolerance = GeometryConstants.ANGULAR_EPSILON * 180 / Math.PI;
            var angle = Math.Abs(v1.GetAngleBetween(v2) * 180 / Math.PI);
            if (angle < angleTolerance) return true;
            return Math.Abs(angle - 180) < angleTolerance;
        }

        /// <summary>
        /// Creates new PointList with new points copied from existing list
        /// </summary>
        /// <param name="points">Existing points to copy</param>
        /// <returns>New list with copied points</returns>
        public static PointList CopyTo(this IEnumerable points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var result = new PointList();
            foreach (Point pt in points) result.Add(new Point(pt));
            return result;
        }

        /// <summary>
        /// Rounds each part of a Point to number of decimal places
        /// </summary>
        /// <param name="pt">Point to round</param>
        /// <param name="rndPlaces">Number of places to round to</param>
        /// <returns>Rounded 3d Point</returns>
        public static Point Round(this Point pt, int rndPlaces = 2)
        {
            var dx = Math.Round(pt.X, rndPlaces);
            var dy = Math.Round(pt.Y, rndPlaces);
            var dz = Math.Round(pt.Z, rndPlaces);
            return new Point(dx, dy, dz);
        }
    }
}