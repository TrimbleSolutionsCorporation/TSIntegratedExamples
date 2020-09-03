namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    public static class TxLineSegment
    {
        /// <summary>
        /// Translates both points of line segment by vector
        /// </summary>
        /// <param name="ls">Original line segment</param>
        /// <param name="v">Translation vector</param>
        /// <returns>New translated line segment</returns>
        public static LineSegment Translate(this LineSegment ls, Vector v)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (v == null) throw new ArgumentNullException(nameof(v));

            var p1 = new Point(ls.Point1);
            var p2 = new Point(ls.Point2);
            p1.Translate(v);
            p2.Translate(v);
            return new LineSegment(p1, p2);
        }

        /// <summary>
        /// Gets midpoint of a line segment
        /// </summary>
        /// <param name="ls">Tekla line segment</param>
        /// <returns>New 3d point at midpoint</returns>
        public static Point GetMidPoint(this LineSegment ls)
        {
            if (ls == null) throw new ApplicationException(nameof(ls));

            var startPoint = new Point(ls.Point1);
            var displacement = ls.GetDirectionVector().GetNormal() * ls.Length() * 0.5;
            startPoint.Translate(displacement.X, displacement.Y, displacement.Z);
            return startPoint;
        }

        /// <summary>
        /// Gets the angle between two line segments in 
        /// Uses arch tangent between point differential
        /// </summary>
        /// <param name="ls1">Line segment 1</param>
        /// <param name="ls2">Line segment 2</param>
        /// <returns>Angle in radians</returns>
        public static double GetAngleBetweenSegments(this LineSegment ls1, LineSegment ls2)
        {
            if (ls1 == null) throw new ArgumentNullException(nameof(ls1));
            if (ls2 == null) throw new ArgumentNullException(nameof(ls2));

            var p1 = ls1.Point1;
            var p2 = ls1.Point2;
            var p3 = ls2.Point1;
            var p4 = ls2.Point2;

            var angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) - Math.Atan2(p4.Y - p3.Y, p4.X - p3.X);
            return angle;
        }

        /// <summary>
        /// Gets the angle between two line segments in
        /// Uses TS internal methods
        /// </summary>
        /// <param name="ls1">Line segment 1</param>
        /// <param name="ls2">Line segment 2</param>
        /// <returns>Angle in radians</returns>
        public static double GetAngleBetween(this LineSegment ls1, LineSegment ls2)
        {
            if (ls1 == null) throw new ArgumentNullException(nameof(ls1));
            if (ls2 == null) throw new ArgumentNullException(nameof(ls2));

            return ls1.GetDirectionVector().GetAngleBetween(ls2.GetDirectionVector());
        }

        /// <summary>
        /// Extrapolates a list of segments from points
        /// </summary>
        /// <param name="points">List of Tekla geometry points</param>
        /// <returns>Strong typed list of line segments</returns>
        public static List<LineSegment> GetLineSegmentsFromPoints(this IList<Point> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2) return new List<LineSegment>();

            var segmentList = new List<LineSegment>();
            var lastPoint = points[0];
            var noPoints = points.Count;
            for (var i = 1; i < noPoints; i++)
            {
                var thisPoint = points[i];
                segmentList.Add(new LineSegment(lastPoint, thisPoint));
                lastPoint = thisPoint;
            }

            return segmentList;
        }

        /// <summary>
        /// Translates existing line segment by 3d matrix
        /// </summary>
        /// <param name="ls">Line segment to translate</param>
        /// <param name="matrix">Matrix transformation</param>
        public static void Translate(this LineSegment ls, Matrix matrix)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));

            var fp1 = new Point(ls.Point1);
            var fp2 = new Point(ls.Point2);
            ls.Point1 = matrix.Transform(fp1);
            ls.Point2 = matrix.Transform(fp2);
        }

        /// <summary>
        /// Gets sequence of line segments from point lists
        /// </summary>
        /// <param name="ptList">Sequential point list to interpolate</param>
        /// <returns>Strong type list of line segments formed by stringing together points in current ucs</returns>
        public static IEnumerable<LineSegment> GetLineSegmentsFromPoints(this List<Point> ptList)
        {
            if (ptList == null) throw new ArgumentNullException(nameof(ptList));

            return ptList.Select((t, i) =>
                i == ptList.Count - 1 ? new LineSegment(t, ptList[0]) : new LineSegment(t, ptList[i + 1])).ToList();
        }

        /// <summary>
        /// Gets intersections points of two Polylines in 3d space
        /// </summary>
        /// <param name="lineS1">LineSegment</param>
        /// <param name="lineS2">LineSegment</param>
        /// <param name="extrudedTolerance">Percent distance first Polyline is extended before comparison</param>
        /// <returns>New list of points, zero count if none found</returns>
        public static List<Point> IntersectElongated(this LineSegment lineS1, LineSegment lineS2,
            int extrudedTolerance = 10)
        {
            var results = new List<Point>();
            if (lineS1 == null) throw new ArgumentNullException();
            var extLine = lineS2.GetElongated(lineS2.Length() * (1 + extrudedTolerance / 100));
            var intersectLine = TxLineSegment.Intersect(lineS1, extLine);
            if (intersectLine == null || Math.Abs(intersectLine.Length()) > GeometryConstants.DISTANCE_EPSILON)
                return results;
            results.Add(intersectLine.Point1);
            return results;
        }

        /// <summary>
        /// Gets intersection of two Line objects in local coordinate system
        /// </summary>
        /// <param name="ls1">First line to compare</param>
        /// <param name="ls2">Second line to compare</param>
        /// <returns>Intersection of two lines = LineSegment, or null if sent null parameters</returns>
        public static LineSegment Intersect(this LineSegment ls1, LineSegment ls2)
        {
            if (ls1 == null || ls2 == null) throw new ArgumentNullException();
            var l1 = new Line(ls1.Point1, ls1.Point2);
            var l2 = new Line(ls2.Point1, ls2.Point2);
            return Intersection.LineToLine(l1, l2);
        }

        /// <summary>
        /// Checks for intersection with line segment
        /// </summary>
        /// <param name="ls1">LineSegment 1</param>
        /// <param name="ls2">LineSegment 2</param>
        /// <param name="tolerance">Tolerance used in checking, default from GeometryConstants</param>
        /// <returns>New LineSegment for 3d intersection of closed distance between two line segments</returns>
        public static LineSegment IntersectLineSegment(this LineSegment ls1, LineSegment ls2,
            double tolerance = GeometryConstants.DISTANCE_EPSILON)
        {
            var p1 = ls1.Point1;
            var p2 = ls1.Point2;
            var p3 = ls2.Point1;
            var p4 = ls2.Point2;
            var p13 = p1 - p3;
            var p43 = p4 - p3;

            if (p43.LengthSq() < GeometryConstants.DISTANCE_EPSILON) return null;
            var p21 = p2 - p1;
            if (p21.LengthSq() < GeometryConstants.DISTANCE_EPSILON) return null;

            var d1343 = p13.X * p43.X + p13.Y * p43.Y + p13.Z * p43.Z;
            var d4321 = p43.X * p21.X + p43.Y * p21.Y + p43.Z * p21.Z;
            var d1321 = p13.X * p21.X + p13.Y * p21.Y + p13.Z * p21.Z;
            var d4343 = p43.X * p43.X + p43.Y * p43.Y + p43.Z * p43.Z;
            var d2121 = p21.X * p21.X + p21.Y * p21.Y + p21.Z * p21.Z;

            var denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < GeometryConstants.DISTANCE_EPSILON) return null;

            var numer = d1343 * d4321 - d1321 * d4343;
            var mua = numer / denom;
            var mub = (d1343 + d4321 * (mua)) / d4343;

            var resultP1 = new Point(p1.X + mua * p21.X, p1.Y + mua * p21.Y, p1.Z + mua * p21.Z);
            var resultP2 = new Point(p3.X + mub * p43.X, p3.Y + mub * p43.Y, p3.Z + mub * p43.Z);

            //Check if points are on a line
            var dx1 = Distance.PointToLineSegment(resultP1, ls1);
            var dx2 = Distance.PointToLineSegment(resultP1, ls2);
            var dy1 = Distance.PointToLineSegment(resultP2, ls1);
            var dy2 = Distance.PointToLineSegment(resultP2, ls2);

            if (dx1 > tolerance || dx2 > tolerance) return null;
            if (dy1 > tolerance || dy2 > tolerance) return null;
            return new LineSegment(resultP1, resultP2);
        }

        ///<summary>
        /// Prints points and line segment information to logger
        ///</summary>
        ///<param name="lg">Geometry line segment to print</param>
        ///<param name="headerText">Header text to add</param>
        public static void Print(this LineSegment lg, string headerText)
        {
            if (lg == null) return;
            if (string.IsNullOrEmpty(headerText)) headerText = "Line Segment Print";
            Debug.WriteLine("======================" + headerText + "===================");
            Debug.WriteLine(string.Format("P1x:{0}\tP1y:{1}\tP1z:{2}\nP2x:{3}\tP2y:{4}\tP2z:{5}",
                Math.Round(lg.Point1.X, 5),
                Math.Round(lg.Point1.Y, 5),
                Math.Round(lg.Point1.Z, 5),
                Math.Round(lg.Point2.X, 5),
                Math.Round(lg.Point2.Y, 5),
                Math.Round(lg.Point2.Z, 5)));
            Debug.WriteLine("===================================================");
            Debug.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// Paints temporary line in the model, Debug model only
        /// </summary>
        /// <param name="ls">Line segment to paint</param>
        /// <param name="col">Color to use, red if skipped</param>
        public static void PaintLine(this LineSegment ls, Color col = null)
        {
            if (col == null) col = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();
            gd.DrawLineSegment(ls.Point1, ls.Point2, col);
        }

        /// <summary>
        /// Paints temporary line in the model, Debug mode only
        /// </summary>
        /// <param name="p1">First point on line</param>
        /// <param name="p2">Last point on line</param>
        /// <param name="col">Color to use, red if skipped</param>
        public static void PaintLine(Point p1, Point p2, Color col = null)
        {
            if (col == null) col = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();
            gd.DrawLineSegment(new LineSegment(p1, p2), col);
        }

        /// <summary>
        /// Lengthens segment in 3d space at each end by value
        /// </summary>
        /// <param name="ls1">Line segment to get original point values from</param>
        /// <param name="distance">mm value</param>
        /// <returns>New line segment with new points</returns>
        public static LineSegment GetElongated(this LineSegment ls1, double distance)
        {
            var p1 = new Point(ls1.Point1);
            var p2 = new Point(ls1.Point2);
            p1.Translate(ls1.GetDirectionVector() * -distance);
            p2.Translate(ls1.GetDirectionVector() * distance);
            return new LineSegment(p1, p2);
        }

        /// <summary>
        /// Checks by measuring points if list of line segments contains the same segment as passed segment
        /// </summary>
        /// <param name="candidates">List of line segment to check against</param>
        /// <param name="ls1">Segment to check with</param>
        /// <param name="mmTolerance">Allowed tolerance between points</param>
        /// <returns>True if segment in list matches passed arg</returns>
        public static bool ContainsSegment(this IEnumerable<LineSegment> candidates, LineSegment ls1,
            double mmTolerance = 0.01)
        {
            if (candidates == null) throw new ArgumentNullException("candidates");
            if (ls1 == null) throw new ArgumentNullException("ls1");

            foreach (var ls in candidates)
            {
                if ((Distance.PointToPoint(ls1.Point1, ls.Point1) < mmTolerance) &&
                    (Distance.PointToPoint(ls1.Point2, ls.Point2) < mmTolerance)) return true;
            }

            return false;
        }

        /// <summary>
        /// Compares 2 LineSegments to see if they are the same in 3d (length and point location)
        /// </summary>
        /// <param name="ls">First LineSegment</param>
        /// <param name="cl">Second LineSegment</param>
        /// <returns>True if same LineSegment</returns>
        public static bool IsSame(this LineSegment ls, LineSegment cl)
        {
            if (ls == null) throw new ArgumentNullException("ls");
            if (cl == null) throw new ArgumentNullException("cl");
            var d1 = Math.Round(Math.Abs(ls.Point1.Distance(cl.Point1)), 2);
            var d2 = Math.Round(Math.Abs(ls.Point2.Distance(cl.Point2)), 2);
            return d1 < GeometryConstants.DISTANCE_EPSILON && d2 < GeometryConstants.DISTANCE_EPSILON;
        }
    }
}