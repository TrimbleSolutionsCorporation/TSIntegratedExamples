using System;
using AnchorBoltsWinform.Tools;
using Tekla.Structures.Plugins;

namespace AnchorBoltsWinform.ViewModel
{
    /// <summary>
    /// Main user interface storage data class
    /// </summary>
    public class AppData
    {
        [StructuresField("AnFilerName")]
        public string AnchorBoltFilerName;

        [StructuresField("DmSettingsName")]
        public string DimensionSettingsName;

        [StructuresField("DimOffset")]
        public double DimensionLineOffset;

        public void CheckDefaults()
        {
            if (IsDefaultValue(AnchorBoltFilerName)) AnchorBoltFilerName = "";
            if (IsDefaultValue(DimensionSettingsName)) DimensionSettingsName = "";
            if (IsDefaultValue(DimensionLineOffset)) DimensionLineOffset = 3.0;
        }

        private static bool IsDefaultValue(double value)
        {
            return Math.Abs(value - TxModel.NullDoubleValue) <= 0.001;
        }

        private static bool IsDefaultValue(int value)
        {
            return value==TxModel.NullIntegerValue;
        }

        private static bool IsDefaultValue(string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}