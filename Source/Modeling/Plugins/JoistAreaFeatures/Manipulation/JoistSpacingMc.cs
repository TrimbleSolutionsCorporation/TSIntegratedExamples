namespace JoistAreaFeatures.Manipulation
{
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using System;
    using System.Collections.Generic;
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
        private List<DistanceManipulator> _joistSpacingManipulators;
        private DistanceManipulator _startSpacingManipulator;
        private JoistAreaMainLogic _liftingLogic;

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
                //Create start and joist manipulators
                this._startSpacingManipulator = this.CreateStartOffsetManipulator(component);
                this._joistSpacingManipulators = this.CreateJoistDistanceManipulators(component);
                this.AttachHandlers();

                //Add manipulators to the system
                if (this._startSpacingManipulator != null) this.AddManipulator(this._startSpacingManipulator);
                this._joistSpacingManipulators.ForEach(this.AddManipulator);

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
            base.UpdateContext();
            this.Component.Select();

            //Update existing manipulators
            this.UpdateStartOffsetManipulator(this.Component);
            this.UpdateJoistDistanceManipulators(this.Component);

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
        /// <param name="plugin">Model plugin instance</param>
        /// <returns>New distance manipulator or null if failed</returns>
        private DistanceManipulator CreateStartOffsetManipulator(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            var spacingSeg1 = this.GetStartOffsetManipulatorLineSegment(plugin);
            if (spacingSeg1 == null) return null;
            return new DistanceManipulator(plugin, this, spacingSeg1);
        }

        /// <summary>
        /// Creates distance manipulators between joist beams and to 1st beam from GuideLine
        /// </summary>
        /// <param name="plugin">Joist Area model instance</param>
        private List<DistanceManipulator> CreateJoistDistanceManipulators(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            var result = new List<DistanceManipulator>();

            try
            {
                var joistManLines = this.GetJoistManipulatorsLineSegments(plugin);
                if (joistManLines == null) return new List<DistanceManipulator>();
                foreach (var ls in joistManLines)
                {
                    var distMan = new DistanceManipulator(plugin, this, ls);
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
        /// <param name="plugin">Model plugin instance</param>
        private void UpdateStartOffsetManipulator(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Use existing method to calculate where manipulator should be
                var spacingSeg1 = this.GetStartOffsetManipulatorLineSegment(plugin);
                if (spacingSeg1 == null && this._startSpacingManipulator != null)
                {
                    this._startSpacingManipulator.MeasureChanged -= this.StartOffset_OnMeasureChangedDel;
                    this._startSpacingManipulator.Dispose();
                }

                //If no distance manipulator exists, create new one
                if (this._startSpacingManipulator == null && spacingSeg1 != null)
                {
                    var distMan = new DistanceManipulator(plugin, this, spacingSeg1);
                    distMan.MeasureChanged += this.StartOffset_OnMeasureChangedDel;
                    this.AddManipulator(distMan);
                    this._startSpacingManipulator = distMan;
                }
                else if (this._startSpacingManipulator != null && spacingSeg1 != null)
                {
                    //Update existing distance manipulator
                    this._startSpacingManipulator.Segment = new LineSegment(spacingSeg1.Point1, spacingSeg1.Point2);
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
        private void UpdateJoistDistanceManipulators(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Calculate segments that define distance lines, if null dispose existing
                var calcJoistManLines = this.GetJoistManipulatorsLineSegments(plugin);

                //No spacings found, remove existing distance manipulators
                if (calcJoistManLines == null)
                {
                    foreach (var distMan in this._joistSpacingManipulators)
                    {
                        distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                    }
                    return;
                }

                //For each calculated new distance line, update existing
                if (this._joistSpacingManipulators == null) return;
                for (var i = 0; i < calcJoistManLines.Count; i++)
                {
                    var calcLine = calcJoistManLines[i];

                    //Check if beyond original list qty
                    if (this._joistSpacingManipulators.Count < i + 1)
                    {
                        var distMan = new DistanceManipulator(plugin, this, calcLine);
                        distMan.MeasureChanged += this.Joist_OnMeasureChangedCenterDel;
                        this._joistSpacingManipulators.Add(distMan);
                        this.AddManipulator(distMan);
                    }
                    else
                    {
                        this._joistSpacingManipulators[i].Segment = new LineSegment(calcLine.Point1, calcLine.Point2);
                    }
                }

                //Remove no-longer used distance manipulators   //todo: verify this works?
                if (this._joistSpacingManipulators.Count > calcJoistManLines.Count)
                {
                    var intRemoveList = new List<int>();
                    var c1 = this._joistSpacingManipulators.Count;

                    //First detach event handlers and dispose each extra manipulator
                    for (var i = calcJoistManLines.Count; i < c1; i++)
                    {
                        var distMan = this._joistSpacingManipulators[i];
                        distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                        intRemoveList.Add(i);
                    }

                    //Remove from list extra manipulators
                    var exCount = 0;
                    foreach (var i in intRemoveList)
                    {
                        var ix = i + exCount;
                        this._joistSpacingManipulators.RemoveAt(ix);
                        exCount--;
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
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

            //Modify plugin spacing max value and call to update
            DmCommon.ModifyComponent(this.Component, "FirstJoistOffset", distMan.Segment.Length());
        }

        /// <summary>
        /// Event handler for joist spacing distance manipulator measure changed
        /// </summary>
        /// <param name="sender">DistanceManipulator</param>
        /// <param name="e">Event args</param>
        private void Joist_OnMeasureChangedCenterDel(object sender, EventArgs e)
        {
            if (!(sender is DistanceManipulator)) return;
            var distMan = (DistanceManipulator)sender;

            try
            {
                var spacingTypIntVal = 0;
                this.Component.GetAttribute("SpacingType", ref spacingTypIntVal);
                var spacingTyp = (MainViewModel.SpacingTypeEnum)spacingTypIntVal;
                switch (spacingTyp)
                {
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                        {
                            var index = this._joistSpacingManipulators.IndexOf(distMan);
                            var spacingList = new List<Distance>();
                            for (var i = 0; i < this._joistSpacingManipulators.Count; i++)
                            {
                                var dManExisting = this._joistSpacingManipulators[i];
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
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DetachHandlers();

            this._joistSpacingManipulators?.ForEach(handle => handle.Dispose());
            this._startSpacingManipulator?.Dispose();

            foreach (var manipulator in this.Manipulators)
            {
                manipulator.Dispose();
            }

            //Clear local caches
            this._joistSpacingManipulators?.Clear();
        }

        /// <summary>
        /// Attach event handlers to each manipulator in cache
        /// </summary>
        private void AttachHandlers()
        {
            //Attach events from start offset spacing
            if (this._startSpacingManipulator != null)
            {
                this._startSpacingManipulator.MeasureChanged += this.StartOffset_OnMeasureChangedDel;
            }

            //Attach events from joist spacing list
            if (this._joistSpacingManipulators == null) return;
            foreach (var distMan in this._joistSpacingManipulators)
            {
                distMan.MeasureChanged += this.Joist_OnMeasureChangedCenterDel;
            }
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            //Detach events from start offset spacing
            if (this._startSpacingManipulator != null)
            {
                this._startSpacingManipulator.MeasureChanged -= this.StartOffset_OnMeasureChangedDel;
            }

            //Detach events from joist spacing list
            if (this._joistSpacingManipulators == null) return;
            foreach (var distMan in this._joistSpacingManipulators)
            {
                distMan.MeasureChanged -= this.Joist_OnMeasureChangedCenterDel;
            }
        }



        /// <summary>
        /// Calculate line segment to represent start offset distance
        /// </summary>
        /// <param name="plugin">Model plugin instance</param>
        /// <returns>New LineSegment or null if failed</returns>
        private LineSegment GetStartOffsetManipulatorLineSegment(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Get data from plugin
                var componentInput = FeatureLogic.GetCurrentInput(plugin);
                var uiData = plugin.GetDataFromComponent();

                //Update internal logic to take into account latest data from plugin
                this._liftingLogic = new JoistAreaMainLogic();
                this._liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //get points for 1st distance segment
                var startPt = componentInput.Item2.GetMidPoint();
                var endPt = new Point(startPt);
                endPt.Translate(this._liftingLogic.GlobalCoordinateSystem.AxisY.GetNormal() * uiData.FirstJoistOffset);

                //If offset is zero - do not return usable line segment
                if (Tekla.Structures.Geometry3d.Distance.PointToPoint(endPt, startPt) <
                    GeometryConstants.DISTANCE_EPSILON) return null;

                //Return new line segment representing measure points
                return new LineSegment(startPt, endPt);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Calculate line segments to represent spacings of joists
        /// </summary>
        /// <param name="plugin">Model plugin instance</param>
        /// <returns>New list of line segments for spacings</returns>
        private List<LineSegment> GetJoistManipulatorsLineSegments(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Update internal logic to take into account latest data from plugin
                var uiData = plugin.GetDataFromComponent();
                var componentInput = FeatureLogic.GetCurrentInput(plugin);
                this._liftingLogic = new JoistAreaMainLogic();
                this._liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //Get joist spacings and distance values from base plugin logic
                return FeatureLogic.GetJoistSpanSegments(this._liftingLogic);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<LineSegment>();
            }
        }
    }
}