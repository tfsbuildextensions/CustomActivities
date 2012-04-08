//-----------------------------------------------------------------------
// <copyright file="SpecificMetricThresholds.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality.Extended
{
    using System;
    using System.Activities;

    /// <summary>
    /// Class for holding the pair of error and warning limits
    /// </summary>
    public class MetricThresholds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricThresholds"/> class
        /// Main Constructor for Metric thresholds class
        /// </summary>
        /// <param name="warning">Warning limit</param>
        /// <param name="error">Error limit</param>
        public MetricThresholds(int warning, int error)
        {
            this.Error = error;
            this.Warning = warning;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricThresholds"/> class
        /// Copy constructor for Metric thresholds class
        /// </summary>
        /// <param name="other">The class to copy from</param>
        public MetricThresholds(MetricThresholds other)
        {
            this.Error = other.Error;
            this.Warning = other.Warning;
        }

        /// <summary>
        /// Error limit
        /// </summary>
        public int Error { get; private set; }

        /// <summary>
        /// Warning limit
        /// </summary>
        public int Warning { get; private set; }

        /// <summary>
        /// Overloaded multiply operator to scale a metrics up (or down) by a given factor.  
        /// </summary>
        /// <param name="target">object to be scaled</param>
        /// <param name="factor">Scale factor</param>
        /// <returns>returns scaled object</returns>
        public static MetricThresholds operator *(MetricThresholds target, int factor)
        {
            target.Error *= factor;
            target.Warning *= factor;
            return target;
        }

        /// <summary>
        /// Named  multiply operator to scale a metrics up (or down) by a given factor.  
        /// </summary>
        /// <param name="target">object to be scaled</param>
        /// <param name="factor">Scale factor</param>
        /// <returns>returns scaled object</returns>
        public static MetricThresholds Multiply(MetricThresholds target, int factor)
        {
            target.Error *= factor;
            target.Warning *= factor;
            return target;
        }
    }

    /// <summary>
    /// Represents the specific set of thresholds for a metric's level.
    /// </summary>
    internal abstract class SpecificMetricThresholds
    {
        private MIMetricCheck miMetricCheck;
        private CCMetricCheck ccMetricCheck;
        private COMetricCheck coMetricCheck;
        private LOCMetricCheck locMetricCheck;
        private DOIMetricCheck doiMetricCheck;

        /// <summary>
        /// Threshold values for  Maintainability Index should fail or partially fail a build. Default is no thresholds, returning null
        /// </summary>
        internal virtual MetricThresholds MaintainabilityIndexThresholds
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should fail or partially fail a build. Default is no thresholds, returning null
        /// </summary>
        internal virtual MetricThresholds CyclomaticComplexityThresholds
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Threshold value for what Efferent Coupling should fail or partially fail a build. Default is no thresholds, returning null
        /// </summary>
        internal virtual MetricThresholds CouplingThresholds
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Threshold value for what Lines Of Code should fail or partially fail a build. Default is no thresholds, returning null
        /// </summary>
        internal virtual MetricThresholds LinesOfCodeThresholds
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Threshold value for what Depth of Inheritance should fail or partially fail a build. Default is no thresholds, returning null
        /// </summary>
        internal virtual MetricThresholds DepthOfInheritanceThresholds
        {
            get
            {
                return null;
            }
        }

        internal MIMetricCheck MIMetricChecker
        {
            get
            {
                return this.miMetricCheck ?? (this.miMetricCheck = new MIMetricCheck(this));
            }
        }

        internal CCMetricCheck CCMetricChecker
        {
            get
            {
                if (this.CyclomaticComplexityThresholds == null)
                {
                    return null;
                }

                return this.ccMetricCheck ?? (this.ccMetricCheck = new CCMetricCheck(this));
            }
        }

        internal COMetricCheck COMetricChecker
        {
            get
            {
                if (this.CouplingThresholds == null)
                {
                    return null;
                }

                return this.coMetricCheck ?? (this.coMetricCheck = new COMetricCheck(this));
            }
        }

        internal LOCMetricCheck LOCMetricChecker
        {
            get
            {
                if (this.LinesOfCodeThresholds == null)
                {
                    return null;
                }

                return this.locMetricCheck ?? (this.locMetricCheck = new LOCMetricCheck(this));
            }
        }

        internal DOIMetricCheck DOIMetricChecker
        {
            get
            {
                if (this.DepthOfInheritanceThresholds == null)
                {
                    return null;
                }

                return this.doiMetricCheck ?? (this.doiMetricCheck = new DOIMetricCheck(this));
            }
        }
    }

    /// <summary>
    /// Thresholds for assemblies
    /// There are no checks for any metrics for assemblies
    /// Placeholder code for extensions
    /// </summary>
    internal class AssemblyMetricsThresholds : SpecificMetricThresholds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyMetricsThresholds"/> class
        /// Assembly thresholds.  Currently blank, as we dont see usable values for this.  Placeholder for user extension code only
        /// </summary>
        /// <param name="activity">Code metrics activity, from where we pick off the incoming values for thresholds</param>
        public AssemblyMetricsThresholds(CodeMetrics activity)
        {
        }
    }

    /// <summary>
    /// Thresholds for namespaces.
    /// There are no check for any metrics on namespaces
    /// Placeholder code for extensions
    /// </summary>
    internal class NameSpaceMetricsThresholds : SpecificMetricThresholds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameSpaceMetricsThresholds"/> class
        /// Name space thresholds.  Currently blank, as we dont see usable values for this.  Placeholder for user extension code only
        /// </summary>
        /// <param name="activity">Code metrics activity, from where we pick off the incoming values for thresholds</param>
        public NameSpaceMetricsThresholds(CodeMetrics activity)
        {
        }
    }

    /// <summary>
    /// Thresholds for types
    /// </summary>
    internal class TypeMetricsThresholds : SpecificMetricThresholds
    {
        private const int CouplingFactor = 2; // How much larger the thresholds are for coupling of types
        private const int LinesOfCodeFactor = 25; // How much larger the thresholds are for lines of code of types

        private readonly MetricThresholds depthOfInheritanceThresholds;

        private readonly MetricThresholds couplingThresholds;

        private readonly MetricThresholds linesOfCodeThresholds;

        private readonly MethodMetricsThresholds methodMetricsThresholds;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMetricsThresholds"/> class
        /// Thresholds for Types (Classes)
        /// </summary>
        /// <param name="activity">Code metrics activity, from where we pick off the incoming values for thresholds</param>
        /// <param name="methodMetricsThresholds">The class thresholds depends on the method thresholds, so we need this first</param>
        internal TypeMetricsThresholds(CodeMetrics activity, MethodMetricsThresholds methodMetricsThresholds)
        {
            this.methodMetricsThresholds = methodMetricsThresholds;
            this.depthOfInheritanceThresholds = new MetricThresholds(activity.DepthOfInheritanceWarningThreshold.Get(activity.Context), activity.DepthOfInheritanceErrorThreshold.Get(activity.Context));
            this.couplingThresholds = new MetricThresholds(methodMetricsThresholds.CouplingThresholds);
            this.couplingThresholds *= CouplingFactor;
            this.linesOfCodeThresholds = new MetricThresholds(methodMetricsThresholds.LinesOfCodeThresholds);
            this.linesOfCodeThresholds *= LinesOfCodeFactor;
        }

        internal override MetricThresholds DepthOfInheritanceThresholds
        {
            get
            {
                return this.depthOfInheritanceThresholds;
            }
        }

        internal override MetricThresholds MaintainabilityIndexThresholds
        {
            get
            {
                return this.methodMetricsThresholds.MaintainabilityIndexThresholds;
            }
        }

        internal override MetricThresholds CouplingThresholds
        {
            get
            {
                return this.couplingThresholds;
            }
        }

        internal override MetricThresholds LinesOfCodeThresholds
        {
            get
            {
                return this.linesOfCodeThresholds;
            }
        }
    }

    /// <summary>
    /// Thresholds for methods
    /// </summary>
    internal class MethodMetricsThresholds : SpecificMetricThresholds
    {
        private readonly MetricThresholds maintainabilityindexThresholds;
        private readonly MetricThresholds cyclomaticComplexityThresholds;
        private readonly MetricThresholds couplingThresholds;
        private readonly MetricThresholds linesOfCodeThresholds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetricsThresholds"/> class
        /// Thresholds for methods in a class
        /// </summary>
        /// <param name="activity">Code metrics activity, from where we pick off the incoming values for thresholds</param>
        internal MethodMetricsThresholds(CodeMetrics activity)
        {
            this.maintainabilityindexThresholds = new MetricThresholds(activity.MaintainabilityIndexWarningThreshold.Get(activity.Context), activity.MaintainabilityIndexErrorThreshold.Get(activity.Context));
            this.cyclomaticComplexityThresholds = new MetricThresholds(activity.CyclomaticComplexityWarningThreshold.Get(activity.Context), activity.CyclomaticComplexityErrorThreshold.Get(activity.Context));
            this.couplingThresholds = new MetricThresholds(activity.CouplingWarningThreshold.Get(activity.Context), activity.CouplingErrorThreshold.Get(activity.Context));
            this.linesOfCodeThresholds = new MetricThresholds(activity.LinesOfCodeWarningThreshold.Get(activity.Context), activity.LinesOfCodeErrorThreshold.Get(activity.Context));
        }

        internal override MetricThresholds MaintainabilityIndexThresholds
        {
            get
            {
                return this.maintainabilityindexThresholds;
            }
        }

        internal override MetricThresholds CyclomaticComplexityThresholds
        {
            get
            {
                return this.cyclomaticComplexityThresholds;
            }
        }

        internal override MetricThresholds CouplingThresholds
        {
            get
            {
                return this.couplingThresholds;
            }
        }

        internal override MetricThresholds LinesOfCodeThresholds
        {
            get
            {
                return this.linesOfCodeThresholds;
            }
        }
    }
}