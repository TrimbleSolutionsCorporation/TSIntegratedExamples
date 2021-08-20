namespace JoistAreaFeatures.Tools
{
    using System;
    using System.Collections.Generic;
    using Tekla.Structures.Geometry3d;

    public static class TxLineSegment
    {
        public static List<LineSegment> GetFromListByIndex(this List<LineSegment> list, int startIndex)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (startIndex < 0 || startIndex > list.Count - 1) return null;

            var result = new List<LineSegment>();
            for (var i = startIndex; i < list.Count; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }

        public static bool IsPointOnLineSegment(this LineSegment ls, Point pt, double tolerance = GeometryConstants.DISTANCE_EPSILON)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            //Check if on line first
            var dx = (pt.X - ls.Point1.X) / (ls.Point2.X - ls.Point1.X);
            var dy = (pt.Y - ls.Point1.Y) / (ls.Point2.Y - ls.Point1.Y);
            var dz = (pt.Z - ls.Point1.Z) / (ls.Point2.Z - ls.Point1.Z);
            if (Math.Abs(dx - dy) > tolerance) return false;
            if (Math.Abs(dy - dz) > tolerance) return false;
            if (Math.Abs(dx - dz) > tolerance) return false;

            //Check if on segment boundary
            if (pt.X < ls.Point1.X || pt.X > ls.Point2.X) return false;
            if (pt.Y < ls.Point1.Y || pt.Y > ls.Point2.Y) return false;
            if (pt.Z < ls.Point1.Z || pt.Z > ls.Point2.Z) return false;
            return true;
        }

        public static bool IsPointOnLine(this LineSegment ls, Point pt, double tolerance = GeometryConstants.DISTANCE_EPSILON)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            //Check if on line first
            var dx = (pt.X - ls.Point1.X) / (ls.Point2.X - ls.Point1.X);
            var dy = (pt.Y - ls.Point1.Y) / (ls.Point2.Y - ls.Point1.Y);
            var dz = (pt.Z - ls.Point1.Z) / (ls.Point2.Z - ls.Point1.Z);
            if (Math.Abs(dx - dy) > tolerance) return false;
            if (Math.Abs(dy - dz) > tolerance) return false;
            if (Math.Abs(dx - dz) > tolerance) return false;
            return true;
        }
    }
}
