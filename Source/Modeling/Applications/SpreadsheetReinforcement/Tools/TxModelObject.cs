using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace SpreadsheetReinforcement.Tools
{
    /// <summary>
    /// Tekla ModelObject service extensions
    /// </summary>
    public static class TxModelObject
    {
        private const string ReportUdaName = "NAME";

        /// <summary>
        /// Measures top center of part (assumes normal global coordinate system)
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <returns>New point in model, null if none found</returns>
        public static Point GetTopCenter(this Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var center = pt.GetGlobalCenterOfGravity();
            var partSolid = pt.GetSolid();
            var dz = partSolid.MaximumPoint.Z - partSolid.MinimumPoint.Z;
            var ls = new LineSegment(new Point(center), new Point(center.X, center.Y, center.Z + dz * 0.5));
#if DEBUG
            ls.PaintLine(new Color(1, 0, 0));
#endif
            var intersections = partSolid.Intersect(ls);
            if (intersections == null || intersections.Count < 1) return null;
            return intersections[0] as Point;
        }

        /// <summary>
        /// Measures bottom center of part (assumes normal global coordinate system)
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <returns>New point in model, null if none found</returns>
        public static Point GetBottomCenter(this Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var center = pt.GetGlobalCenterOfGravity();
            var partSolid = pt.GetSolid();
            var dz = partSolid.MaximumPoint.Z - partSolid.MinimumPoint.Z;
            var ls = new LineSegment(new Point(center), new Point(center.X, center.Y, center.Z - dz * 0.5));
#if DEBUG
            ls.PaintLine(new Color(0, 1, 0));
#endif
            var intersections = partSolid.Intersect(ls);
            if (intersections == null || intersections.Count < 1) return null;
            return intersections[0] as Point;
        }

        /// <summary>
        /// Gets report value name for model part
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <returns>String name value</returns>
        public static string GetName(this Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var tempValue = string.Empty;
            pt.GetReportProperty(ReportUdaName, ref tempValue);
            return tempValue;
        }

        /// <summary>
        /// Returns the Global Center of Gravity report property from model
        /// </summary>
        /// <param name="modelObject">Model object</param>
        /// <returns>Value from report property</returns>
        public static Point GetGlobalCenterOfGravity(this ModelObject modelObject)
        {
            if (modelObject == null) return null;
            double dx = 0.0, dy = 0.0, dz = 0.0;

            modelObject.GetReportProperty("COG_X", ref dx);
            modelObject.GetReportProperty("COG_Y", ref dy);
            modelObject.GetReportProperty("COG_Z", ref dz);
            return new Point(dx, dy, dz);
        }

        /// <summary>
        /// Gets assembly mark report property
        /// </summary>
        /// <param name="mo">Model assembly</param>
        /// <returns>String mark</returns>
        public static string GetAssemblyMark(this ModelObject mo)
        {
            if (mo == null) return string.Empty;
            var tempString = string.Empty;
            mo.GetReportProperty("ASSEMBLY_POS", ref tempString);
            return tempString;
        }

        /// <summary>
        /// Returns the length report property from model
        /// </summary>
        /// <param name="modelObject">Model object</param>
        /// <returns>Value from report property</returns>
        public static double GetLength(this ModelObject modelObject)
        {
            if (modelObject == null) return 0.0;
            var tempString = 0.0;
            modelObject.GetReportProperty("LENGTH", ref tempString);
            return tempString;
        }
    }
}