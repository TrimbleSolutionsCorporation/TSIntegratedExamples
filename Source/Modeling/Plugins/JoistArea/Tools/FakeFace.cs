namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Media;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Solid;

    /// <summary>
    /// Face geometry represented by points and segments
    /// </summary>
    public class FakeFace
    {
        /// <summary>
        /// All points in order on face
        /// </summary>
        public List<Point> AllPoints
        {
            get
            {
                var result = new List<Point>();
                foreach (Tekla.Structures.Geometry3d.LineSegment ls in Segments)
                {
                    if (!result.Contains(ls.Point1)) result.Add(ls.Point1);
                    if (!result.Contains(ls.Point2)) result.Add(ls.Point2);
                }
                return result;
            }
        }

        /// <summary>
        /// All outer segments that represent face in order
        /// </summary>
        public List<Tekla.Structures.Geometry3d.LineSegment> Segments { get; set; }

        /// <summary>
        /// Coordinate sytem of face
        /// </summary>
        public CoordinateSystem CoordSystem { get; set; }

        /// <summary>
        /// Polygon from AllPoints
        /// </summary>
        public Polygon Polygon
        {
            get
            {
                var result = new Polygon();
                foreach (var pt in AllPoints.Where(pt => !result.Points.Contains(pt))) result.Points.Add(pt.Round(5));
                return result;
            }
        }

        /// <summary>
        /// Delta in current workplane X vector
        /// </summary>
        public double Length
        {
            get
            {
                var maxValue = 0.0;
                foreach (var ls in Segments)
                {
                    var currentLength = new Vector(ls.Point2 - ls.Point1);
                    var projectedLs = Math.Abs(currentLength.Dot(CoordSystem.AxisX.GetNormal()));
                    if (projectedLs > maxValue) maxValue = projectedLs;
                }
                return maxValue;
            }
        }

        /// <summary>
        /// Delta in current workplane Y vector
        /// </summary>
        public double Width
        {
            get
            {
                var maxValue = 0.0;
                foreach (var ls in Segments)
                {
                    var currentLength = new Vector(ls.Point2 - ls.Point1);
                    var projectedLs = Math.Abs(currentLength.Dot(CoordSystem.AxisY.GetNormal()));
                    if (projectedLs > maxValue) maxValue = projectedLs;
                }
                return maxValue;
            }
        }

        public Point Center { get; set; }

        /// <summary>
        /// Face normal, points out away from face using coordinate system
        /// </summary>
        public Vector Normal { get { return Vector.Cross(CoordSystem.AxisX, CoordSystem.AxisY).GetNormal(); } }

        /// <summary>
        /// New instance of face form segments
        /// </summary>
        /// <param name="segments">Segments that represent closed face in order (outer only)</param>
        /// <param name="coordSys">Coordinate sytem to use for face</param>
        public FakeFace(List<Tekla.Structures.Geometry3d.LineSegment> segments, CoordinateSystem coordSys)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (coordSys == null) throw new ArgumentNullException(nameof(coordSys));
            Segments = segments;
            CoordSystem = coordSys;
        }

        /// <summary>
        /// Paints face segments and coordinate system in model with temporary graphics
        /// </summary>
        /// <param name="color">Model UI color to use</param>
        public void DrawInModel(Tekla.Structures.Model.UI.Color color = null)
        {
            if (color == null) color = new Tekla.Structures.Model.UI.Color(1, 0, 0);
            foreach (var ls in Segments) ls.PaintLine(color);
            CoordSystem.PaintCoordinateSystem();
        }

        /// <summary>
        /// Gets face intersection area in new face proxy
        /// </summary>
        /// <param name="middlePlane">Middle plane for joint</param>
        /// <param name="primaryProjection">Primary part face</param>
        /// <param name="secondaryProjection">Secondary part face</param>
        /// <param name="faceDifference">Joint proxy to get thickness from</param>
        /// <returns>New face proxy from result</returns>
        public static FakeFace GetCombinedResult(CoordinateSystem middlePlane, FakeFace primaryProjection, FakeFace secondaryProjection, FaceDifference faceDifference)
        {
            if (primaryProjection == null) throw new ArgumentNullException(nameof(primaryProjection));
            if (secondaryProjection == null) throw new ArgumentNullException(nameof(secondaryProjection));
            if (faceDifference == null) throw new ArgumentNullException(nameof(faceDifference));

            //Get face polygon shape
            var polygon3D1 = primaryProjection.Polygon;
            var polygon3D2 = secondaryProjection.Polygon;

            var polygon1 = polygon3D1.Points.TransformPointsToPlane(middlePlane);
            var polygon2 = polygon3D2.Points.TransformPointsToPlane(middlePlane);

            //WPF polygon method
            {
                var polygonToBeCut = TxPathGeometry.PolygonToWpfPolygon(polygon1);
                var polygonToCutWith = TxPathGeometry.PolygonToWpfPolygon(polygon2);
                if (polygonToBeCut == null || polygonToCutWith == null)
                {
                    Trace.WriteLine("FakeFace:GetCombinedResult failed to get polygons from faces.");
                    return null;
                }

                //Try to combine two polygons using intersect method
                var combinedGeometry = new CombinedGeometry(GeometryCombineMode.Intersect, polygonToBeCut, polygonToCutWith);
                var combinedPoints = TxPathGeometry.WpfPolygonToPointList(combinedGeometry.GetFlattenedPathGeometry(0.5, ToleranceType.Relative));
                if (combinedPoints != null)
                {
                    //Set center for new region
                    var rectangle = combinedGeometry.Bounds;
                    var p1 = new Point(rectangle.BottomLeft.X, rectangle.BottomLeft.Y);
                    var p2 = new Point(rectangle.TopRight.X, rectangle.TopRight.Y);
                    var center = new Tekla.Structures.Geometry3d.LineSegment(p1, p2).GetMidPoint();

                    //Get points back to 3d model
                    var pointsBackTo3D = combinedPoints.TransformPointsFromPlane(middlePlane);
                    var center3d = new[] { center }.TransformPointsFromPlane(middlePlane);
                    var pointsOnMiddlePlane = TxPoint.ProjectPointsToPlane(pointsBackTo3D, middlePlane);
                    var centerPlane = TxPoint.ProjectPointsToPlane(center3d, middlePlane);
                    return new FakeFace((List<Tekla.Structures.Geometry3d.LineSegment>)pointsOnMiddlePlane.GetLineSegmentsFromPoints(), primaryProjection.CoordSystem) { Center = centerPlane[0] };
                }

                Trace.WriteLine("FakeFace:GetCombinedResult failed to combine geomtry of two projected faces.");
#if DEBUG
                for (var i = 0; i < polygon1.Count; i++) polygon1[i].InsertLayoutPoint(string.Format("P1: {0}", i));
                for (var i = 0; i < polygon2.Count; i++) ((Point)polygon2[i]).InsertLayoutPoint(string.Format("P2: {0}", i));
#endif
            }
            return null;
        }

        /// <summary>
        /// Gets fakeface projection from a solid face and geometric plane
        /// </summary>
        /// <param name="solidFace">Modle part solid face to project</param>
        /// <param name="plane">Geometric plane to project onto</param>
        /// <param name="v">Vector to translate after projection</param>
        /// <returns>New projected fake face</returns>
        public static FakeFace ProjectToPlane(Face solidFace, GeometricPlane plane, Vector v)
        {
            if (solidFace == null) throw new ArgumentNullException(nameof(solidFace));
            if (plane == null) throw new ArgumentNullException(nameof(plane));

            var projectedSegments = new List<Tekla.Structures.Geometry3d.LineSegment>();
            var outerSegments = solidFace.GetFaceOuterSegments();
            foreach (var outerSegment in outerSegments)
            {
                var projectedLineSegment = Projection.LineSegmentToPlane(outerSegment, plane);
                if (projectedLineSegment != null) projectedSegments.Add(projectedLineSegment);
            }
            if (projectedSegments.Count < 1) return null;
            return new FakeFace(projectedSegments, solidFace.GetCoordinateSystem().Transform(v));
        }
    }
}
