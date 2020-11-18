namespace JoistAreaFeatures.Tools
{
    using System;
    using System.Collections.Generic;
    using JoistArea.Tools;
    using Tekla.Common.Geometry;
    using Tekla.Structures.Geometry3d;

    public static class TxPoint
    {
        public static List<LineSegment> ToSegments(IEnumerable<Point> pxList)
        {
            if (pxList == null) throw new ArgumentNullException();

            var ptList = new List<Point>(pxList);
            var lsgList = new List<LineSegment>();
            for (var i = 0; i < ptList.Count; i++)
            {
                if (i == ptList.Count - 1) lsgList.Add(new LineSegment(ptList[i], ptList[0]));
                else lsgList.Add(new LineSegment(ptList[i], ptList[i + 1]));
            }
            return lsgList;
        }

        public static List<LineSegment> ToSegments(IEnumerable<Vector3> pxList)
        {
            if (pxList == null) throw new ArgumentNullException();
            var ptList = new List<Point>();
            foreach (var v3 in pxList)
            {
                ptList.Add(v3.ToPoint());
            }
            return ToSegments(ptList);
        }

        public static List<LineSegment> ToSegments(this Vector3[] pxList)
        {
            if (pxList == null) throw new ArgumentNullException();
            var ptList = new List<Point>();
            foreach (var v3 in pxList)
            {
                ptList.Add(v3.ToPoint());
            }
            return ToSegments(ptList);
        }
    }
}
