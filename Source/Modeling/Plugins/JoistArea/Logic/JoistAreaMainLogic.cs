namespace JoistArea.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;
    using Tools;
    using ViewModel;

    /// <summary>
    /// Main DataModel logic service class for plugin detailing
    /// </summary>
    public class JoistAreaMainLogic
    {
        public double MinJoistLength = 12.0 * 25.4;

        /// <summary>
        /// User interface data
        /// </summary>
        private JoistAreaData _uiData;

        /// <summary>
        /// Picked points from user DefineInput
        /// </summary>
        public List<Point> PickedPoints { get; set; }

        /// <summary>
        /// Floor area staring point and direction guide-line
        /// </summary>
        public LineSegment GuideLine { get; set; }

        /// <summary>
        /// Center spacing distance list service data variable to represent parsed spacings
        /// </summary>
        public TxDistanceList DistanceList
        {
            get
            {
                if (string.IsNullOrEmpty(_uiData.CenterSpacingList)) return null;
                return new TxDistanceList(_uiData.CenterSpacingList);
            }
        }

        /// <summary>
        /// Center max spacing value
        /// </summary>
        public double CenterSpacingMax => _uiData.CenterSpacingMax;

        /// <summary>
        /// Collection of Virtual Joists and needed data
        /// </summary>
        public List<VirtualJoist> VirtualJoistList { get; set; }

        /// <summary>
        /// Coordinate system local to plugin calculated
        /// </summary>
        public CoordinateSystem LocalCoordinateSystem { get; set; }

        /// <summary>
        /// Plugin calculated coordinate system transformed to Global
        /// </summary>
        public CoordinateSystem GlobalCoordinateSystem { get; set; }

        /// <summary>
        /// Calculated from polygon points in constructors - max point to point distance
        /// </summary>
        public double MaxPolygonDistance { get; set; }

        /// <summary>
        /// Joist span direction vector - local y direction
        /// </summary>
        public Vector SpanDir => LocalCoordinateSystem.AxisY.GetNormal();

        /// <summary>
        /// Joist direction vector from guideline
        /// </summary>
        public Vector JoistDir => GuideLine.GetDirectionVector().GetNormal();

        /// <summary>
        /// Spacing type used: list vs center to center
        /// </summary>
        public MainViewModel.SpacingTypeEnum SpacingTyp => (MainViewModel.SpacingTypeEnum) _uiData.SpacingType;

        /// <summary>
        /// Transformation from local plugin coordinate system to global
        /// </summary>
        public Matrix TransGlobalMatrix { get; set; }

        /// <summary>
        /// Main Logic helper class - to be called from plugin
        /// </summary>
        public JoistAreaMainLogic()
        {
            _uiData = null;
            LocalCoordinateSystem = null;
            GlobalCoordinateSystem = null;
            VirtualJoistList = null;
            PickedPoints = null;
            MaxPolygonDistance = 0.0;
            TransGlobalMatrix = null;
        }

        /// <summary>
        /// Logic helper class - to be called outside plugin
        /// </summary>
        /// <param name="pickedPoints">Picked points from DefineInput</param>
        /// <param name="guideLine">Optional guideline</param>
        /// <param name="uiData">User Interface Data</param>
        /// <returns>False if error</returns>
        public bool ExternalInitialize(List<Point> pickedPoints, LineSegment guideLine, JoistAreaData uiData)
        {
            if (pickedPoints == null) throw new ArgumentNullException(nameof(pickedPoints));
            if (guideLine == null) throw new ArgumentNullException(nameof(guideLine));
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));
            _uiData = uiData;

            //Assume not in plugin local coordinate system already, transform
            var originalPlane = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try
            {
                //Get plugin actual coordinate system and transform workplane
                var pluginCalculatedCoordSys = JoistAreaPlugin.GetJointCoordinateSystem(pickedPoints, guideLine);
                if (pluginCalculatedCoordSys == null) return false;
                TransGlobalMatrix = MatrixFactory.ToCoordinateSystem(pluginCalculatedCoordSys);
                new Model().GetWorkPlaneHandler()
                    .SetCurrentTransformationPlane(new TransformationPlane(pluginCalculatedCoordSys));

                //Set local data in new Cs
                LocalCoordinateSystem = new CoordinateSystem();
                GlobalCoordinateSystem = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                    .TransformationMatrixToGlobal.Transform(new CoordinateSystem());
                PickedPoints = pickedPoints.Transform(TransGlobalMatrix);
                GuideLine = guideLine.Transform(TransGlobalMatrix);
                SetMaxPolygonDistance();

                //Cache local properties, get data in local plugin coordinate system
                if (!CalculateJoistBeams())
                {
                    GlobalServices.LogException("CalculateJoistBeams...failed!");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return false;
            }
            finally
            {
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(originalPlane);
            }
        }

        /// <summary>
        /// Creates parts and detailing for main plugin
        /// </summary>
        /// <param name="pickedPoints">Picked points from plugin</param>
        /// <param name="guideLine">Optional guideline</param>
        /// <param name="uiData">User interface data</param>
        /// <returns>True so plugin will insert</returns>
        public bool CreateNew(List<Point> pickedPoints, LineSegment guideLine, JoistAreaData uiData)
        {
            if (pickedPoints == null) throw new ArgumentNullException(nameof(pickedPoints));
            if (guideLine == null) throw new ArgumentNullException(nameof(guideLine));
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));

            try
            {
                _uiData = uiData;
                PickedPoints = pickedPoints;
                GuideLine = guideLine;
                LocalCoordinateSystem = new CoordinateSystem(); //Set by plugin run code already local
                GlobalCoordinateSystem = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                    .TransformationMatrixToGlobal.Transform(LocalCoordinateSystem);
                SetMaxPolygonDistance();
                TransGlobalMatrix = new Matrix();
                //DebugPaintPoints();
                //DebugPaintEdges();

                if (!CalculateJoistBeams())
                {
                    GlobalServices.LogException("CalculateJoistBeams...failed!");
                    return false;
                }

                if (!InsertJoistBeams())
                {
                    GlobalServices.LogException("InsertJoistBeams...failed!");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Set max distance from point edges used to find largest approximate distance across polygon
        /// </summary>
        private void SetMaxPolygonDistance()
        {
            Point lastPoint = null;
            for (var i = 0; i < PickedPoints.Count - 1; i++)
            {
                //Check if first time through and set last point
                var currentPt = new Point(PickedPoints[i]);
                if (lastPoint == null)
                {
                    lastPoint = new Point(currentPt);
                    continue;
                }

                //Check previous point and current point distance
                var dist = Distance.PointToPoint(lastPoint, currentPt);
                if (dist > MaxPolygonDistance)
                {
                    MaxPolygonDistance = dist;
                }

                lastPoint = new Point(currentPt);

                //Check last and first point at end of loop
                if (i == PickedPoints.Count - 1)
                {
                    var p1 = new Point(PickedPoints[0]);
                    var distEnd = Distance.PointToPoint(p1, currentPt);
                    if (distEnd > MaxPolygonDistance)
                    {
                        MaxPolygonDistance = distEnd;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void DebugPaintPoints()
        {
            var counter = 0;
            foreach (var pt in PickedPoints)
            {
                var offsetV = new Vector(0.5, 0.5, 0) * 20.0;
                var textPt = new Point(pt);
                textPt.Translate(offsetV);
                new GraphicsDrawer().DrawText(textPt, counter.ToString(), new Color(1, 0, 0));
                counter++;
            }
        }

        [Conditional("DEBUG")]
        private void DebugPaintEdges()
        {
            var lines = PickedPoints.GetLineSegmentsFromPoints();
            foreach (var ln in lines)
            {
                ln.PaintLine(new Color(0, 0, 1));
            }
        }

        private bool InsertJoistBeams()
        {
            if (VirtualJoistList == null) return false;
            try
            {
                foreach (var vj in VirtualJoistList)
                {
                    vj.Insert();
                }

                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return false;
            }
        }

        private bool CalculateJoistBeams()
        {
            if (GuideLine == null || PickedPoints == null || PickedPoints.Count < 3) return false;
            VirtualJoistList = null;
            var result = new List<VirtualJoist>();

            try
            {
                //Start at midpoint of guideline
                var currentPosition = GuideLine.GetMidPoint();

                //Get offset from guideline
                currentPosition.Translate(SpanDir * _uiData.FirstJoistOffset);
                var boundaryPt = GetPtIntersectPolygon(currentPosition, SpanDir, false, true);
                if (boundaryPt == null) return false;

                switch (SpacingTyp)
                {
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                    {
                        //Insert 1st Joist at zero
                        //Get beam start and end points clipped by polygon edges
                        var startPt = GetPtIntersectPolygon(currentPosition, JoistDir * -1, false, false);
                        var endPt = GetPtIntersectPolygon(currentPosition, JoistDir, false, false);

                        //Add 1st virtual beam joist to results list
                        if (startPt != null && endPt != null)
                        {
                            var vj = GetVirtualJoist(startPt, endPt);
                            result.Add(vj);
                        }

                        if (DistanceList == null) return false;
                        foreach (var dv in DistanceList.DistanceValues)
                        {
                            currentPosition.Translate(SpanDir * dv.Millimeters);

                            //Calculate position to next spacing value and distance to edge boundary
                            var distLeft = new Vector(boundaryPt - currentPosition).Dot(SpanDir);
                            if (distLeft < 0) break; //Past boundary

                            //Get beam start and end points clipped by polygon edges
                            startPt = GetPtIntersectPolygon(currentPosition, JoistDir * -1, false, false);
                            endPt = GetPtIntersectPolygon(currentPosition, JoistDir, false, false);

                            //Add virtual beam joist to results list
                            if (startPt != null && endPt != null)
                            {
                                var vj = GetVirtualJoist(startPt, endPt);
                                result.Add(vj);
                            }
                        }
                        break;
                    }
                    case MainViewModel.SpacingTypeEnum.albl_CenterToCenter:
                    {
                        //Calculate 1st position and distance to boundary end point
                        var distLeft = new Vector(boundaryPt - currentPosition).Dot(SpanDir);

                        //Check each distance moving along span
                        while (distLeft >= CenterSpacingMax)
                        {
                            //Get beam start and end points clipped by polygon edges
                            var startPt = GetPtIntersectPolygon(currentPosition, JoistDir * -1, false, false);
                            var endPt = GetPtIntersectPolygon(currentPosition, JoistDir, false, false);

                            //Add virtual beam joist to results list
                            if (startPt != null && endPt != null &&
                                Distance.PointToPoint(startPt, endPt) > MinJoistLength)
                            {
                                var vj = GetVirtualJoist(startPt, endPt);
                                result.Add(vj);
                            }

                            //Move position to next spot along span
                            distLeft = new Vector(boundaryPt - currentPosition).Dot(SpanDir);
                            currentPosition.Translate(SpanDir * CenterSpacingMax);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //Return list of virtual joists
                VirtualJoistList = result;
                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return false;
            }
        }

        private Point GetPtIntersectPolygon(Point currentPosition, Vector intDir, bool drawPlanes,
            bool offsetFromOrigin)
        {
            var boundaryPoints = new List<Point>();
            try
            {
                //Create new point at origin
                var p1 = new Point(currentPosition);
                if (offsetFromOrigin)
                {
                    //Move away from original position to avoid conflict w/guideline
                    p1.Translate(intDir.GetNormal() * 25.4);
                }

                //Get point along check axis away from origin
                var p2 = new Point(currentPosition);
                p2.Translate(intDir.GetNormal() * MaxPolygonDistance * 1.5);

                //Create check line and get LineSegments from points
                var checkLineSegment = new LineSegment(p1, p2);
                var polygonLines = PickedPoints.GetLineSegmentsFromPoints();
                foreach (var ls in polygonLines)
                {
                    //Get vertical plane from each polygon edge line
                    var edgeLinePlane = GetPlaneFromLine(ls);
                    if (drawPlanes) edgeLinePlane.Paint();

                    //Intersect check line and edge segment plane, add to results list
                    var endBoundaryIntPt = Intersection.LineSegmentToPlane(checkLineSegment, edgeLinePlane);
                    if (endBoundaryIntPt != null) boundaryPoints.Add(endBoundaryIntPt);
                }

                //Return shortest found plane if beam line check, longest if boundary check
                if (offsetFromOrigin)
                    return boundaryPoints.OrderBy(f => Distance.PointToPoint(currentPosition, f)).Last();
                return boundaryPoints.OrderBy(f => Distance.PointToPoint(currentPosition, f)).First();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        private VirtualJoist GetVirtualJoist(Point startPt, Point endPt)
        {
            if (startPt == null) throw new ArgumentNullException(nameof(startPt));
            if (endPt == null) throw new ArgumentNullException(nameof(endPt));

            try
            {
                var result = new VirtualJoist();
                var beam = new Beam
                {
                    StartPoint = startPt,
                    EndPoint = endPt,
                    Profile = {ProfileString = _uiData.JoistProfile},
                    Material = {MaterialString = _uiData.Material},
                    Class = _uiData.JoistClass.ToString(),
                    Finish = _uiData.Finish,
                    AssemblyNumber = new NumberingSeries(_uiData.AssmNoPrefix, _uiData.AssmStartNo),
                    PartNumber = new NumberingSeries(_uiData.PartNoPrefix, _uiData.PartStartNo),
                    Name = _uiData.JoistName,
                    Position =
                    {
                        Depth = Position.DepthEnum.BEHIND,
                        Plane = Position.PlaneEnum.MIDDLE,
                        Rotation = Position.RotationEnum.TOP,
                        DepthOffset = _uiData.DepthOffset
                    }
                };
                result.ModelBeam = beam;
                return result;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        private GeometricPlane GetPlaneFromLine(LineSegment ls)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            try
            {
                var xAxis = ls.GetDirectionVector().GetNormal();
                var normal = Vector.Cross(LocalCoordinateSystem.GetAxisZ(), xAxis);
                return new GeometricPlane(ls.GetMidPoint(), normal);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }
    }
}