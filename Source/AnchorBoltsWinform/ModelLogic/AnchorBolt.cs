namespace AnchorBoltsWinform.ModelLogic
{
    using System;
    using Tools;
    using ViewModel;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.Operations;

    /// <summary>
    /// Custom class to represent model anchor bolt
    /// </summary>
    public class AnchorBolt
    {
        private Point _dimPoint;
        private Point _btmPoint;
        private Tekla.Structures.Drawing.View _view;

        /// <summary> Model part instance </summary>
        public Part ModelPart { get; private set; }

        /// <summary> Drawing view referenced from </summary>
        public Tekla.Structures.Drawing.View DrawingView
        {
            get { return _view; }
            private set { _view = value; }
        }

        /// <summary>
        /// Dimension point in view display coordinate system
        /// </summary>
        public Point DimensionPoint
        {
            get
            {
                if (_dimPoint != null) return _dimPoint;
                if (DrawingView == null || ModelPart==null) return null;
                return _dimPoint = GetDimensionPoint(DrawingView, ModelPart);
            }
        }

        /// <summary>
        /// Bottom point in model coordinate system
        /// </summary>
        public Point BottomPoint
        {
            get
            {
                if (_btmPoint != null) return _btmPoint;
                if (ModelPart == null) return null;
                return _btmPoint = GetBottomPoint(ModelPart);
            }
        }

        /// <summary>
        /// Creates new instance of anchor bolt
        /// </summary>
        /// <param name="pt">Part in model</param>
        /// <param name="view">View referenced from</param>
        public AnchorBolt(Part pt, Tekla.Structures.Drawing.View view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            ModelPart = pt;
            DrawingView = view;
        }

        /// <summary>
        /// Checks to see if part is anchor bolt and is good candidate to dimension
        /// </summary>
        /// <param name="tView">Drawing view</param>
        /// <param name="settings">Application settings</param>
        /// <returns>True if dimension candidate</returns>
        public bool IsCandidateToDimension(Tekla.Structures.Drawing.View tView, AppData settings)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return IsInPlaneOfView(tView) && IsAnchorBolt(settings);
        }

        /// <summary>
        /// Checks to see if part passes anchor bolt select filter
        /// </summary>
        /// <param name="settings">UI data to get anchor bolt filter name</param>
        /// <returns>True if object matches filter</returns>
        private bool IsAnchorBolt(AppData settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(settings.AnchorBoltFilerName)) return true;
            return Operation.ObjectMatchesToFilter(ModelPart, settings.AnchorBoltFilerName);
        }

        /// <summary>
        /// Checks if view is planar xy
        /// </summary>
        /// <param name="tView">Drawing view</param>
        /// <returns>True if plan type view orientation</returns>
        private bool IsInPlaneOfView(Tekla.Structures.Drawing.View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            var coordSys = tView.DisplayCoordinateSystem;
            if (coordSys.AxisZ().IsParallel(new Vector(0, 0, 1))) return true;
            return false;
        }

        /// <summary>
        /// Gets dimension point for anchor bolt
        /// </summary>
        /// <param name="tView">Drawing view to dimension in</param>
        /// <param name="modelPart">Model part to get coordinate system from</param>
        /// <returns>Coordinate system origin adjusted to view display coordinate system and z origin value</returns>
        private static Point GetDimensionPoint(Tekla.Structures.Drawing.View tView, Part modelPart)
        {
            if (tView == null || modelPart == null) return null;
            var transMatrix = MatrixFactory.ToCoordinateSystem(tView.DisplayCoordinateSystem);
            var origin = modelPart.GetCoordinateSystem().Origin;
            var center = new Point(origin.X, origin.Y, tView.DisplayCoordinateSystem.Origin.Z).Round();
            var adjCenter = transMatrix.Transform(center);
            return adjCenter;
        }

        /// <summary>
        /// Gets part bottom point
        /// </summary>
        /// <param name="modelPart">Model part</param>
        /// <returns>Bottom position in current work plane</returns>
        private static Point GetBottomPoint(Part modelPart)
        {
            if (modelPart == null) return null;
            var bm = modelPart as Beam;
            if (bm != null) { return bm.EndPoint; }
            var origin = modelPart.GetCoordinateSystem().Origin;
            var tempValue = 0.0;
            modelPart.GetReportProperty("BOTTOM_LEVEL", ref tempValue);
            return new Point(origin.X, origin.Y, tempValue);
        }
    }
}
