namespace JoistAreaFeatures
{
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.View;
    using JoistArea.ViewModel;
    using Manipulation;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Controls;

    /// <seealso cref="PluginManipulationFeatureBase" />
    public class ManipulationFeature : PluginManipulationFeatureBase
    {
        private ValueBoxControl firstSpacing;
        private ValueBoxControl depthOffset;
        private DropDownListControl spacingType;

        public IEnumerable<string> AllSpacingTypes => EnumTools.EnumToTranslatedStrings<MainViewModel.SpacingTypeEnum>();

        public ManipulationFeature() : base(Constants.PluginName, true)
        { }

        protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        {
            //Only create toolbar if some components are selected
            if (this.Components == null || this.Components.Count < 1) return;

            //Use first component from selected to get UI default data from
            var target = this.Components.First();
            try
            {
                //Get data from plugin to show default in controls
                var uiData = PluginDataFetcher.GetDataFromComponent(target);
                var spacingTypeUsed = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;

                //Create control for spacing type
                this.spacingType = toolbar.CreateDropDown(this.AllSpacingTypes, spacingTypeUsed.ToString());
                this.spacingType.Tooltip = "Spacing Type";
                this.spacingType.StateChanged += delegate
                {
                    foreach (var component in this.Components)
                    {
                        var strVal = this.spacingType.SelectedItem.ToString();
                        var value = (MainViewModel.SpacingTypeEnum)Enum.Parse(typeof(MainViewModel.SpacingTypeEnum), strVal);
                        DmCommon.ModifyComponent(component, "SpacingType", (int)value);
                    }
                };

                //First spacing offset control
                this.firstSpacing = toolbar.CreateValueTextBox(uiData.FirstJoistOffset);
                this.firstSpacing.Tooltip = "First Joist Offset Distance";
                this.firstSpacing.Title = "First";
                this.firstSpacing.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in this.Components)
                    {
                        DmCommon.ModifyComponent(component, "FirstJoistOffset", this.firstSpacing.Value);
                    }
                };

                //Depth offset control
                var pluginValue = TxModel.NullDoubleValue;
                target.GetAttribute("DepthOffset", ref pluginValue);
                if (PluginDataHelper.IsBlankValue(pluginValue))
                {
                    this.depthOffset = toolbar.CreateValueTextBox();
                    this.depthOffset.Title = "Depth (Calculated)";
                }
                else
                {
                    this.depthOffset = toolbar.CreateValueTextBox(uiData.DepthOffset);
                    this.depthOffset.Title = "Depth";
                }
                this.depthOffset.Tooltip = "Depth Below Joist Offset";
                this.depthOffset.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in this.Components)
                    {
                        var depthOffsetValue = this.depthOffset.Value;
                        if (depthOffsetValue != null && PluginDataHelper.IsBlankValue((double)depthOffsetValue))
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", TxModel.NullDoubleValue);
                            this.depthOffset.Title = "Depth (Calculated)";
                        }
                        else if (depthOffsetValue != null)
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", (double)depthOffsetValue);
                            this.depthOffset.Title = "Depth";
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        protected override void Refresh()
        {
            this.UpdateToolbar();
        }

        /// <summary>
        /// Update toolbar data used for controls for selected objects on Refresh()
        /// </summary>
        private void UpdateToolbar()
        {
            //Only create toolbar if some components are selected
            if (this.Components == null || this.Components.Count < 1) return;
            var target = this.Components.First();

            //Get data from plugin
            var uiData = PluginDataFetcher.GetDataFromComponent(target);
            if (uiData == null) return;
            var spacingTypeUsed = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;

            //Update control values from PluginData
            if (this.spacingType != null) this.spacingType.SelectedItem = spacingTypeUsed.ToString();
            if (this.firstSpacing != null) this.firstSpacing.Value = uiData.FirstJoistOffset;
            if (this.depthOffset != null) this.depthOffset.Value = uiData.DepthOffset;
        }

        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(Component component)
        {
            yield return new PolygonShapeMc(component, this);
            yield return new JoistSpacingMc(component, this);
            yield return new GuideLineMc(component, this);
            yield return new JoistHandlesAddMc(component, this);
        }
    }
}