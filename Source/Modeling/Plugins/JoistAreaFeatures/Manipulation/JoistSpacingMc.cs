namespace JoistAreaFeatures.Manipulation
{
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;
    using Distance = Tekla.Structures.Datatype.Distance;

    /// <summary>
    /// Main class to create joist spacing distance manipulators
    /// </summary>
    public sealed class JoistSpacingMc : ManipulationContext
    {
        private List<DistanceManipulator> joistSpacingManipulators;
        private DistanceManipulator startSpacingManipulator;

        /// <summary>
        /// Main constructor to call methods to create distance manipulators
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="feature">DM parent feature</param>
        public JoistSpacingMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            try
            {
                //Get data from plugin
                var componentInput = FeatureLogic.GetCurrentInput(this.Component);
                var uiData = this.Component.GetDataFromComponent();

                //Update internal logic to take into account latest data from plugin
                var liftingLogic = new JoistAreaMainLogic();
                liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //Create start and joist manipulators
                this.startSpacingManipulator = this.CreateStartOffsetManipulator(component, componentInput.Item2, liftingLogic, uiData);
                this.joistSpacingManipulators = this.CreateJoistDistanceManipulators(component, liftingLogic);

                //Add manipulators to the system
                if (this.startSpacingManipulator != null) this.AddManipulator(this.startSpacingManipulator);
                this.joistSpacingManipulators.ForEach(this.AddManipulator);

                //Draw custom graphics
                this.ReCreateGraphics();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Calls update method for Text and Distance manipulators then recreates graphics
        /// </summary>
        public override void UpdateContext()
        {
            //Sync component data with model
            this.Component.Select();

            //Get data from plugin
            var componentInput = FeatureLogic.GetCurrentInput(this.Component);
            var uiData = this.Component.GetDataFromComponent();

            //Update internal logic to take into account latest data from plugin
            var liftingLogic = new JoistAreaMainLogic();
            liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

            //Update existing manipulators
            this.UpdateStartOffsetManipulator(this.Component, componentInput.Item2, liftingLogic, uiData);
            this.UpdateJoistDistanceManipulators(this.Component, liftingLogic);

            //Update graphics based on plugin and manipulators
            this.ReCreateGraphics();
        }

        /// <summary>
        /// Redraws graphics for feature
        /// </summary>
        private void ReCreateGraphics()
        {
            this.Graphics?.Clear();
            //Any additional graphics here
        }

        /// <summary>
        /// Creates new DistanceManipulator to represent start offset
        /// </summary>
        /// <param name="plugin">Joist Area model instance</param>
        /// <param name="pluginLogic">Plugin logic external calculations</param>
        /// <param name="ls">Start edge guideline lineSegment</param>
        /// <param name="uiData">Plugin user interface data</param>
        /// <returns>New distance manipulator or null if failed</returns>
        private DistanceManipulator CreateStartOffsetManipulator(Component plugin,
            LineSegment ls,
            JoistAreaMainLogic pluginLogic,
            JoistAreaData uiData)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));

            var spacingSeg1 = this.GetStartOffsetManipulatorLineSegment(ls, pluginLogic, uiData);
            if (spacingSeg1 == null) return null;

            var distMan = new DistanceManipulator(plugin, this, spacingSeg1);
            distMan.MeasureChanged += this.StartOffset_OnMeasureChangedDel;
            return distMan;
        }

        /// <summary>
        /// Creates distance manipulators between joist beams and to 1st beam from GuideLine
        /// </summary>
        /// <param name="plugin">Joist Area model instance</param>
        /// <param name="pluginLogic">Plugin logic external calculations</param>
        private List<DistanceManipulator> CreateJoistDistanceManipulators(Component plugin,
            JoistAreaMainLogic pluginLogic)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            try
            {
                var result = new List<DistanceManipulator>();
                var joistManLines = FeatureLogic.GetJoistSpanSegments(pluginLogic);
                if (joistManLines == null) return new List<DistanceManipulator>();
                foreach (var ls in joistManLines)
                {
                    var distMan = new DistanceManipulator(plugin, this, ls);
                    distMan.MeasureChanged += this.Joist_OnMeasureChangedCenterDel;
                    result.Add(distMan);
                }
                return result;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<DistanceManipulator>();
            }
        }

        /// <summary>
        /// Updates existing start offset distance manipulator
        /// </summary>
        /// <param name="plugin">Joist Area model instance</param>
        /// <param name="pluginLogic">Plugin logic external calculations</param>
        /// <param name="ls">Start edge guideline lineSegment</param>
        /// <param name="uiData">Plugin user interface data</param>
        private void UpdateStartOffsetManipulator(Component plugin,
            LineSegment ls,
            JoistAreaMainLogic pluginLogic,
            JoistAreaData uiData)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));
            try
            {
                //Use existing method to calculate where manipulator should be
                var spacingSeg1 = this.GetStartOffsetManipulatorLineSegment(ls, pluginLogic, uiData);
                if (spacingSeg1 == null && this.startSpacingManipulator != null)
                {
                    this.startSpacingManipulator.MeasureChanged -= this.StartOffset_OnMeasureChangedDel;
                    this.startSpacingManipulator.Dispose();
                }

                //If no distance manipulator exists, create new one
                if (this.startSpacingManipulator == null && spacingSeg1 != null)
                {
                    var distMan = new DistanceManipulator(plugin, this, spacingSeg1);
                    distMan.MeasureChanged += this.StartOffset_OnMeasureChangedDel;
                    this.AddManipulator(distMan);
                    this.startSpacingManipulator = distMan;
                }
                else if (this.startSpacingManipulator != null && spacingSeg1 != null)
                {
                    //Update existing distance manipulator
                    this.startSpacingManipulator.Segment = new LineSegment(spacingSeg1.Point1, spacingSeg1.Point2);
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Updates existing joist spacing distance manipulators
        /// </summary>
        /// <param name="plugin">Model plugin instance</param>
        /// <param name="pluginLogic">Plugin logic external data calculations</param>
        private void UpdateJoistDistanceManipulators(Component plugin, JoistAreaMainLogic pluginLogic)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            try
            {
                //Calculate segments that define distance lines, if null dispose existing
                var currentSegments = FeatureLogic.GetJoistSpanSegments(pluginLogic);

                //No spacings found, remove all existing distance manipulators
                if (currentSegments == null)
                {
                    foreach (var distMan in this.joistSpacingManipulators)
                    {
                        distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                    }
                    this.joistSpacingManipulators.Clear();
                    Debug.WriteLine("Info: All existing joist spacing manipulators cleared, no new calculated spaces.");
                    return;
                }

                //Update existing segments
                var toRemoveIndexList = new List<int>();
                for (var i = 0; i < this.joistSpacingManipulators.Count; i++)
                {
                    var distMan = this.joistSpacingManipulators[i];
                    if (distMan == null) continue;
                    if (i > currentSegments.Count - 1)
                    {
                        //No longer needed distance manipulators
                        distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                        toRemoveIndexList.Add(i); //for removing outside enumeration
                        //Todo: how to remove from active feature context manipulators, deactivate?
                        Debug.WriteLine($"Existing: DistanceManipulator removed from cache: {distMan.Segment.Length():F3} at index: {i}.");
                    }
                    else
                    {
                        //Still needed, update segment
                        distMan.Segment = new LineSegment(currentSegments[i].StartPoint, currentSegments[i].EndPoint);
                        Debug.WriteLine($"Existing: DistanceManipulator updated: {distMan.Segment.Length():F3} at index: {i}.");
                    }
                }
                this.joistSpacingManipulators = this.GetPrunedList(this.joistSpacingManipulators, toRemoveIndexList);

                //Create DistanceManipulator objects for new segments
                var newSegments = Tools.TxLineSegment.GetFromListByIndex(currentSegments, this.joistSpacingManipulators.Count);
                if (newSegments != null)
                {
                    foreach (var ls in newSegments)
                    {
                        var distMan = new DistanceManipulator(plugin, this, ls);
                        distMan.MeasureChanged += this.Joist_OnMeasureChangedCenterDel;
                        this.joistSpacingManipulators.Add(distMan);
                        this.AddManipulator(distMan);
                        Debug.WriteLine($"New DistanceManipulator added: {distMan.Segment.Length():F3}.");
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private List<DistanceManipulator> GetPrunedList(List<DistanceManipulator> distanceManipulators, ICollection<int> toRemoveIndexList)
        {
            if (toRemoveIndexList == null || toRemoveIndexList.Count < 1) return distanceManipulators;
            var result = new List<DistanceManipulator>();
            for (var i = 0; i < distanceManipulators.Count; i++)
            {
                if (toRemoveIndexList.Contains(i)) continue;
                var distMan = distanceManipulators[i];
                result.Add(distMan);
            }
            return result;
        }

        /// <summary>
        /// Event handler for start offset distance manipulator measure changed
        /// </summary>
        /// <param name="sender">Distance Manipulator</param>
        /// <param name="e">Event args</param>
        private void StartOffset_OnMeasureChangedDel(object sender, EventArgs e)
        {
            if (!(sender is DistanceManipulator)) return;
            var distMan = (DistanceManipulator)sender;

            //Modify plugin spacing max value and call to update (also commits changes)
            DmCommon.ModifyComponent(this.Component, "FirstJoistOffset", distMan.Segment.Length());
        }

        /// <summary>
        /// Event handler for joist spacing distance manipulator measure changed
        /// </summary>
        /// <param name="sender">DistanceManipulator</param>
        /// <param name="e">Event args</param>
        private void Joist_OnMeasureChangedCenterDel(object sender, EventArgs e)
        {
            var distMan = sender as DistanceManipulator;
            if (distMan == null) return;
            try
            {
                var spacingTypIntVal = 0;
                this.Component.GetAttribute("SpacingType", ref spacingTypIntVal);
                var spacingTyp = (MainViewModel.SpacingTypeEnum)spacingTypIntVal;
                switch (spacingTyp)
                {
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                        {
                            var index = this.joistSpacingManipulators.IndexOf(distMan);
                            var spacingList = new List<Distance>();
                            for (var i = 0; i < this.joistSpacingManipulators.Count; i++)
                            {
                                var dManExisting = this.joistSpacingManipulators[i];
                                var length = i == index ? distMan.Segment.Length() : dManExisting.Segment.Length();
                                spacingList.Add(new Distance(length));
                            }

                            //Modify plugin exact spacing values and call to update
                            var updatedSpacings = new TxDistanceList(spacingList);
                            var spacingString = updatedSpacings.MetricStringList;

                            DmCommon.ModifyComponent(this.Component, "CenterSpacingList", spacingString);
                            break;
                        }
                    case MainViewModel.SpacingTypeEnum.albl_CenterToCenter:
                        {
                            //Modify plugin spacing max value and call to update
                            DmCommon.ModifyComponent(this.Component, "CenterSpacingMax", distMan.Segment.Length());
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Calculate line segment to represent start offset distance
        /// </summary>
        /// <param name="ls">LineSegment to get points used for manipulator</param>
        /// <param name="pluginLogic">Plugin logic external data calculations</param>
        /// <param name="uiData">Plugin active user interface data</param>
        /// <returns>New LineSegment or null if failed</returns>
        private LineSegment GetStartOffsetManipulatorLineSegment(LineSegment ls,
            JoistAreaMainLogic pluginLogic,
            JoistAreaData uiData)
        {
            if (ls == null) throw new ArgumentNullException(nameof(ls));
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            if (uiData == null) throw new ArgumentNullException(nameof(uiData));
            try
            {
                //get points for 1st distance segment
                var startPt = ls.GetMidPoint();
                var endPt = new Point(startPt);
                var localYdir = pluginLogic.GlobalCoordinateSystem.AxisY.GetNormal();
                endPt.Translate(localYdir * uiData.FirstJoistOffset);

                //If offset is zero - do not return usable line segment
                return Tekla.Structures.Geometry3d.Distance.PointToPoint(endPt, startPt) <
                       GeometryConstants.DISTANCE_EPSILON ? null : new LineSegment(startPt, endPt);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();

            this.joistSpacingManipulators?.ForEach(handle => handle.Dispose());
            this.joistSpacingManipulators?.Clear();
            this.startSpacingManipulator?.Dispose();

            //Dispose distance/measure type manipulators
            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            //Detach events from start offset spacing
            if (this.startSpacingManipulator != null)
            {
                this.startSpacingManipulator.MeasureChanged -= this.StartOffset_OnMeasureChangedDel;
            }

            //Detach events from joist spacing list
            if (this.joistSpacingManipulators == null) return;
            foreach (var distMan in this.joistSpacingManipulators)
            {
                distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
            }
        }
    }
}