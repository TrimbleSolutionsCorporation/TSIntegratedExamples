namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    public static class TxAssembly
    {
        public static AABB GetGlobalBoundingBox(this Assembly assm)
        {
            if (assm == null) throw new ArgumentNullException(nameof(assm));
            var min_x = 0.0;
            var min_y = 0.0;
            var min_z = 0.0;
            var max_x = 0.0;
            var max_y = 0.0;
            var max_z = 0.0;

            assm.GetReportProperty("BOUNDING_BOX_MIN_X", ref min_x);
            assm.GetReportProperty("BOUNDING_BOX_MIN_Y", ref min_y);
            assm.GetReportProperty("BOUNDING_BOX_MIN_Z", ref min_z);
            var minPt = new Point(min_x, min_y, min_z);

            assm.GetReportProperty("BOUNDING_BOX_MAX_X", ref max_x);
            assm.GetReportProperty("BOUNDING_BOX_MAX_Y", ref max_y);
            assm.GetReportProperty("BOUNDING_BOX_MAX_Z", ref max_z);
            var maxPt = new Point(max_x, max_y, max_z);

            return new AABB(minPt, maxPt);
        }

        public static AABB GetGlobalBoundingBox(this Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var min_x = 0.0;
            var min_y = 0.0;
            var min_z = 0.0;
            var max_x = 0.0;
            var max_y = 0.0;
            var max_z = 0.0;

            pt.GetReportProperty("BOUNDING_BOX_MIN_X", ref min_x);
            pt.GetReportProperty("BOUNDING_BOX_MIN_Y", ref min_y);
            pt.GetReportProperty("BOUNDING_BOX_MIN_Z", ref min_z);
            var minPt = new Point(min_x, min_y, min_z);

            pt.GetReportProperty("BOUNDING_BOX_MAX_X", ref max_x);
            pt.GetReportProperty("BOUNDING_BOX_MAX_Y", ref max_y);
            pt.GetReportProperty("BOUNDING_BOX_MAX_Z", ref max_z);
            var maxPt = new Point(max_x, max_y, max_z);

            return new AABB(minPt, maxPt);
        }

        public static AABB GetLocalBoundingBox(this Assembly assm)
        {
            if (assm == null) throw new ArgumentNullException(nameof(assm));
            
            var gBox = GetGlobalBoundingBox(assm);
            var transMatrix = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                .TransformationMatrixToLocal;
            var pMin = transMatrix.Transform(new Point(gBox.MinPoint));
            var pMax = transMatrix.Transform(new Point(gBox.MaxPoint));

            return new AABB(pMin, pMax);
        }

        public static AABB GetLocalBoundingBox(this Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));

            var gBox = GetGlobalBoundingBox(pt);
            var transMatrix = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane()
                .TransformationMatrixToLocal;
            var pMin = transMatrix.Transform(new Point(gBox.MinPoint));
            var pMax = transMatrix.Transform(new Point(gBox.MaxPoint));

            return new AABB(pMin, pMax);
        }
    }
}
