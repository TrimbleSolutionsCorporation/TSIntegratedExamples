namespace JoistArea.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    /// <summary>
    /// Extensions class for Geometry3d Point
    /// </summary>
    public static class TxPoint
    {
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

        /// <summary>
        /// Gets vector between two points
        /// </summary>
        /// <param name="p1">Origin point</param>
        /// <param name="p2">Destination point</param>
        /// <returns>new Vector to point</returns>
        public static Vector GetVectorTo(this Point p1, Point p2) => new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);

        /// <summary>
        /// Translates point vector direction and distance
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="tVector"></param>
        public static void Translate(this Point pt, Vector tVector) => pt.Translate(tVector.X, tVector.Y, tVector.Z);

        /// <summary>
        /// Gets new point translated from current
        /// </summary>
        /// <param name="pt">Point of origin</param>
        /// <param name="v">Vector translation to move copy of point</param>
        /// <returns>New translated point</returns>
        public static Point GetTranslated(this Point pt, Vector v)
        {
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var tempPt = new Point(pt);
            tempPt.Translate(v.X, v.Y, v.Z);
            return tempPt;
        }

        /// <summary>
        /// Transforms points from current workplane to passed workplane
        /// </summary>
        /// <param name="points">Polygon to get points from</param>
        /// <param name="cs">Coordinate system to transform from</param>
        /// <returns>New list of points</returns>
        public static List<Point> TransformPointsToPlane(this IEnumerable points, CoordinateSystem cs)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (cs == null)
            {
                throw new ArgumentNullException("cs");
            }

            var result = new List<Point>();
            var transMatrix = MatrixFactory.FromCoordinateSystem(cs);
            foreach (Point pt in points)
            {
                result.Add(transMatrix.Transform(pt));
            }

            return result;
        }

        /// <summary>
        /// Transforms points from current workplane to coordinate system
        /// </summary>
        /// <param name="points">Points to transform</param>
        /// <param name="cs">Coordinate system to transform to</param>
        /// <returns>New points</returns>
        public static List<Point> TransformPointsFromPlane(this IEnumerable points, CoordinateSystem cs)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (cs == null)
            {
                throw new ArgumentNullException("cs");
            }

            var result = new List<Point>();
            var transMatrix = MatrixFactory.ToCoordinateSystem(cs);
            foreach (Point pt in points)
            {
                result.Add(transMatrix.Transform(pt));
            }

            return result;
        }

        /// <summary>
        /// Projects opints to plane
        /// </summary>
        /// <param name="points">Points</param>
        /// <param name="cs">Plane to project to</param>
        /// <returns>New points</returns>
        public static List<Point> ProjectPointsToPlane(this IEnumerable points, CoordinateSystem cs)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (cs == null)
            {
                throw new ArgumentNullException("cs");
            }

            var result = new List<Point>();
            var plane = new GeometricPlane(cs);
            foreach (Point pt in points)
            {
                result.Add(Projection.PointToPlane(pt, plane));
            }

            return result;
        }

        /// <summary>
        /// Inserts new layout point object
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="label"></param>
        public static void InsertLayoutPoint(this Point pt, string label = "")
        {
            if (pt == null)
            {
                return;
            }

            var componentInput = new ComponentInput();
            componentInput.AddOneInputPosition(pt);

            var component = new Component(componentInput)
            {
                Name = "LayoutPointPlugin",
                Number = -100000
            };

            component.SetAttribute("PointLabel", label);
            component.Insert();
        }

        /// <summary>
        /// Gets list of points form array
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Point> ToList(this ArrayList list)
        {
            var result = new List<Point>();
            foreach (var mo in list)
            {
                var pt = mo as Point;
                if (pt == null)
                {
                    continue;
                }

                result.Add(pt);
            }
            return result;
        }

        /// <summary>
        /// Transforms list of points to new coordinate system
        /// </summary>
        /// <param name="pts">Points to transform</param>
        /// <param name="transMatrix">Matrix transformation</param>
        /// <returns>New list of transformed points</returns>
        public static List<Point> Transform(this IEnumerable<Point> pts, Matrix transMatrix)
        {
            var result = new List<Point>();
            foreach (var ptx in pts)
            {
                var pt = new Point(ptx);
                var ptMoved = transMatrix.Transform(pt);
                result.Add(ptMoved);
            }
            return result;
        }
    }
}
