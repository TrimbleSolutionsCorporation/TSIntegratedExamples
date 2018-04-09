using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Solid;

namespace AnchorBoltsWinform.Tools
{
    /// <summary>
    /// Basic class extensions
    /// </summary>
    public static class TxExtensions
    {
        /// <summary>
        /// Check if is parallel to other vector
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="angleTolerance">Tolerance in degree to check parallel</param>
        /// <returns>True if angle is close to 0 or 180</returns>
        public static bool IsParallel(this Vector v1, Vector v2, double angleTolerance = 0.0)
        {
            if (v2 == null) return false;
            if (Math.Abs(angleTolerance) < 0.001) angleTolerance = GeometryConstants.ANGULAR_EPSILON * 180 / Math.PI;
            var angle = Math.Abs(v1.GetAngleBetween(v2) * 180 / Math.PI);
            if (angle < angleTolerance) return true;
            return Math.Abs(angle - 180) < angleTolerance;
        }

        /// <summary>
        /// Creates new PointList with new points copied from existing list
        /// </summary>
        /// <param name="points">Existing points to copy</param>
        /// <param name="round">Round points to tenth of mm</param>
        /// <returns>New list with copied points</returns>
        public static PointList ToPointList(this IEnumerable points, bool round)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var result = new PointList();
            foreach (Point pt in points)
            {
                result.Add(round ? new Point(pt.Round(1)) : 
                    new Point(pt));
            }
            return result;
        }

        /// <summary>
        /// Rounds each part of a Point to number of decimal places
        /// </summary>
        /// <param name="pt">Point to round</param>
        /// <param name="rndPlaces">Number of places to round to</param>
        /// <returns>Rounded 3d Point</returns>
        public static Point Round(this Point pt, int rndPlaces = 2)
        {
            var dx = Math.Round(pt.X, rndPlaces);
            var dy = Math.Round(pt.Y, rndPlaces);
            var dz = Math.Round(pt.Z, rndPlaces);
            return new Point(dx, dy, dz);
        }

        /// <summary>
        /// Returns z vector for coordinate system
        /// </summary>
        /// <param name="coordSys">Coordinate system to get vectors from</param>
        /// <returns>Cross product of x-y vector</returns>
        public static Vector AxisZ(this CoordinateSystem coordSys)
        {
            if (coordSys == null) throw new ArgumentNullException(nameof(coordSys));
            return Vector.Cross(coordSys.AxisX, coordSys.AxisY);
        }

        /// <summary>
        /// Checks assembly type for part
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <returns>True if precast or cast in place type</returns>
        public static bool IsConcrete(this Tekla.Structures.Model.Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var assmTyp = pt.GetAssembly().GetAssemblyType();
            if (assmTyp == Assembly.AssemblyTypeEnum.IN_SITU_ASSEMBLY) return true;
            if (assmTyp == Assembly.AssemblyTypeEnum.PRECAST_ASSEMBLY) return true;
            return false;
        }

        /// <summary>
        /// Gets volume from part report property
        /// </summary>
        /// <param name="pt">Model part</param>
        /// <returns>Volume in mm3</returns>
        public static double GetVolume(this Tekla.Structures.Model.Part pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var tempValue = 0.0;
            pt.GetReportProperty("VOLUME", ref tempValue);
            return tempValue;
        }

        /// <summary>
        /// Translates point vector direction and distance
        /// </summary>
        /// <param name="pt">Point to get x,y,z coordinates from</param>
        /// <param name="tVector">Vector to get component translations from</param>
        /// <returns>New point translated</returns>
        public static Point Translate(this Point pt, Vector tVector)
        {
            var result = new Point(pt);
            result.Translate(tVector.X, tVector.Y, tVector.Z);
            return result;
        }

        /// <summary>
        /// Paints coordinate system in the model with temporary graphics
        /// </summary>
        /// <param name="cs"></param>
        public static void Paint(this CoordinateSystem cs)
        {
            if (cs == null) cs = new CoordinateSystem();

            //Local variables
            const double vScalar = 500.0;
            var graphicsDrawer = new GraphicsDrawer();

            //Color constants
            var red = new Color(1, 0, 0);
            var green = new Color(0, 1, 0);
            var blue = new Color(0, 0, 1);

            //Initialize axis points for arrows at origin
            var endX = new Point(cs.Origin);
            var endY = new Point(cs.Origin);
            var endZ = new Point(cs.Origin);

            //Move axis end points for arrows along vector scaled
            endX.Translate(cs.AxisX.GetNormal() * vScalar);
            endY.Translate(cs.AxisY.GetNormal() * vScalar);
            endZ.Translate(Vector.Cross(cs.AxisX, cs.AxisY).GetNormal() * vScalar);

            //Draw result lines from origin
            graphicsDrawer.DrawLineSegment(cs.Origin, endX, red);
            graphicsDrawer.DrawLineSegment(cs.Origin, endY, green);
            graphicsDrawer.DrawLineSegment(cs.Origin, endZ, blue);
        }

        /// <summary>
        /// Paints line segment in the model view with temporary graphics
        /// </summary>
        /// <param name="ls">Line segment to paint</param>
        /// <param name="col">Color to use, red if skipped</param>
        public static void Paint(this LineSegment ls, Color col = null)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (col == null) col = new Color(1, 0, 0);

