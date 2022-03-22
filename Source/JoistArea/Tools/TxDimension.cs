namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using Tekla.BIM.Quantities;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Temporary graphics that represent a dimension in the model view
    /// </summary>
    public static class TxDimension
    {
        /// <summary>
        /// Draws temporary dimension in model view
        /// </summary>
        /// <param name="distList">List of points to dimension</param>
        /// <param name="offsetDist">Offset distance from points to put dimension line</param>
        /// <param name="color">Color to display</param>
        /// <exception cref="ArgumentNullException">Input cannot be null</exception>
        public static void DrawDimensionsInModel(List<Point> distList, double offsetDist = 600, Color color = null)
        {
            if (distList == null) throw new ArgumentNullException(nameof(distList));
            if (color == null) color = new Color(1, 0, 0);

            Point lastPoint = null;
            foreach (var pt in distList)
            {
                if (lastPoint != null) DrawDimensionInModel(lastPoint, pt, offsetDist, color);
                lastPoint = pt;
            }
        }

        /// <summary>
        /// Draws temporary dimension in model view
        /// </summary>
        /// <param name="p1">First point to dimension</param>
        /// <param name="p2">Second point to dimension</param>
        /// <param name="offsetDist">Offset distance from points to put dimension line</param>
        /// <param name="color">Color to display</param>
        public static void DrawDimensionInModel(Point p1, Point p2, double offsetDist = 600, Color color = null)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            if (color == null) color = new Color(1, 0, 0);

            var distance = Distance.PointToPoint(p1, p2);
            var xAxis = new Vector(p2 - p1).GetNormal();
            var yAxis = xAxis.Cross(new Vector(0, 0, 1));

            var leg1 = new LineSegment(p1, p1.GetTranslated(yAxis * offsetDist));
            var leg2 = new LineSegment(leg1.Point2, leg1.Point2.GetTranslated(xAxis * distance));
            var leg3 = new LineSegment(leg2.Point2, p2);
            leg1.PaintLine(color);
            leg2.PaintLine(color);
            leg3.PaintLine(color);
            DrawText(new Length(distance).ToCurrentUnits(), leg2.GetMidPoint(), color);
        }

        /// <summary>
        /// Draws text in the model view
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="tPoint">Point to insert as base of text</param>
        /// <param name="tColor">Color to make text</param>
        private static void DrawText(string text, Point tPoint, Color tColor)
        {
            var gd = new GraphicsDrawer();
            gd.DrawText(tPoint, text, tColor);
        }
    }
}
