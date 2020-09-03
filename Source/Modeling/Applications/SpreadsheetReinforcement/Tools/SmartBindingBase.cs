namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    /// <summary>
    /// Base object for WPF that implements INotifyPropertyChanged, IDataErrorInfo, ISerializable
    /// </summary>
    public abstract class SmartBindingBase : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary> Event trigger that class property has changed </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Class validator  </summary>
        [XmlIgnore] [Browsable(false)] public SmartPropertiesValidator Validator = new SmartPropertiesValidator();

        [XmlIgnore]
        [Browsable(false)]
        public bool HasErrors => Enumerable.Any<KeyValuePair<string, string>>(Validator.DataErrors);

        /// <summary> Error string list </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string Error
        {
            get { return Validator.DataError; }
            set
            {
                Validator.DataError = value;
                RaisePropertyChanged(nameof(Error));
            }
        }

        /// <summary>
        /// New instance of object that implements basic WPF notifications
        /// </summary>
        public SmartBindingBase()
        {
            Validator.ErrorsChanged += ErrorsChangedDel;
        }

        private void ErrorsChangedDel(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// Returns data error for column name key
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get { return Validator.DataErrors.ContainsKey(columnName) ? Validator.DataErrors[columnName] : null; }
        }

        /// <summary>
        /// Raises property changed by name manually
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises property changed event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectorExpression"></param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null) throw new ArgumentNullException(nameof(selectorExpression));
            var body = selectorExpression.Body as MemberExpression;
            if (body == null) throw new ArgumentException("The body must be a member expression");
            RaisePropertyChanged(body.Member.Name);
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string caller = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(caller);
            return true;
        }

        private readonly Dictionary<string, object> _propertyDictionary = new Dictionary<string, object>();
        private readonly Dictionary<string, int> _propertyLengthDictionary = new Dictionary<string, int>();

        /// <summary>
        /// Gets cached value for property from dictionary: No validation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected T GetDynamicValue<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return default(T);
            object value;
            if (!_propertyDictionary.TryGetValue(propertyName, out value)) return default(T);
            return (T) value;
        }

        /// <summary>
        /// Gets cached value for property from dictionary: Creates error validation entry if needed
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="allowNull">Allow value to be null or empty</param>
        /// <param name="propertyName">Calling property name</param>
        /// <returns>Property value from dictionary</returns>
        protected T GetDynamicValue<T>(bool allowNull, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return default(T);
            object value;
            if (!_propertyDictionary.TryGetValue(propertyName, out value)) return default(T);
            if (value != null) IsValid(value.ToString(), allowNull, propertyName);
            return (T) value;
        }

        protected string GetDynamicValue(int maxLength, bool allowNull, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            object value;
            if (!_propertyDictionary.TryGetValue(propertyName, out value)) return default(string);
            if (value == null) return default(string);
            IsValid(value.ToString(), maxLength, allowNull, propertyName);
            return (string) value;
        }

        protected double GetDynamicValue([CallerMemberName] string propertyName = null,
            double minLength = double.MinValue, double maxLength = double.MaxValue)
        {
            if (string.IsNullOrEmpty(propertyName)) return 0.0;
            object value;
            if (!_propertyDictionary.TryGetValue(propertyName, out value)) return default(double);
            if (value == null) return default(double);
            IsValid((double) value, minLength, maxLength, propertyName);
            return (double) value;
        }

        protected int GetDynamicValue(int minLength = int.MinValue, int maxLength = int.MaxValue,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return -1;
            object value;
            if (!_propertyDictionary.TryGetValue(propertyName, out value)) return default(int);
            if (value == null) return default(int);
            IsValid((int) value, minLength, maxLength, propertyName);
            return (int) value;
        }

        protected bool SetDynamicValue<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            if (EqualityComparer<T>.Default.Equals(newValue, GetDynamicValue<T>(propertyName))) return false;
            _propertyDictionary[propertyName] = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetDynamicValue(string newValue, bool allowNull, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            IsValid(newValue, allowNull, propertyName);
            if (EqualityComparer<string>.Default.Equals(newValue, GetDynamicValue<string>(propertyName))) return false;
            _propertyDictionary[propertyName] = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetDynamicValue(string newValue, int maxLength, bool allowNull,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            if (EqualityComparer<string>.Default.Equals(newValue, GetDynamicValue<string>(propertyName))) return false;
            _propertyDictionary[propertyName] = newValue;
            RaisePropertyChanged(propertyName);
            IsValid(newValue, maxLength, allowNull, propertyName);
            return true;
        }

        protected bool SetDynamicValue(int newValue, int minValue = int.MinValue, int maxValue = int.MaxValue,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            if (EqualityComparer<int>.Default.Equals(newValue, GetDynamicValue<int>(propertyName))) return false;
            _propertyDictionary[propertyName] = newValue;
            RaisePropertyChanged(propertyName);
            IsValid(newValue, minValue, maxValue, propertyName);
            return true;
        }

        protected bool SetDynamicValue(double newValue, double minValue = double.MinValue,
            double maxValue = double.MaxValue, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;
            if (EqualityComparer<double>.Default.Equals(newValue, GetDynamicValue<double>(propertyName))) return false;
            _propertyDictionary[propertyName] = newValue;
            RaisePropertyChanged(propertyName);
            IsValid(newValue, minValue, maxValue, propertyName);
            return true;
        }

        protected void IsValid(string value, int maxLength, bool allowNull,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            ClearPropertyErrors(propertyName);
            if (string.IsNullOrEmpty(value) && !allowNull)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must not be blank");
            else if (value != null && value.Length > maxLength)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must be less than {maxLength} characters");
        }

        protected void IsValid(string value, bool allowNull, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            ClearPropertyErrors(propertyName);
            if (string.IsNullOrEmpty(value) && !allowNull)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must not be blank");
        }

        protected void IsValid(int value, int minValue = int.MinValue, int maxValue = int.MaxValue,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            ClearPropertyErrors(propertyName);
            if (value < minValue)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must not be greater than {minValue}");
            else if (value > maxValue)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must be less than {maxValue} ");
        }

        protected void IsValid(double value, double minValue = double.MinValue, double maxValue = double.MaxValue,
            [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            ClearPropertyErrors(propertyName);
            if (value < minValue)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must not be greater than {minValue}");
            else if (value > maxValue)
                Validator.DataErrors.Add(propertyName, $"{propertyName} must be less than {maxValue} ");
        }

        protected void ClearPropertyErrors(string propertyName)
        {
            Validator?.ClearPropertyErrors(propertyName);
        }
    }

    /// <summary>
    /// Properties validator class for WPF validation
    /// </summary>
    public class SmartPropertiesValidator
    {
        private string _dataError = "";
        private Dictionary<string, string> _dataErrors = new Dictionary<string, string>();
        public EventHandler ErrorsChanged;

        /// <summary>
        /// String data error for cell
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string DataError
        {
            get { return _dataError; }
            set { _dataError = value; }
        }

        /// <summary>
        /// List of string data errors for cell
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Dictionary<string, string> DataErrors
        {
            get { return _dataErrors; }
            set { _dataErrors = value; }
        }

        /// <summary>
        /// New instance of properties validator
        /// </summary>
        public SmartPropertiesValidator()
        {
        }

        /// <summary>
        /// Removes data errors by key field name if exist
        /// </summary>
        /// <param name="propertyName"></param>
        public void ClearPropertyErrors(string propertyName)
        {
            if (_dataErrors.ContainsKey(propertyName)) _dataErrors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new EventArgs());
        }

        public void AddError(string propertyName, string error)
        {
            //Clear existing for property
            if (DataErrors.ContainsKey(propertyName)) DataErrors.Remove(propertyName);

            //Add new error for property
            DataErrors.Add(propertyName, error);
            ErrorsChanged?.Invoke(this, new EventArgs());
        }
    }
}