namespace SpreadsheetReinforcement.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Tools for extending the non-orientated Tekla bounding box
    /// </summary>
    public static class TxAabb
    {
        /// <summary>
        /// Gets bounding box for part in local ucs
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <param name="solidType">Solid accuracy</param>
        /// <returns>New bounding box</returns>
        /// <exception cref="ArgumentNullException">Part cannot be null</exception>
        public static AABB GetBoundingBox(this Part pt,
            Solid.SolidCreationTypeEnum solidType = Solid.SolidCreationTypeEnum.NORMAL)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var solid = pt.GetSolid(solidType);
            return new AABB(solid.MinimumPoint, solid.MaximumPoint);
        }

        /// <summary>
        /// Paints outline of box with temporary graphics
        /// </summary>
        /// <param name="box">Tekla AABB</param>
        /// <param name="color">If null, red is chosen</param>
        public static void PaintBox(this AABB box, Color color = null)
        {
            if (box == null) throw new ArgumentNullException(nameof(box));
            if (color == null) color = new Color(1, 0, 0);

            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z),
                    new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z),
                    new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z),
                    new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z),
                    new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z)), color);

            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z),
                    new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z),
                    new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z),
                    new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z),
                    new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z)), color);

            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z),
                    new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z),
                    new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z),
                    new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z)), color);
            DrawLineFromViewCoordSysToModel(
                new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z),
                    new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)), color);
        }

        private static void DrawLineFromViewCoordSysToModel(LineSegment line, Color color)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var drawer = new GraphicsDrawer();
            drawer.DrawLineSegment(line.Point1, line.Point2, color);
        }
    }
}