//-----------------------------------------------------------------------
// <copyright file="MetricCheckClasses.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.CodeMetrics.Extended;
    using TfsBuildExtensions.Activities.CodeQuality.Extended;

    internal abstract class MetricCheck
    {
        protected MetricCheck(MetricThresholds thresholdvalues)
        {
            this.WarningLimit = thresholdvalues.Warning;
            this.ErrorLimit = thresholdvalues.Error;
        }

        internal abstract string Name { get; }

        internal string Format
        {
            get
            {
                return this.Name + " for {0} is {1} which is " + this.AboveOrBelow + " {2} threshold of {3}";
            }
        }

        internal int WarningLimit { get; private set; }

        internal int ErrorLimit { get; private set; }

        protected abstract string AboveOrBelow { get; }

        internal abstract bool Compare(int value, int limit);

        internal int LimitThatFailed(BuildStatus status)
        {
            return (status == BuildStatus.Failed) ? this.ErrorLimit : this.WarningLimit;
        }

        internal bool CheckWarning(int value)
        {
            return this.Compare(value, this.WarningLimit);
        }

        internal bool CheckError(int value)
        {
            return this.Compare(value, this.ErrorLimit);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "*", Justification = "Fine in this case")]
    internal class MIMetricCheck : MetricCheck
    {
        internal MIMetricCheck(SpecificMetricThresholds thresholds)
            : base(thresholds.MaintainabilityIndexThresholds)
        {
        }

        internal override string Name
        {
            get
            {
                return "MaintainabilityIndex";
            }
        }

        protected override string AboveOrBelow
        {
            get
            {
                return "below";
            }
        }

        internal override bool Compare(int value, int limit)
        {
            return value < limit;
        }
    }

    internal abstract class HigherIsWorseMetric : MetricCheck
    {
        protected HigherIsWorseMetric(MetricThresholds thresholdvalues)
            : base(thresholdvalues)
        {
        }

        protected override string AboveOrBelow
        {
            get
            {
                return "above";
            }
        }

        internal override bool Compare(int value, int limit)
        {
            return value > limit;
        }
    }

    internal class CCMetricCheck : HigherIsWorseMetric
    {
        internal CCMetricCheck(SpecificMetricThresholds thresholds)
            : base(thresholds.CyclomaticComplexityThresholds)
        {
        }

        internal override string Name
        {
            get
            {
                return "CyclomaticComplexity";
            }
        }
    }

    internal class COMetricCheck : HigherIsWorseMetric
    {
        internal COMetricCheck(SpecificMetricThresholds thresholds)
            : base(thresholds.CouplingThresholds)
        {
        }

        internal override string Name
        {
            get
            {
                return "Coupling";
            }
        }
    }

    internal class LOCMetricCheck : HigherIsWorseMetric
    {
        internal LOCMetricCheck(SpecificMetricThresholds thresholds)
            : base(thresholds.LinesOfCodeThresholds)
        {
        }

        internal override string Name
        {
            get
            {
                return "LinesOfCode";
            }
        }
    }

    internal class DOIMetricCheck : HigherIsWorseMetric
    {
        internal DOIMetricCheck(SpecificMetricThresholds thresholds)
            : base(thresholds.DepthOfInheritanceThresholds)
        {
        }

        internal override string Name
        {
            get
            {
                return "DepthOfInheritance";
            }
        }
    }

    internal class MemberInformation
    {
        internal MemberInformation(Member member, Type type, Namespace namespacet, Module module)
        {
            this.TheMember = member;
            this.TheClass = type;
            this.TheNamespace = namespacet;
            this.TheModule = module;
        }

        internal Module TheModule { get; private set; }

        internal Namespace TheNamespace { get; private set; }

        internal Type TheClass { get; private set; }

        internal Member TheMember { get; private set; }

        internal string FullyQualifiedName
        {
            get
            {
                var memberName = this.TheMember != null ? "." + this.TheMember.Name : string.Empty;
                var typename = this.TheClass != null ? "." + this.TheClass.Name : string.Empty;
                var namespacename = this.TheNamespace != null ? this.TheNamespace.Name : string.Empty;
                return this.TheModule.Name + ":" + namespacename + typename + memberName;
            }
        }
    }
}
