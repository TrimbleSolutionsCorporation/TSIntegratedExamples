namespace JoistAreaFeatures.Manipulation
{
    using System;
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
    using DragEventArgs = Tekla.Structures.Plugins.DirectManipulation.Services.Handles.DragEventArgs;

    public sealed class GuideLineMc : ManipulationContext
    {
        private readonly IHandleManager _handleManager;
        private List<PointHandle> _guidelineHandles;
        private LineHandle _guideLine;
        private JoistAreaData _uiData;
        private readonly JoistAreaMainLogic _liftingLogic;

        public GuideLineMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            try
            {
                //_graphics = feature.Graphics;
                _handleManager = feature.HandleManager;

                //Get part and plugin information
                _uiData = component.GetDataFromComponent();
                //var uiDataStored = _featureBase.GetUserInterfaceData(component); //all blank
                var componentInput = FeatureLogic.GetCurrentInput(component);

                //Create new instance of logic service class
                _liftingLogic = new JoistAreaMainLogic();
                _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

                //Line and point manipulators
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
            var componentInput = FeatureLogic.GetCurrentInput(Component);
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Refresh existing manipulator handles from plugin input
            var ls = componentInput.Item2;
            if (_guideLine?.Line != null)
            {
                _guideLine.Line.Point1 = new Point(ls.Point1);
                _guideLine.Line.Point2 = new Point(ls.Point2);
            }
            if (_guidelineHandles?.Count == 2)
            {
                _guidelineHandles[0].Point = new Point(ls.Point1);
                _guidelineHandles[1].Point = new Point(ls.Point2);
            }

            //Update graphics based on plugin and manipulators
            ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            Graphics?.Clear();
            DrawGuidelineArrow();
            //DrawPolygonEdgeDimensions();
        }

        /// <summary>
        /// Draw outer polygon edge segment dimension graphics
        /// </summary>
        private void DrawPolygonEdgeDimensions()
        {
            var componentInput = FeatureLogic.GetCurrentInput(Component);

            //Draw edges around picked points
            Point lastPoint = null;
            var pts = componentInput.Item1;
            foreach (var pg in pts)
            {
                if (lastPoint != null)
                {
                    var currPt = new Point(pg);
                    var ls = new LineSegment(lastPoint, currPt);
                    Graphics?.DrawDimension(ls, null, DimensionEndPointSizeType.FixedSmall);
                }
                lastPoint = new Point(pg);
            }

            //Draw connecting edge from last point to 1st
            var lsEnd = new LineSegment(pts[pts.Count - 1], pts[0]);
            Graphics?.DrawDimension(lsEnd, null, DimensionEndPointSizeType.FixedMedium);
        }

        private void DrawGuidelineArrow()
        {
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));
            if (_guidelineHandles == null || _guidelineHandles.Count != 2) return;
            if (Graphics == null) return;

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

                    Graphics?.DrawProfile(shaftProfile, xShaft, new Vector(0, 0, 0), 0, LineType.Error);
                    Graphics?.DrawProfile(arrowHeadProfile, xHead, new Vector(0, 0, 0), 0, LineType.Error);
                    var textPt = new Point(guideLine.Point2).GetTranslated(globalCs.AxisX.GetNormal() * 75);
                    Graphics?.DrawText("X", textPt, TextRepresentationTypes.Label);
                }

                //Define y-axis arrow
                {
                    var yEnd = new Point(p0);
                    yEnd.Translate(yAxisCalc * yAxisLength);
                    var pyTrans = new Point(yEnd);
                    pyTrans.Translate(yAxisCalc * -TipLength);

                    var yShaft = new LineSegment(p0, pyTrans);
                    var yHead = new LineSegment(pyTrans, yEnd);

                    Graphics?.DrawProfile(shaftProfile, yShaft, new Vector(0, 0, 0));
                    Graphics?.DrawProfile(arrowHeadProfile, yHead, new Vector(0, 0, 0));
                    var textPt = new Point(yEnd).GetTranslated(yAxisCalc * 75);
                    Graphics?.DrawText("Y", textPt, TextRepresentationTypes.Label);
                }
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
            if(_guidelineHandles == null) _guidelineHandles = new List<PointHandle>();
            else _guidelineHandles.Clear();

            try
            {
                var componentInput = FeatureLogic.GetCurrentInput(component);
                var ls = componentInput.Item2;

                //Add line manipulator for GuidLine segment
                {
                    _guideLine =
                        _handleManager.CreateLineHandle(ls, HandleLocationType.First, HandleEffectType.Geometry);
                    _guideLine.DragOngoing += Guideline_DragOngoing;
                    _guideLine.DragEnded += Guideline_DragEnded;
                }

                //Get GuideLine handles from points
                var handles = new List<PointHandle>();
                var h1 = _handleManager.CreatePointHandle(_guideLine.Line.Point1,
                    HandleLocationType.First, HandleEffectType.Property);
                handles.Add(h1);

                var h2 = _handleManager.CreatePointHandle(_guideLine.Line.Point2,
                    HandleLocationType.Last, HandleEffectType.Property);
                handles.Add(h2);

                //Setup GuideLine handles and events
                _guidelineHandles = handles;
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

        /// <summary>
        /// Updates model plugin for DM handle changes
        /// </summary>
        private void ModifyPluginInputFromManipulators()
        {
            if (_guidelineHandles == null) throw new ArgumentNullException(nameof(_guidelineHandles));

            var pastInput = FeatureLogic.GetCurrentInput(Component);
            var adjustedInput = new ComponentInput();

            //Add polygon input from previous run
            var polygon = new Polygon();
            foreach (var pg in pastInput.Item1)
            {
                polygon.Points.Add(new Point(pg));
            }
            adjustedInput.AddInputPolygon(polygon);

            //Add new adjusted guideline input
            var gp1 = new Point(_guidelineHandles[0].Point);
            var gp2 = new Point(_guidelineHandles[1].Point);
            adjustedInput.AddTwoInputPositions(gp1, gp2);

            //Call component to update
            ModifyComponentInput(adjustedInput);
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragEnded(object sender, DragEventArgs e)
        {
            if (_guidelineHandles == null || _guideLine == null) return;
            if (_guidelineHandles.Count != 2 || _guideLine.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            _guidelineHandles[0].Point = new Point(_guideLine.Line.Point1);
            _guidelineHandles[1].Point = new Point(_guideLine.Line.Point2);

            //Update plugin input from manipulators
            ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragOngoing(object sender, DragEventArgs e)
        {
            if (_guidelineHandles?.Count != 2) return;
            if (_guideLine?.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            _guidelineHandles[0].Point = new Point(_guideLine.Line.Point1);
            _guidelineHandles[1].Point = new Point(_guideLine.Line.Point2);

            //Update graphics for GuideLine as handles move
            ReCreateGraphics();
        }

        /// <summary>
        /// Event handler for GuideLineHandle move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuidelineHandle_DragEnded(object sender, DragEventArgs e)
        {
            if (_guidelineHandles?.Count != 2) return;
            if (_guideLine?.Line == null) return;

            _guideLine.Line.Point1 = new Point(_guidelineHandles[0].Point);
            _guideLine.Line.Point2 = new Point(_guidelineHandles[1].Point);

            //Update plugin input from manipulators
            ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuidelineHandle move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void GuidelineHandle_DragOngoing(object sender, DragEventArgs e)
        {
            if (_guidelineHandles?.Count != 2) return;
            if (_guideLine?.Line == null) return;

            _guideLine.Line.Point1 = new Point(_guidelineHandles[0].Point);
            _guideLine.Line.Point2 = new Point(_guidelineHandles[1].Point);

            //Update graphics for GuideLine as handles move
            ReCreateGraphics();
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //Detach GuideLine handle events and dispose handles
            _guidelineHandles?.ForEach(handle =>
            {
                handle.DragOngoing -= GuidelineHandle_DragOngoing;
                handle.DragEnded -= GuidelineHandle_DragEnded;
                handle.Dispose();
            });
            _guidelineHandles?.Clear();

            //Detach GuideLine events and dispose
            if (_guideLine != null)
            {
                _guideLine.DragOngoing -= Guideline_DragOngoing;
                _guideLine.DragOngoing -= Guideline_DragOngoing;
                _guideLine.Dispose();
                _guideLine = null;
            }
        }
    }
}