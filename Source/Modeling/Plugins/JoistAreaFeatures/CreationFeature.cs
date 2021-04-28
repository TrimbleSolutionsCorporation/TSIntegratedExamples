namespace JoistAreaFeatures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using JoistArea.View;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools.Picking;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Utilities;

    public class CreationFeature : PluginCreationFeatureBase
    {

        private readonly InputRange inputRange;
        private PickingTool polygonPickingTool;
        private PickingTool guidelinePickingTool;
        private readonly List<Point> pickedPolygonPoints = new List<Point>();
        private readonly List<Point> guidlinePoints = new List<Point>();

        /// <summary>
        /// Initializes a new instance of creation feature
        /// </summary>
        public CreationFeature()
            : base(Constants.PluginName)
        {
            inputRange = InputRange.AtLeast(3);
        }

        /// <summary>
        /// Inherited start point for DM selected
        /// </summary>
        protected override void Initialize()
        {
            DetachHandlers();
            polygonPickingTool?.Dispose();

            //Start picking routine to define polygon shape and attach event handlers
            polygonPickingTool = this.CreatePickingTool(inputRange, InputTypes.Point);
            polygonPickingTool.PreviewRequested += Polygon_OnPreviewRequested;
            polygonPickingTool.ObjectPicked += Polygon_OnObjectPicked;
            polygonPickingTool.PickUndone += Polygon_OnPickingUndone;
            polygonPickingTool.PickSessionEnded += Polygon_OnPickEnded;
            polygonPickingTool.StartPickingSession("Pick points to define polygonal area.");
        }

        /// <summary>
        /// Polygon end defined, start next picking routine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polygon_OnPickEnded(object sender, EventArgs e)
        {
            guidelinePickingTool?.Dispose();

            //Get user to pick two points to define guid-line
            guidelinePickingTool = this.CreatePickingTool(InputRange.Exactly(2), InputTypes.Point);
            guidelinePickingTool.PreviewRequested += Guideline_OnPreviewRequested;
            guidelinePickingTool.ObjectPicked += Guideline_OnObjectPicked;
            guidelinePickingTool.PickUndone += Guideline_OnPickingUndone;
            guidelinePickingTool.PickSessionEnded += Guideline_OnPickEnded;
            guidelinePickingTool.StartPickingSession("Pick two point to establish a guide line.");
        }

        /// <summary>
        /// Inherited refresh to clear pickers and point objects
        /// </summary>
        protected override void Refresh()
        {
            pickedPolygonPoints?.Clear();
            guidlinePoints?.Clear();
        }
        
        /// <summary>
        /// Detaches handlers from the picking tools
        /// </summary>
        private void DetachHandlers()
        {
            if (polygonPickingTool != null)
            {
                polygonPickingTool.PreviewRequested -= Polygon_OnPreviewRequested;
                polygonPickingTool.ObjectPicked -= Polygon_OnObjectPicked;
                polygonPickingTool.PickUndone -= Polygon_OnPickingUndone;
            }

            if (guidelinePickingTool != null)
            {
                guidelinePickingTool.PreviewRequested -= Guideline_OnPreviewRequested;
                guidelinePickingTool.ObjectPicked -= Guideline_OnObjectPicked;
                guidelinePickingTool.PickUndone -= Guideline_OnPickingUndone;
                guidelinePickingTool.PickSessionEnded -= Guideline_OnPickEnded;
            }
        }

        /// <summary>
        /// Creates graphics preview for slab as it is being defined for polygon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs">DM handle event args.</param>
        private void Polygon_OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            Graphics.Clear();
            //var currentAppliedValues = GetAppliedAttributes(Component);     //this method seems to have been removed

            if (pickedPolygonPoints.Count < 1) return;
            if (pickedPolygonPoints.Count == 1)
            {
                //Only one point so far, draw dimension graphic from 1st point to hit point
                var ls = new LineSegment(pickedPolygonPoints[0], eventArgs.HitPoint);
                var yDir = Vector.Cross(new Vector(0, 0, 1), ls.GetDirectionVector());
                var zDir = Vector.Cross(ls.GetDirectionVector(), yDir).GetNormal();
                Graphics.DrawDimension(ls, zDir, DimensionEndPointSizeType.Dynamic);
            }
            else
            {
                //Create dimension graphic for each point in existing loop and current hit point
                for (var i = 0; i < pickedPolygonPoints.Count; i++)
                {
                    var curPt = new Point(pickedPolygonPoints[i]);
                    var nextPt = i == pickedPolygonPoints.Count - 1 ? 
                        eventArgs.HitPoint : pickedPolygonPoints[i + 1];
                    
                    var ls = new LineSegment(curPt, nextPt);
                    var yDir = Vector.Cross(new Vector(0, 0, 1), ls.GetDirectionVector());
                    var zDir = Vector.Cross(ls.GetDirectionVector(), yDir).GetNormal();
                    Graphics.DrawDimension(ls, zDir, DimensionEndPointSizeType.Dynamic);
                }

                //Close polygon loop to 1st point with dimension graphic
                var lsEnd = new LineSegment(eventArgs.HitPoint, pickedPolygonPoints[0]);
                var yDirEnd = Vector.Cross(new Vector(0, 0, 1), lsEnd.GetDirectionVector());
                var zDirEnd = Vector.Cross(lsEnd.GetDirectionVector(), yDirEnd).GetNormal();
                Graphics.DrawDimension(lsEnd, zDirEnd, DimensionEndPointSizeType.Dynamic);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.ObjectPicked"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Polygon_OnObjectPicked(object sender, ToleratedObjectEventArgs eventArgs)
        {
            if (!eventArgs.IsValid) return;
            pickedPolygonPoints.Add(eventArgs.HitPoint);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Guideline_OnPickEnded(object sender, EventArgs eventArgs)
        {
            var input = new ComponentInput();
            input.AddInputPolygon(new Polygon { Points = new ArrayList(pickedPolygonPoints) });
            input.AddTwoInputPositions(guidlinePoints[0], guidlinePoints[1]);
            CommitComponentInput(input);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Polygon_OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (pickedPolygonPoints.Count > 0)
            {
                pickedPolygonPoints.RemoveAt(pickedPolygonPoints.Count - 1);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.ObjectPicked"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Guideline_OnObjectPicked(object sender, ToleratedObjectEventArgs eventArgs)
        {
            if (!eventArgs.IsValid) return;
            guidlinePoints.Add(eventArgs.HitPoint);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Guideline_OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (guidlinePoints.Count > 0)
            {
                guidlinePoints.RemoveAt(guidlinePoints.Count - 1);
            }
        }

        private void Guideline_OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            Graphics.Clear();
            //var currentAppliedValues = GetAppliedAttributes(Component); 

            if (!guidlinePoints.Any() || eventArgs.HitPoint == null) return;
            var p1 = new Point(guidlinePoints[0]);
            var p2 = new Point(eventArgs.HitPoint);
            Graphics.DrawLine(p1, p2);
        }
    }
}
