namespace JoistArea.Tools
{
    using Tekla.BIM.Quantities;

    /// <summary>
    /// Extensions class for Tekla.BIM.Quantities class
    /// </summary>
    public static class TxLength
    {
        /// <summary>
        /// Returns ft-fraction inch string rounded to 1/16 if IMPERIAL=TRUE, mm to to decimal places otherwise
        /// </summary>
        public static string ToCurrentUnits(this Length ln) { return TxModel.IsImperial ? ln.ToString(LengthUnit.Foot, "1/16") : 
                ln.ToString(LengthUnit.Millimeter, "0.00"); }

        /// <summary>
        /// Uses BIM Length.Parse to convert string to Length and returns Millimeter value
        /// Use IMPERIAL to decide if input is Foot versus millimeter value in string format
        /// May not work if input is other than foot or mm and flag not set
        /// </summary>
        /// <param name="str">Numeric value in string format</param>
        /// <returns>Double millimeter value</returns>
        public static double FromCurrrentUnits(this string str)
        {
            var length = Length.Parse(str, TxModel.IsImperial ? LengthUnit.Inch : LengthUnit.Millimeter);
            return length.Millimeters;
        }
    }
}
