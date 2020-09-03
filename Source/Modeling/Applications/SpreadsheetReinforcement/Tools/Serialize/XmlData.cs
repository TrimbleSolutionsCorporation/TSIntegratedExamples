namespace SpreadsheetReinforcement.Tools.Serialize
{
    using System;

    public class XmlData : SmartBindingBase, IXmlData
    {
        /// <summary>
        /// File save name to disk
        /// </summary>
        public string SaveName
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public XmlData()
        {
        }

        public void IsValidStringUdaValue(string value, string propertyName, bool canBeBlank = false)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (string.IsNullOrEmpty(value) && !canBeBlank)
            {
                Validator.AddError(propertyName, $"{propertyName} uda value must not be blank.");
            }
            else if (!string.IsNullOrEmpty(value) && value.Length > 79)
            {
                Validator.AddError(propertyName, $"{propertyName} uda value must be less than 80 characters");
            }
            else
            {
                Validator.ClearPropertyErrors(propertyName);
            }
        }

        public void IsValidIntValue(int value, string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (value < 0)
            {
                Validator.AddError(propertyName, $"{propertyName} uda value must be non-negative.");
            }
            else
            {
                Validator.ClearPropertyErrors(propertyName);
            }
        }

        public void IsValidDateValue(DateTime value, string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (value <= TxDate.EpochDate)
            {
                Validator.AddError(propertyName, $"{propertyName} uda date value must be greater than 1970.");
            }

            if (value > DateTime.MaxValue)
            {
                Validator.AddError(propertyName,
                    $"{propertyName} uda date value must be less than {DateTime.MaxValue.ToShortDateString()}.");
            }
            else
            {
                Validator.ClearPropertyErrors(propertyName);
            }
        }

        public void IsValidUdaName(string value, string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (string.IsNullOrEmpty(value))
            {
                Validator.AddError(propertyName, $"{propertyName} uda name must not be blank.");
            }
            else if (!string.IsNullOrEmpty(value) && value.Length > 19)
            {
                Validator.AddError(propertyName, $"{propertyName} uda name must be less than 20 characters");
            }
            else
            {
                Validator.ClearPropertyErrors(propertyName);
            }
        }

        public bool Equals(IXmlData other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return SaveName == other.SaveName;
        }

        public int CompareTo(object obj)
        {
            var other = obj as IXmlData;
            if (other == null) return -1;
            if (ReferenceEquals(this, other)) return 0;
            return SaveName == other.SaveName ? 0 : 1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IXmlData;
            if (other == null) return false;
            return SaveName == other.SaveName;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(SaveName)) return 0;
            return SaveName.GetHashCode();
        }

        public override string ToString() => SaveName;
    }
}