//-----------------------------------------------------------------------
// <copyright file="MetricThresholdsByLevel.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
using System;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    /// <summary>
    /// Represents the specific thresholds by each metric's level.
    /// </summary>
    public class MetricThresholdsByLevel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MetricThresholdsByLevel()
        {
            Assembly = new SpecificMetricThresholds();
            Namespace = new SpecificMetricThresholds();
            Type = new SpecificMetricThresholds();
            Member = new SpecificMetricThresholds();
        }
        /// <summary>
        /// Specific Thresholds for the assembly metrics
        /// </summary>
        public SpecificMetricThresholds Assembly { get; set; }
        /// <summary>
        /// Specific Thresholds for the Namespace metrics
        /// </summary>
        public SpecificMetricThresholds Namespace { get; set; }
        /// <summary>
        /// Specific Thresholds for the Type metrics
        /// </summary>
        public SpecificMetricThresholds Type { get; set; }
        /// <summary>
        /// Specific Thresholds for the Member metrics
        /// </summary>
        public SpecificMetricThresholds Member { get; set; }

        /// <summary>
        /// Initialize the metrics thresholds for the assembly level with a string.<para/>
        /// </summary>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        /// 
        /// <param name="thresholdsString">
        /// String with the following format "9999;9999;9999;9999"<para/>
        /// Four integer separated by a semicolon.  A default (ignored) value is 0<para/>
        /// Each number represents an Assembly Threshold.<para/>
        /// 1st:    MaintainabilityIndexErrorThreshold<para/>
        /// 2nd:    MaintainabilityIndexWarningThreshold<para/>
        /// 3rd:    CyclomaticComplexityErrorThreshold<para/>
        /// 4th:    CyclomaticComplexityWarningThreshold<para/>
        /// </param>
        public void InitializeAssemblyThresholds(string thresholdsString)
        {
            InitializeThresholds(thresholdsString, Assembly);
        }

        /// <summary>
        /// Initialize the metrics thresholds for the namespace level with a string.<para/>
        /// </summary>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        /// 
        /// <param name="thresholdsString">
        /// String with the following format "9999;9999;9999;9999"<para/>
        /// Four integer separated by a semicolon.  A default (ignored) value is 0<para/>
        /// Each number represents an Assembly Threshold.<para/>
        /// 1st:    MaintainabilityIndexErrorThreshold<para/>
        /// 2nd:    MaintainabilityIndexWarningThreshold<para/>
        /// 3rd:    CyclomaticComplexityErrorThreshold<para/>
        /// 4th:    CyclomaticComplexityWarningThreshold<para/>
        /// </param>
        public void InitializeNamespaceThresholds(string thresholdsString)
        {
            InitializeThresholds(thresholdsString, Namespace);
        }

        /// <summary>
        /// Initialize the metrics thresholds for the type level with a string.<para/>
        /// </summary>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        /// 
        /// <param name="thresholdsString">
        /// String with the following format "9999;9999;9999;9999"<para/>
        /// Four integer separated by a semicolon.  A default (ignored) value is 0<para/>
        /// Each number represents an Assembly Threshold.<para/>
        /// 1st:    MaintainabilityIndexErrorThreshold<para/>
        /// 2nd:    MaintainabilityIndexWarningThreshold<para/>
        /// 3rd:    CyclomaticComplexityErrorThreshold<para/>
        /// 4th:    CyclomaticComplexityWarningThreshold<para/>
        /// </param>
        public void InitializeTypeThresholds(string thresholdsString)
        {
            InitializeThresholds(thresholdsString, Type);
        }

        /// <summary>
        /// Initialize the metrics thresholds for the Member level with a string.<para/>
        /// </summary>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        /// 
        /// <param name="thresholdsString">
        /// String with the following format "9999;9999;9999;9999"<para/>
        /// Four integer separated by a semicolon.  A default (ignored) value is 0<para/>
        /// Each number represents an Assembly Threshold.<para/>
        /// 1st:    MaintainabilityIndexErrorThreshold<para/>
        /// 2nd:    MaintainabilityIndexWarningThreshold<para/>
        /// 3rd:    CyclomaticComplexityErrorThreshold<para/>
        /// 4th:    CyclomaticComplexityWarningThreshold<para/>
        /// </param>
        public void InitializeMemberThresholds(string thresholdsString)
        {
            InitializeThresholds(thresholdsString, Member);
        }

        private void InitializeThresholds(string thresholdsString, SpecificMetricThresholds levelThresholds)
        {
            if (string.IsNullOrWhiteSpace(thresholdsString))
                ResetThresholds(levelThresholds);
            else
            {
                var values = thresholdsString.Split(';');

                levelThresholds.MaintainabilityIndexErrorThreshold = ConvertThreshold(values[0]);
                levelThresholds.MaintainabilityIndexWarningThreshold = ConvertThreshold(values[1]);
                levelThresholds.CyclomaticComplexityErrorThreshold = ConvertThreshold(values[2]);
                levelThresholds.CyclomaticComplexityWarningThreshold = ConvertThreshold(values[3]);
            }
        }

        private static int ConvertThreshold(string value)
        {
            Int32 threshold;

            if (Int32.TryParse(value, out threshold))
                return threshold;
            throw new ArgumentOutOfRangeException("value", value, "The value cannot be converted in integer value");
        }

        private void ResetThresholds(SpecificMetricThresholds thresholds)
        {
            thresholds.CyclomaticComplexityErrorThreshold = 0;
            thresholds.CyclomaticComplexityWarningThreshold = 0;
            thresholds.MaintainabilityIndexErrorThreshold = 0;
            thresholds.MaintainabilityIndexWarningThreshold = 0;
        }
    }
}