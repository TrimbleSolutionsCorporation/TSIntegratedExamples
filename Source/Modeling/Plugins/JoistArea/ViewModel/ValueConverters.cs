namespace JoistArea.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using Tekla.BIM.Quantities;
    using Tools;

    [ValueConversion(typeof(double), typeof(string))]
    public class WeightConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(value.ToString()) || value.ToString() == " ") return value;
            try
            {
                var dist = new Mass(System.Convert.ToDouble(value));
                if (TxModel.IsImperial) return dist.ToString(MassUnit.Pound);
                return dist.To(MassUnit.Kilogram);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class NegativeVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return Visibility.Visible;
            var flag = (bool) value;
            return flag ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var back = value is Visibility && (Visibility) value == Visibility.Visible;
            if (parameter == null) return back;
            if ((bool) parameter) back = !back;
            return back;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class NegativeBooleanConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return false;
            var flag = (bool)value;
            return !flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return false;
            var flag = (bool)value;
            return !flag;
        }
    }

    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}