namespace JoistArea.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Services;

    /// <summary>
    /// Basic tools for dealing with windows enum types
    /// </summary>
    public static class EnumTools
    {
        /// <summary>
        /// Gets list from enum values
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>List of values</returns>
        public static IEnumerable<T> EnumToList<T>()
        {
            var enumType = typeof(T);
            if (enumType.BaseType != typeof(Enum)) throw new ArgumentException("T must be of type System.Enum");

            var enumValArray = Enum.GetValues(enumType);
            var enumValList = new List<T>(enumValArray.Length);
            enumValList.AddRange(from int val in enumValArray select (T)Enum.Parse(enumType, val.ToString(CultureInfo.InvariantCulture)));
            return enumValList;
        }

        public static List<string> EnumToTranslatedStrings<T>()
        {
            var enumType = typeof(T);
            if (enumType.BaseType != typeof(Enum)) throw new ArgumentException("T must be of type System.Enum");

            var enumValArray = Enum.GetValues(enumType);
            var enumValList = new List<T>(enumValArray.Length);
            enumValList.AddRange(from int val in enumValArray select (T)Enum.Parse(enumType, val.ToString(CultureInfo.InvariantCulture)));
            return enumValList.Select(typ => GlobalServices.GetTranslated(typ.ToString())).ToList();
        }

        public static double GetMaxValue(params double[] values)
        {
            if (values == null) return 0.0;
            var max = values[0];
            foreach (var dbl in values)
            {
                if (dbl > max) max = dbl;
            }
            return max;
        }
    }
}
