namespace AnchorBoltsSimple
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using Tekla.Structures.Drawing;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tools;
    using Point = Tekla.Structures.Geometry3d.Point;
    using Vector = Tekla.Structures.Geometry3d.Vector;

    /// <summary>
    /// Main logic service class for dimensioning
    /// </summary>
    public static class MainLogic
    {
        public const string AnchorBoltFilterName = "Steel_Anchor_Rod";
        public const double DimensionLineOffset = 150.0;
        public const string DimensionSettingsName = "standard";

        /// <summary>
        /// Launch method to create dimensions in drawing view
        /// </summary>
        /// <returns>True if at least one view was used to call dimensioning logic in</returns>
        public static bool Run()
        {
            //Check that a drawing is open
            var dwg = DrawingTools.ActiveDrawing;
            if (dwg == null) return false;

            //Enumerate all drawing views
            var viewsEdited = 0;
            var views = dwg.GetSheet().GetViews();
            while (views.MoveNext())
            {
                //Cast to model view, check if null
                var tView = views.Current as View;
                if (tView == null) continue;

                //Find certain view to dimension only
                if (IsPlanView(tView)) { if (CreateDimensions(tView)) viewsEdited++; }
            }
            return viewsEdited > 0;
        }

        /// <summary>
        /// Main dimension creation logic for view
        /// </summary>
        /// <param name="tView">Drawing view to dimension</param>
        /// <returns>False exception caught or CreateDimensionSet returns null</returns>
        private static bool CreateDimensions(View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            try
            {
                //Get objects to dimension
                var anchorBolts = GetDimensionObjects(tView, AnchorBoltFilterName);
                if (!anchorBolts.Any()) return false;

                //Get points to dimension
                var dimensionPoints = GetAnchorBoltPositions(anchorBolts, tView);
                if (!dimensionPoints.Any()) return false;

                //Create dimension settings to use
                var dimSettings = new StraightDimensionSet.StraightDimensionSetAttributes(null, DimensionSettingsName);
                dimSettings.Placing.Placing = DimensionSetBaseAttributes.Placings.Free;
                var dimSet = new StraightDimensionSetHandler();

                //Create new dimensions sets
                var dim1 = dimSet.CreateDimensionSet(tView, dimensionPoints.CopyTo(), 
                                                     new Vector(-1, 0, 0), DimensionLineOffset, dimSettings);
                var dim2 = dimSet.CreateDimensionSet(tView, dimensionPoints.CopyTo(), 
                                                     new Vector(0, 1, 0), DimensionLineOffset, dimSettings);

                //Check result and save to drawing database
                if (dim1 == null || dim2 == null) return false;
                DrawingTools.ActiveDrawing.CommitChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Failed to create dimension sets.");
                Debug.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Gets positions of anchor bolts to dimension
        /// </summary>
        /// <param name="anchorBolts">Parts in model that are anchor bolts to dimension</param>
        /// <param name="tView">Drawing view to get coordinate system from</param>
        /// <returns>List of points to dimension in view display coordinate system</returns>
        private static List<Point> GetAnchorBoltPositions(List<Tekla.Structures.Model.Part> anchorBolts, View tView)
        {
            if (anchorBolts == null) throw new ArgumentNullException(nameof(anchorBolts));
            if (tView == null) throw new ArgumentNullException(nameof(tView));

            //Get view transformation matrix
            var transMatrix = MatrixFactory.ToCoordinateSystem(tView.DisplayCoordinateSystem);

            var result = new List<Point>();
            foreach (var anchorBolt in anchorBolts)
            {
                //Get anchor bolt point
                var origin = anchorBolt.GetCoordinateSystem().Origin;
                var center = new Point(origin.X, origin.Y, origin.Z).Round(1);

                //Transform to view display coordinate system and add to results
                var adjCenter = transMatrix.Transform(center);
                result.Add(adjCenter);
            }
            return result;
        }

        /// <summary>
        /// Gets anchor bolt parts from view
        /// </summary>
        /// <param name="tView">Drawing view</param>
        /// <param name="filterName">Pre-existing select filter name to distinguish anchor bolt parts, cannot be null/empty</param>
        /// <returns>List of anchor bolt model parts found</returns>
        private static List<Tekla.Structures.Model.Part> GetDimensionObjects(View tView, string filterName)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            var result = new List<Tekla.Structures.Model.Part>();
            var viewObjEnum = tView.GetModelObjects();
            while (viewObjEnum.MoveNext())
            {
                //Cast and filter out objects not drawing part type
                var dwgPart = viewObjEnum.Current as Tekla.Structures.Drawing.Part;
                if (dwgPart == null) continue;

                //Get model part from drawing part
                var mdlPart = new Model().SelectModelObject(dwgPart.ModelIdentifier) as Tekla.Structures.Model.Part;
                if (mdlPart == null) continue;

                //Check if model part is anchor bolt by using filter check method, add to result list if pass
                if (!Tekla.Structures.Model.Operations.Operation.ObjectMatchesToFilter(mdlPart, filterName)) continue;
                result.Add(mdlPart);
            }
            return result;
        }

        /// <summary>
        /// Check if view is in orientation that is plan type orientation
        /// </summary>
        /// <param name="tView">Drawing view to check orientation of</param>
        /// <returns>True if view coordinate system has z vector matching global z</returns>
        private static bool IsPlanView(View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            var coordSys = tView.DisplayCoordinateSystem;
            if (Vector.Cross(coordSys.AxisX, coordSys.AxisY).IsParallel(new Vector(0, 0, 1))) return true;
            return false;
        }
    }
}
