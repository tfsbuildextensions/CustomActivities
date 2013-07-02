//-----------------------------------------------------------------------
// <copyright file="StringExtension.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities
{
    using System;
    using System.Globalization;
    using System.Text;

    internal static class StringExtension
    {
        public static string AppendFormat(this string originalValue, IFormatProvider provider, string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format) || args == null)
            {
                return originalValue ?? string.Empty;
            }

            StringBuilder builder = new StringBuilder(originalValue ?? string.Empty);
            builder.AppendFormat(provider, format, args);
            return builder.ToString();
        }

        public static string AppendFormat(this string originalValue, string format, params object[] args)
        {
            return AppendFormat(originalValue, CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Returns a value indicating whether the specified System.String object occurs within this string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="valueLookingFor">The value looking for.</param>
        /// <param name="stringComparison">The string comparison.</param>
        /// <returns> Returns <c>true</c> if match exists, otherwise <c>false</c>.</returns>
        public static bool Contains(this string source, string valueLookingFor, StringComparison stringComparison)
        {
            return source.IndexOf(valueLookingFor, stringComparison) >= 0;
        }
    }
}