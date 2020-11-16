namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// 3d triangle object
    /// </summary>
    public class TxTriangle : IComparable, IEquatable<TxTriangle>
    {
        private const double Epsilon = 0.01;

        /// <summary> First point in triangle </summary>
        public Point Point1 { get; set; }

        /// <summary> Second point in triangle </summary>
        public Point Point2 { get; set; }

        /// <summary> Third point in triangle </summary>
        public Point Point3 { get; set; }

        /// <summary>
        /// New instance of triangle from three points
        /// </summary>
        public TxTriangle(Point p1, Point p2, Point p3)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;
        }

        public int CompareTo(object obj)
        {
            var other = obj as TxTriangle;
            if (other == null) return -1;
            var dx = Distance.PointToPoint(other.Point1, Point1);
            var dy = Distance.PointToPoint(other.Point2, Point2);
            var dz = Distance.PointToPoint(other.Point3, Point3);
            if (Math.Abs(dx) < Epsilon && Math.Abs(dy) < Epsilon && Math.Abs(dz) < Epsilon) return 0;
            return 1;
        }

        public bool Equals(TxTriangle other)
        {
            if (other == null) return false;
            var dx = Distance.PointToPoint(other.Point1, Point1);
            var dy = Distance.PointToPoint(other.Point2, Point2);
            var dz = Distance.PointToPoint(other.Point3, Point3);
            if (Math.Abs(dx) < Epsilon && Math.Abs(dy) < Epsilon && Math.Abs(dz) < Epsilon) return true;
            return false;
        }

        /// <summary>
        /// Returns model mesh graphical object from triangle
        /// </summary>
        /// <returns>Model graphical mesh</returns>
        private Mesh ToMesh()
        {
            var mesh = new Mesh();

            //Add points
            mesh.AddPoint(Point1);
            mesh.AddPoint(Point2);
            mesh.AddPoint(Point3);

            //Add triangle in two directions
            mesh.AddTriangle(0, 1, 2);
            mesh.AddTriangle(2, 1, 0);

            //Addlines
            mesh.AddLine(0, 1);
            mesh.AddLine(1, 2);
            mesh.AddLine(2, 0);
            return mesh;
        }

        /// <summary>
        /// Paints mesh surface in model (border + surface)
        /// </summary>
        /// <param name="meshColor">Mesh color</param>
        /// <param name="lineColor">Line color</param>
        public void DrawInModel(Color meshColor = null, Color lineColor = null)
        {
            if (meshColor == null) meshColor = new Color(0, 1, 0);
            if (lineColor == null) lineColor = new Color(1, 0, 0);
            var gd = new GraphicsDrawer();

            var mesh = ToMesh();
            gd.DrawMeshSurface(mesh, meshColor);
            gd.DrawMeshLines(mesh, lineColor);
        }
    }
}
