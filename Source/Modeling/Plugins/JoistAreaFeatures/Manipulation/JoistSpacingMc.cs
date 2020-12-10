namespace JoistAreaFeatures.Manipulation
{
    using System;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;
    using Distance = Tekla.Structures.Datatype.Distance;

    public sealed class JoistSpacingMc : ManipulationContext
    {
        private IHandleManager _handleManager;
        private List<DistanceManipulator> _joistSpacingManipulators;
        private DistanceManipulator _startSpacingManipulator;
        private JoistAreaMainLogic _liftingLogic;

        public JoistSpacingMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            try
            {
                _handleManager = feature.HandleManager;

                //Create start and joist manipulators
                _startSpacingManipulator = CreateStartOffsetManipulator(component);
                _joistSpacingManipulators = CreateJoistDistanceManipulators(component);

                //Add manipulators to the system
                AttachHandlers();
                if (_startSpacingManipulator != null) AddManipulator(_startSpacingManipulator);
                _joistSpacingManipulators.ForEach(AddManipulator);

                //Draw custom graphics
                ReCreateGraphics();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        public override void UpdateContext()
        {
            base.UpdateContext();
            Component.Select();

            //Update existing manipulators
            UpdateStartOffsetManipulator(Component);
            UpdateJoistDistanceManipulators(Component);

            //Re-create all distance manipulators (qty changes)
            //ReCreateDistanceManipulators();

            //Update graphics based on plugin and manipulators
            ReCreateGraphics();
        }

        /// <summary>
        /// Any graphics as part of this context manipulator
        /// </summary>
        private void ReCreateGraphics()
        {
        }

        /// <summary>
        /// Disposes of all existing distance joist DistanceManipulators then calls method to CreateJoistDistanceManipulators
        /// </summary>
        /// <param name="component"></param>
        private void ReCreateDistanceManipulators(Component component)
        {
            //Call 1st to detach events and dispose of all existing handlers
            Dispose();

            //Create start and joist manipulators
            _startSpacingManipulator = CreateStartOffsetManipulator(component);
            _joistSpacingManipulators = CreateJoistDistanceManipulators(component);

            //Add manipulators to the system
            if (_startSpacingManipulator != null) AddManipulator(_startSpacingManipulator);
            _joistSpacingManipulators.ForEach(AddManipulator);
        }

        private DistanceManipulator CreateStartOffsetManipulator(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            var spacingSeg1 = GetStartOffsetManipulatorLineSegment(plugin);
            return new DistanceManipulator(plugin, this, spacingSeg1);
        }

        private LineSegment GetStartOffsetManipulatorLineSegment(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Get data from plugin
                var componentInput = FeatureLogic.GetCurrentInput(Component);
                var uiData = Component.GetDataFromComponent();

                //Update internal logic to take into account latest data from plugin
                _liftingLogic = new JoistAreaMainLogic();
                _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //get points for 1st distance segment
                var startPt = componentInput.Item2.GetMidPoint();
                var endPt = new Point(startPt);
                endPt.Translate(_liftingLogic.GlobalCoordinateSystem.AxisY.GetNormal() * uiData.FirstJoistOffset);

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

        private void UpdateStartOffsetManipulator(Component plugin)
        {
            //Use existing method to calculate where manipulator should be
            var spacingSeg1 = GetStartOffsetManipulatorLineSegment(plugin);
            if (spacingSeg1 == null) //todo: risky?
            {
                _startSpacingManipulator.MeasureChanged -= StartOffset_OnMeasureChangedDel;
                _startSpacingManipulator.Dispose();
            }

            //If no distance manipulator exists, create new one
            if (_startSpacingManipulator == null)
            {
                var distMan = new DistanceManipulator(plugin, this, spacingSeg1);
                distMan.MeasureChanged += StartOffset_OnMeasureChangedDel;
                AddManipulator(distMan);
            }
            else
            {
                //Update existing distance manipulator
                _startSpacingManipulator.Segment = spacingSeg1;
            }
        }

        private void StartOffset_OnMeasureChangedDel(object sender, EventArgs e)
        {
            if (!(sender is DistanceManipulator)) return;
            var distMan = sender as DistanceManipulator;

            //Modify plugin spacing max value and call to update
            DmCommon.ModifyComponent(Component, "FirstJoistOffset", distMan.Segment.Length());
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
                var joistManLines = GetJoistManipulatorsLineSegments(plugin);
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

        private void UpdateJoistDistanceManipulators(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Calculate segments that define distance lines, if null dispose existing
                var joistManLines = GetJoistManipulatorsLineSegments(plugin);
                if (joistManLines == null) //todo: risky?
                {
                    foreach (var distMan in _joistSpacingManipulators)
                    {
                        distMan.MeasureChanged -= Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                    }

                    return;
                }

                //For each calculated new distance line, update existing
                if (_joistSpacingManipulators == null) return;
                for (var i = 0; i < joistManLines.Count; i++)
                {
                    var calcLine = joistManLines[i];

                    //Check if beyond original list qty
                    if (_joistSpacingManipulators.Count < i + 1)
                    {
                        var distMan = new DistanceManipulator(plugin, this, calcLine);
                        distMan.MeasureChanged += Joist_OnMeasureChangedCenterDel;
                        AddManipulator(distMan);
                    }
                    else
                    {
                        var existingManipulator = _joistSpacingManipulators[i];
                        existingManipulator.Segment = calcLine;
                    }
                }

                //Remove no-longer used distance manipulators   //todo: risky?
                if (_joistSpacingManipulators.Count > joistManLines.Count)
                {
                    var counter = _joistSpacingManipulators.Count;
                    for (var i = joistManLines.Count - 1; i < counter; i++)
                    {
                        var distMan = _joistSpacingManipulators[i];
                        distMan.MeasureChanged -= Joist_OnMeasureChangedCenterDel;
                        distMan.Dispose();
                        _joistSpacingManipulators.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        private List<LineSegment> GetJoistManipulatorsLineSegments(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            try
            {
                //Update internal logic to take into account latest data from plugin
                var uiData = Component.GetDataFromComponent();
                var componentInput = FeatureLogic.GetCurrentInput(Component);
                _liftingLogic = new JoistAreaMainLogic();
                _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //Get joist spacings and distance values from base plugin logic
                return FeatureLogic.GetJoistSpanSegments(_liftingLogic);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<LineSegment>();
            }
        }

        private void Joist_OnMeasureChangedCenterDel(object sender, EventArgs e)
        {
            if (!(sender is DistanceManipulator)) return;
            var distMan = sender as DistanceManipulator;

            var spacingTypIntVal = 0;
            Component.GetAttribute("SpacingTyp", ref spacingTypIntVal);
            var spacingTyp = (MainViewModel.SpacingTypeEnum) spacingTypIntVal;
            switch (spacingTyp)
            {
                case MainViewModel.SpacingTypeEnum.albl_ExactList:
                {
                    var index = _joistSpacingManipulators.IndexOf(distMan);
                    var spacingList = new List<Distance>();
                    for (var i = 0; i < _joistSpacingManipulators.Count; i++)
                    {
                        var dManExisting = _joistSpacingManipulators[i];
                        var length = i == index ? distMan.Segment.Length() : dManExisting.Segment.Length();
                        spacingList.Add(new Distance(length));
                    }

                    //Modify plugin exact spacing values and call to update
                    var updatedSpacings = new TxDistanceList(spacingList);
                    var spacingString = updatedSpacings.MetricStringList;
                    DmCommon.ModifyComponent(Component, "CenterSpacingList", spacingString);


                    ////Get data from plugin
                    //var componentInput = FeatureLogic.GetCurrentInput(Component);
                    //var uiData = Component.GetDataFromComponent();

                    ////Update internal logic to take into account latest data from plugin
                    //_liftingLogic = new JoistAreaMainLogic();
                    //_liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);
                    //var localYdir = _liftingLogic.GlobalCoordinateSystem.AxisY.GetNormal();

                    //Point p0;
                    //if (index == 0) //First joist spacing
                    //{
                    //    p0 = _startSpacingManipulator == null
                    //        ? componentInput.Item2.GetMidPoint()
                    //        : new Point(_startSpacingManipulator.Segment.Point2);
                    //}
                    //else //Intermediate joist spacing
                    //{
                    //    var prevDistMan = _joistSpacingManipulators[index - 1];
                    //    p0 = new Point(prevDistMan.Segment.Point2);
                    //}

                    //var currDistMan = _joistSpacingManipulators[index];
                    //var p1 = new Point(p0);
                    //p1.Translate(localYdir * distMan.Segment.Length());
                    //currDistMan.Segment = new LineSegment(new Point(p0), new Point(p1));

                    ////Update all other distance manipulator points from new end
                    //var pLast = new Point(p1);
                    //for (var j = index + 1; j < _joistSpacingManipulators.Count; j++)
                    //{
                    //    //Get next distance manipulator from list
                    //    var distMan2 = _joistSpacingManipulators[j];

                    //    //Get existing length and create new calc point
                    //    var currentLength = distMan2.Segment.Length();
                    //    var p2 = new Point(pLast);

                    //    //Move calc point past segment length along axis to find new end
                    //    p2.Translate(localYdir * currentLength);
                    //    distMan2.Segment = new LineSegment(pLast, p2);

                    //    //Set start point for next manipulator in list
                    //    pLast = new Point(p2);
                    //}

                    break;
                }
                case MainViewModel.SpacingTypeEnum.albl_CenterToCenter:
                {
                    //Modify plugin spacing max value and call to update
                    DmCommon.ModifyComponent(Component, "CenterSpacingMax", distMan.Segment.Length());
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Calculates spacings from existing lifters
        /// </summary>
        /// <returns></returns>
        private TxDistanceList GetSpacingListFromDistManipulators()
        {
            var allSpacings = new List<Distance>();
            for (var i = 1; i < _joistSpacingManipulators.Count; i++)
            {
                var distMan = _joistSpacingManipulators[i];
                allSpacings.Add(new Distance(distMan.Segment.Length()));
            }

            //var modDistList = new DistanceList(allSpacings);
            var modDistList2 = new TxDistanceList(allSpacings);
            return modDistList2;
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DetachHandlers();

            foreach (var manipulator in Manipulators)
            {
                manipulator.Dispose();
            }
        }

        private void AttachHandlers()
        {
            //Attach events from start offset spacing
            if (_startSpacingManipulator != null)
            {
                _startSpacingManipulator.MeasureChanged += StartOffset_OnMeasureChangedDel;
            }

            //Attach events from joist spacing list
            if (_joistSpacingManipulators == null) return;
            foreach (var distMan in _joistSpacingManipulators)
            {
                distMan.MeasureChanged += Joist_OnMeasureChangedCenterDel;
            }
        }

        private void DetachHandlers()
        {
            //Detach events from start offset spacing
            if (_startSpacingManipulator != null)
            {
                _startSpacingManipulator.MeasureChanged -= StartOffset_OnMeasureChangedDel;
            }

            //Detach events from joist spacing list
            if (_joistSpacingManipulators == null) return;
            foreach (var distMan in _joistSpacingManipulators)
            {
                distMan.MeasureChanged -= Joist_OnMeasureChangedCenterDel;
            }
        }
    }
}