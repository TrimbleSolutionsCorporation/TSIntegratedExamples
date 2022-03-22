namespace JoistAreaFeatures.Services
{
    using JoistArea.Tools;
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;

    public class SpacingZone
    {
        private double dyValue = 400.0;

        private Vector OffsetFactor
        {
            get
            {
                var upDir = new Vector(0, 0, 1);
                var xDir = this.Segment.GetDirectionVector();
                var yDir = Vector.Cross(upDir, xDir).GetNormal();
                return yDir * this.dyValue;
            }
        }

        public Point OffsetPoint => this.Center.GetTranslated(this.OffsetFactor);

        public double Length => this.Segment.Length();

        public Point Center { get; private set; }

        public LineSegment Segment { get; private set; }

        public PointHandle Handle { get; set; }

        public SpacingZone(LineSegment ls)
        {
            this.Segment = ls;
            this.Center = ls.GetMidPoint();
        }

        public void Select(LineSegment ls)
        {
            this.Segment = ls ?? throw new ArgumentNullException(nameof(ls));
            this.Center = ls.GetMidPoint();
            if (this.Handle != null)
            {
                this.Handle.Point = this.OffsetPoint;
            }
        }
    }
}