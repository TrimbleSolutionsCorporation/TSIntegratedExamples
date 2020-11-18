namespace JoistAreaFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JoistArea.Logic;
    using JoistArea.Tools;
    using JoistArea.View;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Controls;

    /// <seealso cref="PluginManipulationFeatureBase" />
    public class JoistAreaManipulationFeature : PluginManipulationFeatureBase
    {
        private ValueBoxControl _centerMaxSpacing;
        private TextBoxControl _centerSpacingList;
        private ValueBoxControl _firstSpacing;
        private ValueBoxControl _depthOffset;
        private DropDownListControl _spacingType;

        public IEnumerable<string> AllSpacingTypes => EnumTools.EnumToTranslatedStrings<MainViewModel.SpacingTypeEnum>();

        public JoistAreaManipulationFeature()
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

                //Create control for center to center max distance spacing
                if (spacingTypeUsed == MainViewModel.SpacingTypeEnum.albl_CenterToCenter)
                {
                    _centerMaxSpacing = toolbar.CreateValueTextBox(uiData.CenterSpacingMax);
                    _centerMaxSpacing.Tooltip = "Center Max Spacing";
                    _centerMaxSpacing.Title = "Max";
                    _centerMaxSpacing.StateChanged += delegate
                    {
                        foreach (var component in Components)
                        {
                            DmCommon.ModifyComponent(component, "CenterSpacingMax", _centerMaxSpacing.Value);
                        }
                    };
                }

                //Create control for center to center max distance spacing
                if (spacingTypeUsed == MainViewModel.SpacingTypeEnum.albl_ExactList)
                {
                    _centerSpacingList = toolbar.CreateTextBox(uiData.CenterSpacingList);
                    _centerSpacingList.Tooltip = "Center Exact Spacing List";
                    _centerSpacingList.Title = "List";
                    _centerSpacingList.StateChanged += delegate
                    {
                        foreach (var component in Components)
                        {
                            DmCommon.ModifyComponent(component, "CenterSpacingList", _centerSpacingList.Text);
                        }
                    };
                }

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
                _depthOffset = toolbar.CreateValueTextBox(uiData.DepthOffset);
                _depthOffset.Tooltip = "Depth Below Joist Offset";
                _depthOffset.Title = "Depth";
                _depthOffset.StateChanged += (control, eventArgs) =>
                {
                    foreach (var component in Components)
                    {
                        DmCommon.ModifyComponent(component, "DepthOffset", _depthOffset.Value);
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
            yield return new JoistAreaManipulationContext(component, this);
        }
    }
}