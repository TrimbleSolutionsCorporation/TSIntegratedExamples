namespace JoistArea.Tools
{
    using System;
    using System.Collections;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model.UI;

    public static class TxGeometricPlane
    {
        public static void Paint(this GeometricPlane gp)
        {
            if (gp == null) throw new ArgumentNullException(nameof(gp));
            double length = 800.0;
            double height = 600.0;

            //Calculate x and y relative vectors from plane
            var center = gp.Origin;
            var zAxis = gp.GetNormal();
            Vector xAxis = null;
            Vector yAxis = null;
            if (zAxis.IsParallel(new Vector(1, 0, 0), 15))
            {
                yAxis = Vector.Cross(zAxis, new Vector(0,1,0)).GetNormal();
                xAxis = Vector.Cross(zAxis, yAxis).GetNormal();
            }
            else
            {
                xAxis = Vector.Cross(zAxis, new Vector(1,0,0)).GetNormal();
                yAxis = Vector.Cross(zAxis, xAxis).GetNormal();
            }

            //Define edge points
            var p1 = new Point(center);
            p1.Translate(xAxis * -(length/2));
            var p2 = new Point(center);
            p2.Translate(xAxis *(length/2));
            var p3 = new Point(p2);
            p3.Translate(yAxis * height);
            var p4 = new Point(p1);
            p4.Translate(yAxis * height);
            var pts = new ArrayList {p1, p2, p3, p4};

            //Define triangles
            var t1 = new ArrayList {0, 2, 1};
            var t2 = new ArrayList { 0, 1, 2 };
            var t3 = new ArrayList {0, 2, 3};
            var t4 = new ArrayList {0, 3, 2};
            var triangles = new ArrayList { t1, t2, t3, t4};

            //Define lines
            var l1 = new ArrayList {0, 1};
            var l2 = new ArrayList { 1, 2};
            var l3 = new ArrayList {2, 3};
            var l4 = new ArrayList { 3, 0};
            var lines = new ArrayList {l1, l2, l3, l4};

            //Define mesh
            var ms = new Mesh();
            foreach (var pt in pts) ms.AddPoint(pt as Point);
            ms.AddTriangle(0, 2, 1);
            ms.AddTriangle(0, 1, 2);
            ms.AddTriangle(0, 2, 3);
            ms.AddTriangle(0, 3, 2);
            ms.AddLine(0, 1);
            ms.AddLine(1, 2);
            ms.AddLine(2, 3);
            ms.AddLine(3, 0);

            //Draw mesh
            new GraphicsDrawer().DrawMeshSurface(ms, new Color(0.5, 0.5, 0.0));

            //Draw normal vector
            var cPlane = new Point(center);
            cPlane.Translate(yAxis * height);
            var cp2 = new Point(cPlane);
            cp2.Translate(zAxis * 500.0);
            new GraphicsDrawer().DrawLineSegment(new LineSegment(cPlane, cp2), new Color(0.0, 1.0, 1.0));
        }
    }
}
