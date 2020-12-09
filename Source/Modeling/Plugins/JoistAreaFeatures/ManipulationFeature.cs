namespace JoistAreaFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.View;
    using JoistArea.ViewModel;
    using Manipulation;
    using Services;
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
            if (Components == null || Components.Count < 1) return;
            try
            {

                //Get data from user interface
                var uiData = PluginDataFetcher.GetDataFromComponent(Components.First());
                var spacingTypeUsed = (MainViewModel.SpacingTypeEnum) uiData.SpacingType;

                //Create control for spacing type
                _spacingType = toolbar.CreateDropDown(AllSpacingTypes, spacingTypeUsed.ToString());
                _spacingType.Tooltip = "Spacing Type";
                _spacingType.StateChanged += delegate
                {
                    foreach (var component in Components)
                    {
                        var strVal = _spacingType.SelectedItem.ToString();
                        var value = (MainViewModel.SpacingTypeEnum)Enum.Parse(typeof(MainViewModel.SpacingTypeEnum), strVal);
                        DmCommon.ModifyComponent(component, "SpacingType", (int)value);
                    }
                };

                //First spacing offset control
                _firstSpacing = toolbar.CreateValueTextBox(uiData.FirstJoistOffset);
                _firstSpacing.Tooltip = "First Joist Offset Distance";
                _firstSpacing.Title = "First";
                _firstSpacing.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in Components)
                    {
                        DmCommon.ModifyComponent(component, "FirstJoistOffset", _firstSpacing.Value);
                    }
                };

                //Depth offset control
                var pluginValue = TxModel.NullDoubleValue;
                Components.First().GetAttribute("DepthOffset", ref pluginValue);
                if (PluginDataHelper.IsBlankValue(pluginValue))
                {
                    _depthOffset = toolbar.CreateValueTextBox();
                    _depthOffset.Title = "Depth (Calculated)";
                }
                else
                {
                    _depthOffset = toolbar.CreateValueTextBox(uiData.DepthOffset);
                    _depthOffset.Title = "Depth";
                }
                _depthOffset.Tooltip = "Depth Below Joist Offset";
                _depthOffset.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in Components)
                    {
                        if (PluginDataHelper.IsBlankValue(_depthOffset.Value))
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", TxModel.NullDoubleValue);
                            _depthOffset.Title = "Depth (Calculated)";
                        }
                        else
                        {
                            DmCommon.ModifyComponent(component, "DepthOffset", _depthOffset.Value);
                            _depthOffset.Title = "Depth";
                        }
                    }
                    Refresh();
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
            if (Components == null || Components.Count < 1 || _centerMaxSpacing == null) return;
            try
            {
                //Get data from plugin
                var uiData = PluginDataFetcher.GetDataFromComponent(Components.First());
                if (uiData == null) return;
                var spacingTypeUsed = (MainViewModel.SpacingTypeEnum)uiData.SpacingType;

                //Update control values from PluginData
                _spacingType.SelectedItem = spacingTypeUsed.ToString();
                if(_centerMaxSpacing!=null) _centerMaxSpacing.Value = uiData.CenterSpacingMax;
                if(_centerSpacingList!=null) _centerSpacingList.Text = uiData.CenterSpacingList;
                _firstSpacing.Value = uiData.FirstJoistOffset;
                _depthOffset.Value = uiData.DepthOffset;
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