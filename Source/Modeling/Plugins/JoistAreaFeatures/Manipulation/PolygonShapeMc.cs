namespace JoistAreaFeatures.Manipulation
{
    using JoistArea.Logic;
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
    /// Main class to create point handle manipulators to plugin input points
    /// </summary>
    public sealed class PolygonShapeMc : ManipulationContext
    {
        private readonly IHandleManager handleManager;
        private List<PointHandle> polygonHandles;

        /// <summary>
        /// Main constructor to call methods to create PointHandles
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="feature">DM parent feature</param>
        public PolygonShapeMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            try
            {
                //Cache local services
                this.handleManager = feature.HandleManager;
                if (this.handleManager == null) return;

                //Get part and plugin information
                var componentInput = FeatureLogic.GetCurrentInput(component);

                //Create and attach events for polygon handle manipulators
                this.polygonHandles = this.CreatePointHandles(componentInput.Item1);

                //Draw custom graphics
                this.ReCreateGraphics();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Updates internal logic, calls method to update PointHandles, and recreates graphics
        /// </summary>
        public override void UpdateContext()
        {
            //Sync component data with model
            this.Component.Select();

            //Get data from plugin
            var componentInput = FeatureLogic.GetCurrentInput(this.Component);

            //Refresh existing manipulator handles from plugin input
            this.UpdatePointHandles(componentInput.Item1);

            //Update graphics based on plugin and manipulators
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            this.Graphics?.Clear();
            this.DrawPolygonEdgeDimensions();
        }

        /// <summary>
        /// Creates new PointHandles through HandleManager and caches both PointHandles
        /// </summary>
        /// <param name="pluginPoints">Plugin component input current points</param>
        /// <returns>New list of created and active point handles</returns>
        private List<PointHandle> CreatePointHandles(List<Point> pluginPoints)
        {
            if (pluginPoints == null) throw new ArgumentNullException(nameof(pluginPoints));
            if (pluginPoints.Count < 3) throw new ArgumentException("Error: pluginPoints must contain at least 3 points...");
            var result = new List<PointHandle>();
            try
            {
                foreach (var pt in pluginPoints)
                {
                    var handle = this.handleManager.CreatePointHandle(pt, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                    handle.DragOngoing += this.OnPointHandleDragOngoing;
                    handle.DragEnded += this.OnPointHandleDragEnded;
                    result.Add(handle);
                }
                return result;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Refresh existing manipulator handles from plugin input
        /// </summary>
        /// <param name="pluginPoints">Plugin component input current points</param>
        private void UpdatePointHandles(List<Point> pluginPoints)
        {
            if (pluginPoints == null) throw new ArgumentNullException(nameof(pluginPoints));
            if (pluginPoints.Count < 3) throw new ArgumentException("Error: pluginPoints must contain at least 3 points...");
            try
            {
                var index = 0;
                foreach (var pt in pluginPoints)
                {
                    this.polygonHandles[index].Point = new Point(pt);
                    index++;
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Draw outer polygon edge segment dimension graphics
        /// </summary>
        private void DrawPolygonEdgeDimensions()
        {
            if (this.polygonHandles == null || this.polygonHandles.Count < 3) return;
            try
            {
                //Draw edges around picked points
                Point lastPoint = null;
                foreach (var pg in this.polygonHandles)
                {
                    if (lastPoint != null)
                    {
                        var currPt = new Point(pg.Point);
                        var ls = new LineSegment(lastPoint, currPt);
                        this.Graphics?.DrawDimension(ls, null, DimensionEndPointSizeType.FixedMedium);
                    }
                    lastPoint = new Point(pg.Point);
                }

                //Draw connecting edge from last point to 1st
                var lsEnd = new LineSegment(this.polygonHandles[this.polygonHandles.Count - 1].Point, this.polygonHandles[0].Point);
                this.Graphics?.DrawDimension(lsEnd, null, DimensionEndPointSizeType.FixedMedium);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Update plugin - change model for moved polygon input points (DM side changes)
        /// </summary>
        private void ModifyPluginInputFromManipulators()
        {
            if (this.polygonHandles == null || this.polygonHandles.Count < 3) return;
            try
            {
                var pastInput = FeatureLogic.GetCurrentInput(this.Component);
                var adjustedInput = new ComponentInput();

                //Add polygon input adjusted new points
                var polygon = new Polygon();
                foreach (var pg in this.polygonHandles)
                {
                    polygon.Points.Add(new Point(pg.Point));
                }
                adjustedInput.AddInputPolygon(polygon);

                //Add guideline input from existing input
                var gp1 = new Point(pastInput.Item2.Point1);
                var gp2 = new Point(pastInput.Item2.Point2);
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
        /// Event handler for polygon points move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPointHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Event handler for polygon points move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPointHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            this.ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Call detach PointHandles then dispose of each
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();

            this.polygonHandles?.ForEach(handle => handle.Dispose());
            this.polygonHandles?.Clear();

            //Dispose distance/measure type manipulators
            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }
        }

        /// <summary>
        /// Detach event handlers for each PointHandle in cache
        /// </summary>
        private void DetachHandlers()
        {
            this.polygonHandles.ForEach(handle =>
            {
                handle.DragOngoing -= this.OnPointHandleDragOngoing;
                handle.DragEnded -= this.OnPointHandleDragEnded;
            });
        }
    }
}