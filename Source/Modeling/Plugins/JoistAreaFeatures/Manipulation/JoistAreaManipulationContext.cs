namespace JoistAreaFeatures.Manipulation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Datatype;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;
    using Distance = Tekla.Structures.Datatype.Distance;
    using DragEventArgs = Tekla.Structures.Plugins.DirectManipulation.Services.Handles.DragEventArgs;

    public sealed class JoistAreaManipulationContext : ManipulationContext
    {
        private IHandleManager _handleManager;
        private List<PointHandle> _polygonHandles;
        private List<PointHandle> _guidelineHandles;
        private LineHandle _guideLine;
        private readonly IGraphicsDrawer _graphics;
        private List<DistanceManipulator> _joistDistanceManipulators;
        private JoistAreaData _uiData;
        private readonly JoistAreaMainLogic _liftingLogic;

        public JoistAreaManipulationContext(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            try
            {
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
                CreateGuideLineManipulator(component);

                //Draw custom graphics
                ReCreateGraphics();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        public override void UpdateContext()
        {
            base.UpdateContext();
            Component.Select();

            //Update internal logic to take into account changes from plugin
            _uiData = Component.GetDataFromComponent();
            var componentInput = GetCurrentInput(Component);
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Refresh existing manipulator handles from plugin input
            UpdatePolygonHandleManipulators(componentInput);
            UpdateLineManipulators(componentInput);
            UpdateGuideHandleManipulators(componentInput);

            //Re-create all Joist center to center manipulators
            ReCreateJoistDistanceManipulators(Component);

            //Update graphics based on plugin and manipulators
            ReCreateGraphics(); //todo?
        }

        private void ReCreateGraphics()
        {
            _graphics.Clear();
            DrawGuidelineArrow();
            DrawPolygonMeasurements();
        }

        private void DrawPolygonMeasurements()
        {
            Point lastPoint = null;
            foreach (var pg in _polygonHandles)
            {
                if (lastPoint != null)
                {
                    var currPt = new Point(pg.Point);
                    var ls = new LineSegment(lastPoint, currPt);
                    _graphics.DrawDimension(ls, null, DimensionEndPointSizeType.FixedSmall);
                }

                lastPoint = new Point(pg.Point);
            }
        }

        private void DrawGuidelineArrow()
        {
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));
            if (_graphics == null) throw new ArgumentNullException(nameof(_graphics));
            if (_guidelineHandles == null || _guidelineHandles.Count != 2) return;

            const double TipLength = 125.0;
            const double ShaftThickness = 25.0;
            const double ArrowThickness = 100.0;
            const double yAxisLength = 900.0;

            try
            {
                var guideLine = new LineSegment
                {
                    Point1 = _guidelineHandles[0].Point, Point2 = _guidelineHandles[1].Point
                };
                var globalCs = _liftingLogic.GlobalCoordinateSystem;
                var p0 = new Point(guideLine.Point1);
                var arrowHeadProfile = $"HXGON{ArrowThickness}-{ShaftThickness}";
                var shaftProfile = $"D{ShaftThickness}";
                var yAxisCalc = Vector.Cross(globalCs.GetAxisZ(), guideLine.GetDirectionVector()).GetNormal();

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
                    yEnd.Translate(yAxisCalc * yAxisLength);
                    var pyTrans = new Point(yEnd);
                    pyTrans.Translate(yAxisCalc * -TipLength);

                    var yShaft = new LineSegment(p0, pyTrans);
                    var yHead = new LineSegment(pyTrans, yEnd);

                    _graphics.DrawProfile(shaftProfile, yShaft, new Vector(0, 0, 0));
                    _graphics.DrawProfile(arrowHeadProfile, yHead, new Vector(0, 0, 0));
                    var textPt = new Point(yEnd).GetTranslated(yAxisCalc * 75);
                    _graphics.DrawText("Y", textPt, TextRepresentationTypes.Label);
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private void CreatePointManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _polygonHandles?.Clear();
            _guidelineHandles?.Clear();

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
                _guidelineHandles.ForEach(handle =>
                {
                    handle.DragOngoing += GuidelineHandle_DragOngoing;
                    handle.DragEnded += GuidelineHandle_DragEnded;
                });
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }


        private void CreateGuideLineManipulator(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _guideLine = null;

            try
            {
                var componentInput = GetCurrentInput(component);
                var pts = componentInput.Item1;
                var ls = componentInput.Item2;

                //Add line manipulator for GuidLine segment
                {
                    var guidSegment = _handleManager.CreateLineHandle(ls, HandleLocationType.First, HandleEffectType.Geometry);
                    guidSegment.DragOngoing += Guideline_DragOngoing;
                    guidSegment.DragEnded += Guideline_DragEnded;
                    _guideLine = guidSegment;
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

            if(_joistDistanceManipulators == null) _joistDistanceManipulators = new List<DistanceManipulator>();
            else _joistDistanceManipulators.Clear();

            try
            {
                //Get joist spacings and distance values from base plugin logic
                var componentInput = GetCurrentInput(component);
                var joistSpacingsGlobal = FeatureLogic.GetJoistSpanSegments(_liftingLogic);
                if (joistSpacingsGlobal == null || joistSpacingsGlobal.Count < 1) return;
                var distValues = new ArrayList(_liftingLogic.DistanceList.DistanceValues);

                //Create first joist to guideline offset
                var j1 = joistSpacingsGlobal[0];
                var px = Projection.PointToLine(j1.Point1, new Line(componentInput.Item2));
                if (px != null)
                {
                    var spacingSeg1 = new LineSegment(j1.Point1, px);
                    var distStartMan = new DistanceManipulator(component, this, spacingSeg1);
                    distStartMan.MeasureChanged += delegate
                    {
                        DmCommon.ModifyComponent(component, "FirstJoistOffset", distStartMan.Segment.Length());
                    };
                    AddManipulator(distStartMan);
                    _joistDistanceManipulators.Add(distStartMan);
                }

                //Depending on spacing type add distance manipulator between each joist
                switch (_liftingLogic.SpacingTyp)
                {
                    //Distance type spacing, handle segment as spacing value in larger string list
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                        for (var i = 0; i < joistSpacingsGlobal.Count; i++)
                        {
                            var ls = joistSpacingsGlobal[i];
                            var distMan = new DistanceManipulator(component, this, ls);
                            distMan.MeasureChanged += DistManExact_OnMeasureChanged;
                            AddManipulator(distMan);
                            _joistDistanceManipulators.Add(distMan);
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
                                DmCommon.ModifyComponent(component, "CenterSpacingMax", distMan.Segment.Length());
                            };
                            AddManipulator(distMan);
                            _joistDistanceManipulators.Add(distMan);
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

        private void DistManExact_OnMeasureChanged(object sender, EventArgs e)
        {
            var allSpacings = new List<Distance>();
            for (var i = 1; i < _joistDistanceManipulators.Count; i++)
            {
                var distMan = _joistDistanceManipulators[i];
                allSpacings.Add(new Distance(distMan.Segment.Length()));
            }

            var modDistList = new DistanceList(allSpacings);
            var modDistList2 = new TxDistanceList(allSpacings);
            DmCommon.ModifyComponent(Component, "CenterSpacingList", modDistList.ToString());
        }

        private void ReCreateJoistDistanceManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));

            if (_joistDistanceManipulators != null)
            {
                //todo: how to dispose of old manipulators, qty changed, need new?
                //todo: below code causes distance manipulators to not generate/show
                //_distanceManipulators.ForEach(f => f.Dispose());
                _joistDistanceManipulators.Clear();
            }

            CreateJoistDistanceManipulators(component);
        }

        private List<PointHandle> GetPolygonPointHandles(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (_handleManager == null) throw new ArgumentNullException(nameof(_handleManager));

            var handles = new List<PointHandle>();
            var componentInput = GetCurrentInput(component);
            foreach (var pt in componentInput.Item1)
            {
                var handle =
                    _handleManager.CreatePointHandle(pt, HandleLocationType.InputPoint, HandleEffectType.Geometry);
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

            var h1 = _handleManager.CreatePointHandle(guideLine.Point1,
                HandleLocationType.First, HandleEffectType.Property);
            handles.Add(h1);

            var h2 = _handleManager.CreatePointHandle(guideLine.Point2,
                HandleLocationType.Last, HandleEffectType.Property);
            handles.Add(h2);

            return handles;
        }


        private void UpdateLineManipulators(Tuple<List<Point>, LineSegment> componentInput)
        {
            if (componentInput == null) throw new ArgumentNullException(nameof(componentInput));
            if (_guideLine == null || _guideLine.Line == null) return;

            //Update guideline manipulator
            var ls = componentInput.Item2;
            _guideLine.Line.Point1 = new Point(ls.Point1);
            _guideLine.Line.Point2 = new Point(ls.Point2);
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
            if (_polygonHandles == null) return;

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
            var adjustedInput = new ComponentInput();

            //Add polygon input
            var polygon = new Polygon();
            foreach (var pg in _polygonHandles)
            {
                polygon.Points.Add(new Point(pg.Point));
            }

            adjustedInput.AddInputPolygon(polygon);

            //Add guideline input
            var gp1 = new Point(_guidelineHandles[0].Point);
            var gp2 = new Point(_guidelineHandles[1].Point);
            adjustedInput.AddTwoInputPositions(gp1, gp2);

            //Call component to update
            ModifyComponentInput(adjustedInput);
        }

        private void Guideline_DragEnded(object sender, DragEventArgs e)
        {
            if (_guidelineHandles == null || _guideLine == null) return;
            if (_guidelineHandles.Count != 2 || _guideLine.Line == null) return;

            //Update main Guidline handle objects from dragged line
            _guidelineHandles[0].Point = new Point(_guideLine.Line.Point1);
            _guidelineHandles[1].Point = new Point(_guideLine.Line.Point2);

            //Update plugin input from manipulators
            ModifyPluginInputFromManipulators();
        }

        private void Guideline_DragOngoing(object sender, DragEventArgs e)
        {
            if (_guidelineHandles == null || _guideLine == null) return;
            if (_guidelineHandles.Count != 2 || _guideLine.Line == null) return;

            //Update main Guidline handle objects from dragged line
            _guidelineHandles[0].Point = new Point(_guideLine.Line.Point1);
            _guidelineHandles[1].Point = new Point(_guideLine.Line.Point2);

            ReCreateGraphics();
        }

        private void GuidelineHandle_DragEnded(object sender, DragEventArgs e)
        {
            //Update guideline manipulator
            if (_guideLine != null && _guideLine.Line != null && _guidelineHandles != null &&
                _guidelineHandles.Count == 2)
            {
                _guideLine.Line.Point1 = new Point(_guidelineHandles[0].Point);
                _guideLine.Line.Point2 = new Point(_guidelineHandles[1].Point);
            }

            //Update plugin input from manipulators
            ModifyPluginInputFromManipulators();
        }

        private void GuidelineHandle_DragOngoing(object sender, DragEventArgs e)
        {
            //Update guideline manipulator
            if (_guideLine != null && _guideLine.Line != null && _guidelineHandles != null &&
                _guidelineHandles.Count == 2)
            {
                _guideLine.Line.Point1 = new Point(_guidelineHandles[0].Point);
                _guideLine.Line.Point2 = new Point(_guidelineHandles[1].Point);
            }
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
            _guidelineHandles?.ForEach(handle =>
            {
                handle.DragOngoing -= GuidelineHandle_DragOngoing;
                handle.DragEnded -= GuidelineHandle_DragEnded;
            });

            //Detach Guideline line modifier
            _guideLine.DragOngoing -= Guideline_DragOngoing;
            _guideLine.DragOngoing -= Guideline_DragOngoing;

            //Detach distance manipulators
            //todo: detach here, point distance manipulators, delegate method?
            //todo: Any need to detach events for joist distance manipulators?
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
            _guidelineHandles.ForEach(f => f.Dispose());
            _guideLine.Dispose();
            _joistDistanceManipulators.ForEach(f => f.Dispose());
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
                        throw new ApplicationException(
                            "GetCurrentInput: failed, Unexpected plugin input encountered...");
                }
            }

            return new Tuple<List<Point>, LineSegment>(polygonPts, guideLine);
        }
    }
}