namespace JoistAreaFeatures.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    public static class TxPolygon
    {
        /// <summary>
        /// Gets point from polygon in usable list
        /// </summary>
        /// <param name="pg"></param>
        /// <returns></returns>
        public static List<Point> GetPoints(this Polygon pg)
        {
            if (pg == null) throw new ArgumentNullException(nameof(pg));
            return (from object pt in pg.Points select pt as Point).ToList();
        }

        /// <summary>
        /// Creates new geometric plane from a polygon
        /// </summary>
        /// <param name="polygon">Polygon</param>
        /// <returns>New geometric plane represented by polygon</returns>
        /// <exception cref="ArgumentNullException">Polygon must Not be null</exception>
        /// <exception cref="IndexOutOfRangeException">Polygon.Points.Count must Not be less than 3</exception>
        public static GeometricPlane CreatePlaneFromPolygon(this Polygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (polygon.Points.Count < 3) throw new IndexOutOfRangeException("polygon must have at least three points");

            var p0 = (Point)polygon.Points[1];
            var p1 = (Point)polygon.Points[0];
            var p2 = (Point)polygon.Points[2];
            var v1 = new Vector(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var refAxis = new Vector(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            var normal = Vector.Cross(v1, refAxis);
            var realx = Vector.Cross(normal, v1);//.Normalize();
            return new GeometricPlane(p0, v1.GetNormal(), realx.GetNormal());
        }
    }
}
