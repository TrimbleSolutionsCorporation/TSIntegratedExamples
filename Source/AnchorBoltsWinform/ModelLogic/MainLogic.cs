namespace AnchorBoltsWinform.ModelLogic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;
    using Tools;
    using ViewModel;
    using Tekla.Structures.Drawing;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;

    /// <summary>
    /// Main logic service class for dimensioning
    /// </summary>
    public static class MainLogic
    {
        /// <summary> UI data property </summary>
        private static AppData UiData { get; set; }

        /// <summary>
        /// Launch method to create dimensions in drawing view
        /// </summary>
        /// <returns>True if at least one view was used to call dimensioning logic in</returns>
        public static bool Run(AppData uiData)
        {
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));
            UiData = uiData;

            //Check that drawing is open
            var dwg = DrawingTools.ActiveDrawing;
            if (dwg == null) return false;

            //Enumerate all drawing views
            var viewsEdited = 0;
            var views = dwg.GetSheet().GetViews();
            while (views.MoveNext())
            {
                //Cast to model view, check if null
                var tView = views.Current as Tekla.Structures.Drawing.View;
                if (tView == null) continue;

                //Find certain view to dimension only
                if (IsPlanView(tView)) { if (CreateDimensions(tView)) viewsEdited++; }
            }
            return viewsEdited > 0;
        }

        /// <summary>
        /// Check if view is in orientation that is plan type orientation
        /// </summary>
        /// <param name="tView">Drawing view to check orientation of</param>
        /// <returns>True if view coordinate system has z vector matching global z</returns>
        private static bool IsPlanView(Tekla.Structures.Drawing.View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            var coordSys = tView.DisplayCoordinateSystem;
            return coordSys.AxisZ().IsParallel(new Vector(0, 0, 1), 0.5);
        }

        /// <summary>
        /// Main dimension creation logic for view
        /// </summary>
        /// <param name="tView">Drawing view to dimension</param>
        /// <returns>False exception caught or CreateDimensionSet returns null</returns>
        private static bool CreateDimensions(Tekla.Structures.Drawing.View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            try
            {
                //Get view parts and anchor bolts
                var allViewParts = GetAllViewParts(tView);
                var anchorBolts = GetAnchorBolts(allViewParts, tView);
                if (!anchorBolts.Any()) return false;

                //Find concrete surrounding part
                var fatherPt = TryGetConcretePart(allViewParts, anchorBolts.First(), tView);

                //Get points to dimension
                var dimensionPoints = GetAnchorBoltPositions(anchorBolts, tView);
                if (!dimensionPoints.Any()) return false;

                //Create dimension settings to use
                var dimSettings = new StraightDimensionSet.StraightDimensionSetAttributes(null, UiData.DimensionSettingsName);
                dimSettings.Placing.Placing = DimensionSetBaseAttributes.Placings.Fixed; //override placement to not move
                var userOffset = UiData.DimensionLineOffset * tView.Attributes.Scale;

                //Create left side direction dimension set
                {
                    //Get points with distinct y values, add only left most in group that line up
                    var dimPoints = new PointList();
                    var results = dimensionPoints.GroupBy(p => p.Y, (key, g) => new {YValue = key, SimPoints = g.ToList()});
                    foreach (var tp in results) { dimPoints.Add(tp.SimPoints.OrderBy(f=>f.X).First());}
                    
                    //Create dimension set handler with calculated offset from solid face edge
                    var dimDirection = new Vector(-1, 0, 0);
                    var offset = GetOffset(anchorBolts, dimDirection, UiData, fatherPt, tView) + userOffset;
                    new StraightDimensionSetHandler().CreateDimensionSet(tView, dimPoints, dimDirection, offset,dimSettings);
                }

                //Create top side direction dimension set
                {
                    //Get points with distinct x values, add only left most in group that line up
                    var dimPoints = new PointList();
                    var results = dimensionPoints.GroupBy(p => p.X, (key, g) => new { XValue = key, SimPoints = g.ToList() });
                    foreach(var tp in results) { dimPoints.Add(tp.SimPoints.OrderBy(f => f.Y).Last()); }

                    var dimDirection = new Vector(0, 1, 0);
                    var offset = GetOffset(anchorBolts, dimDirection, UiData, fatherPt, tView) + userOffset;
                    new StraightDimensionSetHandler().CreateDimensionSet(tView, dimPoints, dimDirection, offset, dimSettings);
                }

                //Check result and save to drawing database
                DrawingTools.ActiveDrawing.CommitChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: Failed to create dimension sets.");
                Debug.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Get concrete part that anchor bolt is contained in if exists
        /// </summary>
        /// <param name="allViewParts">All parts from view</param>
        /// <param name="anchorBolt">Anchor bolt part to use for checking location boundary</param>
        /// <param name="tView">Drawing view for referencing coordinate system</param>
        /// <returns>Model part if found, null otherwise</returns>
        private static Tekla.Structures.Model.Part TryGetConcretePart(
            IReadOnlyCollection<Tekla.Structures.Model.Part> allViewParts, AnchorBolt anchorBolt, Tekla.Structures.Drawing.View tView)
        {
            if (allViewParts == null || tView==null || allViewParts.Count < 1 || anchorBolt == null) return null;
            var candidates = new List<Tekla.Structures.Model.Part>();
            foreach (var pt in allViewParts)
            {
                //Check if concrete and get solid box
                if (!pt.IsConcrete()) continue;
                var partSolid = pt.GetSolid();
                var partBox = new AABB(partSolid.MinimumPoint, partSolid.MaximumPoint);

//#if DEBUG //temp paint methods for troubleshooting
//                partBox.Paint(new Color(0, 1, 0));
//                anchorBolt.BottomPoint.Paint();
//#endif
                //Check anchor bolt bottom point if it is inside current part box solid
                if (!partBox.IsInside(anchorBolt.BottomPoint)) continue;
                candidates.Add(pt);
            }

            //If no candidates return null, otherwise return largest by volume from list
            if (candidates.Count < 1) return null;
            return candidates.OrderBy(f => f.GetVolume()).LastOrDefault();
        }

        /// <summary>
        /// Gets minimum offset from anchor bolt points and part containing part solid
        /// </summary>
        /// <param name="anchorBolts">Anchor bolts to dimension</param>
        /// <param name="dimDirection">Direction to dimension</param>
        /// <param name="settings">Application settings used</param>
        /// <param name="fatherPart">Contained concrete part to dimension are reference to</param>
        /// <param name="tView">Drawing view parts are from and dimensioned in</param>
        /// <returns>Dimension offset to use</returns>
        private static double GetOffset(List<AnchorBolt> anchorBolts, Vector dimDirection, AppData settings,
                                        Tekla.Structures.Model.Part fatherPart, Tekla.Structures.Drawing.View tView)
        {
            if (anchorBolts == null) throw new ArgumentNullException(nameof(anchorBolts));
            if (dimDirection == null) throw new ArgumentNullException(nameof(dimDirection));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (tView == null) throw new ArgumentException(nameof(tView));
            if (fatherPart == null) return settings.DimensionLineOffset;

            //Get part solid
            var offsetValues = new List<double>();
            var fatherPartSolid = fatherPart.GetSolid();
            foreach (var ab in anchorBolts)
            {
                //Create new solid intersect line from anchor bolt outwards in dimension direction
                var p1 = new Point(ab.BottomPoint);
                var p2 = new Point(p1.Translate(dimDirection.GetNormal() * 250));
                var intersectLine = new LineSegment(p1, p2);
#if DEBUG
                intersectLine.Paint(); //temp troubleshooting paint method
#endif

                //Check intersection of test line and part solid to see if collision
                var intersectResult = fatherPartSolid.Intersect(intersectLine);
                if (intersectResult == null || intersectResult.Count < 1) continue;
#if DEBUG
                (intersectResult[intersectResult.Count - 1] as Point).Paint();
#endif
                //Add to results list measured distance to solid intersection
                offsetValues.Add(Distance.PointToPoint(p1, intersectResult[intersectResult.Count - 1] as Point));
            }

            //Return 0.0 if no collisions found, otherwise return min value from list
            if (offsetValues.Count < 1) return 0.0;
            return offsetValues.Min();
        }

        /// <summary>
        /// Gets anchor bolt positions from list of anchor bolts
        /// </summary>
        /// <param name="anchorBolts">Custom anchor bolt objects</param>
        /// <param name="tView">Drawing view to reference</param>
        /// <returns>New list of rounded points to dimension</returns>
        private static List<Point> GetAnchorBoltPositions(List<AnchorBolt> anchorBolts,
                                                          Tekla.Structures.Drawing.View tView)
        {
            if (anchorBolts == null) throw new ArgumentNullException(nameof(anchorBolts));
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            return anchorBolts.Select(ab => ab.DimensionPoint.Round(1)).ToList();
        }

        /// <summary>
        /// Gets anchor bolts from view parts list, checks if good candidate to dimension in view
        /// </summary>
        /// <param name="allViewParts">All view parts</param>
        /// <param name="tView">Drawing view to reference</param>
        /// <returns>New list of custom anchor bolts</returns>
        private static List<AnchorBolt> GetAnchorBolts(List<Tekla.Structures.Model.Part> allViewParts,
                                                       Tekla.Structures.Drawing.View tView)
        {
            if (allViewParts == null) throw new ArgumentNullException(nameof(allViewParts));
            var result = new List<AnchorBolt>();
            foreach (var pt in allViewParts)
            {
                var ancBolt = new AnchorBolt(pt, tView);
                if (!ancBolt.IsCandidateToDimension(tView, UiData)) continue;
                result.Add(ancBolt);
            }
            return result;
        }

        /// <summary>
        /// Gets all model parts from view parts
        /// </summary>
        /// <param name="tView">Drawing view to get parts from</param>
        /// <returns>New list of model parts</returns>
        private static List<Tekla.Structures.Model.Part> GetAllViewParts(Tekla.Structures.Drawing.View tView)
        {
            if (tView == null) throw new ArgumentNullException(nameof(tView));
            var result = new List<Tekla.Structures.Model.Part>();
            var viewObjEnum = tView.GetModelObjects();
            while (viewObjEnum.MoveNext())
            {
                var dwgPart = viewObjEnum.Current as Tekla.Structures.Drawing.Part;
                if (dwgPart == null) continue;
                var mdlPart = new Model().SelectModelObject(dwgPart.ModelIdentifier) as Tekla.Structures.Model.Part;
                if (mdlPart == null) continue;
                result.Add(mdlPart);
            }
            return result;
        }
    }
}
