namespace SpreadsheetReinforcement.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using Tools;
    using Tekla.BIM.Quantities;

    [ValueConversion(typeof(double), typeof(string))]
    public class TsLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!(value is double)) return value;
                var metricValue = (double) value;
                return ConvertToString(metricValue);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return value;
            }
        }

        public static string ConvertToString(double metricValue)
        {
            var len = new Length(metricValue);
            return len.ToString(TxModel.IsImperial ? LengthUnit.Inch : LengthUnit.Millimeter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!(value is string)) return value;
                var imperialValue = (string) value;
                return imperialValue.FromCurrentUnits();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return value;
            }
        }
    }
}