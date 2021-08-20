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
    using Tekla.Structures.Plugins.DirectManipulation.Services.Handles;
    using Distance = Tekla.Structures.Datatype.Distance;

    /// <summary>
    /// Main class to create joist spacing distance manipulators
    /// </summary>
    public sealed class JoistHandlesAddMc : ManipulationContext
    {
        private readonly IHandleManager handleManager;
        private List<SpacingZone> addHandleZones;
        private JoistAreaMainLogic liftingLogic;

        /// <summary>
        /// Main constructor to call methods to create distance manipulators
        /// </summary>
        /// <param name="component">Model plugin instance</param>
        /// <param name="feature">DM parent feature</param>
        public JoistHandlesAddMc(Component component, ManipulationFeature feature)
            : base(component, feature)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            try
            {
                //Cache local services
                this.handleManager = feature.HandleManager;
                if (this.handleManager == null) return;

                //Get data from plugin
                var componentInput = FeatureLogic.GetCurrentInput(this.Component);
                var uiData = this.Component.GetDataFromComponent();

                //Update internal logic to take into account latest data from plugin
                this.liftingLogic = new JoistAreaMainLogic();
                this.liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

                //If spacing type is center-center skip
                var spacingTyp = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;
                if (spacingTyp == MainViewModel.SpacingTypeEnum.albl_CenterToCenter)
                {
                    this.Dispose();
                    return;
                }

                //Create add handles with events
                this.addHandleZones = this.CreateManipulators(this.liftingLogic);

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

            //If spacing type is center-center skip
            var spacingTyp = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;
            if (spacingTyp == MainViewModel.SpacingTypeEnum.albl_CenterToCenter)
            {
                this.Dispose();
                return;
            }

            //Update internal logic to take into account latest data from plugin
            this.liftingLogic = new JoistAreaMainLogic();
            this.liftingLogic.ExternalInitialize(componentInput.Item1, componentInput.Item2, uiData);

            //Update existing manipulators
            this.UpdateManipulators(this.liftingLogic);

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
        /// Creates distance manipulators between joist beams and to 1st beam from GuideLine
        /// </summary>
        /// <param name="pluginLogic">Plugin logic external calculations</param>
        private List<SpacingZone> CreateManipulators(JoistAreaMainLogic pluginLogic)
        {
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            try
            {
                var result = new List<SpacingZone>();
                var joistManLines = FeatureLogic.GetJoistSpanSegments(pluginLogic);
                if (joistManLines == null) return new List<SpacingZone>();

                var spacingZones = this.GetSpacingZones(joistManLines);
                foreach (var sz in spacingZones)
                {
                    var handle = this.handleManager.CreatePointHandle(sz.OffsetPoint, HandleLocationType.PointExtension, HandleEffectType.Addition);
                    handle.DragStarted += this.OnPointHandleDragEnded;
                    sz.Handle = handle;
                    result.Add(sz);
                }
                return result;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<SpacingZone>();
            }
        }

        private List<SpacingZone> GetSpacingZones(List<LineSegment> joistManLines)
        {
            if (joistManLines == null) throw new ArgumentNullException(nameof(joistManLines));
            var result = new List<SpacingZone>();
            foreach (var jm in joistManLines)
            {
                result.Add(new SpacingZone(jm));
            }
            return result;
        }

        private void OnPointHandleDragEnded(object sender, DragEventArgs args)
        {
            if (sender == null || args == null || args.StartPosition == null) return;
            var ptHandle = sender as PointHandle;
            if (ptHandle == null) return;
            var startLocation = args.StartPosition;

            var newSpacings = new List<Distance>();
            foreach (var sz in this.addHandleZones)
            {
                if (Tekla.Structures.Geometry3d.Distance.PointToPoint(sz.OffsetPoint, startLocation) < GeometryConstants.DISTANCE_EPSILON)
                {
                    //New joist needed, Handle clicked: Split spacing into two
                    var seg1 = new LineSegment(sz.Segment.StartPoint, startLocation);
                    var seg2 = new LineSegment(startLocation, sz.Segment.EndPoint);
                    newSpacings.Add(new Distance(seg1.Length(), Distance.UnitType.Millimeter));
                    newSpacings.Add(new Distance(seg2.Length(), Distance.UnitType.Millimeter));
                }
                else
                {
                    //Handle not here: Keep spacing, add to new list
                    newSpacings.Add(new Distance(sz.Length, Distance.UnitType.Millimeter));
                }
            }

            //Modify plugin exact spacing values and call to update
            var updatedSpacings = new TxDistanceList(newSpacings);
            var spacingString = updatedSpacings.MetricStringList;

            DmCommon.ModifyComponent(this.Component, "CenterSpacingList", spacingString);
            this.UpdateContext();
        }

        /// <summary>
        /// Updates existing joist spacing distance manipulators
        /// </summary>
        /// <param name="pluginLogic">Plugin logic external data calculations</param>
        private void UpdateManipulators(JoistAreaMainLogic pluginLogic)
        {
            if (pluginLogic == null) throw new ArgumentNullException(nameof(pluginLogic));
            try
            {
                //Calculate segments that define distance lines, if null dispose existing
                var currentSegments = FeatureLogic.GetJoistSpanSegments(pluginLogic);

                //No spacings found, remove all existing distance manipulators
                if (currentSegments == null)
                {
                    foreach (var handle in this.addHandleZones)
                    {
                        handle.Handle.DragStarted -= this.OnPointHandleDragEnded;
                        handle.Handle.Dispose();
                    }
                    this.addHandleZones.Clear();
                    Debug.WriteLine("Info: All existing joist add point handles cleared, no new calculated spaces.");
                    return;
                }
                var currentSpacingZones = this.GetSpacingZones(currentSegments);

                //Update existing segments
                var toRemoveIndexList = new List<int>();
                for (var i = 0; i < this.addHandleZones.Count; i++)
                {
                    var pHandle = this.addHandleZones[i];
                    if (pHandle == null) continue;
                    if (i > currentSegments.Count - 1)
                    {
                        //No longer needed handle manipulators
                        pHandle.Handle.DragStarted -= this.OnPointHandleDragEnded;
                        pHandle.Handle.Dispose();
                        toRemoveIndexList.Add(i); //for removing outside enumeration
                        //Todo: how to remove from active feature context manipulators, deactivate?
                        Debug.WriteLine($"Existing: PointHandle removed from cache: {pHandle.Center} at index: {i}.");
                    }
                    else
                    {
                        //Still needed, update segment
                        var sz = currentSpacingZones[i];
                        pHandle.Handle.Point = sz.OffsetPoint;
                        Debug.WriteLine($"Existing: PointHandle updated: {sz.Center} at index: {i}.");
                    }
                }
                this.addHandleZones = this.addHandleZones.GetPrunedList(toRemoveIndexList);

                //Create DistanceManipulator objects for new segments
                var newSegments = Tools.TxLineSegment.GetFromListByIndex(currentSegments, this.addHandleZones.Count);
                if (newSegments != null)
                {
                    var zones = this.GetSpacingZones(newSegments);
                    foreach (var sz in zones)
                    {
                        var handle = this.handleManager.CreatePointHandle(sz.OffsetPoint, HandleLocationType.Other, HandleEffectType.Addition);
                        handle.DragStarted += this.OnPointHandleDragEnded;
                        sz.Handle = handle;
                        this.addHandleZones.Add(sz);
                        Debug.WriteLine($"New PointHandle added: {sz.Handle}.");
                    }
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

            this.addHandleZones?.ForEach(handle => handle.Handle?.Dispose());
            this.addHandleZones?.Clear();
        }

        /// <summary>
        /// Detach event handlers for each manipulator in cache
        /// </summary>
        private void DetachHandlers()
        {
            //Detach events from joist spacing list
            if (this.addHandleZones == null) return;
            foreach (var handle in this.addHandleZones)
            {
                if (handle.Handle == null) continue;
                handle.Handle.DragStarted -= this.OnPointHandleDragEnded;
            }
        }
    }
}