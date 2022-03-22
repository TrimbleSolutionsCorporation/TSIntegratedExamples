namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;
    using Tekla.Structures.Solid;

    /// <summary>
    /// Geometry3d Face class extensions
    /// </summary>
    public static class TxFace
    {
        /// <summary>
        /// Gets Face origin center point
        /// Only works for Rectangles so far
        /// </summary>
        /// <param name="tFace">Tekla.Structures.Solid.Face to get center of</param>
        /// <returns>Center of face</returns>
        public static Point GetFaceOrigin(this Face tFace)
        {
            if (tFace == null) throw new ArgumentNullException(nameof(tFace));
            return GetCenterOfPolygon(tFace.GetPoints());
        }

        /// <summary>
        /// Gets infinite geometric plane from solid face
        /// </summary>
        /// <param name="fc">Solid finite face</param>
        /// <returns>Geometric plane</returns>
        public static GeometricPlane ToPlane(this Face fc)
        {
            return new GeometricPlane(fc.GetFaceOrigin(), fc.Normal);
        }

        public static bool ContainsPoint(this Face fc, Point pt,
            double tolerance = GeometryConstants.DISTANCE_EPSILON)
        {
            var gp = ToPlane(fc);
            var dr = Distance.PointToPlane(pt, gp);
            return !(dr > tolerance);
        }

        /// <summary>
        /// Finds center of rectangular plane
        /// </summary>
        /// <param name="pointCollection"></param>
        /// <returns>Center point of plane</returns>
        private static Point GetCenterOfPolygon(this ICollection<Point> pointCollection)
        {
            if (pointCollection == null) throw new ArgumentNullException();
            if (pointCollection.Count < 3) throw new ApplicationException("Method failed, send list of points less than 3, not valid polygon");

            double dx = 0, dy = 0, dz = 0;
            foreach (var pt in pointCollection)
            {
                dx += pt.X;
                dy += pt.Y;
                dz += pt.Z;
            }
            return new Point(dx / pointCollection.Count, dy / pointCollection.Count, dz / pointCollection.Count);
        }

        /// <summary>
        /// Returns outer edge (ignores cut loops) into list of linesegments
        /// </summary>
        /// <param name="face">Solid face</param>
        /// <returns>Empty list if failed, new list of line segments otherwise</returns>
        public static List<LineSegment> GetFaceOuterSegments(this Face face)
        {
            if (face == null) throw new ArgumentNullException();
            var result = new List<LineSegment>();

            var loops = face.GetLoopEnumerator();
            while (loops.MoveNext())
            {
                //Get loops
                var outsideLoop = loops.Current as Loop;
                if (outsideLoop == null) continue;
                var vertices = outsideLoop.GetVertexEnumerator();

                //Enumerate loops
                var usablePoints = new List<Point>();
                while (vertices.MoveNext()) usablePoints.Add(vertices.Current as Point);
                for (var i = 1; i < usablePoints.Count; i++)
                {
                    result.Add(new LineSegment(usablePoints[i - 1], usablePoints[i]));
                    if (i == usablePoints.Count - 1) result.Add(new LineSegment(usablePoints[i], usablePoints[0]));
                }
                break; //Only use first loop on face
            }
            return result;
        }

        /// <summary>
        /// Gets points from face
        /// </summary>
        /// <param name="fc">Face to query</param>
        /// <param name="ignoreCuts">True to ignore cut loops</param>
        /// <returns>List of TS type points</returns>
        public static List<Point> GetPoints(this Face fc, bool ignoreCuts = true)
        {
            //Get points from outer face
            var result = new List<Point>();

            //Enumerate faces
            var loops = fc.GetLoopEnumerator();
            while (loops.MoveNext())
            {
                //Get loops
                var loop = loops.Current as Loop;
                if (loop == null) continue;
                var vertexEnum = loop.GetVertexEnumerator();
                if (vertexEnum == null) continue;

                //Get usable points
                while (vertexEnum.MoveNext()) result.Add(vertexEnum.Current as Point);

                //Use first loop only if set to ignore cut loops
                if (ignoreCuts) break;
            }
            return result;
        }

        /// <summary>
        /// Gets 3d coordinate system of face plane using first point as x dir
        /// </summary>
        /// <param name="fc">3d Face</param>
        /// <returns>new Coordinate System on place of face centered</returns>
        public static CoordinateSystem GetCoordinateSystem(this Face fc)
        {
            var points = fc.GetPoints();
            var origin = fc.GetFaceOrigin();
            var xVector = new Vector(origin.GetVectorTo(points[0])).GetNormal();
            var zVector = fc.Normal;
            var yVector = Vector.Cross(zVector, xVector);
            return new CoordinateSystem(origin, xVector, yVector);
        }

        /// <summary>
        /// Paints outer segment lines of face
        /// </summary>
        /// <param name="fc">Face to paint</param>
        /// <param name="color">Optional color parameter</param>
        public static void PaintPerimeterLines(this Face fc, Color color = null)
        {
            var lines = fc.GetFaceOuterSegments();
            foreach (var lineSegment in lines) lineSegment.PaintLine(color);
        }
    }
}
