namespace DrawingRectangle
{
    using System;
    using Tekla.Structures.Geometry3d;

    public static class TxExtensions
    {
        /// <summary>
        /// Gets midpoint of a line segment
        /// </summary>
        /// <param name="ls">Tekla line segment</param>
        /// <returns>New 3d point at midpoint</returns>
        public static Point GetMidPoint(this LineSegment ls)
        {
            if(ls == null)
                throw new ApplicationException();
            var startPoint = new Point(ls.Point1);
            var displacement = ls.GetDirectionVector().GetNormal() * ls.Length() * 0.5;
            startPoint.Translate(displacement.X, displacement.Y, displacement.Z);
            return startPoint;
        }
    }
}
