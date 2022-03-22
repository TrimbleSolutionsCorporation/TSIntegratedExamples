namespace SpreadsheetReinforcement.ModelLogic
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tekla.Structures.Catalogs;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;
    using Tools;

    public class PadFootingMinReinfCalculator
    {
        private readonly Part _footing;
        private List<RebarItem> _rebarCatalog;
        private readonly SpreadsheetResultData _designResult;
        private readonly SavedSetting _savedSetting;

        /// <summary> Rebar size in x direction </summary>
        public string RebarSizeX { get; set; }

        /// <summary> Rebar spacing in x direction </summary>
        public double RebarSpacingX { get; set; }

        /// <summary> Rebar size in y direction </summary>
        public string RebarSizeZ { get; set; }

        /// <summary> Rebar spacing in y direction </summary>
        public double RebarSpacingZ { get; set; }

        /// <summary>
        /// New instance of reinforcement calculator
        /// Calculates and fills public properties with constructor
        /// </summary>
        /// <param name="footing">Pad footing</param>
        /// <param name="dr">Spreadsheet design results for min area</param>
        /// <param name="setting">Program settings</param>
        public PadFootingMinReinfCalculator(Part footing, SpreadsheetResultData dr, SavedSetting setting)
        {
            _footing = footing ?? throw new ArgumentNullException(nameof(footing));
            _designResult = dr ?? throw new ArgumentNullException(nameof(dr));
            _savedSetting = setting ?? throw new ArgumentNullException(nameof(setting));
            CacheRebarSize();
            CalculateSizes();
        }

        /// <summary>
        /// Creates cache of rebar catalog to get area and size information from
        /// </summary>
        private void CacheRebarSize()
        {
            var rebarItems = new CatalogHandler().GetRebarItems().ToList<RebarItem>();
            _rebarCatalog = new List<RebarItem>();
            var filteredRebarItems = rebarItems.Where(f => f.Grade == _designResult.RebarGrade)
                .OrderBy(f => f.ActualDiameter);
            foreach (var itm in filteredRebarItems)
            {
                _rebarCatalog.Add(itm);
            }
        }

        /// <summary>
        /// Calculates quantity and spacing for bars to meet minimum area reinforcement from spreadsheet results
        /// </summary>
        private void CalculateSizes()
        {
            var maxSpacing = _savedSetting.MaxSpacing;
            CalculateBottomSteelQtyAndSize(maxSpacing);
            //todo: CalculateTopSteelQtyAndSize
        }

        private void CalculateBottomSteelQtyAndSize(double maxSpacing)
        {
            //Get size and spacing for local x-Direction
            {
                var foundQty = false;
                var calcSize = string.Empty;
                var calcSpacing = 0.0;
                int calcQty;
                var metricMinAreaSteel = _designResult.BottomXSteel;
                var xDistance = GetMeasureDx(_footing);
                var xDistanceNet = xDistance - 2 * _savedSetting.MinClearCoverSides;
                foreach (var rg in _rebarCatalog)
                {
                    if (foundQty) break;
                    var db = rg.ActualDiameter;
                    var minSpacing = GetMinSpacing(rg);
                    calcSpacing = maxSpacing;
                    calcSize = rg.Size;

                    while (calcSpacing > minSpacing)
                    {
                        calcQty = Convert.ToInt32(Math.Floor((xDistanceNet + db) / calcSpacing) + 1);
                        var ar = rg.CrossSectionArea * calcQty;

                        //Check area still is enough
                        if (ar >= metricMinAreaSteel)
                        {
                            foundQty = true;
                            break;
                        }
                        calcSpacing -= _savedSetting.SpacingStepInterval;
                    }
                }
                RebarSizeX = calcSize;
                RebarSpacingX = calcSpacing;
            }

            //Get size and spacing for local z-Direction
            {
                var foundQty = false;
                var calcSize = string.Empty;
                var calcSpacing = 0.0;
                int calcQty;
                var metricMinAreaSteel = _designResult.BottomZSteel;
                var xDistance = GetMeasureDz(_footing);
                var xDistanceNet = xDistance - 2 * _savedSetting.MinClearCoverSides;
                foreach (var rg in _rebarCatalog)
                {
                    if (foundQty) break;
                    var db = rg.ActualDiameter;
                    var minSpacing = GetMinSpacing(rg);
                    calcSpacing = maxSpacing;
                    calcSize = rg.Size;

                    while (calcSpacing > minSpacing)
                    {
                        calcQty = Convert.ToInt32(Math.Floor((xDistanceNet + db) / calcSpacing) + 1);
                        var ar = rg.CrossSectionArea * calcQty;

                        //Check area still is enough
                        if (ar >= metricMinAreaSteel)
                        {
                            foundQty = true;
                            break;
                        }
                        calcSpacing -= _savedSetting.SpacingStepInterval;
                    }
                }
                RebarSizeZ = calcSize;
                RebarSpacingZ = calcSpacing;
            }
        }

        /// <summary>
        /// Measure footing width in local x-direction
        /// </summary>
        /// <param name="mo">Pad footing</param>
        /// <returns>mm value</returns>
        private static double GetMeasureDx(Part mo)
        {
            if (mo == null)
            {
                throw new ArgumentNullException(nameof(mo));
            }

            var partSolid = mo.GetSolid();
            var center = new LineSegment(partSolid.MinimumPoint, partSolid.MaximumPoint).GetMidPoint();
            var localXDir = mo.GetCoordinateSystem().AxisX.GetNormal();

            var dxHalfDist = 0.0;
            mo.GetReportProperty("HEIGHT", ref dxHalfDist);
            dxHalfDist = dxHalfDist * 1.50; //Make distance longer
            var p1 = new Point(center);
            var p2 = new Point(center);

            p1.Translate(localXDir * dxHalfDist);
            p2.Translate(localXDir * -dxHalfDist);
            var measureSolidLine = new LineSegment(p1, p2);
#if DEBUG
            measureSolidLine.PaintLine(new Color(0, 0.75, 0.65));
#endif
            var intersections = partSolid.Intersect(measureSolidLine);
            if (intersections == null || intersections.Count < 2)
            {
                return 0.0;
            }
#if DEBUG
            foreach (Point pt in intersections)
            {
                new ControlPoint(pt).Insert();
            }

#endif
            return Distance.PointToPoint(intersections[intersections.Count - 1] as Point, intersections[0] as Point);
        }

        /// <summary>
        /// Measure footing width in local z-direction
        /// </summary>
        /// <param name="mo">Pad footing</param>
        /// <returns>mm value</returns>
        private static double GetMeasureDz(Part mo)
        {
            if (mo == null)
            {
                throw new ArgumentNullException(nameof(mo));
            }

            var partSolid = mo.GetSolid();
            var center = new LineSegment(partSolid.MinimumPoint, partSolid.MaximumPoint).GetMidPoint();
            var localzDir = mo.GetCoordinateSystem().GetAxisZ();

            var dzHalfDist = 0.0;
            mo.GetReportProperty("WIDTH", ref dzHalfDist);
            dzHalfDist = dzHalfDist * 1.50; //Make distance longer
            var p1 = new Point(center);
            var p2 = new Point(center);

            p1.Translate(localzDir * dzHalfDist);
            p2.Translate(localzDir * -dzHalfDist);
            var measureSolidLine = new LineSegment(p1, p2);
#if DEBUG
            measureSolidLine.PaintLine(new Color(0, 0.75, 0.65));
#endif
            var intersections = partSolid.Intersect(measureSolidLine);
            if (intersections == null || intersections.Count < 2)
            {
                return 0.0;
            }
#if DEBUG
            foreach (Point pt in intersections)
            {
                new ControlPoint(pt).Insert();
            }

#endif
            return Distance.PointToPoint(intersections[intersections.Count - 1] as Point, intersections[0] as Point);
        }

        /// <summary>
        /// Gets minimum spacing between bar for ACI 318
        /// Also takes into account design preference for MinBarSpacingFactor setting
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private double GetMinSpacing(RebarItem item)
        {
            var m1 = Math.Max(25.4, item.ActualDiameter);
            var m2 = Math.Max(m1, _savedSetting.MinBarSpacingFactor * item.ActualDiameter);
            //Get max of 1.33 * maximum aggregate size?
            return Math.Max(m1, m2);
        }
    }
}