namespace JoistAreaFeatures.Manipulation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Datatype;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools;
    using Distance = Tekla.Structures.Datatype.Distance;

    public sealed class JoistSpacingMc : ManipulationContext
    {
        private IHandleManager _handleManager;
        private List<DistanceManipulator> _joistDistanceManipulators;
        private JoistAreaData _uiData;
        private readonly JoistAreaMainLogic _liftingLogic;

        public JoistSpacingMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            try
            {
                _handleManager = feature.HandleManager;

                //Get part and plugin information
                _uiData = component.GetDataFromComponent();
                //var uiDataStored = _featureBase.GetUserInterfaceData(component); //all blank
                var componentInput = FeatureLogic.GetCurrentInput(component);

                //Create new instance of logic service class
                _liftingLogic = new JoistAreaMainLogic();
                _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

                //Create point, distance, and line manipulators
                CreateJoistDistanceManipulators(component);

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

            //Update internal logic to take into account changes from plugin
            _uiData = Component.GetDataFromComponent();
            var componentInput = FeatureLogic.GetCurrentInput(Component);
            _liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, _uiData);

            //Re-create all Joist center to center manipulators
            ReCreateJoistDistanceManipulators(Component);

            //Update graphics based on plugin and manipulators
            ReCreateGraphics();
        }

        /// <summary>
        /// Any graphics as part of this context manipulator
        /// </summary>
        private void ReCreateGraphics()
        { }

        /// <summary>
        /// Disposes of all existing distance joist DistanceManipulators then calls method to CreateJoistDistanceManipulators
        /// </summary>
        /// <param name="component"></param>
        private void ReCreateJoistDistanceManipulators(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (_joistDistanceManipulators != null)
            {
                //todo: how to dispose of old manipulators, qty changed, need new?
                //todo: below code causes distance manipulators to not generate/show
                //_distanceManipulators.ForEach(f => f.Dispose());
                _joistDistanceManipulators.Clear();
            }
            CreateJoistDistanceManipulators(component);
        }

        /// <summary>
        /// Creates distance manipulators between joist beams and to 1st beam from GuideLine
        /// </summary>
        /// <param name="plugin">Joist Area model instance</param>
        private void CreateJoistDistanceManipulators(Component plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (_liftingLogic == null) throw new ArgumentNullException(nameof(_liftingLogic));

            if (_joistDistanceManipulators == null) _joistDistanceManipulators = new List<DistanceManipulator>();
            else _joistDistanceManipulators.Clear();

            try
            {
                //Get joist spacings and distance values from base plugin logic
                var componentInput = FeatureLogic.GetCurrentInput(plugin);
                var joistSpacingsGlobal = FeatureLogic.GetJoistSpanSegments(_liftingLogic);
                if (joistSpacingsGlobal == null || joistSpacingsGlobal.Count < 1) return;
                var distValues = new ArrayList(_liftingLogic.DistanceList.DistanceValues);

                //Create first joist to guideline offset
                var j1 = joistSpacingsGlobal[0];
                var px = Projection.PointToLine(j1.Point1, new Line(componentInput.Item2));
                if (px != null)
                {
                    var spacingSeg1 = new LineSegment(j1.Point1, px);
                    var distStartMan = new DistanceManipulator(plugin, this, spacingSeg1);
                    distStartMan.MeasureChanged += delegate
                    {
                        DmCommon.ModifyComponent(plugin, "FirstJoistOffset", distStartMan.Segment.Length());
                    };
                    AddManipulator(distStartMan);
                    _joistDistanceManipulators.Add(distStartMan);
                }

                //Depending on spacing type add distance manipulator between each joist
                switch (_liftingLogic.SpacingTyp)
                {
                    //Distance type spacing, handle segment as spacing value in larger string list
                    case MainViewModel.SpacingTypeEnum.albl_ExactList:
                        for (var i = 0; i < joistSpacingsGlobal.Count; i++)
                        {
                            var ls = joistSpacingsGlobal[i];
                            var distMan = new DistanceManipulator(plugin, this, ls);
                            distMan.MeasureChanged += DistManExact_OnMeasureChanged;
                            AddManipulator(distMan);
                            _joistDistanceManipulators.Add(distMan);
                        }
                        break;

                    //Single max spacing for all joist, handle segment as same spacing max value
                    case MainViewModel.SpacingTypeEnum.albl_CenterToCenter:
                        foreach (var ls in joistSpacingsGlobal)
                        {
                            var distMan = new DistanceManipulator(plugin, this, ls);
                            distMan.MeasureChanged += delegate
                            {
                                //Modify plugin spacing max value and call to update
                                DmCommon.ModifyComponent(plugin, "CenterSpacingMax", distMan.Segment.Length());
                            };
                            AddManipulator(distMan);
                            _joistDistanceManipulators.Add(distMan);
                        }
                        break;
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
        /// Event handler for DistanceManipulator is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DistManExact_OnMeasureChanged(object sender, EventArgs e)
        {
            var allSpacings = new List<Distance>();
            for (var i = 1; i < _joistDistanceManipulators.Count; i++)
            {
                var distMan = _joistDistanceManipulators[i];
                allSpacings.Add(new Distance(distMan.Segment.Length()));
            }

            var modDistList = new DistanceList(allSpacings);
            var modDistList2 = new TxDistanceList(allSpacings);
            DmCommon.ModifyComponent(Component, "CenterSpacingList", modDistList.ToString());
        }

        /// <summary>
        /// Detach and then dispose of all manipulator objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _joistDistanceManipulators.ForEach(handle => handle.Dispose());
            _joistDistanceManipulators.Clear();
        }
    }
}