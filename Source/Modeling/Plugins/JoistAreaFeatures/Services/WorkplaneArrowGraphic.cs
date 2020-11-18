namespace JoistAreaFeatures.Services
{
    using Tekla.BIM.DirectManipulation.Services.Graphics;
    using Tekla.Common.Geometry;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tools;

    /// <summary>
    /// The <see cref="WorkplaneArrowGraphic"/> class represents the workplane arrow graphic.
    /// </summary>
    public class WorkplaneArrowGraphic : AdvancedGraphic
    {
        /// <summary>
        /// The arrow width.
        /// </summary>
        private const double Width = 400.0;

        /// <summary>
        /// The arrow length.
        /// </summary>
        private const double Length = 1000.0;

        /// <summary>
        /// The arrow head width.
        /// </summary>
        private const double HeadWidth = 600.0;

        /// <summary>
        /// The arrow head length.
        /// </summary>
        private const double HeadLength = Length * 0.3;

        /// <summary>
        /// The line points of arrows.
        /// </summary>
        private readonly Vector3[] linePoints;

        /// <summary>
        /// Points of the X character coordinates.
        /// </summary>
        private readonly Vector3[] pointsX1;

        /// <summary>
        /// Points of the X character coordinates.
        /// </summary>
        private readonly Vector3[] pointsX2;

        /// <summary>
        /// Points of the Y character coordinates.
        /// </summary>
        private readonly Vector3[] pointsY1;

        /// <summary>
        /// Points of the Y character coordinates.
        /// </summary>
        private readonly Vector3[] pointsY2;

        /// <summary>
        /// Points of the Y character coordinates.
        /// </summary>
        private readonly Vector3[] pointsY3;

        /// <summary>
        /// Points for the tick mark at origin of new work plane
        /// </summary>
        private readonly Vector3[] originIndicator;

        /// <summary>
        /// Graphic context used in drawings.
        /// </summary>
        private IGraphicsContext graphicContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkplaneArrowGraphic"/> class.
        /// </summary>
        /// <param name="matrix">
        /// The matrix.
        /// </param>
        public WorkplaneArrowGraphic(Matrix matrix)
        {
            this.linePoints = new[]
                                  {
                                      new Vector3(-Width / 2, -Width / 2, 0),
                                      new Vector3(Length - HeadLength, -Width / 2, 0),
                                      new Vector3(Length - HeadLength, -HeadWidth / 2, 0), new Vector3(Length, 0, 0),
                                      new Vector3(Length - HeadLength, +HeadWidth / 2, 0),
                                      new Vector3(Length - HeadLength, +Width / 2, 0),
                                      new Vector3(+Width / 2, +Width / 2, 0),
                                      new Vector3(+Width / 2, Length - HeadLength, 0),
                                      new Vector3(+HeadWidth / 2, Length - HeadLength, 0), new Vector3(0, Length, 0),
                                      new Vector3(-HeadWidth / 2, Length - HeadLength, 0),
                                      new Vector3(-Width / 2, Length - HeadLength, 0),
                                      new Vector3(-Width / 2, -Width / 2, 0)
                                  };
            this.linePoints = TrasnformPoints(this.linePoints, matrix);

            // X character coordinates
            this.pointsX1 = new[] { new Vector3(700 - 60, -100, 0), new Vector3(700 + 60, 100, 0) };
            this.pointsX1 = TrasnformPoints(this.pointsX1, matrix);

            this.pointsX2 = new[] { new Vector3(700 + 60, -100, 0), new Vector3(700 - 60, 100, 0) };
            this.pointsX2 = TrasnformPoints(this.pointsX2, matrix);

            // Y character coordinates
            this.pointsY1 = new[] { new Vector3(0, 700 - 100, 0), new Vector3(0, 700, 0) };
            this.pointsY1 = TrasnformPoints(this.pointsY1, matrix);

            this.pointsY2 = new[] { new Vector3(100, 700 + 100, 0), new Vector3(0, 700, 0) };
            this.pointsY2 = TrasnformPoints(this.pointsY2, matrix);

            this.pointsY3 = new[] { new Vector3(-100, 700 + 100, 0), new Vector3(0, 700, 0) };
            this.pointsY3 = TrasnformPoints(this.pointsY3, matrix);

            this.originIndicator = new[]
                                       {
                                           new Vector3(-50, 0, 0), new Vector3(50, 0, 0), new Vector3(0, -50, 0),
                                           new Vector3(0, 50, 0)
                                       };
            this.originIndicator = TrasnformPoints(this.originIndicator, matrix);
        }

        /// <summary>
        /// Opens the IGraphicsContext to which UpdateView will draw the workplane graphics.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        protected override void OnInitialize(IGraphicsDrawerService service)
        {
            this.graphicContext = service.OpenContext();
        }

        /// <summary>
        /// The method where drawings are done.
        /// </summary>
        protected override void UpdateView()
        {
            this.graphicContext.Clear();

            this.graphicContext.DrawLine(this.linePoints, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.pointsX1, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.pointsX2, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.pointsY1, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.pointsY2, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.pointsY3, CurveType.CurveOnFace);

            this.graphicContext.DrawLine(this.originIndicator[0], this.originIndicator[1], CurveType.CurveOnFace);
            this.graphicContext.DrawLine(this.originIndicator[2], this.originIndicator[3], CurveType.CurveOnFace);
        }

        public void DrawGraphics(IGraphicsDrawer graphics, bool clear)
        {
            if (clear) graphics.Clear();
            graphics.DrawLines(linePoints.ToSegments());
            graphics.DrawLines(pointsX1.ToSegments());
            graphics.DrawLines(pointsX2.ToSegments());
            graphics.DrawLines(pointsY1.ToSegments());
            graphics.DrawLines(pointsY2.ToSegments());
            graphics.DrawLines(pointsY3.ToSegments());

            graphics.DrawLine(originIndicator[0].ToPoint(), originIndicator[1].ToPoint());
            graphics.DrawLine(originIndicator[2].ToPoint(), originIndicator[3].ToPoint());
        }

        /// <summary>
        /// This method transforms the points.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="matrix">
        /// The matrix.
        /// </param>
        /// <returns>
        /// An array of transformed points.
        /// </returns>
        private static Vector3[] TrasnformPoints(Vector3[] points, Matrix matrix)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points.SetValue(matrix.Transform(points[i].ToPoint()).ToVector3(), i);
            }

            return points;
        }
    }
}
