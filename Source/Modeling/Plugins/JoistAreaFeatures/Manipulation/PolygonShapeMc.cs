namespace JoistAreaFeatures.Manipulation
{
    using System;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using Services;
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
        private readonly IHandleManager _handleManager;
        private List<PointHandle> _polygonHandles;

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
                _handleManager = feature.HandleManager;

                //Create and setup PointHandle manipulators
                _polygonHandles = CreatePointHandles(component);
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
        /// Updates internal logic, calls method to update PointHandles, and recreates graphics
        /// </summary>
        public override void UpdateContext()
        {
            base.UpdateContext();
            Component.Select();

            //Refresh existing manipulator handles from plugin input
            UpdatePointHandles(Component);

            //Update graphics based on plugin and manipulators
            ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            Graphics?.Clear();
            DrawPolygonEdgeDimensions();
        }

        /// <summary>
        /// Creates new PointHandles through HandleManager and caches both PointHandles
        /// </summary>
        /// <param name="plugin">Model plugin instance</param>
        /// <returns></returns>
        private List<PointHandle> CreatePointHandles(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            var handles = new List<PointHandle>();

            try
            {
                var componentInput = FeatureLogic.GetCurrentInput(plugin);
                foreach (var pt in componentInput.Item1)
                {
                    var handle = _handleManager.CreatePointHandle(pt, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                    _polygonHandles.Add(handle);
                }
                return handles;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<PointHandle>();
            }
        }

        /// <summary>
        /// Refresh existing manipulator handles from plugin input
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        private void UpdatePointHandles(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            try
            {
                //Get input objects from component
                var componentInput = FeatureLogic.GetCurrentInput(component);
                //Update each PointHandle position
                var index = 0;
                foreach (var pt in componentInput.Item1)
                {
                    _polygonHandles[index].Point = new Point(pt);
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
            try
            {
                //Draw edges around picked points
                Point lastPoint = null;
                foreach (var pg in _polygonHandles)
                {
                    if (lastPoint != null)
                    {
                        var currPt = new Point(pg.Point);
                        var ls = new LineSegment(lastPoint, currPt);
                        Graphics?.DrawDimension(ls, null, DimensionEndPointSizeType.FixedMedium);
                    }
                    lastPoint = new Point(pg.Point);
                }

                //Draw connecting edge from last point to 1st
                var lsEnd = new LineSegment(_polygonHandles[_polygonHandles.Count - 1].Point, _polygonHandles[0].Point);
                Graphics?.DrawDimension(lsEnd, null, DimensionEndPointSizeType.FixedMedium);
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
            if (_polygonHandles == null) throw new ArgumentNullException(nameof(_polygonHandles));
            try
            {
                var pastInput = FeatureLogic.GetCurrentInput(Component);
                var adjustedInput = new ComponentInput();

                //Add polygon input adjusted new points
                var polygon = new Polygon();
                foreach (var pg in _polygonHandles)
                {
                    polygon.Points.Add(new Point(pg.Point));
                }
                adjustedInput.AddInputPolygon(polygon);

                //Add guideline input from existing input
                var gp1 = new Point(pastInput.Item2.Point1);
                var gp2 = new Point(pastInput.Item2.Point2);
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
        /// Event handler for polygon points move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPointHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            DrawPolygonEdgeDimensions();
        }

        /// <summary>
        /// Event handler for polygon points move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPointHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Call detach PointHandles then dispose of each
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DetachHandlers();

            _polygonHandles.ForEach(handle => handle.Dispose());

            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }

            //Clear local caches
            _polygonHandles?.Clear();
        }

        /// <summary>
        /// Attach event handlers to each PointHandle in cache
        /// </summary>
        private void AttachHandlers()
        {
            _polygonHandles.ForEach(handle =>
            {
                handle.DragOngoing += OnPointHandleDragOngoing;
                handle.DragEnded += OnPointHandleDragEnded;
            });
        }

        /// <summary>
        /// Detach event handlers for each PointHandle in cache
        /// </summary>
        private void DetachHandlers()
        {
            _polygonHandles.ForEach(handle =>
            {
                handle.DragOngoing -= OnPointHandleDragOngoing;
                handle.DragEnded -= OnPointHandleDragEnded;
            });
        }
    }
}