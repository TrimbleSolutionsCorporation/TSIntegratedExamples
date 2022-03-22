namespace JoistArea.Tools
{
    using System;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Extensions class for Geometry3d Vector
    /// </summary>
    public static class TxVector
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
        /// Check if is the same direction to other vector
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="angleTolerance">Angle tolerance (radians)</param>
        /// <returns>True if angle is close to 0</returns>
        public static bool IsSameDirection(this Vector v1, Vector v2, double angleTolerance = GeometryConstants.ANGULAR_EPSILON)
        {
            if (v2 == null) return false;
            var angle = v1.GetAngleBetween(v2) * 180 / Math.PI;
            return Math.Abs(angle) < angleTolerance;
        }

        /// <summary>
        /// Check if is the opposite direction to other vector
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="angleTolerance">Angle tolerance (radians)</param>
        /// <returns>True if angle is close to 180</returns>
        public static bool IsOppositeDirection(this Vector v1, Vector v2, double angleTolerance = GeometryConstants.ANGULAR_EPSILON)
        {
            if (v2 == null) return false;
            var angle = v1.GetAngleBetween(v2) * 180 / Math.PI;
            return Math.Abs(angle - 180) < angleTolerance;
        }

        /// <summary>
        /// Checks if vector is perpendicular
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="angleTolerance">Angle tolerance (radians)</param>
        /// <returns>True if perpendicular</returns>
        public static bool IsPerpendicular(this Vector v1, Vector v2, double angleTolerance = GeometryConstants.ANGULAR_EPSILON)
        {
            return Math.Abs(v1.Dot(v2) - 0) < angleTolerance;
        }

        /// <summary>
        /// Checks if vector is unit vector length
        /// </summary>
        /// <param name="v1">Vector to check</param>
        /// <returns>True if already unit vector</returns>
        public static bool IsUnitVector(Vector v1)
        {
            return Math.Abs(v1.GetLength() - 1) < GeometryConstants.DISTANCE_EPSILON;
        }

        /// <summary>
        /// Gets new Point from Vector
        /// </summary>
        /// <param name="tVector">Vector to use</param>
        /// <returns>New Geometry3d Point</returns>
        public static Point ToPoint(this Vector tVector)
        {
            return new Point(tVector.X, tVector.Y, tVector.Z);
        }

        /// <summary>
        /// Transforms a vector to a different Coordinate System using matrix
        /// </summary>
        /// <param name="v">Vector to use</param>
        /// <param name="m">Matrix to use</param>
        /// <returns>Multiplier vector</returns>
        public static Vector Mutiplier(this Vector v, Matrix m)
        {
            var dx = v.X * m[0, 0] + v.Y * m[1, 0] + v.Z * m[2, 0] + 1 * m[3, 0];
            var dy = v.X * m[0, 1] + v.Y * m[1, 1] + v.Z * m[2, 1] + 1 * m[3, 1];
            var dz = v.X * m[0, 2] + v.Y * m[1, 2] + v.Z * m[2, 2] + 1 * m[3, 2];
            return new Vector(dx, dy, dz);
        }

        /// <summary>
        /// Gives translated vector back
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector Translate(this Vector v1, Vector v2)
        {
            var result = new Vector(v1);
            result.X = v2.X * result.X;
            result.Y = v2.Y * result.Y;
            result.Z = v2.Z * result.Z;
            return result;
        }

        /// <summary>
        /// Rounds vector to five decimal places for each leg
        /// </summary>
        /// <param name="vector">Vector that needs rounding</param>
        /// <param name="decimalPlaces">How many places past zero to round to</param>
        /// <returns>Resulting rounded vector</returns>
        public static Vector Round(this Vector vector, int decimalPlaces = 5)
        {
            var result = new Vector
            {
                X = Math.Round(vector.X, decimalPlaces),
                Y = Math.Round(vector.Y, decimalPlaces),
                Z = Math.Round(vector.Z, decimalPlaces)
            };
            return result;
        }

        /// <summary>
        /// Creates small line segment graphic in model view to represent point
        /// </summary>
        /// <param name="v"></param>
        /// <param name="color"></param>
        public static void Paint(this Vector v, Color color = null)
        {
            if (color == null) color = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();
            var ln = new LineSegment(new Point(0, 0, 0), new Point(v));
            gd.DrawLineSegment(ln, color);
        }

        /// <summary>
        /// Prints vector information to global log and output/console
        /// </summary>
        /// <param name="v">Vector to get info from</param>
        /// <param name="header">Header text for this block</param>
        public static void Print(this Vector v, string header = "Vector")
        {
            Trace.WriteLine(header);
            Trace.WriteLine(string.Format("{0}: ( X:{1}:0.000, Y:{2}:0.000, Z:{3}:0.000 )", header, v.X, v.Y, v.Z));
        }
    }
}
