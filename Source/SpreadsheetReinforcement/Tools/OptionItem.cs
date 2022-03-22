namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents integer core value and string output for ComboBox itemsource for WPF
    /// </summary>
    public class OptionItem : SmartObservableObject, IComparable, IEquatable<OptionItem>
    {
        private string _stringValue;
        private int _stringIndex;

        /// <summary>
        /// String value user sees in combobox
        /// </summary>
        public string StringValue
        {
            get { return _stringValue; }
            set { SetValue(ref _stringValue, value); }
        }

        /// <summary>
        /// Integer value from combobox item
        /// </summary>
        public int StringIndex
        {
            get { return _stringIndex; }
            set { SetValue(ref _stringIndex, value); }
        }

        /// <summary>
        /// New instance of option item with values
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public OptionItem(int index, string value)
        {
            StringValue = value;
            StringIndex = index;
        }

        /// <summary>
        /// New blank instance of option item
        /// </summary>
        public OptionItem()
        {
            //Needed for serialization
        }

        /// <summary>
        /// Custom deserialization method
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _stringValue = (string) info.GetValue("_name", typeof(string));
            _stringIndex = (int) info.GetValue("_label", typeof(int));
        }

        /// <summary>
        /// Custom serialization method
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public OptionItem(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("_stringValue", _stringValue);
            info.AddValue("_stringIndex", _stringIndex);
        }

        public int CompareTo(object obj)
        {
            var other = obj as OptionItem;
            if (other == null) return -1;
            return other.StringValue == StringValue ? 0 : 1;
        }

        public bool Equals(OptionItem other)
        {
            if (other == null) return false;
            return other.StringValue == StringValue;
        }
    }
}