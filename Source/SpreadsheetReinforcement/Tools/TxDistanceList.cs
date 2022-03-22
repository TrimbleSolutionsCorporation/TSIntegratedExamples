namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Tekla.BIM.Quantities;
    using Tekla.Structures.Datatype;

    /// <summary>
    /// Represents a distance list in metric and imperial when needed
    /// </summary>
    public class TxDistanceList
    {
        private readonly ArrayList _rawDistanceList = new ArrayList();
        private readonly ArrayList _formattedList = new ArrayList();

        /// <summary>
        /// List of distances parsed from raw distance list as input
        /// </summary>
        public List<Distance> DistanceValues
        {
            get
            {
                var result = new List<Distance>();
                foreach (var mmStr in _rawDistanceList)
                {
                    double mmValue;
                    if (!double.TryParse(mmStr.ToString(), out mmValue)) continue;
                    var dist = new Distance(mmValue);
                    result.Add(dist);
                }

                return result;
            }
        }

        /// <summary>
        /// Distance list string formatted
        /// </summary>
        public string FormattedDistanceString
        {
            get
            {
                return _formattedList.Cast<object>().Aggregate(string.Empty, (current, str) => current + (str + " "));
            }
        }

        /// <summary>
        /// Un-formatted distance list string
        /// </summary>
        public string RawDistanceString
        {
            get
            {
                return _rawDistanceList.Cast<object>().Aggregate(string.Empty, (current, str) => current + (str + " "));
            }
        }

        /// <summary>
        /// Creates new distance list proxy object and parses string
        /// </summary>
        /// <param name="stringList">Distance list string from core Tekla</param>
        /// <param name="lengthUnit">Current units to use</param>
        /// <param name="lengthFormatString">Current formatting to use</param>
        public TxDistanceList(ArrayList stringList, LengthUnit lengthUnit, string lengthFormatString)
        {
            if (stringList == null) throw new ArgumentNullException(nameof(stringList));
            _rawDistanceList = stringList;
            TryParseList(stringList, lengthUnit, lengthFormatString);
        }

        /// <summary>
        /// Creates new distance list proxy object and parses string
        /// </summary>
        public TxDistanceList(ArrayList stringList)
        {
            if (stringList == null) throw new ArgumentNullException(nameof(stringList));
            _rawDistanceList = stringList;
            var currentUnit = TxModel.IsImperial ? LengthUnit.Foot : LengthUnit.Millimeter;
            var currentFormat = TxModel.IsImperial ? "1/16" : "0.00";
            TryParseList(_rawDistanceList, currentUnit, currentFormat);
        }

        /// <summary>
        /// Creates new distance list proxy object and parses string
        /// </summary>
        public TxDistanceList(string combinedStringList, Distance.UnitType unitType)
        {
            if (combinedStringList == null) throw new ArgumentNullException(nameof(combinedStringList));
            var distList = DistanceList.Parse(combinedStringList, CultureInfo.InvariantCulture, unitType);
            foreach (var dist in distList) _rawDistanceList.Add(dist.Millimeters);
            var currentUnit = TxModel.IsImperial ? LengthUnit.Foot : LengthUnit.Millimeter;
            var currentFormat = TxModel.IsImperial ? "1/16" : "0.00";
            TryParseList(_rawDistanceList, currentUnit, currentFormat);
        }

        /// <summary>
        /// Creates new distance list proxy object and parses string
        /// </summary>
        public TxDistanceList(string combinedStringList)
        {
            if (combinedStringList == null) throw new ArgumentNullException("combinedStringList");
            var distList = DistanceList.Parse(combinedStringList, CultureInfo.InvariantCulture,
                Distance.UnitType.Millimeter);
            foreach (var dist in distList) _rawDistanceList.Add(dist.Millimeters);
            var currentUnit = TxModel.IsImperial ? LengthUnit.Foot : LengthUnit.Millimeter;
            var currentFormat = TxModel.IsImperial ? "1/16" : "0.00";
            TryParseList(_rawDistanceList, currentUnit, currentFormat);
        }

        private void TryParseList(IEnumerable stringList, LengthUnit lengthUnit, string lengthFormatString)
        {
            if (stringList == null) throw new ArgumentNullException();
            foreach (var str in stringList)
            {
                var tempValue = str.ToString();
                double dblValue;
                if (!double.TryParse(tempValue, out dblValue)) continue;
                var tempLength = new Length(dblValue);
                _formattedList.Add(tempLength.ToString(lengthUnit, lengthFormatString, null,
                    QuantityUnitSymbols.NoSymbols));
            }
        }
    }
}