namespace JoistAreaFeatures.Services
{
    using System;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    public static class FeatureLogic
    {
        public static List<LineSegment> GetJoistSpanSegments(JoistAreaMainLogic logic)
        {
            if (logic == null) throw new ArgumentNullException(nameof(logic));
            var result = new List<LineSegment>();
            var transMatrix = logic.TransGlobalMatrix.GetTranspose();

            Point firstCenterGlobal = null;
            VirtualJoist lastJoist = null;
            foreach (var currentJoist in logic.VirtualJoistList)
            {
                if (firstCenterGlobal == null)
                {
                    firstCenterGlobal = transMatrix.Transform(currentJoist.CenterBeam);
                }

                //Every other beam create new line segment
                if (lastJoist != null)
                {
                    var lastJoistLineGlobal = lastJoist.BeamLine.Transform(transMatrix);
                    var currentJoistLineGlobal = currentJoist.BeamLine.Transform(transMatrix);
                    //currentJoistLineGlobal.PaintLine(new Color(1,0,0));

                    var p1 = Projection.PointToLine(firstCenterGlobal, new Line(lastJoistLineGlobal));
                    var p2 = Projection.PointToLine(firstCenterGlobal, new Line(currentJoistLineGlobal));
                    if (p1 != null && p2 != null)
                    {
                        result.Add(new LineSegment(p1, p2));
                    }
                }
                lastJoist = currentJoist;
            }

            return result;
        }
    }
}
