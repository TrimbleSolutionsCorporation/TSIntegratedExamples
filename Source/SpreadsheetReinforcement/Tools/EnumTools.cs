namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

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
            enumValList.AddRange(from int val in enumValArray
                select (T) Enum.Parse(enumType, val.ToString(CultureInfo.InvariantCulture)));
            return enumValList;
        }
    }
}