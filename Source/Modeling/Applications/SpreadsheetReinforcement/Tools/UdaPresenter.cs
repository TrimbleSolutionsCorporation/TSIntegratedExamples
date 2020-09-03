namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Tekla.Structures;
    using Tekla.Structures.Catalogs;
    using Tekla.Structures.Model;

    /// <summary>
    /// WPF view model presenter class for user property
    /// </summary>
    public class UdaPresenter : SmartObservableObject, IComparable, IEquatable<UdaPresenter>
    {
        #region Properties

        private UserPropertyItem _userProperty;
        private bool _affectsNumbering;
        private UserPropertyLevelEnum _level;
        private UserPropertyFieldTypeEnum _fieldType;
        private string _udaName;
        private PropertyTypeEnum _type;
        private bool _unique;
        private UserPropertyVisibilityEnum _visibility;
        private string _label;
        private bool _isDefined;
        private List<KeyValuePair<int, string>> _optionsList = new List<KeyValuePair<int, string>>();
        private object _udaValue;
        private readonly ModelObject _modelObject;

        /// <summary>
        /// If uda affects numbering
        /// </summary>
        public bool AffectsNumbering
        {
            get { return _affectsNumbering; }
            set { SetValue(ref _affectsNumbering, value); }
        }

        /// <summary>
        /// Long Tekla uda type
        /// </summary>
        public UserPropertyFieldTypeEnum FieldType
        {
            get { return _fieldType; }
            set { SetValue(ref _fieldType, value); }
        }

        /// <summary>
        /// Level uda defined at
        /// </summary>
        public UserPropertyLevelEnum Level
        {
            get { return _level; }
            set { SetValue(ref _level, value); }
        }

        /// <summary>
        /// Internal uda name
        /// </summary>
        public string UdaName
        {
            get { return _udaName; }
            set { SetValue(ref _udaName, value); }
        }

        /// <summary>
        /// Uda value
        /// </summary>
        public object UdaValue
        {
            get { return _udaValue; }
            set
            {
                SetValue(ref _udaValue, value);
                if (_modelObject != null && TxModel.IsConnected) SetValueInModel(UdaName, value);
            }
        }

        /// <summary>
        /// Short uda type: int, double, string
        /// </summary>
        public PropertyTypeEnum Type
        {
            get { return _type; }
            set { SetValue(ref _type, value); }
        }

        /// <summary>
        /// If uda is unique property = true
        /// </summary>
        public bool IsUnique
        {
            get { return _unique; }
            set { SetValue(ref _unique, value); }
        }

        /// <summary>
        /// Visibility setting for uda
        /// </summary>
        public UserPropertyVisibilityEnum Visibility
        {
            get { return _visibility; }
            set { SetValue(ref _visibility, value); }
        }

        /// <summary>
        /// String uda label that user sees
        /// </summary>
        public string Label
        {
            get { return _label; }
            set { SetValue(ref _label, value); }
        }

        /// <summary>
        /// Gets strings but excludes null (index less than 1)
        /// </summary>
        public IEnumerable<string> OptionStrings
        {
            get { return _optionsList.Select(pair => pair.Value).ToList(); }
        }

        /// <summary>
        /// Returns all items including negative index
        /// </summary>
        public List<KeyValuePair<int, string>> OptionList
        {
            get { return _optionsList; }
            set { SetValue(ref _optionsList, value); }
        }

        /// <summary>
        /// User property item
        /// </summary>
        public UserPropertyItem UserProperty
        {
            get { return _userProperty; }
            private set { SetValue(ref _userProperty, value); }
        }

        /// <summary>
        /// True if defined in system of object.inp or environments.db
        /// </summary>
        public bool IsDefined
        {
            get { return _isDefined; }
            set { SetValue(ref _isDefined, value); }
        }

        /// <summary>
        /// Integery pair options if valid
        /// </summary>
        public List<OptionItem> IntegerOptions
        {
            get { return OptionList.Select(pair => new OptionItem(pair.Key, pair.Value)).ToList(); }
        }

        /// <summary>
        /// String editor to use based on fieldtype
        /// </summary>
        public string UdaTypeEditor
        {
            get
            {
                switch (FieldType)
                {
                    case UserPropertyFieldTypeEnum.FIELDTYPE_NUMBER:
                        return "optionsEditor";
                    case UserPropertyFieldTypeEnum.FIELDTYPE_DISTANCE:
                        return "distanceEditor";
                    case UserPropertyFieldTypeEnum.FIELDTYPE_DIMENSION:
                        return "distanceEditor";
                    case UserPropertyFieldTypeEnum.FIELDTYPE_DATE:
                        return "dateEditor";
                    case UserPropertyFieldTypeEnum.FIELDTYPE_DATE_TIME_SEC:
                        return "timeEditor";
                    case UserPropertyFieldTypeEnum.FIELDTYPE_DATE_TIME_MIN:
                        return "timeEditor";
                }

                return "stringEditor";
            }
        }

        #endregion

        /// <summary>
        /// New instance of udapresenter for serialization
        /// </summary>
        public UdaPresenter()
        {
            //Required for serialization
        }

        /// <summary>
        /// Creates new user property proxy from user propery, cache of date is immediate
        /// </summary>
        /// <param name="udaItem"></param>
        /// <param name="mo"></param>
        /// <param name="value"></param>
        public UdaPresenter(UserPropertyItem udaItem, ModelObject mo, object value)
        {
            if (udaItem == null) throw new ApplicationException();
            IsDefined = true;

            //Cache data, data fetched from core wrapper
            _modelObject = mo;
            Select(udaItem);
            _udaValue = value;
        }

        /// <summary>
        /// Creates new user property proxy from user propery, cache of date is immediate
        /// </summary>
        /// <param name="udaItem"></param>
        public UdaPresenter(UserPropertyItem udaItem)
        {
            if (udaItem == null) throw new ApplicationException();
            IsDefined = true;

            //Cache data, data fetched from core wrapper
            Select(udaItem);
        }

        /// <summary>
        /// Creates new user property proxy from raw data
        /// </summary>
        /// <param name="name">Real Tekla name of uda</param>
        /// <param name="type">Type of uda</param>
        /// <param name="mo"></param>
        /// <param name="value"></param>
        public UdaPresenter(string name, PropertyTypeEnum type, ModelObject mo, object value)
        {
            //Base data
            var fakeUserProperty = new UserPropertyItem {Name = name, Type = type,};
            IsDefined = false;

            //Cache data, defaults fetched from class instance
            _modelObject = mo;
            Select(fakeUserProperty);
            _udaValue = value;
        }

        /// <summary>
        /// Fills in internal values for proxy from definition
        /// </summary>
        /// <param name="udaItem"></param>
        private void Select(UserPropertyItem udaItem)
        {
            UserProperty = udaItem;
            AffectsNumbering = udaItem.AffectsNumbering;
            FieldType = udaItem.FieldType;
            Level = udaItem.Level;
            UdaName = udaItem.Name;
            Type = udaItem.Type;
            IsUnique = udaItem.Unique;
            Visibility = udaItem.Visibility;
            Label = udaItem.GetLabel();
            udaItem.GetOptions(ref _optionsList);
            PurgeNegativeOptions(ref _optionsList);
            RaisePropertyChanged(() => OptionList);
        }

        private static void PurgeNegativeOptions(ref List<KeyValuePair<int, string>> optionsList)
        {
            var result = optionsList.Where(kp => kp.Key >= 0).ToList();
            optionsList = result;
        }

        /// <summary>
        /// Fills in internal values for proxy from model object
        /// </summary>
        /// <param name="modelObject"></param>
        private void Select(ModelObject modelObject)
        {
            if (modelObject == null) throw new NullReferenceException();
            switch (Type)
            {
                case PropertyTypeEnum.TYPE_STRING:
                {
                    var tempValue = string.Empty;
                    modelObject.GetUserProperty(UdaName, ref tempValue);
                    _udaValue = tempValue;
                }
                    break;
                case PropertyTypeEnum.TYPE_INT:
                {
                    var tempValue = -1;
                    modelObject.GetUserProperty(UdaName, ref tempValue);
                    _udaValue = tempValue;
                }
                    break;
                case PropertyTypeEnum.TYPE_DOUBLE:
                {
                    var tempValue = 0.0;
                    modelObject.GetUserProperty(UdaName, ref tempValue);
                    _udaValue = tempValue;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets value in model if connected and model object is associated
        /// </summary>
        public void UpdateModelValue()
        {
            if (_modelObject != null && TxModel.IsConnected) SetValueInModel(UdaName, _udaValue);
        }

        private void SetValueInModel(string name, object value)
        {
            if (_modelObject == null || value == null) return;
            switch (Type)
            {
                case PropertyTypeEnum.TYPE_INT:
                    int tempInt;
                    if (int.TryParse(value.ToString(), out tempInt)) _modelObject.SetUserProperty(name, tempInt);
                    break;
                case PropertyTypeEnum.TYPE_DOUBLE:
                    double tempDouble;
                    if (double.TryParse(value.ToString(), out tempDouble))
                        _modelObject.SetUserProperty(name, tempDouble);
                    break;
                case PropertyTypeEnum.TYPE_STRING:
                    _modelObject.SetUserProperty(name, (string) value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets type based on definition fieldtype
        /// </summary>
        /// <param name="userDef"></param>
        public static void SetShortType(UserPropertyItem userDef)
        {
            if (userDef == null) throw new ArgumentNullException();
            switch (userDef.FieldType)
            {
                case UserPropertyFieldTypeEnum.FIELDTYPE_UNDEFINED:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_NUMBER:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_TEXT:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DISTANCE:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_PROFILE:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_MATERIAL:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_TEXT_LIST_DISTANCE:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_FILE_IN:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_FILE_OUT:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_BOLT_STANDARD:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_BOLT_SIZE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_RATIO:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_STRAIN:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_ANGLE:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DEFORMATION:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DIMENSION:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_RADIUSOFINERTIA:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_AREA:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_AREAPERLENGTH:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_SECTIONMODULUS:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_MOMENTOFINERTIA:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_TORSIONCONSTANT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_WARPINGCONSTANT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_FORCE:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_WEIGHT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DISTRIBLOAD:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_SPRINGCONSTANT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_SURFACELOAD:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_STRENGTH:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_MODULUS:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DENSITY:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_MOMENT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DISTRIBMOMENT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_ROTSPRINGCONST:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_TEMPERATURE:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_THERMDILATCOEFF:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_ANALYSIS_RESTRAINT:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_VOLUME:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_MAIN:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_STIRRUP:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DATE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DATE_TIME_SEC:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_DATE_TIME_MIN:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_STUD_STANDARD:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_STUD_SIZE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_STUD_LENGTH:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_HOLE_TYPE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_HOLE_DIRECTION:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_WELD_TYPE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_CHAMFER_TYPE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_WELDING_SITE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_FACTOR:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_PART_NAME:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_BOLT_TYPE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_COMPONENT_NAME:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_MESH:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_USERDEFINED:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_YES_NO:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_COMPONENT_STANDARD_FILE:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_GRADE:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_RADIUS:
                    userDef.Type = PropertyTypeEnum.TYPE_DOUBLE;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_REBAR_SIZE:
                    userDef.Type = PropertyTypeEnum.TYPE_STRING;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_HOOK_SHAPE:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                case UserPropertyFieldTypeEnum.FIELDTYPE_CROSSBAR_POSITION:
                    userDef.Type = PropertyTypeEnum.TYPE_INT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Custom deserialization method
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //_userProperty = (UserPropertyItem)info.GetValue("_userProperty", typeof(UserPropertyItem));
            _affectsNumbering = (bool) info.GetValue("_affectsNumbering", typeof(bool));
            _level = (UserPropertyLevelEnum) info.GetValue("_level", typeof(UserPropertyLevelEnum));
            _fieldType = (UserPropertyFieldTypeEnum) info.GetValue("_fieldType", typeof(UserPropertyFieldTypeEnum));
            _udaName = (string) info.GetValue("_udaName", typeof(string));
            _type = (PropertyTypeEnum) info.GetValue("_type", typeof(PropertyTypeEnum));
            _unique = (bool) info.GetValue("_unique", typeof(bool));
            _visibility = (UserPropertyVisibilityEnum) info.GetValue("_visibility", typeof(UserPropertyVisibilityEnum));
            _label = (string) info.GetValue("_label", typeof(string));
            _isDefined = (bool) info.GetValue("_isDefined", typeof(bool));
            _optionsList =
                (List<KeyValuePair<int, string>>) info.GetValue("_optionsList",
                    typeof(List<KeyValuePair<int, string>>));
            _udaValue = info.GetValue("_udaValue", typeof(object));
        }

        /// <summary>
        /// Custom serialization method
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public UdaPresenter(SerializationInfo info, StreamingContext ctxt)
        {
            //info.AddValue("_userProperty", _userProperty);
            info.AddValue("_affectsNumbering", _affectsNumbering);
            info.AddValue("_level", _level);
            info.AddValue("_fieldType", _fieldType);
            info.AddValue(" _udaName", _udaName);
            info.AddValue("_type", _type);
            info.AddValue("_unique", _unique);
            info.AddValue("_visibility", _visibility);
            info.AddValue("_label", _label);
            info.AddValue("_isDefined", _isDefined);
            info.AddValue("_optionsList", _optionsList);
            info.AddValue("_udaValue", _udaValue);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            var other = obj as UdaPresenter;
            if (other == null) return -1;
            return other.UdaName == UdaName ? 0 : 1;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(UdaPresenter other)
        {
            if (other == null) return false;
            return other.UdaName == UdaName;
        }
    }
}