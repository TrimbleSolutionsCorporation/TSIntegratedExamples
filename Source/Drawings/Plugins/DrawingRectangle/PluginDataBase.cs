namespace DrawingRectangle
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Plugins;

    public abstract class PluginDataBase
    {
        public static int DEFAULT_VALUE = int.MinValue;

        [StructuresField("HorizOffset")]
        public double HorizOffset;

        [StructuresField("TempName")]
        public string TempName;

        public virtual void CheckDefaults()
        {
            if(IsDefaultValue(HorizOffset)) HorizOffset = 0.0;
            if (IsDefaultValue(TempName)) TempName = string.Empty;
        }

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
            return Math.Abs(value - DEFAULT_VALUE) < GeometryConstants.DISTANCE_EPSILON;
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
