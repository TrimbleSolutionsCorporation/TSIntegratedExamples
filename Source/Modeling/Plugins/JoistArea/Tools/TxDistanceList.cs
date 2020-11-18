namespace JoistArea.Tools
{
    using System;
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
        /// <summary>
        /// List of distances parsed from raw distance list as input
        /// </summary>
        public List<Distance> DistanceValues { get; private set; }

        private LengthUnit CurrentLengthUnit { get; set; }

        public string LengthFormat { get; set; }

        public string FormattedStringList
        {
            get
            {
                var result = string.Empty;
                foreach (var ds in DistanceValues)
                {
                    var length = new Length(ds.Millimeters);
                    result += length.ToString(CurrentLengthUnit, LengthFormat) + " ";
                }
                return result;
            }
        }

        public TxDistanceList(string formattedDistListStr)
        {
            if(string.IsNullOrEmpty(formattedDistListStr)) return;
            var distList = DistanceList.Parse(formattedDistListStr, CultureInfo.InvariantCulture, Distance.UnitType.Millimeter);
            DistanceValues = distList.ToList();

            CurrentLengthUnit = TxModel.IsImperial ? LengthUnit.Foot : LengthUnit.Millimeter;
            LengthFormat = TxModel.IsImperial ? "1/16" : "0.00";
        }

        public TxDistanceList(List<Distance> distances)
        {
            if (distances == null) throw new ArgumentNullException(nameof(distances));
            DistanceValues = distances;

            CurrentLengthUnit = TxModel.IsImperial ? LengthUnit.Foot : LengthUnit.Millimeter;
            LengthFormat = TxModel.IsImperial ? "1/16" : "0.00";
        }

        public TxDistanceList(List<Distance> distances, LengthUnit currentUnit, string lengthFormat)
        {
            if (distances == null) throw new ArgumentNullException(nameof(distances));
            DistanceValues = distances;
            CurrentLengthUnit = currentUnit;
            LengthFormat = lengthFormat;
        }
    }
}
