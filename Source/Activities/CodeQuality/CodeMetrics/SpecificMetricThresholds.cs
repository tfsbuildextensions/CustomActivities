//-----------------------------------------------------------------------
// <copyright file="SpecificMetricThresholds.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality
{
    /// <summary>
    /// Represents the specific thresholds for a metric's level.
    /// </summary>
    public class SpecificMetricThresholds
    {
        /// <summary>
        /// Threshold value for what Maintainability Index should fail the build
        /// </summary>
        public int MaintainabilityIndexErrorThreshold { get; set; }

        /// <summary>
        /// Threshold value for what Maintainability Index should partially fail the build
        /// </summary>
        public int MaintainabilityIndexWarningThreshold { get; set; }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should fail the build
        /// </summary>
        public int CyclomaticComplexityErrorThreshold { get; set; }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should partially fail the build
        /// </summary>
        public int CyclomaticComplexityWarningThreshold { get; set; }
    }
}