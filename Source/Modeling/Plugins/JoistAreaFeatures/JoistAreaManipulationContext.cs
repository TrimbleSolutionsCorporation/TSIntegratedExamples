namespace JoistAreaFeatures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;
    using Distance = Tekla.Structures.Datatype.Distance;

    /// <summary>
    /// Manipulation context class for the <see cref="JoistAreaManipulationFeature"/> class.
    /// </summary>
    public sealed class JoistAreaManipulationContext : ManipulationContext
    {
        private IHandleManager _handleManager;
        private List<PointHandle> _polygonHandles;
        private List<PointHandle> _guidelineHandles;
        private List<LineHandle> _guideLine;
        private List<LineHandle> _edgeLines;
        private readonly IGraphicsDrawer _graphics;
        private JoistAreaManipulationFeature _featureBase;
        private List<DistanceManipulator> _distanceManipulators;
        private JoistAreaData _uiData;
        private readonly JoistAreaMainLogic _liftingLogic;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoistAreaManipulationContext"/> class.
        /// </summary>
        /// <param name="component">The component to be manipulated.</param>
        /// <param name="feature">The parent feature.</param>
        public JoistAreaManipulationContext(Component component, JoistAreaManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            _featureBase = feature;
            _graphics = feature.Graphics;
            _handleManager = feature.HandleManager;

            //Get part and plugin information
            _uiData = component.GetDataFromComponent();
            //var uiDataStored = _featureBase.GetUserInterfaceData(component); //all blank
            var componentInput = GetCurrentInput(component);

            //Create new instance of logic service class
            _liftingLogic = new JoistAreaMainLogic();
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Create point, distance, and line manipulators
            CreatePointManipulators(component);
            CreateJoistDistanceManipulators(component);
            CreateLineManipulators(component);

            //Draw custom graphics
            ReCreateGraphics();
        }

        private void ReCreateGraphics()
        {
            _graphics.Clear();
            DrawFaceWorkPlane();
            DrawGuidelineArrow();
        }

        private void DrawGuidelineArrow()
        {
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));
            if (_graphics == null) throw new ArgumentNullException(nameof(_graphics));
            if(_guidelineHandles == null || _guidelineHandles.Count != 2) return;

            const double TipLength = 125.0;
            const double ShaftThickness = 25.0;
            const double ArrowThickness = 100.0;
            const double yAxisLength = 900.0;

            try
            {
                var guideLine = new LineSegment
                {
                    Point1 = _guidelineHandles[0].Point, 
                    Point2 = _guidelineHandles[1].Point
                };
                var globalCs = _liftingLogic.GlobalCoordinateSystem;
                var p0 = new Point(guideLine.Point1);
                var arrowHeadProfile = $"HXGON{ArrowThickness}-{ShaftThickness}";
                var shaftProfile = $"D{ShaftThickness}";

                //Define x-axis arrow
                {
                    var pxTrans = new Point(guideLine.Point2);
                    pxTrans.Translate(guideLine.GetDirectionVector() * -TipLength);

                    var xShaft = new LineSegment(p0, pxTrans);
                    var xHead = new LineSegment(pxTrans, guideLine.Point2);

                    _graphics.DrawProfile(shaftProfile, xShaft, new Vector(0, 0, 0), 0, LineType.Error);
                    _graphics.DrawProfile(arrowHeadProfile, xHead, new Vector(0, 0, 0), 0, LineType.Error);
                    var textPt = new Point(guideLine.Point2).GetTranslated(globalCs.AxisX.GetNormal() * 75);
                    _graphics.DrawText("X", textPt, TextRepresentationTypes.Label);
                }

                //Define y-axis arrow
                {
                    var yEnd = new Point(p0);
                    yEnd.Translate(globalCs.AxisY.GetNormal() * yAxisLength);
                    var pyTrans = new Point(yEnd);
                    pyTrans.Translate(globalCs.AxisY.GetNormal() * -TipLength);

                    var yShaft = new LineSegment(p0, pyTrans);
                    var yHead = new LineSegment(pyTrans, yEnd);

                    _graphics.DrawProfile(shaftProfile, yShaft, new Vector(0, 0, 0));
                    _graphics.DrawProfile(arrowHeadProfile, yHead, new Vector(0, 0, 0));
                    var textPt = new Point(yEnd).GetTranslated(globalCs.AxisY.GetNormal() * 75);
                    _graphics.DrawText("Y", textPt, TextRepresentationTypes.Label);
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void DrawFaceWorkPlane()
        {
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));
            if (_graphics == null) throw new ArgumentNullException(nameof(_graphics));

            try
            {
                var globalCs = _liftingLogic.GlobalCoordinateSystem;
                var transMatrixFc = MatrixFactory.ToCoordinateSystem(globalCs);

                // Draw UCS Graphic
                var ucsGraphics = new WorkplaneArrowGraphic(transMatrixFc);
                ucsGraphics.DrawGraphics(_graphics, false);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void CreatePointManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            try
            {
                //Setup polygon handles
                _polygonHandles = GetPolygonPointHandles(component);
                _polygonHandles.ForEach(handle =>
                    {
                        handle.DragOngoing += PolygonHandleDragOngoing;
                        handle.DragEnded += PolygonHandleDragEnded;
                    }
                );

                //Setup guideline point handles
                _guidelineHandles = GetGuidelineHandles(component);
                //Drag events already set in above method ---

            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void CreateLineManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _guideLine = new List<LineHandle>();
            _edgeLines = new List<LineHandle>();

            try
            {
                var componentInput = GetCurrentInput(component);
                var pts = componentInput.Item1;
                var ls = componentInput.Item2;

                //Add line manipulators for polygon segments
                {
                    var edgeSegments = new List<LineSegment>();
                    Point lastPoint = null;
                    for (var i = 0; i < pts.Count; i++)
                    {
                        var pt = pts[i];
                        if (i == 0)
                        {
                            lastPoint = pt;
                            continue;
                        }
                        edgeSegments.Add(new LineSegment(lastPoint, pt));
                        lastPoint = pt;

                        //Complete loop if last point in array
                        if (i == pts.Count - 1)
                        {
                            edgeSegments.Add(new LineSegment(pt, pts[0]));
                        }
                    }

                    foreach (var es in edgeSegments)
                    {
                        var edgeSeg =
                            _handleManager.CreateLineHandle(es, HandleLocationType.Other, HandleEffectType.Geometry);
                        edgeSeg.DragOngoing += EdgeSegment_DragOngoing;
                        edgeSeg.DragEnded += EdgeSegment_DragEnded;
                        _edgeLines.Add(edgeSeg);
                    }
                }

                //Add line manipulator for Guidline segment
                {
                    var guidSegment =
                        _handleManager.CreateLineHandle(ls, HandleLocationType.First, HandleEffectType.Geometry);
                    guidSegment.DragOngoing += GuideLine_DragOngoing;
                    guidSegment.DragEnded += GuideLine_DragEnded;
                    _guideLine.Add(guidSegment);
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void CreateJoistDistanceManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));
            _distanceManipulators = new List<DistanceManipulator>();

            try
            {
                //Get joist spacings and distance values from base plugin logic
                var joistSpacingsGlobal = FeatureLogic.GetJoistSpanSegments(_liftingLogic);
                var distValues = new ArrayList(_liftingLogic.DistanceList.DistanceValues);

                //Depending on spacing type add distance manipulator between each joist
                switch (_liftingLogic.SpacingTyp)
                {
                    //Distance type spacing, handle segment as spacing value in larger string list
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                        for (var i = 0; i < joistSpacingsGlobal.Count; i++)
                        {
                            var ls = joistSpacingsGlobal[i];
                            var distMan = new DistanceManipulator(component, this, ls);
                            distMan.MeasureChanged += delegate
                            {
                                if (distValues.Count > i - 1)
                                {
                                    //Replace spacing value at position in distance value list
                                    distValues[i] = new Distance(distMan.Segment.Length());
                                    var modDistList = new TxDistanceList(distValues);

                                    //Modify plugin spacing list string value and call to update
                                    component.SetAttribute("CenterSpacingList", modDistList.FormattedDistanceString);
                                    //component.Modify();
                                    //ModifyPluginInputFromManipulators();
                                    ModifyComponentInput(component.GetComponentInput());
                                }
                            };
                            AddManipulator(distMan);
                            _distanceManipulators.Add(distMan);
                        }
                        break;

                    //Single max spacing for all joist, handle segment as same spacing max value
                    case MainViewModel.SpacingTypeEnum.albl_CenterToCenter:
                        foreach (var ls in joistSpacingsGlobal)
                        {
                            var distMan = new DistanceManipulator(component, this, ls);
                            distMan.MeasureChanged += delegate
                            {
                                //Modify plugin spacing max value and call to update
                                component.SetAttribute("CenterSpacingMax", distMan.Segment.Length());
                                //component.Modify();
                                //ModifyPluginInputFromManipulators();
                                ModifyComponentInput(component.GetComponentInput());
                            };
                            AddManipulator(distMan);
                            _distanceManipulators.Add(distMan);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private List<PointHandle> GetPolygonPointHandles(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (_handleManager == null) throw new ArgumentNullException(nameof(_handleManager));

            var handles = new List<PointHandle>();
            var componentInput = GetCurrentInput(component);
            foreach (var pt in componentInput.Item1)
            {
                var handle = _handleManager.CreatePointHandle(pt, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                handles.Add(handle);
            }
            return handles;
        }

        private List<PointHandle> GetGuidelineHandles(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (_handleManager == null) throw new ArgumentNullException(nameof(_handleManager));

            var handles = new List<PointHandle>();
            var componentInput = GetCurrentInput(component);
            var guideLine = componentInput.Item2;

            var h1 = _handleManager.CreatePointHandle(guideLine.Point1, HandleLocationType.First, HandleEffectType.Property);
            h1.DragOngoing += GuidelineHandleStart_DragOngoing;
            h1.DragEnded += GuidelineHandleStart_DragEnded;

            var h2 = _handleManager.CreatePointHandle(guideLine.Point2, HandleLocationType.Last, HandleEffectType.Property);
            h2.DragOngoing += GuidelineHandleEnd_DragOngoing;
            h2.DragEnded += GuidelineHandleEnd_DragEnded;

            handles.Add(h1);
            handles.Add(h2);
            return handles;
        }

        /// <inheritdoc />
        public override void UpdateContext()
        {
            if(Component == null) return;

            //Update internal logic to take into account changes from plugin
            _uiData = Component.GetDataFromComponent();
            var componentInput = GetCurrentInput(Component);
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Refresh existing manipulator handles from plugin input
            UpdatePolygonHandleManipulators(componentInput);
            UpdateLineManipulators(componentInput);
            UpdateGuideHandleManipulators(componentInput);

            //Re-create all Joist center to center manipulators
            _distanceManipulators = null;
            CreateJoistDistanceManipulators(Component);

            //Update graphics based on plugin and manipulators
            ReCreateGraphics(); //todo?
        }



        private void UpdateLineManipulators(Tuple<List<Point>, LineSegment> componentInput)
        {
            if (componentInput == null) throw new ArgumentNullException(nameof(componentInput));
            if(_guideLine == null || _guideLine.Count !=2) return;

            //Update guideline manipulator
            var ls = componentInput.Item2;
            _guideLine[0].Line.Point1 = new Point(ls.Point1);
            _guideLine[0].Line.Point2 = new Point(ls.Point2);

            //Update edge line manipulators
            Point lastPoint = null;
            for (var i = 0; i < _polygonHandles.Count; i++)
            {
                var pt = _polygonHandles[i];
                if (i==0)
                {
                    lastPoint = pt.Point;
                    continue;
                }
                _edgeLines[i-1].Line = new LineSegment(lastPoint, pt.Point);

                //Complete loop if last point in array
                if (i == _polygonHandles.Count - 1)
                {
                    _edgeLines[i].Line = new LineSegment(pt.Point, _polygonHandles[0].Point);
                }
            }
        }

        private void UpdateGuideHandleManipulators(Tuple<List<Point>, LineSegment> componentInput)
        {
            if (componentInput == null) throw new ArgumentNullException(nameof(componentInput));
            if (_guidelineHandles == null || _guidelineHandles.Count != 2) return;

            var ls = componentInput.Item2;
            _guidelineHandles[0].Point = new Point(ls.Point1);
            _guidelineHandles[1].Point = new Point(ls.Point2);
        }

        private void UpdatePolygonHandleManipulators(Tuple<List<Point>, LineSegment> componentInput)
        {
            if (componentInput == null) throw new ArgumentNullException(nameof(componentInput));
            if(_polygonHandles == null) return;

            var index = 0;
            foreach (var pt in componentInput.Item1)
            {
                _polygonHandles[index].Point = new Point(pt);
                index++;
            }
        }


        private void ModifyPluginInputFromManipulators()
        {
            if (_polygonHandles == null) throw new ArgumentNullException(nameof(_polygonHandles));
            if (_guidelineHandles == null) throw new ArgumentNullException(nameof(_guidelineHandles));

            var originalInput = Component.GetComponentInput();
            if (originalInput == null) return;
            var input = new ComponentInput();

            //Add polygon input
            var polygon = new Polygon();
            foreach (var pg in _polygonHandles)
            {
                polygon.Points.Add(new Point(pg.Point));
            }
            input.AddInputPolygon(polygon);

            //Add guideline input
            var gp1 = _guidelineHandles[0].Point;
            var gp2 = _guidelineHandles[1].Point;
            input.AddTwoInputPositions(gp1, gp2);

            //Call component to update
            ModifyComponentInput(input);
        }




        private void GuidelineHandleEnd_DragEnded(object sender, DragEventArgs e)
        {
            if (_guidelineHandles == null || _guideLine == null) return;
            if (_guidelineHandles.Count != 2 || _guideLine.Count != 2) return;

            //Update main Guidline handle objects from dragged line
            var ls = _guideLine[0].Line;
            _guidelineHandles[0].Point = new Point(ls.Point1);
            _guidelineHandles[1].Point = new Point(ls.Point2);

            //Update plugin input from manipulators
            ModifyPluginInputFromManipulators();
        }

        private void GuidelineHandleEnd_DragOngoing(object sender, DragEventArgs e)
        { }

        private void GuidelineHandleStart_DragEnded(object sender, DragEventArgs e)
        {
            ModifyPluginInputFromManipulators();
        }

        private void GuidelineHandleStart_DragOngoing(object sender, DragEventArgs e)
        {
            ReCreateGraphics();
        }

        private void GuideLine_DragEnded(object sender, DragEventArgs e)
        {
            ModifyPluginInputFromManipulators();
        }

        private void GuideLine_DragOngoing(object sender, DragEventArgs e)
        {
            ReCreateGraphics();
        }

        private void EdgeSegment_DragEnded(object sender, DragEventArgs e)
        {
            var originalPt = new Point(e.StartPosition);
            var movedLocation = new Point(e.StartPosition);
            movedLocation.Translate(e.TotalTranslation);
            FindUpdatePolygonFromEdgePtChange(originalPt, movedLocation);

            ModifyPluginInputFromManipulators();
        }

        private void FindUpdatePolygonFromEdgePtChange(Point originalPt, Point movedLocation)
        {
            if(originalPt==null || movedLocation == null || _polygonHandles==null) return;
            for (var i = 0; i < _polygonHandles.Count; i++)
            {
                var pt = _polygonHandles[i].Point;
                if (Tekla.Structures.Geometry3d.Distance.PointToPoint(originalPt, pt) <
                    GeometryConstants.DISTANCE_EPSILON)
                {
                    _polygonHandles[i].Point = movedLocation;
                }
            }
        }

        private void EdgeSegment_DragOngoing(object sender, DragEventArgs e)
        {
            ReCreateGraphics();
        }

        private void PolygonHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            ReCreateGraphics();
        }

        private void PolygonHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Detaches the event handlers.
        /// </summary>
        private void DetachHandlers()
        {
            //Detach polygon shape handles
            _polygonHandles?.ForEach(handle =>
            {
                handle.DragOngoing -= PolygonHandleDragOngoing;
                handle.DragEnded -= PolygonHandleDragEnded;
            });

            //Detach guideline individual two handles
            if (_guidelineHandles != null && _guidelineHandles.Count == 2)
            {
                var h1 = _guidelineHandles[0];
                h1.DragOngoing -= GuidelineHandleStart_DragOngoing;
                h1.DragEnded -= GuidelineHandleStart_DragEnded;

                var h2 = _guidelineHandles[1];
                h2.DragOngoing -= GuidelineHandleEnd_DragOngoing;
                h2.DragEnded -= GuidelineHandleEnd_DragEnded;
            }

            //Detach Guideline line modifier
            _guideLine?.ForEach(handle =>
            {
                handle.DragOngoing -= GuideLine_DragOngoing;
                handle.DragEnded -= GuideLine_DragEnded;
            });

            //Detach Edges line modifiers
            _edgeLines?.ForEach(handle =>
            {
                handle.DragOngoing -= EdgeSegment_DragOngoing;
                handle.DragEnded -= EdgeSegment_DragEnded;
            });
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DetachHandlers();
            _polygonHandles.ForEach(f => f.Dispose());
            _guidelineHandles.ForEach(f=>f.Dispose());
            _guideLine.ForEach(f => f.Dispose());
            _edgeLines.ForEach(f=>f.Dispose());
        }

        private static Tuple<List<Point>, LineSegment> GetCurrentInput(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            List<Point> polygonPts = null;
            LineSegment guideLine = null;
            var ci = component.GetComponentInput();
            if (ci == null) return null;

            foreach (var inputItem in ci)
            {
                var item = inputItem as InputItem;
                if (item == null) continue;

                switch (item.GetInputType())
                {
                    case InputItem.InputTypeEnum.INPUT_2_POINTS:
                        var guidPts = item.GetData() as ArrayList ?? new ArrayList();
                        guideLine = new LineSegment(guidPts[0] as Point, guidPts[1] as Point);
                        break;
                    case InputItem.InputTypeEnum.INPUT_POLYGON:
                        var pts = item.GetData() as ArrayList ?? new ArrayList();
                        polygonPts = pts.ToList();
                        break;
                    default:
                        throw new ApplicationException("GetCurrentInput: failed, Unexpected plugin input encountered...");
                }
            }
            return new Tuple<List<Point>, LineSegment>(polygonPts, guideLine);
        }
    }
}
