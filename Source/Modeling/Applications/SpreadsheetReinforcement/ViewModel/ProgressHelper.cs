namespace SpreadsheetReinforcement.ViewModel
{
    using System;
    using Tools;

    public class ProgressHelper: SmartBindingBase
    {
        public string ProgressText
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public bool IsProgressIndeterminate
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public int ProgressValue
        {
            get { return GetDynamicValue<int>(); }
            set
            {
                SetDynamicValue<int>(value);
                if (ProgressMax > 0)
                {
                    var pValue = Convert.ToDouble(Convert.ToDouble(ProgressValue) / Convert.ToDouble(ProgressMax));
                    ProgressText = string.Format("{0:P2}", pValue);
                }
            }
        }

        public int ProgressMax
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        public ProgressHelper()
        {
            ProgressValue = 0;
            ProgressMax = 0;
            IsProgressIndeterminate = true;
            ProgressText = string.Empty;
        }
    }
}
