// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;

namespace WebQuark.QueryString.Utilities
{
    /// <summary>
    /// Provides utility methods for converting strings to typed values and vice versa.
    /// Supports common types such as enums, GUIDs, DateTime, booleans, and integers.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Converts a string to the specified type <typeparamref name="T"/>.
        /// If conversion fails, the provided default value is returned.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="input">The input string to convert.</param>
        /// <param name="defaultValue">The default value to return if conversion fails.</param>
        /// <returns>A value of type <typeparamref name="T"/> converted from the input string, or the default value.</returns>
        public static T ConvertTo<T>(string input, T defaultValue = default(T))
        {
            T result;
            return TryConvert(input, out result) ? result : defaultValue;
        }

        /// <summary>
        /// Converts a typed value to its string representation.
        /// For DateTime and Guid, special formatting is applied.
        /// </summary>
        /// <typeparam name="T">The type of the value to convert.</typeparam>
        /// <param name="value">The value to convert to a string.</param>
        /// <returns>A string representation of the value.</returns>
        public static string ConvertFrom<T>(T value)
        {
            if (value == null)
                return string.Empty;

            if (typeof(T) == typeof(DateTime))
                return ((DateTime)(object)value).ToString("o"); // ISO 8601

            if (typeof(T) == typeof(Guid))
                return ((Guid)(object)value).ToString();

            return value.ToString();
        }

        /// <summary>
        /// Attempts to convert a string to the specified type <typeparamref name="T"/>.
        /// Returns a boolean indicating whether the conversion succeeded.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="input">The input string to convert.</param>
        /// <param name="result">When this method returns, contains the converted value if successful; otherwise, the default value.</param>
        /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryConvert<T>(string input, out T result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var targetType = typeof(T);
            var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (nonNullableType == typeof(string))
                {
                    result = (T)(object)input;
                    return true;
                }

                if (nonNullableType.IsEnum)
                {
                    var enumValue = Enum.Parse(nonNullableType, input, ignoreCase: true);
                    result = (T)enumValue;
                    return true;
                }

                if (nonNullableType == typeof(Guid) && Guid.TryParse(input, out var guid))
                {
                    result = (T)(object)guid;
                    return true;
                }

                if (nonNullableType == typeof(DateTime) && DateTime.TryParse(input, out var dt))
                {
                    result = (T)(object)dt;
                    return true;
                }

                if (nonNullableType == typeof(bool) && bool.TryParse(input, out var b))
                {
                    result = (T)(object)b;
                    return true;
                }

                if (nonNullableType == typeof(int) && int.TryParse(input, out var i))
                {
                    result = (T)(object)i;
                    return true;
                }

                // Fallback
                result = (T)Convert.ChangeType(input, nonNullableType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}