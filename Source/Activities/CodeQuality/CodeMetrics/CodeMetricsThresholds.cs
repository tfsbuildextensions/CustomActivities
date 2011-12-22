//-----------------------------------------------------------------------
// <copyright file="CodeMetricsThresholds.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Extended
{
    using System.Activities;

    /// <summary>
    /// Class to manage the various thresholds used by the CodeMetrics activity.
    /// </summary>
    internal static class CodeMetricsThresholds
    {
        public static SpecificMetricThresholds GetForAssembly(CodeMetrics activity, CodeActivityContext context)
        {
            var thresholds = GetThresholdsByLevel(activity, context).Assembly;

            return ApplyGlobalThresholdsWhenSpecificsMissing(thresholds, activity, context);
        }

        public static SpecificMetricThresholds GetForNamespace(CodeMetrics activity, CodeActivityContext context)
        {
            var thresholds = GetThresholdsByLevel(activity, context).Namespace;

            return ApplyGlobalThresholdsWhenSpecificsMissing(thresholds, activity, context);
        }

        public static SpecificMetricThresholds GetForType(CodeMetrics activity, CodeActivityContext context)
        {
            var thresholds = GetThresholdsByLevel(activity, context).Type;

            return ApplyGlobalThresholdsWhenSpecificsMissing(thresholds, activity, context);
        }

        public static SpecificMetricThresholds GetForMember(CodeMetrics activity, CodeActivityContext context)
        {
            var thresholds = GetThresholdsByLevel(activity, context).Member;

            return ApplyGlobalThresholdsWhenSpecificsMissing(thresholds, activity, context);
        }

        private static MetricThresholdsByLevel GetThresholdsByLevel(CodeMetrics activity, CodeActivityContext context)
        {
            var thresholds = new MetricThresholdsByLevel();

            thresholds.InitializeAssemblyThresholds(activity.AssemblyThresholdsString.Get(context));
            thresholds.InitializeNamespaceThresholds(activity.NamespaceThresholdsString.Get(context));
            thresholds.InitializeTypeThresholds(activity.TypeThresholdsString.Get(context));
            thresholds.InitializeMemberThresholds(activity.MemberThresholdsString.Get(context));
            return thresholds;
        }

        private static SpecificMetricThresholds ApplyGlobalThresholdsWhenSpecificsMissing(SpecificMetricThresholds thresholds, CodeMetrics activity, CodeActivityContext context)
        {
            thresholds.CyclomaticComplexityErrorThreshold = ReplaceWhenSpecificMissing(thresholds.CyclomaticComplexityErrorThreshold, activity.CyclomaticComplexityErrorThreshold, context);
            thresholds.CyclomaticComplexityWarningThreshold = ReplaceWhenSpecificMissing(thresholds.CyclomaticComplexityWarningThreshold, activity.CyclomaticComplexityWarningThreshold, context);
            thresholds.MaintainabilityIndexErrorThreshold = ReplaceWhenSpecificMissing(thresholds.MaintainabilityIndexErrorThreshold, activity.MaintainabilityIndexErrorThreshold, context);
            thresholds.MaintainabilityIndexWarningThreshold = ReplaceWhenSpecificMissing(thresholds.MaintainabilityIndexWarningThreshold, activity.MaintainabilityIndexWarningThreshold, context);
            return thresholds;
        }

        private static int ReplaceWhenSpecificMissing(int threshold, InArgument<int> globalThreshold, CodeActivityContext context)
        {
            return threshold <= 0 ? globalThreshold.Get(context) : threshold;
        }
    }
}
