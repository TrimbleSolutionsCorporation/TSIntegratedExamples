namespace SpreadsheetReinforcement.PartProxy
{
    using System;
    using System.Collections.ObjectModel;
    using Data;
    using ModelLogic;
    using Tekla.Structures.Model;

    /// <summary>
    /// Wrapper for pad footing that sets specific attributes for reinforcement
    /// Creates or  modifies existing found from model
    /// </summary>
    public class DesignPadFooting : DesignPartBase
    {
        readonly Part _padFooting;

        /// <summary>
        /// New instance of design pad footing service
        /// </summary>
        /// <param name="footing">Pad footing</param>
        /// <param name="setting">Program settings</param>
        /// <param name="designs">Spreadsheet design cache</param>
        public DesignPadFooting(Part footing, SavedSetting setting, ObservableCollection<SpreadsheetResultData> designs)
            : base(setting, designs)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));
            if (designs == null) throw new ArgumentNullException(nameof(designs));
            _padFooting = footing ?? throw new ArgumentNullException(nameof(footing));
        }

        /// <summary>
        /// Creates new reinforcement component for pad footing
        /// </summary>
        /// <returns>True if successful</returns>
        public override bool InsertModify()
        {
            var componentInput = new ComponentInput();
            componentInput.AddInputObject(_padFooting);

            //Get existing component to modify
            var useExisting = false;
            var foundComponent = GetExistingComponent();
            if (foundComponent != null) useExisting = true;
            else //Create new component with input
            {
                foundComponent = new Component(componentInput)
                {
                    Name = _savedSetting.FootingComponentName, Number = _savedSetting.FootingComponentNumber
                };
            }

            foundComponent.LoadAttributesFromFile(_savedSetting.FootingComponentSettings);

            //Get calculated design values
            var design = GetDesign(_padFooting);
            if (design == null) return false;
            var calcRes = new PadFootingMinReinfCalculator(_padFooting, design, _savedSetting);

            //Set attributes from design results
            SetComponentAttributes(foundComponent, calcRes, design);

            //Insert new or modify existing component to model
            return useExisting ? foundComponent.Modify() : foundComponent.Insert();
        }

        /// <summary>
        /// Gets existing reinforcement component from pad footing in model
        /// </summary>
        /// <returns>Found component or null if failed</returns>
        private Component GetExistingComponent()
        {
            var padChildren = _padFooting.GetComponents();
            if (padChildren == null) return null;
            while (padChildren.MoveNext())
            {
                var cp = padChildren.Current as Component;
                if (cp == null) continue;
                if (cp.Number != _savedSetting.FootingComponentNumber) continue;
                return cp;
            }

            return null;
        }

        /// <summary>
        /// Sets component attributes per design calculation needs
        /// </summary>
        /// <param name="foundComponent">Component to apply attributes to</param>
        /// <param name="calcRes">Pad footing calculator</param>
        /// <param name="design">Design Calculations to apply</param>
        private void SetComponentAttributes(Component foundComponent, PadFootingMinReinfCalculator calcRes,
            SpreadsheetResultData design)
        {
            if (foundComponent == null) throw new ArgumentNullException(nameof(foundComponent));
            if (calcRes == null) throw new ArgumentNullException(nameof(calcRes));
            if (design == null) throw new ArgumentNullException(nameof(design));

            //Set covers
            foundComponent.SetAttribute("cover_t_bottom", _savedSetting.MinClearCoverBottom);
            foundComponent.SetAttribute("cover_side", _savedSetting.MinClearCoverSides);

            //Apply design values for x direction
            foundComponent.SetAttribute("s_zone1size", calcRes.RebarSizeX);
            foundComponent.SetAttribute("s_zone1grade", design.RebarGrade);
            foundComponent.SetAttribute("s_spacing_zone1", calcRes.RebarSpacingX);

            //Apply design values for y direction
            foundComponent.SetAttribute("zone1size", calcRes.RebarSizeZ);
            foundComponent.SetAttribute("zone1grade", design.RebarGrade);
            foundComponent.SetAttribute("spacing_zone1", calcRes.RebarSpacingZ);
        }
    }
}