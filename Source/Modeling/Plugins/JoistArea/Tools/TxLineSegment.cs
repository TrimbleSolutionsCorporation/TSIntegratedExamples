namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Extensions class for LineSegment
    /// </summary>
    public static class TxLineSegment
    {
        /// <summary>
        /// Gets midpoint of a line segment
        /// </summary>
        /// <param name="ls">Tekla line segment</param>
        /// <returns>New 3d point at midpoint</returns>
        public static Point GetMidPoint(this LineSegment ls)
        {
            if (ls == null) throw new ApplicationException();
            var startPoint = new Point(ls.Point1);
            var displacement = ls.GetDirectionVector().GetNormal() * ls.Length() * 0.5;
            startPoint.Translate(displacement.X, displacement.Y, displacement.Z);
            return startPoint;
        }

        /// <summary>
        /// Translates existing line segment by 3d matrix
        /// </summary>
        /// <param name="ls">Line segment to translate</param>
        /// <param name="matrix">Matrix transformation</param>
        public static LineSegment Transform(this LineSegment ls, Matrix matrix)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));

            var p1 = matrix.Transform(new Point(ls.Point1));
            var p2 = matrix.Transform(new Point(ls.Point2));
            return new LineSegment(p1, p2);
        }

        /// <summary>
        /// Gets sequence of line segments from point lists
        /// </summary>
        /// <param name="ptList">Sequential point list to interpolate</param>
        /// <returns>Strong type list of line segments formed by stringing together points in current ucs</returns>
        public static IEnumerable<LineSegment> GetLineSegmentsFromPoints(this List<Point> ptList)
        {
            if (ptList == null) throw new ArgumentNullException();
            return ptList.Select((t, i) => i == ptList.Count - 1 ? 
                new LineSegment(t, ptList[0]) : new LineSegment(t, ptList[i + 1])).ToList();
        }

        /// <summary>
        /// Paints temporary line in the model, Debug model only
        /// </summary>
        /// <param name="ls">Line segment to paint</param>
        /// <param name="col">Color to use, red if skipped</param>
        [Conditional("DEBUG")]
        public static void PaintLine(this LineSegment ls, Color col = null)
        {
            if (col == null) col = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();
            gd.DrawLineSegment(ls.Point1, ls.Point2, col);
        }

        /// <summary>
        /// Paints temporary line in the model, Debug model only
        /// </summary>
        /// <param name="ls">Line segment to paint</param>
        /// <param name="col">Color to use, red if skipped</param>
        public static void ReleasePaintLine(this LineSegment ls, Color col = null)
        {
            if (col == null) col = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();
            gd.DrawLineSegment(ls.Point1, ls.Point2, col);
        }
    }
}
