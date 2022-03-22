namespace DrawingRectangle
{
    using System;
    using Tekla.Structures.Drawing;
    using Tekla.Structures.Geometry3d;
    using Tools;

    public static class MainLogic
    {
        public static bool RunLogic(ViewBase tView, Point pt, PluginData data)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            if (data == null) throw new ArgumentNullException(nameof(data));

            //Calculate position
            var btmLeftPt = new Point(pt);
            var topRightPt = new Point(pt);
            btmLeftPt.Translate(data.HorizOffset, 0, 0);
            topRightPt.Translate(data.HorizOffset, 0, 0);
            topRightPt.Translate(data.RectWidth, data.RectHeight, 0);
            var pMid = new LineSegment(btmLeftPt, topRightPt).GetMidPoint();

            //Insert new rectangle object
            var dwgRect = new Rectangle(tView, btmLeftPt, topRightPt);
            dwgRect.Attributes.Line.Color = DrawingColors.Red;
            var r1 = dwgRect.Insert();

            //Get text, check value - cannot be null
            var textValue = string.IsNullOrEmpty(data.TempName) ? "Default" : data.TempName;

            //Insert new text object
            var dwgText = new Text(tView, pMid, textValue);
            dwgText.Attributes.ArrowHead.ArrowPosition = ArrowheadPositions.None;
            dwgText.Attributes.ArrowHead.Head = ArrowheadTypes.NoArrow;
            dwgText.Attributes.Font = new FontAttributes(DrawingColors.Blue, 5.0, "Arial", false, false);
            var r2 = dwgText.Insert();

            return r1 && r2;
        }
    }
}
