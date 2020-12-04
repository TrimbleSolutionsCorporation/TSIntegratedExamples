﻿namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;

    public abstract class PluginDataHelper
    {
        public static int DEFAULT_VALUE = int.MinValue;

        public abstract void CheckDefaults();

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsBlankValue(int value)
        {
            return value == DEFAULT_VALUE;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value for this type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsBlankValue(double value)
        {
            return Math.Abs(value - TxModel.NullDoubleValue) < GeometryConstants.DISTANCE_EPSILON;
        }

        /// <summary>
        /// Returns true if the given value is set to the default value (empty string).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is set to the default.</returns>
        public static bool IsBlankValue(string value)
        {
            return value == "";
        }
    }
}
