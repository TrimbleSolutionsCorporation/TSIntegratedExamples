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

    public sealed class PolygonShapeMc : ManipulationContext
    {
        private readonly IHandleManager _handleManager;
        private List<PointHandle> _polygonHandles;

        public PolygonShapeMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            try
            {
                _handleManager = feature.HandleManager;

                //Create point, distance, and line manipulators
                CreatePointManipulators(component);

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

            //Get input objects from component
            var componentInput = FeatureLogic.GetCurrentInput(Component);

            //Refresh existing manipulator handles from plugin input
            var index = 0;
            foreach (var pt in componentInput.Item1)
            {
                _polygonHandles[index].Point = new Point(pt);
                index++;
            }

            //Update graphics based on plugin and manipulators
            ReCreateGraphics();
        }

        private void ReCreateGraphics()
        {
            Graphics?.Clear();
            DrawPolygonEdgeDimensions();
        }

        /// <summary>
        /// Draw outer polygon edge segment dimension graphics
        /// </summary>
        private void DrawPolygonEdgeDimensions()
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

        /// <summary>
        /// Create polygon handles from plugin input points
        /// </summary>
        /// <param name="plugin">Joist Area plugin instance</param>
        private void CreatePointManipulators(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if(_polygonHandles == null) _polygonHandles = new List<PointHandle>();
            else _polygonHandles?.Clear();

            try
            {
                var componentInput = FeatureLogic.GetCurrentInput(plugin);
                foreach (var pt in componentInput.Item1)
                {
                    var handle =
                        _handleManager.CreatePointHandle(pt, HandleLocationType.InputPoint, HandleEffectType.Geometry);
                    handle.DragOngoing += PolygonHandleDragOngoing;
                    handle.DragEnded += PolygonHandleDragEnded;
                    _polygonHandles.Add(handle);
                }
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

        /// <summary>
        /// Event handler for polygon points move in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void PolygonHandleDragOngoing(object sender, DragEventArgs eventArgs)
        {
            ReCreateGraphics();
        }

        /// <summary>
        /// Event handler for polygon points move ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void PolygonHandleDragEnded(object sender, DragEventArgs eventArgs)
        {
            ModifyPluginInputFromManipulators();
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _polygonHandles?.ForEach(handle =>
            {
                handle.DragOngoing -= PolygonHandleDragOngoing;
                handle.DragEnded -= PolygonHandleDragEnded;
            });
        }
    }
}