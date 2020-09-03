namespace SpreadsheetReinforcement.PartProxy
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using Data;
    using Tekla.Structures.Model;
    using Tools;

    /// <summary>
    /// Wrapper for pillar that sets specific attributes for reinforcement
    /// Creates or  modifies existing found from model
    /// </summary>
    public class DesignPillar : DesignPartBase
    {
        private readonly FoundationPair _foundPair;

        /// <summary>
        /// New instance of design pillar service
        /// </summary>
        /// <param name="foundPair">Pad footing and pillar pair</param>
        /// <param name="setting">Program settings</param>
        /// <param name="designs">Spreadsheet design cache</param>
        public DesignPillar(FoundationPair foundPair, SavedSetting setting,
            ObservableCollection<SpreadsheetResultData> designs)
            : base(setting, designs)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));
            if (designs == null) throw new ArgumentNullException(nameof(designs));
            _foundPair = foundPair ?? throw new ArgumentNullException(nameof(foundPair));
        }

        /// <summary>
        /// Creates new reinforcement component between pillar and pad footing
        /// </summary>
        /// <returns>True if successful</returns>
        public override bool InsertModify()
        {
            //Setup component input in case new
            var componentInput = new ComponentInput();
            componentInput.AddInputObject(_foundPair.Footing);
            componentInput.AddInputObject(_foundPair.Pillar);

            //Get existing component to modify
            var useExisting = false;
            var foundComponent = GetExistingComponent();
            if (foundComponent != null) useExisting = true;
            else //Create new component with input
            {
                foundComponent = new Component(componentInput)
                {
                    Name = _savedSetting.PillarComponentName, Number = _savedSetting.PillarComponentNumber,
                };
            }

            foundComponent.LoadAttributesFromFile(_savedSetting.FootingComponentSettings);

            var design = GetDesign(_foundPair.Footing);
            if (design == null) return false;

            //Set attributes from design results
            SetComponentAttributes(foundComponent, design);

            //Insert new or modify existing component to model
            return useExisting ? foundComponent.Modify() : foundComponent.Insert();
        }

        /// <summary>
        /// Sets component attributes per design calculation needs
        /// </summary>
        /// <param name="foundComponent">Component to apply attributes to</param>
        /// <param name="design">Design Calculations to apply</param>
        private void SetComponentAttributes(Component foundComponent, SpreadsheetResultData design)
        {
            //Set covers
            foundComponent.SetAttribute("L1", -_savedSetting.MinClearCoverTop);
            foundComponent.SetAttribute("TopCover", _savedSetting.MinClearCoverTop);
            foundComponent.SetAttribute("BottomCover",
                _savedSetting.MinClearCoverBottom); //todo: add flex steel diameters

            //Put in vertical corner bars
            foundComponent.SetAttribute("CStarterGrade", design.RebarGrade);
            foundComponent.SetAttribute("CStarterSize", design.LongBarSize);
            //todo: set radius

            //Put in vertical face bars
            var sideQty = Convert.ToInt32(Math.Floor((design.LongBarQty - 4.0) / 4.0));
            if (design.LongBarQty > 4)
            {
                foundComponent.SetAttribute("txtNosLong", sideQty);
                foundComponent.SetAttribute("txtPos", sideQty);
            }
            else
            {
                foundComponent.SetAttribute("txtNosLong", 0);
                foundComponent.SetAttribute("txtPos", 0);
            }

            foundComponent.SetAttribute("StarterGrade", design.RebarGrade);
            foundComponent.SetAttribute("StarterSize", design.LongBarSize);
            foundComponent.SetAttribute("StirrupGrade", design.LongBarQty);
            //todo: set radius

            //Put in stirrups
            foundComponent.SetAttribute("StirrupGrade", design.RebarGrade);
            foundComponent.SetAttribute("StirrupSize", design.TransBarSize);
            //todo: set radius
            foundComponent.SetAttribute("KickerSideCover", _savedSetting.MinClearCoverSides);
            foundComponent.SetAttribute("StirrupSpacingList",
                design.TransBarSpacing.ToString(CultureInfo.InvariantCulture));
            var covers = _savedSetting.MinClearCoverTop;
            var qty = Convert.ToInt32(Math.Floor((_foundPair.Pillar.GetLength() - covers) / design.TransBarSpacing));
            foundComponent.SetAttribute("nStirrups", qty);
        }

        /// <summary>
        /// Gets existing reinforcement component from pillar in model
        /// </summary>
        /// <returns>Found component or null if failed</returns>
        private Component GetExistingComponent()
        {
            var pileChildren = _foundPair.Pillar.GetComponents();
            if (pileChildren == null) return null;
            while (pileChildren.MoveNext())
            {
                var cp = pileChildren.Current as Component;
                if (cp == null) continue;
                if (cp.Number != _savedSetting.PillarComponentNumber) continue;
                return cp;
            }

            return null;
        }
    }
}