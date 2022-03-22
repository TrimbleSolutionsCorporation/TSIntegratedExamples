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
    public abstract class SmartObservableObject : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary> Event trigger that class property has changed </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Class validator  </summary>
        [XmlIgnore] public PropertiesValidator Validator = new PropertiesValidator();

        [XmlIgnore]
        public bool HasErrors
        {
            get { return Validator.DataErrors.Any(); }
        }

        /// <summary> Error string list </summary>
        [XmlIgnore]
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
        public SmartObservableObject()
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

        /// <summary>
        /// Sets internal property field and triggers notification
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string caller = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(caller);
            return true;
        }

        /// <summary>
        /// Sets internal property field and triggers notification
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected bool SetProperty<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(selectorExpression);
            return true;
        }
    }

    /// <summary>
    /// Properties validator class for WPF validation
    /// </summary>
    public class PropertiesValidator
    {
        private string dataError = "";
        private Dictionary<string, string> _dataErrors = new Dictionary<string, string>();
        public EventHandler ErrorsChanged;

        /// <summary>
        /// String data error for cell
        /// </summary>
        [XmlIgnore]
        public string DataError
        {
            get { return dataError; }
            set { dataError = value; }
        }

        /// <summary>
        /// List of string data errors for cell
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, string> DataErrors
        {
            get { return _dataErrors; }
            set { _dataErrors = value; }
        }

        /// <summary>
        /// New instance of properties validator
        /// </summary>
        public PropertiesValidator()
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