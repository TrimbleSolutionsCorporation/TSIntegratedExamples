namespace SpreadsheetReinforcement.ModelLogic
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using PartProxy;
    using Services;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tools;
    using Tekla.Structures.Model.Operations;
    using ViewModel;

    public static class MainLogic
    {
        private const double MaxFootingGap = 16 * 25.4;
        private static ObservableCollection<SpreadsheetResultData> _designResults;
        private static SavedSetting _savedSetting;

        /// <summary>
        /// Imports spreadsheet data and adds reinforcement components to model for selected parts only
        /// </summary>
        /// <param name="savedSetting">Program settings</param>
        /// <param name="designResults">Cached design results</param>
        /// <param name="progressProxy"></param>
        public static void ImportForSelected(SavedSetting savedSetting,
            ObservableCollection<SpreadsheetResultData> designResults, ProgressHelper progressProxy)
        {
            if (savedSetting == null) throw new ArgumentNullException(nameof(savedSetting));
            if (progressProxy == null) throw new ArgumentNullException(nameof(progressProxy));
            if (string.IsNullOrEmpty(savedSetting.PillarFilter))
                throw new ApplicationException("Pile filter must not be blank or null.");
            if (string.IsNullOrEmpty(savedSetting.FootingFilter))
                throw new ApplicationException("Footing filter must not be blank or null.");

            _savedSetting = savedSetting;
            _designResults = designResults;

            var originalPlane = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try
            {
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                var selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
                var selectedObjects = selector.GetSelectedObjects();

                var footings = new List<Part>();
                var piles = new List<Part>();

                foreach (var soe in selectedObjects)
                {
                    var pt = soe as Part;
                    if (pt == null) continue;
                    if (Operation.ObjectMatchesToFilter(pt, savedSetting.PillarFilter)) piles.Add(pt);
                    else if (Operation.ObjectMatchesToFilter(pt, savedSetting.FootingFilter)) footings.Add(pt);
                }

                SortAndProcessParts(footings, piles, progressProxy);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
            finally
            {
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(originalPlane);
            }
        }

        /// <summary>
        /// Imports spreadsheet data and adds reinforcement components to model for all parts im model
        /// </summary>
        /// <param name="savedSetting">Program settings</param>
        /// <param name="designResults">Cached design results</param>
        /// <param name="progressProxy"></param>
        public static void ImportForAll(SavedSetting savedSetting,
            ObservableCollection<SpreadsheetResultData> designResults, ProgressHelper progressProxy)
        {
            if (savedSetting == null) throw new ArgumentNullException(nameof(savedSetting));
            if (progressProxy == null) throw new ArgumentNullException(nameof(progressProxy));
            if (string.IsNullOrEmpty(savedSetting.PillarFilter))
                throw new ApplicationException("Pile filter must not be blank or null.");
            if (string.IsNullOrEmpty(savedSetting.FootingFilter))
                throw new ApplicationException("Footing filter must not be blank or null.");
            _savedSetting = savedSetting;
            _designResults = designResults ?? throw new ArgumentNullException(nameof(designResults));

            var originalPlane = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try
            {
                var footings = new Model().GetModelObjectSelector().GetObjectsByFilterName(savedSetting.FootingFilter)
                    .ToList<Part>();
                var piles = new Model().GetModelObjectSelector().GetObjectsByFilterName(savedSetting.PillarFilter)
                    .ToList<Part>();
                SortAndProcessParts(footings, piles, progressProxy);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
            finally
            {
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(originalPlane);
            }
        }

        /// <summary>
        /// Tries to match pillars with pad footings
        /// </summary>
        /// <param name="footings"></param>
        /// <param name="pillars"></param>
        /// <param name="progressProxy"></param>
        private static void SortAndProcessParts(IEnumerable<Part> footings, List<Part> pillars, ProgressHelper progressProxy)
        {
            if (footings == null) throw new ArgumentNullException(nameof(footings));
            if (pillars == null) throw new ArgumentNullException(nameof(pillars));
            if (progressProxy == null) throw new ArgumentNullException(nameof(progressProxy));

            try
            {
                var matchedPairs = new List<FoundationPair>();
                var loneFootings = new List<Part>();
                foreach (var ft in footings)
                {
                    var footingTopCenter = ft.GetTopCenter();
                    if (footingTopCenter == null) continue;
                    var matchPiles =
                        pillars.Where(f => Distance.PointToPoint(f.GetBottomCenter(), footingTopCenter) < MaxFootingGap)
                            .OrderBy(f => Distance.PointToPoint(f.GetBottomCenter(), footingTopCenter))
                            .ToList();
                    if (matchPiles.Any()) matchedPairs.Add(new FoundationPair(ft, matchPiles[0]));
                    else loneFootings.Add(ft);
                }

                Import(matchedPairs, loneFootings, progressProxy);
                new Model().CommitChanges();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Applies spreadsheet import results to model, create reinforcement components
        /// </summary>
        /// <param name="matchedElements">Matched pillar and pad footing pairs</param>
        /// <param name="loneFootings">Isolated pad footings</param>
        /// <param name="progressProxy"></param>
        private static void Import(List<FoundationPair> matchedElements, List<Part> loneFootings, ProgressHelper progressProxy)
        {
            if (matchedElements == null) throw new ArgumentNullException(nameof(matchedElements));
            if (loneFootings == null) throw new ArgumentNullException(nameof(loneFootings));

            try
            {
                progressProxy.IsProgressIndeterminate = false;
                progressProxy.ProgressMax = matchedElements.Count() + loneFootings.Count();
                var counter = 0;

                foreach (var me in matchedElements)
                {
                    counter++;
                    var myPillar = new DesignPillar(me, _savedSetting, _designResults);
                    var myPadFooting = new DesignPadFooting(me.Footing, _savedSetting, _designResults);
                    Trace.WriteLine(!myPadFooting.InsertModify()
                        ? $"Failed to insert footing reinforcement for part w/Guid: {me.Footing.Identifier.GUID}"
                        : $"Insert succeeded for footing reinforcement for part w/Guid: {me.Footing.Identifier.GUID}");
                    Trace.WriteLine(!myPillar.InsertModify()
                        ? $"Failed to insert pile reinforcement for part w/Guid: {me.Pillar.Identifier.GUID}"
                        : $"Insert pile reinforcement succeeded for part w/Guid: {me.Footing.Identifier.GUID}");
                    progressProxy.ProgressValue = counter;
                }

                foreach (var pt in loneFootings)
                {
                    counter++;
                    new DesignPadFooting(pt, _savedSetting, _designResults).InsertModify();
                    progressProxy.ProgressValue = counter;
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Checks for valid file and returns read results
        /// </summary>
        /// <param name="savedSetting">Current settings</param>
        /// <returns>New file collection with results</returns>
        public static ObservableCollection<SpreadsheetResultData> GetFileResults(SavedSetting savedSetting)
        {
            if (savedSetting == null) throw new ArgumentNullException(nameof(savedSetting));
            try
            {
                if (string.IsNullOrEmpty(savedSetting.ImportFilePath))
                {
                    return new ObservableCollection<SpreadsheetResultData>();
                }

                var inputFile = new FileInfo(savedSetting.ImportFilePath);
                return !inputFile.Exists
                    ? new ObservableCollection<SpreadsheetResultData>()
                    : SpreadsheetReaderService.ReadFile(inputFile, savedSetting);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new ObservableCollection<SpreadsheetResultData>();
            }
        }
    }
}