            var gd = new GraphicsDrawer();
            gd.DrawLineSegment(ls.Point1, ls.Point2, col);
        }

        /// <summary>
        /// Creates small line segment graphic in model view to represent point
        /// </summary>
        /// <param name="pt">Creates new control point in the model to show location</param>
        public static void Paint(this Point pt)
        {
            if (pt == null) throw new ArgumentNullException(nameof(pt));
            var ptx = new ControlPoint(pt);
            ptx.Insert();
        }

        /// <summary>
        /// Paints outline of box in the model with temporary graphics
        /// </summary>
        /// <param name="box">Tekla AABB</param>
        /// <param name="color">If null, red is chosen</param>
        public static void Paint(this AABB box, Color color = null)
        {
            if (box == null) throw new ArgumentNullException(nameof(box));
            if (color == null) color = new Color(1, 0, 0);

            Paint(new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z), new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z), new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z), new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z), new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z)),
                  color);

            Paint(new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z), new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z), new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z), new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z), new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z)),
                  color);

            Paint(new LineSegment(new Point(box.MinPoint.X, box.MinPoint.Y, box.MinPoint.Z), new Point(box.MinPoint.X, box.MinPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MinPoint.X, box.MaxPoint.Y, box.MinPoint.Z), new Point(box.MinPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MinPoint.Y, box.MinPoint.Z), new Point(box.MaxPoint.X, box.MinPoint.Y, box.MaxPoint.Z)),
                  color);
            Paint(new LineSegment(new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MinPoint.Z), new Point(box.MaxPoint.X, box.MaxPoint.Y, box.MaxPoint.Z)),
                  color);
        }

        /// <summary>
        /// Draws faces of solid in model as mesh
        /// </summary>
        /// <param name="part">Part to get solid from</param>
        /// <param name="color">Color to paint faces</param>
        /// <returns>True</returns>
        public static bool Paint(this Solid part, Color color = null)
        {
            if(part == null) throw new ArgumentNullException(nameof(part));
            if(color == null) color = new Color(1, 0, 0);

            const int faceCount = 0;
            var loopCount = 0;
            foreach(var face in part.GetFaces())
            {
                foreach(var loop in face.GetLoops())
                {
                    loopCount++;
                    new List<Point>(loop.GetPoints()).DrawSurfaceInModel(false, color);
                }
            }
            Debug.WriteLine("Faces: " + faceCount + "\nLoops: " + loopCount);
            return true;
        }

        /// <summary>
        /// Draws mesh for list of points in the model using triangulation
        /// </summary>
        /// <param name="pointList">True to draw inside surface</param>
        /// <param name="drawInsideSurface">Draw cut surfaces, default false</param>
        /// <param name="color">Color to use for mesh</param>
        private static void DrawSurfaceInModel(this List<Point> pointList, bool drawInsideSurface = false, Color color = null)
        {
            if(color==null) color = new Color(1,0,0);
            var gd = new GraphicsDrawer();
            var mesh = new Mesh(new ArrayList(pointList), null, null);
            mesh.AddLine(1, 0);
            for(var i = pointList.Count - 1; i > 1; i--)
            {
                // Surfaces are "one sided" so we need to draw both sides in order to see a "backside" surface
                mesh.AddLine(i, i - 1);

                // Draws the 'outside' surface
                mesh.AddTriangle(i, i - 1, 0);

                // Draws the 'inside' surface
                if(drawInsideSurface) mesh.AddTriangle(0, i - 1, i);
            }
            mesh.AddLine(0, pointList.Count - 1);
            gd.DrawMeshLines(mesh, color);
            gd.DrawMeshSurface(mesh, color);
        }

        /// <summary>
        /// Gets the faces from the solid
        /// </summary>
        /// <param name="solid">Model solid</param>
        /// <returns>List of faces</returns>
        public static IEnumerable<Face> GetFaces(this Solid solid)
        {
            var faceList = solid.GetFaceEnumerator();
            while(faceList.MoveNext())
            {
                var face = faceList.Current;
                if(face != null) yield return face;
            }
        }

        /// <summary>
        /// Gets the loops from face
        /// </summary>
        /// <param name="face">Face</param>
        /// <returns>New list of loops</returns>
        public static IEnumerable<Loop> GetLoops(this Face face)
        {
            var loopList = face.GetLoopEnumerator();
            while(loopList.MoveNext())
            {
                var loop = loopList.Current;
                if(loop != null) yield return loop;
            }
        }

        /// <summary>
        /// Gets points from loop
        /// </summary>
        /// <param name="loop">Loop to enumerate</param>
        /// <returns>List of points from loop</returns>
        public static IEnumerable<Point> GetPoints(this Loop loop)
        {
            var vertexList = loop.GetVertexEnumerator();
            while(vertexList.MoveNext())
            {
                var point = vertexList.Current;
                if(point != null) yield return point;
            }
        }

        /// <summary>
        /// Inserts new layout point plugin
        /// </summary>
        /// <param name="pt">Point to insert at</param>
        /// <param name="label">Text label</param>
        public static void InsertLayoutPoint(this Point pt, string label = "")
        {
            if(pt == null) return;
            var componentInput = new ComponentInput();
            componentInput.AddOneInputPosition(pt);

            var component = new Component(componentInput)
                            {
                                Name = "LayoutPointPlugin",
                                Number = -100000
                            };

            component.SetAttribute("PointLabel", label);
            component.Insert();
        }
    }
}