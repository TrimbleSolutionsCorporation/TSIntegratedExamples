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

    /// <summary>
    /// Manipulation context class for creating manipulators for GuidLine feature
    /// </summary>
    public sealed class GuideLineMc : ManipulationContext
    {
        private readonly IHandleManager _handleManager;
        private List<PointHandle> _guidelineHandles;
        private LineHandle _guideLine;
        private JoistAreaData _uiData;
        private readonly JoistAreaMainLogic _liftingLogic;

        /// <summary>
        /// Main constructor to call methods to create GuideLine and handle manipulators
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="feature">DM feature from parent</param>
        public GuideLineMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            try
            {
                _handleManager = feature.HandleManager;

                //Get part and plugin information
                _uiData = component.GetDataFromComponent();
                var componentInput = FeatureLogic.GetCurrentInput(component);

                //Create new instance of logic service class
                _liftingLogic = new JoistAreaMainLogic();
                _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

                //Line and point manipulators
                _guideLine = CreateGuideLine(Component, componentInput.Item2);
                _guidelineHandles = CreateGuideLineHandles(Component, componentInput.Item2);
                AttachHandlers();

                //Draw custom graphics
                ReCreateGraphics();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Updates internal logic, calls method to update manipulators, and recreates graphics
        /// </summary>
        public override void UpdateContext()
        {
            base.UpdateContext();
            Component.Select();

            //Update internal logic to take into account changes from plugin
            _uiData = Component.GetDataFromComponent();
            var componentInput = FeatureLogic.GetCurrentInput(Component);
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Refresh existing manipulators
            UpdateGuideLine(Component, componentInput.Item2);
            UpdateGuideLinePoints(Component, componentInput.Item2);

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
        }

        /// <summary>
        /// Creates new LineHandle for GuideLine manipulation
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="inputLine">LineSegment from component input</param>
        /// <returns>New list of PointHandles</returns>
        private LineHandle CreateGuideLine(Component component, LineSegment inputLine)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            try
            {
                return _handleManager.CreateLineHandle(inputLine, HandleLocationType.First, HandleEffectType.Geometry);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Creates new PointHandles for GuideLine
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="inputLine">LineSegment from component input</param>
        /// <returns>New list of PointHandles</returns>
        private List<PointHandle> CreateGuideLineHandles(Component component, LineSegment inputLine)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            try
            {
                var handles = new List<PointHandle>();

                var h1 = _handleManager.CreatePointHandle(inputLine.Point1, HandleLocationType.First, HandleEffectType.Property);
                handles.Add(h1);

                var h2 = _handleManager.CreatePointHandle(inputLine.Point2, HandleLocationType.Last, HandleEffectType.Property);
                handles.Add(h2);

                return handles;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<PointHandle>();
            }
        }

        /// <summary>
        /// Updates existing GuideLine LineHandle
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="inputLine">LineSegment from component input</param>
        private void UpdateGuideLine(Component component, LineSegment inputLine)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            if (_guideLine == null || _guideLine.Line == null) return;
            if (_guideLine.Line.Point1 == null || _guideLine.Line.Point2 == null) return;

            _guideLine.Line.Point1 = new Point(inputLine.Point1);
            _guideLine.Line.Point2 = new Point(inputLine.Point2);
        }

        /// <summary>
        /// Updates existing GuideLine PointHandles
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="inputLine">LineSegment from component input</param>
        private void UpdateGuideLinePoints(Component component, LineSegment inputLine)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            if (_guidelineHandles == null || _guidelineHandles.Count != 2) return;

            _guidelineHandles[0].Point = new Point(inputLine.Point1);
            _guidelineHandles[1].Point = new Point(inputLine.Point2);
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
            DrawGuidelineArrow();
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
            DrawGuidelineArrow();
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DetachHandlers();

            _guideLine?.Dispose();
            _guidelineHandles.ForEach(handle => handle.Dispose());

            foreach (var manipulator in Manipulators)
            {
                manipulator.Dispose();
            }

            //Clear local caches
            _guidelineHandles.Clear();
        }

        /// <summary>
        /// Attach event handlers to each manipulator in cache
        /// </summary>
        private void AttachHandlers()
        {
            _guidelineHandles.ForEach(handle =>
            {
                handle.DragOngoing += GuidelineHandle_DragOngoing;
                handle.DragEnded += GuidelineHandle_DragEnded;
            });

            if (_guideLine != null)
            {
                _guideLine.DragOngoing += Guideline_DragOngoing;
                _guideLine.DragEnded += Guideline_DragEnded;
            }
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            _guidelineHandles.ForEach(handle =>
            {
                handle.DragOngoing -= GuidelineHandle_DragOngoing;
                handle.DragEnded -= GuidelineHandle_DragEnded;
            });

            if (_guideLine != null)
            {
                _guideLine.DragOngoing -= Guideline_DragOngoing;
                _guideLine.DragEnded -= Guideline_DragEnded;
            }
        }



        /// <summary>
        /// Updates model plugin for DM handle changes
        /// </summary>
        private void ModifyPluginInputFromManipulators()
        {
            if (_guidelineHandles == null) throw new ArgumentNullException(nameof(_guidelineHandles));
            try
            {
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
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Draws local x-y axis in model with part profile graphics
        /// </summary>
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
                    Point1 = _guidelineHandles[0].Point,
                    Point2 = _guidelineHandles[1].Point
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
    }
}