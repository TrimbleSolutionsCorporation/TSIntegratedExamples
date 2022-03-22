namespace JoistAreaFeatures.Manipulation
{
    using JoistArea.Logic;
    using JoistArea.Tools;
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
        private readonly IHandleManager handleManager;
        private List<PointHandle> guidelineHandles;
        private readonly LineHandle guideLine;
        private CoordinateSystem faceCoordinateSystem;

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
                //Cache local services
                this.handleManager = feature.HandleManager;
                if (this.handleManager == null) return;

                //Get data from plugin
                var componentInput = FeatureLogic.GetCurrentInput(this.Component);
                var uiData = this.Component.GetDataFromComponent();

                //Update internal logic to take into account latest data from plugin
                var liftingLogic = new JoistAreaMainLogic();
                liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);
                this.faceCoordinateSystem = new CoordinateSystem(liftingLogic.GlobalCoordinateSystem.Origin,
                    liftingLogic.GlobalCoordinateSystem.AxisX, liftingLogic.GlobalCoordinateSystem.AxisY);

                //Create and attach events for single line handle manipulator
                this.guideLine = this.CreateGuideLine(componentInput.Item2);

                //Create and attach events for two point handles
                this.guidelineHandles = this.CreateGuideLineHandles(componentInput.Item2);

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
            //Sync component data with model
            this.Component.Select();

            //Get data from plugin
            var componentInput = FeatureLogic.GetCurrentInput(this.Component);
            var uiData = this.Component.GetDataFromComponent();

            //Update internal logic to take into account latest data from plugin
            var liftingLogic = new JoistAreaMainLogic();
            liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);
            this.faceCoordinateSystem = new CoordinateSystem(liftingLogic.GlobalCoordinateSystem.Origin,
                liftingLogic.GlobalCoordinateSystem.AxisX, liftingLogic.GlobalCoordinateSystem.AxisY);

            //Refresh existing manipulators
            this.UpdateGuideLine(componentInput.Item2);
            this.UpdateGuideLinePoints(componentInput.Item2);

            //Update graphics based on plugin and manipulators
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            this.Graphics?.Clear();
            this.DrawGuidelineArrow(this.faceCoordinateSystem);
        }

        /// <summary>
        /// Creates guide line handle manipulator and attaches events
        /// </summary>
        /// <param name="ls">Line segment representing line</param>
        /// <returns>New LineHandle manipulator</returns>
        private LineHandle CreateGuideLine(LineSegment ls)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            try
            {
                var lineHandle = this.handleManager.CreateLineHandle(ls, HandleLocationType.Other, HandleEffectType.Geometry);
                lineHandle.DragOngoing += this.Guideline_DragOngoing;
                lineHandle.DragEnded += this.Guideline_DragEnded;
                return lineHandle;
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
        /// <param name="inputLine">LineSegment from component input</param>
        /// <returns>New list of PointHandles</returns>
        private List<PointHandle> CreateGuideLineHandles(LineSegment inputLine)
        {
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            try
            {
                var handles = new List<PointHandle>();

                var h1 = this.handleManager.CreatePointHandle(inputLine.Point1, HandleLocationType.First, HandleEffectType.Property);
                h1.DragOngoing += this.GuidelineHandle_DragOngoing;
                h1.DragEnded += this.GuidelineHandle_DragEnded;
                handles.Add(h1);

                var h2 = this.handleManager.CreatePointHandle(inputLine.Point2, HandleLocationType.Last, HandleEffectType.Property);
                h2.DragOngoing += this.GuidelineHandle_DragOngoing;
                h2.DragEnded += this.GuidelineHandle_DragEnded;
                handles.Add(h2);

                return handles;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Updates existing GuideLine LineHandle
        /// </summary>
        /// <param name="inputLine">LineSegment from component input</param>
        private void UpdateGuideLine(LineSegment inputLine)
        {
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            if (this.guideLine == null || this.guideLine.Line == null) return;
            if (this.guideLine.Line.Point1 == null || this.guideLine.Line.Point2 == null) return;

            this.guideLine.Line.Point1 = new Point(inputLine.Point1);
            this.guideLine.Line.Point2 = new Point(inputLine.Point2);
        }

        /// <summary>
        /// Updates existing GuideLine PointHandles
        /// </summary>
        /// <param name="inputLine">LineSegment from component input</param>
        private void UpdateGuideLinePoints(LineSegment inputLine)
        {
            if (inputLine == null) throw new ArgumentNullException(nameof(inputLine));
            if (this.guidelineHandles == null || this.guidelineHandles.Count != 2) return;

            this.guidelineHandles[0].Point = new Point(inputLine.Point1);
            this.guidelineHandles[1].Point = new Point(inputLine.Point2);
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragEnded(object sender, DragEventArgs e)
        {
            if (this.guidelineHandles == null || this.guideLine == null) return;
            if (this.guidelineHandles.Count != 2 || this.guideLine.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            this.guidelineHandles[0].Point = new Point(this.guideLine.Line.Point1);
            this.guidelineHandles[1].Point = new Point(this.guideLine.Line.Point2);

            //DM plugin input update (also calls plugin.Modify() and new Model.CommitChanges() for you)
            this.ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuideLine move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void Guideline_DragOngoing(object sender, DragEventArgs e)
        {
            if (this.guidelineHandles?.Count != 2) return;
            if (this.guideLine?.Line == null) return;

            //Update main GuidLine handle objects from dragged line
            this.guidelineHandles[0].Point = new Point(this.guideLine.Line.Point1);
            this.guidelineHandles[1].Point = new Point(this.guideLine.Line.Point2);

            //Update graphics for GuideLine as handles move
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Event handler for GuideLineHandle move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuidelineHandle_DragEnded(object sender, DragEventArgs e)
        {
            if (this.guidelineHandles?.Count != 2) return;
            if (this.guideLine?.Line == null) return;

            this.guideLine.Line.Point1 = new Point(this.guidelineHandles[0].Point);
            this.guideLine.Line.Point2 = new Point(this.guidelineHandles[1].Point);

            //DM plugin input update (also calls plugin.Modify() and new Model.CommitChanges() for you)
            this.ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Event handler for GuidelineHandle move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DragEventArgs</param>
        private void GuidelineHandle_DragOngoing(object sender, DragEventArgs e)
        {
            if (this.guidelineHandles?.Count != 2) return;
            if (this.guideLine?.Line == null) return;

            this.guideLine.Line.Point1 = new Point(this.guidelineHandles[0].Point);
            this.guideLine.Line.Point2 = new Point(this.guidelineHandles[1].Point);

            //Update graphics for GuideLine as handles move
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();

            //Dispose point/handle type objects
            this.guidelineHandles?.ForEach(handle => handle.Dispose());
            this.guidelineHandles?.Clear();
            this.guideLine?.Dispose();

            //Dispose distance/measure type manipulators
            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            this.guidelineHandles?.ForEach(handle =>
            {
                handle.DragOngoing -= this.GuidelineHandle_DragOngoing;
                handle.DragEnded -= this.GuidelineHandle_DragEnded;
            });

            if (this.guideLine != null)
            {
                this.guideLine.DragOngoing -= this.Guideline_DragOngoing;
                this.guideLine.DragEnded -= this.Guideline_DragEnded;
            }
        }

        /// <summary>
        /// Updates model plugin for DM handle changes and commits changes
        /// </summary>
        private void ModifyPluginInputFromManipulators()
        {
            if (this.guidelineHandles == null) throw new ArgumentNullException(nameof(this.guidelineHandles));
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
                var gp1 = new Point(this.guidelineHandles[0].Point);
                var gp2 = new Point(this.guidelineHandles[1].Point);
                adjustedInput.AddTwoInputPositions(gp1, gp2);

                //DM plugin input update (also calls plugin.Modify() and new Model.CommitChanges() for you)
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
        private void DrawGuidelineArrow(CoordinateSystem faceCs)
        {
            if (faceCs == null) throw new ArgumentNullException(nameof(faceCs));
            if (this.guidelineHandles == null || this.guidelineHandles.Count != 2) return;
            if (this.Graphics == null) return;

            const double tipLength = 125.0;
            const double shaftThickness = 25.0;
            const double arrowThickness = 100.0;
            const double yAxisLength = 900.0;

            try
            {
                var ls = new LineSegment
                {
                    Point1 = this.guidelineHandles[0].Point,
                    Point2 = this.guidelineHandles[1].Point
                };
                var p0 = new Point(ls.Point1);
                var arrowHeadProfile = $"HXGON{arrowThickness}-{shaftThickness}";
                var shaftProfile = $"D{shaftThickness}";
                var yAxisCalc = Vector.Cross(faceCs.GetAxisZ(), ls.GetDirectionVector()).GetNormal();

                //Define x-axis arrow
                {
                    var pxTrans = new Point(ls.Point2);
                    pxTrans.Translate(ls.GetDirectionVector() * -tipLength);

                    var xShaft = new LineSegment(p0, pxTrans);
                    var xHead = new LineSegment(pxTrans, ls.Point2);

                    this.Graphics?.DrawProfile(shaftProfile, xShaft, new Vector(0, 0, 0), 0, LineType.Error);
                    this.Graphics?.DrawProfile(arrowHeadProfile, xHead, new Vector(0, 0, 0), 0, LineType.Error);
                    var textPt = new Point(ls.Point2).GetTranslated(faceCs.AxisX.GetNormal() * 75);
                    this.Graphics?.DrawText("X", textPt, TextRepresentationTypes.Label);
                }

                //Define y-axis arrow
                {
                    var yEnd = new Point(p0);
                    yEnd.Translate(yAxisCalc * yAxisLength);
                    var pyTrans = new Point(yEnd);
                    pyTrans.Translate(yAxisCalc * -tipLength);

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