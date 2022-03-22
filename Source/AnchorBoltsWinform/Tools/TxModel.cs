using Tekla.Structures;

namespace AnchorBoltsWinform.Tools
{
    /// <summary>
    /// Tekla model class extensions
    /// </summary>
    public static class TxModel
    {
        /// <summary> Tekla Core null int value </summary>
        public const int NullIntegerValue = -2147483648;

        /// <summary> Tekla Core null double value </summary>
        public const double NullDoubleValue = -2147483648.0;

        /// <summary>
        /// Returns if imperial units are being used
        /// </summary>
        public static bool IsImperial
        {
            get
            {
                var stringTemp = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_IMPERIAL", ref stringTemp);
                if (stringTemp.Trim().ToUpper() == "TRUE") return true;
                return false;
            }
        }
    }
}