namespace JoistAreaFeatures.Manipulation
{
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using System;
    using System.Collections.Generic;
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
                this._handleManager = feature.HandleManager;

                //Get part and plugin information
                this._uiData = component.GetDataFromComponent();
                var componentInput = FeatureLogic.GetCurrentInput(component);

                //Create new instance of logic service class
                this._liftingLogic = new JoistAreaMainLogic();
                this._liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, this._uiData);

                //Line and point manipulators
                this._guideLine = this.CreateGuideLine(this.Component, componentInput.Item2);
                this._guidelineHandles = this.CreateGuideLineHandles(this.Component, componentInput.Item2);
                this.AttachHandlers();

                //Draw custom graphics
                this.ReCreateGraphics();
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
            this.Component.Select();

            //Update internal logic to take into account changes from plugin
            this._uiData = this.Component.GetDataFromComponent();
            var componentInput = FeatureLogic.GetCurrentInput(this.Component);
            this._liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, this._uiData);

            //Refresh existing manipulators
            this.UpdateGuideLine(this.Component, componentInput.Item2);
            this.UpdateGuideLinePoints(this.Component, componentInput.Item2);

            //Update graphics based on plugin and manipulators
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            this.Graphics?.Clear();
            this.DrawGuidelineArrow();
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
                return this._handleManager.CreateLineHandle(inputLine, HandleLocationType.First, HandleEffectType.Geometry);
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

                var h1 = this._handleManager.CreatePointHandle(inputLine.Point1, HandleLocationType.First, HandleEffectType.Property);
                handles.Add(h1);

                var h2 = this._handleManager.CreatePointHandle(inputLine.Point2, HandleLocationType.Last, HandleEffectType.Property);
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
            if (this._guideLine == null || this._guideLine.Line == null) return;
            if (this._guideLine.Line.Point1 == null || this._guideLine.Line.Point2 == null) return;

            this._guideLine.Line.Point1 = new Point(inputLine.Point1);
            this._guideLine.Line.Point2 = new Point(inputLine.Point2);
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
            if (this._guidelineHandles == null || this._guidelineHandles.Count != 2) return;

            this._guidelineHandles[0].Point = new Point(inputLine.Point1);
            this._guidelineHandles[1].Point = new Point(inputLine.Point2);
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragEnded(object sender, DragEventArgs e)
        {
            if (this._guidelineHandles == null || this._guideLine == null) return;
            if (this._guidelineHandles.Count != 2 || this._guideLine.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            this._guidelineHandles[0].Point = new Point(this._guideLine.Line.Point1);
            this._guidelineHandles[1].Point = new Point(this._guideLine.Line.Point2);

            //Update plugin input from manipulators
            this.ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragOngoing(object sender, DragEventArgs e)
        {
            if (this._guidelineHandles?.Count != 2) return;
            if (this._guideLine?.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            this._guidelineHandles[0].Point = new Point(this._guideLine.Line.Point1);
            this._guidelineHandles[1].Point = new Point(this._guideLine.Line.Point2);

            //Update graphics for GuideLine as handles move
            this.DrawGuidelineArrow();
        }

        /// <summary>
        /// Event handler for GuideLineHandle move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuidelineHandle_DragEnded(object sender, DragEventArgs e)
        {
            if (this._guidelineHandles?.Count != 2) return;
            if (this._guideLine?.Line == null) return;

            this._guideLine.Line.Point1 = new Point(this._guidelineHandles[0].Point);
            this._guideLine.Line.Point2 = new Point(this._guidelineHandles[1].Point);

            //Update plugin input from manipulators
            this.ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuidelineHandle move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void GuidelineHandle_DragOngoing(object sender, DragEventArgs e)
        {
            if (this._guidelineHandles?.Count != 2) return;
            if (this._guideLine?.Line == null) return;

            this._guideLine.Line.Point1 = new Point(this._guidelineHandles[0].Point);
            this._guideLine.Line.Point2 = new Point(this._guidelineHandles[1].Point);

            //Update graphics for GuideLine as handles move
            this.DrawGuidelineArrow();
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();

            this._guideLine?.Dispose();
            this._guidelineHandles.ForEach(handle => handle.Dispose());

            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }

            //Clear local caches
            this._guidelineHandles.Clear();
        }

        /// <summary>
        /// Attach event handlers to each manipulator in cache
        /// </summary>
        private void AttachHandlers()
        {
            this._guidelineHandles.ForEach(handle =>
            {
                handle.DragOngoing += this.GuidelineHandle_DragOngoing;
                handle.DragEnded += this.GuidelineHandle_DragEnded;
            });

            if (this._guideLine != null)
            {
                this._guideLine.DragOngoing += this.Guideline_DragOngoing;
                this._guideLine.DragEnded += this.Guideline_DragEnded;
            }
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            this._guidelineHandles.ForEach(handle =>
            {
                handle.DragOngoing -= this.GuidelineHandle_DragOngoing;
                handle.DragEnded -= this.GuidelineHandle_DragEnded;
            });

            if (this._guideLine != null)
            {
                this._guideLine.DragOngoing -= this.Guideline_DragOngoing;
                this._guideLine.DragEnded -= this.Guideline_DragEnded;
            }
        }



        /// <summary>
        /// Updates model plugin for DM handle changes
        /// </summary>
        private void ModifyPluginInputFromManipulators()
        {
            if (this._guidelineHandles == null) throw new ArgumentNullException(nameof(this._guidelineHandles));
            try
            {
                var pastInput = FeatureLogic.GetCurrentInput(this.Component);
                var adjustedInput = new ComponentInput();

                //Add polygon input from previous run
                var polygon = new Polygon();
                foreach (var pg in pastInput.Item1)
                {
                    polygon.Points.Add(new Point(pg));
                }
                adjustedInput.AddInputPolygon(polygon);

                //Add new adjusted guideline input
                var gp1 = new Point(this._guidelineHandles[0].Point);
                var gp2 = new Point(this._guidelineHandles[1].Point);
                adjustedInput.AddTwoInputPositions(gp1, gp2);

                //Call component to update
                this.ModifyComponentInput(adjustedInput);
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
            if (this._liftingLogic == null) throw new ArgumentNullException(nameof(this._liftingLogic));
            if (this._guidelineHandles == null || this._guidelineHandles.Count != 2) return;
            if (this.Graphics == null) return;

            const double TipLength = 125.0;
            const double ShaftThickness = 25.0;
            const double ArrowThickness = 100.0;
            const double yAxisLength = 900.0;

            try
            {
                var guideLine = new LineSegment
                {
                    Point1 = this._guidelineHandles[0].Point,
                    Point2 = this._guidelineHandles[1].Point
                };
                var globalCs = this._liftingLogic.GlobalCoordinateSystem;
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

                    this.Graphics?.DrawProfile(shaftProfile, xShaft, new Vector(0, 0, 0), 0, LineType.Error);
                    this.Graphics?.DrawProfile(arrowHeadProfile, xHead, new Vector(0, 0, 0), 0, LineType.Error);
                    var textPt = new Point(guideLine.Point2).GetTranslated(globalCs.AxisX.GetNormal() * 75);
                    this.Graphics?.DrawText("X", textPt, TextRepresentationTypes.Label);
                }

                //Define y-axis arrow
                {
                    var yEnd = new Point(p0);
                    yEnd.Translate(yAxisCalc * yAxisLength);
                    var pyTrans = new Point(yEnd);
                    pyTrans.Translate(yAxisCalc * -TipLength);

                    var yShaft = new LineSegment(p0, pyTrans);
                    var yHead = new LineSegment(pyTrans, yEnd);

                    this.Graphics?.DrawProfile(shaftProfile, yShaft, new Vector(0, 0, 0));
                    this.Graphics?.DrawProfile(arrowHeadProfile, yHead, new Vector(0, 0, 0));
                    var textPt = new Point(yEnd).GetTranslated(yAxisCalc * 75);
                    this.Graphics?.DrawText("Y", textPt, TextRepresentationTypes.Label);
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }
    }
}