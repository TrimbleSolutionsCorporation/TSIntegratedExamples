namespace SpreadsheetReinforcement.Tools
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Extensions class that converts between tekla dates and windows
    /// </summary>
    public class TxDate
    {
        /// <summary> Tekla null date value </summary>
        public static readonly DateTime EpochDate = Convert.ToDateTime("01.01.1970", new CultureInfo("en-US", false));

        /// <summary> Integer value for date </summary>
        public int IntegerValue { get; set; }

        /// <summary> Windows value for date </summary>
        public DateTime WindowsValue { get; set; }

        /// <summary>
        /// New blank instance of date
        /// </summary>
        public TxDate()
        {
        }

        /// <summary>
        /// New instance of date using Tekla value
        /// </summary>
        /// <param name="intValue">Tekla integer value, seconds form Epoch</param>
        public TxDate(int intValue)
        {
            IntegerValue = intValue;
            WindowsValue = ConvertToWindowsDate(intValue);
        }

        /// <summary>
        /// New instance of date using Windows value
        /// </summary>
        /// <param name="dateValue">Windows date value</param>
        public TxDate(DateTime dateValue)
        {
            WindowsValue = dateValue;
            IntegerValue = ConvertToTeklaDate(dateValue);
        }

        /// <summary>
        /// Converts to Windows date
        /// </summary>
        /// <param name="secondsSinceEpoch">Tekla int date/time value</param>
        /// <returns>New Windows date</returns>
        public static DateTime ConvertToWindowsDate(int secondsSinceEpoch)
        {
            var timeSinceEpoch = new TimeSpan(0, 0, 0, secondsSinceEpoch);
            return (EpochDate + timeSinceEpoch);
        }

        /// <summary>
        /// Converts to Tekla date
        /// </summary>
        /// <param name="actualDate">Windows date value</param>
        /// <returns>Tekla int value for date/time</returns>
        public static int ConvertToTeklaDate(DateTime actualDate)
        {
            var seconds = (actualDate - EpochDate).TotalSeconds;
            return (int) Convert.ToUInt32(seconds);
        }
    }
}