namespace JoistAreaFeatures
{
    using JoistArea.View;
    using Services;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
            this.inputRange = InputRange.AtLeast(3);
        }

        /// <summary>
        /// Inherited start point for DM selected
        /// </summary>
        protected override void Initialize()
        {
            this.DetachHandlers();
            this.polygonPickingTool?.Dispose();

            //Start picking routine to define polygon shape and attach event handlers
            this.polygonPickingTool = this.CreatePickingTool(this.inputRange, InputTypes.Point);
            this.polygonPickingTool.PreviewRequested += this.Polygon_OnPreviewRequested;
            this.polygonPickingTool.ObjectPicked += this.Polygon_OnObjectPicked;
            this.polygonPickingTool.PickUndone += this.Polygon_OnPickingUndone;
            this.polygonPickingTool.PickSessionEnded += this.Polygon_OnPickEnded;

            this.polygonPickingTool.StartPickingSession("Pick points to define polygonal area.");
        }

        /// <summary>
        /// Polygon end defined, start next picking routine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polygon_OnPickEnded(object sender, EventArgs e)
        {
            this.guidelinePickingTool?.Dispose();

            //Get user to pick two points to define guid-line
            this.guidelinePickingTool = this.CreatePickingTool(InputRange.Exactly(2), InputTypes.Point);
            this.guidelinePickingTool.PreviewRequested += this.Guideline_OnPreviewRequested;
            this.guidelinePickingTool.ObjectPicked += this.Guideline_OnObjectPicked;
            this.guidelinePickingTool.PickUndone += this.Guideline_OnPickingUndone;
            this.guidelinePickingTool.PickSessionEnded += this.Guideline_OnPickEnded;
            this.guidelinePickingTool.StartPickingSession("Pick two point to establish a guide line.");
        }

        /// <summary>
        /// Inherited refresh to clear pickers and point objects
        /// </summary>
        protected override void Refresh()
        {
            this.pickedPolygonPoints?.Clear();
            this.guidlinePoints?.Clear();
        }

        /// <summary>
        /// Detaches handlers from the picking tools
        /// </summary>
        private void DetachHandlers()
        {
            if (this.polygonPickingTool != null)
            {
                this.polygonPickingTool.PreviewRequested -= this.Polygon_OnPreviewRequested;
                this.polygonPickingTool.ObjectPicked -= this.Polygon_OnObjectPicked;
                this.polygonPickingTool.PickUndone -= this.Polygon_OnPickingUndone;
            }

            if (this.guidelinePickingTool != null)
            {
                this.guidelinePickingTool.PreviewRequested -= this.Guideline_OnPreviewRequested;
                this.guidelinePickingTool.ObjectPicked -= this.Guideline_OnObjectPicked;
                this.guidelinePickingTool.PickUndone -= this.Guideline_OnPickingUndone;
                this.guidelinePickingTool.PickSessionEnded -= this.Guideline_OnPickEnded;
            }
        }

        /// <summary>
        /// Creates graphics preview for slab as it is being defined for polygon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs">DM handle event args.</param>
        private void Polygon_OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            var currentAppliedValues = this.Component.GetDataFromComponent();

            if (this.pickedPolygonPoints.Count < 1) return;
            if (this.pickedPolygonPoints.Count == 1)
            {
                //Only one point so far, draw dimension graphic from 1st point to hit point
                var ls = new LineSegment(this.pickedPolygonPoints[0], eventArgs.HitPoint);
                var yDir = Vector.Cross(new Vector(0, 0, 1), ls.GetDirectionVector());
                var zDir = Vector.Cross(ls.GetDirectionVector(), yDir).GetNormal();
                if (ls.Length() > GeometryConstants.DISTANCE_EPSILON)
                {
                    this.Graphics.DrawDimension(ls, zDir, DimensionEndPointSizeType.Dynamic);
                }
            }
            else
            {
                //Create dimension graphic for each point in existing loop and current hit point
                for (var i = 0; i < this.pickedPolygonPoints.Count; i++)
                {
                    var curPt = new Point(this.pickedPolygonPoints[i]);
                    var nextPt = i == this.pickedPolygonPoints.Count - 1 ?
                        eventArgs.HitPoint : this.pickedPolygonPoints[i + 1];

                    var ls = new LineSegment(curPt, nextPt);
                    var yDir = Vector.Cross(new Vector(0, 0, 1), ls.GetDirectionVector());
                    var zDir = Vector.Cross(ls.GetDirectionVector(), yDir).GetNormal();
                    if (ls.Length() > GeometryConstants.DISTANCE_EPSILON)
                    {
                        this.Graphics.DrawDimension(ls, zDir, DimensionEndPointSizeType.Dynamic);
                    }
                }

                //Close polygon loop to 1st point with dimension graphic
                var lsEnd = new LineSegment(eventArgs.HitPoint, this.pickedPolygonPoints[0]);
                var yDirEnd = Vector.Cross(new Vector(0, 0, 1), lsEnd.GetDirectionVector());
                var zDirEnd = Vector.Cross(lsEnd.GetDirectionVector(), yDirEnd).GetNormal();
                this.Graphics.DrawDimension(lsEnd, zDirEnd, DimensionEndPointSizeType.Dynamic);
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
            this.pickedPolygonPoints.Add(eventArgs.HitPoint);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickSessionEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Guideline_OnPickEnded(object sender, EventArgs eventArgs)
        {
            var input = new ComponentInput();
            input.AddInputPolygon(new Polygon { Points = new ArrayList(this.pickedPolygonPoints) });
            input.AddTwoInputPositions(this.guidlinePoints[0], this.guidlinePoints[1]);
            this.CommitComponentInput(input);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Polygon_OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (this.pickedPolygonPoints.Count > 0)
            {
                this.pickedPolygonPoints.RemoveAt(this.pickedPolygonPoints.Count - 1);
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
            this.guidlinePoints.Add(eventArgs.HitPoint);
        }

        /// <summary>
        /// Event handler for the <see cref="PickingTool.PickUndone"/> event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="eventArgs">The event argument for the handler.</param>
        private void Guideline_OnPickingUndone(object sender, EventArgs eventArgs)
        {
            if (this.guidlinePoints.Count > 0)
            {
                this.guidlinePoints.RemoveAt(this.guidlinePoints.Count - 1);
            }
        }

        private void Guideline_OnPreviewRequested(object sender, ToleratedObjectEventArgs eventArgs)
        {
            this.Graphics.Clear();
            var currentAppliedValues = this.Component.GetDataFromComponent();

            if (!this.guidlinePoints.Any() || eventArgs.HitPoint == null) return;
            var p1 = new Point(this.guidlinePoints[0]);
            var p2 = new Point(eventArgs.HitPoint);
            this.Graphics.DrawLine(p1, p2);
        }
    }
}
