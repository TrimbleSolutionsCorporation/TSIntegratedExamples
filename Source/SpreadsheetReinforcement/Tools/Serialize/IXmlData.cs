namespace SpreadsheetReinforcement.Tools.Serialize
{
    using System;

    public interface IXmlData : IEquatable<IXmlData>, IComparable
    {
        /// <summary>
        /// File save name to disk
        /// </summary>
        string SaveName { get; set; }

        void IsValidStringUdaValue(string value, string propertyName, bool canBeBlank = false);

        void IsValidIntValue(int value, string propertyName);

        void IsValidDateValue(DateTime value, string propertyName);

        void IsValidUdaName(string value, string propertyName);
    }
}