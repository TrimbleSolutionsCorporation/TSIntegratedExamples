namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Tekla.BIM.Quantities;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Extensions class for Geometry3d Point
    /// </summary>
    public static class TxPoint
    {
        /// <summary>
        /// Gives dot product as vector type extension
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double Dot(this Point pt, Vector v)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (v == null) throw new ArgumentNullException(nameof(v));

            return new Vector(pt).Dot(v);
        }

        /// <summary>
        /// Gives dot product as vector type extension
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static double Dot(this Point pt, Point pt2)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (pt2 == null) throw new ArgumentNullException(nameof(pt2));

            return new Vector(pt).Dot(new Vector(pt2));
        }

        /// <summary>
        /// Gives cross product as Vector type extension
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector Cross(this Point pt, Vector v)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (v == null) throw new ArgumentNullException(nameof(v));

            return new Vector(pt).Cross(v);
        }

        /// <summary>
        /// Gives cross product as Vector type extension
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Vector Cross(this Point pt, Point pt2)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (pt2 == null) throw new ArgumentNullException(nameof(pt2));

            return new Vector(pt).Cross(new Vector(pt2));
        }

        /// <summary>
        /// Gets length of 3 parts together
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static double Length(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            return new Vector(pt).GetLength();
        }

        /// <summary>
        /// Rounds each part of a Point to number of decimal places
        /// </summary>
        /// <param name="pt">Point to round</param>
        /// <param name="rndPlaces">Number of places to round to</param>
        /// <returns>Rounded 3d Point</returns>
        public static Point Round(this Point pt, int rndPlaces = 2)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            var dx = Math.Round(pt.X, rndPlaces);
            var dy = Math.Round(pt.Y, rndPlaces);
            var dz = Math.Round(pt.Z, rndPlaces);
            return new Point(dx, dy, dz);
        }

        /// <summary>
        /// Returns new point scaled down by factor
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Point Divide(this Point pt, double value)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            return new Point(pt.X / value, pt.Y / value, pt.Z / value);
        }

        /// <summary>
        /// Tells if points are neighbors: wtf?
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="c">c</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool ArePointsNeighbors(int a, int b, int c, int count)
        {
            var indexList = new List<int>(3) {a, b, c};
            indexList.Sort();
            if (indexList[0] + 1 == indexList[1] && indexList[1] + 1 == indexList[2]) return true;
            if (indexList[0] == 0 && indexList[2] == count - 1)
                if (indexList[1] == 1 || indexList[1] == count - 2)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns True if points are on same plane in 3d space
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool ArePointsCoplanar(this Point a, Point b, Point c)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (c == null) throw new ArgumentNullException(nameof(c));

            return Math.Abs(Tekla.Structures.Geometry3d.Distance.PointToPoint(new Point(),
                       Vector.Cross(new Vector(b - a), new Vector(c - b)))) <
                   GeometryConstants.DISTANCE_EPSILON;
        }

        /// <summary>
        /// Gets vector between two points
        /// </summary>
        /// <param name="p1">Origin point</param>
        /// <param name="p2">Destination point</param>
        /// <returns>new Vector to point</returns>
        public static Vector GetVectorTo(this Point p1, Point p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            return new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        }

        /// <summary>
        ///Gets new Vector from a point
        /// </summary>
        /// <param name="p">Point to get vector from</param>
        /// <returns>New vector from point</returns>
        public static Vector ToVector(this Point p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            return new Vector(p.X, p.Y, p.Z);
        }

        /// <summary>
        /// Translates point vector direction and distance
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="tVector"></param>
        public static void Translate(this Point pt, Vector tVector)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (tVector == null) throw new ArgumentNullException(nameof(tVector));
            pt.Translate(tVector.X, tVector.Y, tVector.Z);
        }

        /// <summary>
        /// Gets new point translated from current
        /// </summary>
        /// <param name="pt">Point of origin</param>
        /// <param name="v">Vector translation to move copy of point</param>
        /// <returns>New translated point</returns>
        public static Point GetTranslated(this Point pt, Vector v)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (v == null) throw new ArgumentNullException(nameof(v));

            var tempPt = new Point(pt);
            tempPt.Translate(v.X, v.Y, v.Z);
            return tempPt;
        }

        /// <summary>
        /// Gets the square of the length
        /// </summary>
        /// <param name="pt">Point ot use</param>
        /// <returns>Value in mm</returns>
        public static double LengthSq(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            return Math.Pow(new Vector(pt).GetLength(), 2);
        }

        /// <summary>
        /// Creates small line segment graphic in model view to represent point
        /// </summary>
        /// <param name="pt"></param>
        public static void DrawInModel(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var gd = new GraphicsDrawer();
            var ln = new LineSegment(pt, new Point(pt.X + 5, pt.Y, pt.Z));
            gd.DrawLineSegment(ln, new Color(1, 0, 0));
        }

        /// <summary>
        /// Prints information about current point with header to console.  Header can be empty string
        /// </summary>
        /// <param name="pt">Point to print information from</param>
        /// <param name="headerText">Header string to print</param>
        public static void Print(this Point pt, string headerText)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            if (string.IsNullOrEmpty(headerText)) headerText = "Point Info";
            Trace.WriteLine("======================" + headerText + "===================");
            Trace.WriteLine("X value: " + Math.Round(pt.X, 5));
            Trace.WriteLine("Y value: " + Math.Round(pt.Y, 5));
            Trace.WriteLine("Z value: " + Math.Round(pt.Z, 5));
            Trace.WriteLine("===================================================");
            Trace.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// Converts point in mm to current unit string
        /// </summary>
        /// <param name="pt">Point to use</param>
        /// <returns>New string representation of point</returns>
        public static string ToCurrentUnits(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            return string.Format("{0}, {1}, {2}",
                new Length(pt.X).ToCurrentUnits(),
                new Length(pt.Y).ToCurrentUnits(),
                new Length(pt.Z).ToCurrentUnits());
        }

        /// <summary>
        /// Gets Geometry3d from string output, assumes it is in metric
        /// </summary>
        /// <param name="str">Metric string</param>
        /// <returns>New Geometry3d Point</returns>
        public static Point ParseToPoint(this string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));

            PugeExtraCharacters(ref str);
            var numbers = SafeSplitNumberString(str);
            if (numbers.Length != 3)
            {
                throw new ApplicationException("Number points in string is wrong.\n" + str);
            }

            var x = StringToNumber(numbers[0].Trim());
            var y = StringToNumber(numbers[1].Trim());
            var z = StringToNumber(numbers[2].Trim());
            return new Point(x, y, z);
        }

        /// <summary>
        /// Gets a series of 3 numbers from a string using CultureInformation
        /// Check for extra comma delimiters and places string back together if needed
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string[] SafeSplitNumberString(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            var parts = str.Split(new[] {','});
            var sp = TxModel.NumberDecimalSeparator;

            if (parts.Length == 3) return parts;
            if (parts.Length == 6)
                return new[] {parts[0] + sp + parts[1], parts[2] + sp + parts[3], parts[4] + sp + parts[5]};
            if (parts.Length == 12)
                return new[]
                {
                    parts[0] + sp + parts[1], parts[2] + sp + parts[3], parts[4] + sp + parts[5],
                    parts[6] + sp + parts[7], parts[8] + sp + parts[9], parts[10] + sp + parts[11]
                };
            return parts;
        }

        /// <summary>
        /// Gets rid of formatting characters hard coded by TS when using the Point toString method
        /// </summary>
        /// <param name="str">Raw string to change</param>
        private static void PugeExtraCharacters(ref string str)
        {
            var forbiddenChars = new[] {'(', ')'};
            foreach (var c in forbiddenChars)
            {
                while (str.Contains(c.ToString(CultureInfo.InvariantCulture)))
                    str = str.Replace(c.ToString(CultureInfo.InvariantCulture), string.Empty);
            }
        }

        /// <summary>
        /// Safely converts a string to a number Using CultureInfo
        /// </summary>
        /// <param name="rawStringPoint">Raw string to convert</param>
        /// <returns>Number double format</returns>
        private static double StringToNumber(string rawStringPoint)
        {
            if (string.IsNullOrEmpty(rawStringPoint)) throw new ArgumentNullException(nameof(rawStringPoint));
            return Convert.ToDouble(rawStringPoint, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Translates points by matrix
        /// </summary>
        /// <param name="points">Points to transform</param>
        /// <param name="matrix">Transformation matrix</param>
        /// <returns>List of transformed points</returns>
        public static List<Point> Translate(this IEnumerable<Point> points, Matrix matrix)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            return points.Select(matrix.Transform).ToList();
        }

        /// <summary>
        /// Gets new point transformed to global using current workplane
        /// </summary>
        /// <param name="pt">Local point to transform</param>
        /// <returns>New global based point</returns>
        public static Point ToGlobal(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException();
            var globalMatix = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                .TransformationMatrixToGlobal;
            var tempPoint = new Point(pt);
            return new Point(globalMatix.Transform(tempPoint));
        }

        /// <summary>
        /// Gets new point transformed to local from global
        /// </summary>
        /// <param name="pt">Point in global coordinates</param>
        /// <returns>Point in local coordinates based on current workplane</returns>
        public static Point ToLocal(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var localMatix = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                .TransformationMatrixToLocal;
            var tempPoint = new Point(pt);
            return new Point(localMatix.Transform(tempPoint));
        }

        /// <summary>
        /// Gets distance between two points in 3d space
        /// </summary>
        /// <param name="firstPoint">Point 1</param>
        /// <param name="secondPoint">Point 2</param>
        /// <returns>Distance from Geometry3d library</returns>
        public static double Distance(this Point firstPoint, Point secondPoint)
        {
            if (firstPoint == null) throw new ArgumentNullException(nameof(firstPoint));
            if (secondPoint == null) throw new ArgumentNullException(nameof(secondPoint));
            return Tekla.Structures.Geometry3d.Distance.PointToPoint(firstPoint, secondPoint);
        }

        /// <summary>
        /// Scales point by value
        /// </summary>
        /// <param name="v1">Point to scale</param>
        /// <param name="x">Distance to scale</param>
        /// <returns></returns>
        public static Vector Scale(this Point v1, double x)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            return new Vector(v1.X * x, v1.Y * x, v1.Z * x);
        }

        /// <summary>
        /// Gets square root of x, y, z values squared
        /// </summary>
        /// <param name="p1">Point to use</param>
        /// <returns>Squared root of point components</returns>
        public static double GetSquareLength(this Point p1)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            return Math.Sqrt(Math.Pow(p1.X, 2) + Math.Pow(p1.Y, 2) + Math.Pow(p1.Z, 2));
        }

        /// <summary>
        /// Inserts dummy sphere into model
        /// </summary>
        /// <param name="pt">Location to insert ball</param>
        /// <param name="myclass">Class to make ball</param>
        /// <param name="name">Name of ball</param>
        /// <param name="height">Diameter of sphere</param>
        public static void InsertDummyBall(this Point pt, int myclass = 901, string name = null, double height = 50.8)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (height < 0.25 * 25.4) height = 2 * 25.4;
            var bm = new Beam(Beam.BeamTypeEnum.BEAM)
            {
                StartPoint = new Point(pt.X - height / 2, pt.Y, pt.Z),
                EndPoint = new Point(pt.X + height / 2, pt.Y, pt.Z),
                Profile =
                {
                    ProfileString = string.Format("SPHERE{0}", height.ToString(CultureInfo.InvariantCulture))
                },
                Position = {Depth = Position.DepthEnum.MIDDLE, Plane = Position.PlaneEnum.MIDDLE},
                Material = {MaterialString = "Misc_Undefined"},
                Class = myclass.ToString(),
                Name = name
            };
            if (!bm.Insert()) Trace.Write("Unable to insert dummy ball at: " + pt);
        }

        /// <summary>
        /// Gets sequence of line segments from point lists
        /// </summary>
        /// <param name="ptList">Sequential point list to interpolate</param>
        /// <returns>Strong type list of line segments formed by stringing together points in current ucs</returns>
        public static List<LineSegment> GetLineSegmentsFromPoints(List<Point> ptList)
        {
            if (ptList == null) throw new ArgumentNullException(nameof(ptList));
            var lsgList = new List<LineSegment>();
            for (var i = 0; i < ptList.Count; i++)
            {
                if (i == ptList.Count - 1) lsgList.Add(new LineSegment(ptList[i], ptList[0]));
                else lsgList.Add(new LineSegment(ptList[i], ptList[i + 1]));
            }

            return lsgList;
        }

        /// <summary>
        /// Transforms points from current workplane to passed workplane
        /// </summary>
        /// <param name="points">Polygon to get points from</param>
        /// <param name="cs">Coordinate system to transform from</param>
        /// <returns>New list of points</returns>
        public static List<Point> TransformPointsToPlane(this IEnumerable points, CoordinateSystem cs)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            var result = new List<Point>();
            var transMatrix = MatrixFactory.FromCoordinateSystem(cs);
            foreach (Point pt in points) result.Add(transMatrix.Transform(pt));
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
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            var result = new List<Point>();
            var transMatrix = MatrixFactory.ToCoordinateSystem(cs);
            foreach (Point pt in points) result.Add(transMatrix.Transform(pt));
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
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            var result = new List<Point>();
            var plane = new GeometricPlane(cs);
            foreach (Point pt in points) result.Add(Projection.PointToPlane(pt, plane));
            return result;
        }
    }
}