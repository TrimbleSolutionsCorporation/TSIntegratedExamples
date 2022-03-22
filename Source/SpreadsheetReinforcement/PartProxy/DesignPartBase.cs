namespace SpreadsheetReinforcement.PartProxy
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Data;
    using Tekla.Structures.Model;
    using Tools;

    /// <summary>
    /// Base Wrapper for footing elements to specific attributes for reinforcement components
    /// Creates or  modifies existing found from model
    /// </summary>
    public abstract class DesignPartBase
    {
        internal readonly SavedSetting _savedSetting;
        private static ObservableCollection<SpreadsheetResultData> _designResults;

        /// <summary>
        /// New instance of base design class
        /// </summary>
        /// <param name="setting">Program settings</param>
        /// <param name="designs">Spreadsheet design cache</param>
        protected DesignPartBase(SavedSetting setting, ObservableCollection<SpreadsheetResultData> designs)
        {
            _savedSetting = setting ?? throw new ArgumentNullException(nameof(setting));
            _designResults = designs ?? throw new ArgumentNullException(nameof(designs));
        }

        /// <summary>
        /// Creates new reinforcement component between pillar and pad footing
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool InsertModify();

        /// <summary>
        /// Gets specific design from cache for selected part by settings
        /// Uses either name or mark
        /// </summary>
        /// <param name="footing">Foundation element</param>
        /// <returns>Spreadsheet data from cache or null if none found</returns>
        internal SpreadsheetResultData GetDesign(Part footing)
        {
            if (footing == null) throw new ArgumentNullException(nameof(footing));
            switch (_savedSetting.BindingName)
            {
                case SavedSetting.BindingNameType.Name:
                    return _designResults.FirstOrDefault(f => f.FootingName == footing.GetName());
                case SavedSetting.BindingNameType.Mark:
                    return _designResults.FirstOrDefault(f => f.FootingName == footing.GetAssemblyMark());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}