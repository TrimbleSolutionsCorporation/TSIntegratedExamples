namespace JoistArea.Tools
{
    public abstract class PluginDataHelper
    {
        public static int DEFAULT_VALUE = int.MinValue;

        public abstract void CheckDefaults();

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsDefaultValue(int value)
        {
            return value == DEFAULT_VALUE;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsDefaultValue(double value)
        {
            return value == DEFAULT_VALUE;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value (empty string).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsDefaultValue(string value)
        {
            return value == "";
        }
    }
}
