//-----------------------------------------------------------------------
// <copyright file="ServiceExtensions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.AWS.Extended
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Extension methods for interacting with the Amazon AWS SDK.
    /// </summary>
    public static class ServiceExtensions
    {
         /// <summary>
        /// Extension method for DateTime to output XSD 
        /// </summary>
        /// <param name="inputDate">The DateTime instance.</param>
        /// <returns>A formatted string represenation of the DateTime.</returns>
        public static string ToAmazonDateTime(this DateTime inputDate)
        {
            // return XmlConvert.ToString(inputDate);
            return inputDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }
}
