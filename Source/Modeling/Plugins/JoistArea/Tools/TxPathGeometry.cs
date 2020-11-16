namespace JoistArea.Tools
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.ModelInternal;

    public static class TxPathGeometry
    {
        /// <summary>
        /// Gets WPF pathgeometry from polygon points
        /// </summary>
        /// <param name="points">Closed polygon</param>
        /// <returns>New pathgeometry with Figures set</returns>
        public static PathGeometry PolygonToWpfPolygon(IList<Point> points)
        {
            var paths = new List<PathSegment>();
            foreach (var point in points) paths.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(point.X, point.Y), false));

            var figures = new List<PathFigure>
            {
                new PathFigure(new System.Windows.Point(points[points.Count - 1].X, points[points.Count - 1].Y), paths, true)
            };

            var result = new PathGeometry(figures);
            return result;
        }

        /// <summary>
        /// Gets pointlist from WPF PathGeometry polygons
        /// </summary>
        /// <param name="polygon">Path geometry</param>
        /// <returns>New list of points for polygon</returns>
        public static PointList WpfPolygonToPointList(PathGeometry polygon)
        {
            if (polygon == null) return null;
            var result = new PointList();
            Point outerLoopLastPoint = null;
            Point outerLoopFirstPoint = null;
            var outerLoop = true;

            if (polygon.Figures == null) return null;
            foreach (var figure in polygon.Figures)
            {
                Point innerLoopFirstPoint = null;
                foreach (var path in figure.Segments)
                {
                    if (path is System.Windows.Media.LineSegment)
                    {
                        result.Add(new Point((path as System.Windows.Media.LineSegment).Point.X, (path as System.Windows.Media.LineSegment).Point.Y));
                        if (innerLoopFirstPoint == null)
                            innerLoopFirstPoint = new Point((path as System.Windows.Media.LineSegment).Point.X, (path as System.Windows.Media.LineSegment).Point.Y);

                        if (!outerLoop) continue;
                        if (outerLoopFirstPoint == null)
                            outerLoopFirstPoint = new Point((path as System.Windows.Media.LineSegment).Point.X, (path as System.Windows.Media.LineSegment).Point.Y);

                        outerLoopLastPoint = new Point((path as System.Windows.Media.LineSegment).Point.X, (path as System.Windows.Media.LineSegment).Point.Y);
                    }
                    else if (path is PolyLineSegment)
                    {
                        foreach (var point in (path as PolyLineSegment).Points)
                        {
                            result.Add(new Point(point.X, point.Y));

                            if (innerLoopFirstPoint == null)
                                innerLoopFirstPoint = new Point(new Point(point.X, point.Y));

                            if (!outerLoop) continue;
                            if (outerLoopFirstPoint == null)
                                outerLoopFirstPoint = new Point(new Point(point.X, point.Y));

                            outerLoopLastPoint = new Point(new Point(point.X, point.Y));
                        }
                    }
                }

                if (!outerLoop)
                {
                    result.Add(innerLoopFirstPoint);
                    result.Add(outerLoopLastPoint);
                }
                outerLoop = false;
            }

            if (result.Count < 1 || outerLoopLastPoint == null) return null;
            result.Add(result[0]);
            result.Add(outerLoopLastPoint);
            return result;
        }
    }
}
