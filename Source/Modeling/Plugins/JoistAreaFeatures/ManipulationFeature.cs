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
        private ValueBoxControl _centerMaxSpacing;
        private TextBoxControl _centerSpacingList;
        private ValueBoxControl _firstSpacing;
        private ValueBoxControl _depthOffset;
        private DropDownListControl _spacingType;

        public IEnumerable<string> AllSpacingTypes => EnumTools.EnumToTranslatedStrings<MainViewModel.SpacingTypeEnum>();

        public ManipulationFeature()
            : base(Constants.PluginName, true)
        { }

        protected override void DefineFeatureContextualToolbar(IToolbar toolbar)
        {
            base.DefineFeatureContextualToolbar(toolbar);
            if (this.Components == null || this.Components.Count < 1) return;
            try
            {

                //Get data from user interface
                var uiData = PluginDataFetcher.GetDataFromComponent(this.Components.First());
                var spacingTypeUsed = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;

                //Create control for spacing type
                this._spacingType = toolbar.CreateDropDown(this.AllSpacingTypes, spacingTypeUsed.ToString());
                this._spacingType.Tooltip = "Spacing Type";
                this._spacingType.StateChanged += delegate
                {
                    foreach (var component in this.Components)
                    {
                        var strVal = this._spacingType.SelectedItem.ToString();
                        var value = (MainViewModel.SpacingTypeEnum)Enum.Parse(typeof(MainViewModel.SpacingTypeEnum), strVal);
                        DmCommon.ModifyComponent(component, "SpacingType", (int)value);
                    }
                };

                //First spacing offset control
                this._firstSpacing = toolbar.CreateValueTextBox(uiData.FirstJoistOffset);
                this._firstSpacing.Tooltip = "First Joist Offset Distance";
                this._firstSpacing.Title = "First";
                this._firstSpacing.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in this.Components)
                    {
                        DmCommon.ModifyComponent(component, "FirstJoistOffset", this._firstSpacing.Value);
                    }
                };

                //Depth offset control
                var pluginValue = TxModel.NullDoubleValue;
                this.Components.First().GetAttribute("DepthOffset", ref pluginValue);
                if (PluginDataHelper.IsBlankValue(pluginValue))
                {
                    this._depthOffset = toolbar.CreateValueTextBox();
                    this._depthOffset.Title = "Depth (Calculated)";
                }
                else
                {
                    this._depthOffset = toolbar.CreateValueTextBox(uiData.DepthOffset);
                    this._depthOffset.Title = "Depth";
                }

                this._depthOffset.Tooltip = "Depth Below Joist Offset";
                this._depthOffset.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in this.Components)
                    {
                        if (PluginDataHelper.IsBlankValue((double)this._depthOffset.Value))
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", TxModel.NullDoubleValue);
                            this._depthOffset.Title = "Depth (Calculated)";
                        }
                        else
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", this._depthOffset.Value);
                            this._depthOffset.Title = "Depth";
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
            base.Refresh();
            if (this.Components == null || this.Components.Count < 1 || this._centerMaxSpacing == null) return;
            try
            {
                //Get data from plugin
                var uiData = PluginDataFetcher.GetDataFromComponent(this.Components.First());
                if (uiData == null) return;
                var spacingTypeUsed = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;

                //Update control values from PluginData
                this._spacingType.SelectedItem = spacingTypeUsed.ToString();
                if (this._centerMaxSpacing != null) this._centerMaxSpacing.Value = uiData.CenterSpacingMax;
                if (this._centerSpacingList != null) this._centerSpacingList.Text = uiData.CenterSpacingList;
                this._firstSpacing.Value = uiData.FirstJoistOffset;
                this._depthOffset.Value = uiData.DepthOffset;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(Component component)
        {
            yield return new PolygonShapeMc(component, this);
            yield return new JoistSpacingMc(component, this);
            yield return new GuideLineMc(component, this);
        }
    }
}