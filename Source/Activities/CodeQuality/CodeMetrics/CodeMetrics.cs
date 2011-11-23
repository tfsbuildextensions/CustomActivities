//-----------------------------------------------------------------------
// <copyright file="CodeMetrics.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Services;
    using TfsBuildExtensions.Activities.CodeMetrics;

    /// <summary>
    /// Activity for processing code metrics using the Visual Studio Code Metrics PowerTool 10.0
    /// (http://www.microsoft.com/downloads/en/details.aspx?FamilyID=edd1dfb0-b9fe-4e90-b6a6-5ed6f6f6e615)
    /// <para/>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <!-- Run Code Metrics for SampleApp.exe and SampleLibrary.dll -->    
    /// <tac:CodeMetrics FailBuildOnError="{x:Null}" TreatWarningsAsErrors="{x:Null}" BinariesDirectory="[BinariesDirectory]" CyclomaticComplexityErrorThreshold="15" CyclomaticComplexityWarningThreshold="10" FilesToProcess="[New List(Of String)(New String() {&quot;SampleApp.exe&quot;, &quot;SampleLibrary.dll&quot;})]" GeneratedFileName="Metrics.xml" LogExceptionStack="True" MaintainabilityIndexErrorThreshold="40" MaintainabilityIndexWarningThreshold="90" />
    /// ]]></code>    
    /// </example>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class CodeMetrics : BaseCodeActivity
    {
        private const string MaintainabilityIndex = "MaintainabilityIndex";
        private const string CyclomaticComplexity = "CyclomaticComplexity";
        private InArgument<bool> searchGac = false;

        /// <summary>
        /// Path to where the binaries are placed
        /// </summary>
        [Description("Path to where the binaries are placed")]
        [RequiredArgument]
        public InArgument<string> BinariesDirectory { get; set; }

        /// <summary>
        /// Optional: Which files that should be processed. Can be a list of files or file match patterns. Defaults to *.dll;*.exe
        /// </summary>
        [Description("Which files that should be processed. Can be a list of files or file match patterns. Defaults to *.dll;*.exe")]
        public InArgument<IEnumerable<string>> FilesToProcess { get; set; }

        /// <summary>
        /// Optional: Which files that should be ignored. Can be a list of files or file match patterns.
        /// </summary>
        [Description("Which files that should be ignored. Can be a list of files or file match patterns.")]
        public InArgument<IEnumerable<string>> FilesToIgnore { get; set; }

        /// <summary>
        /// Optional: Name of the generated metrics result file. Should end with .xml
        /// </summary>
        [Description("Optional: Name of the generated metrics result file. Default Metrics.xml")]
        public InArgument<string> GeneratedFileName { get; set; }

        /// <summary>
        /// Optional: Enables/Disables searchGac. Default false
        /// </summary>
        [Description("Optional: Enables/Disables searchGac. Default false")]
        public InArgument<bool> SearchGac
        {
            get { return this.searchGac; }
            set { this.searchGac = value; }
        }

        /// <summary>
        /// Threshold value for what Maintainability Index should fail the build
        /// </summary>
        [RequiredArgument]
        public InArgument<int> MaintainabilityIndexErrorThreshold { get; set; }

        /// <summary>
        /// Threshold value for what Maintainability Index should partially fail the build
        /// </summary>
        [RequiredArgument]
        public InArgument<int> MaintainabilityIndexWarningThreshold { get; set; }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should fail the build
        /// </summary>
        [RequiredArgument]
        public InArgument<int> CyclomaticComplexityErrorThreshold { get; set; }
                               

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should partially fail the build
        /// </summary>
        [RequiredArgument]
        public InArgument<int> CyclomaticComplexityWarningThreshold { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Assembly Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<String> AssemblyThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Namespace Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Namespace Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<String> NamespaceThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Type Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<String> TypeThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Member Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<String> MemberThresholdsString { get; set; }
         
        private IBuildDetail BuildDetail { get; set; }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            this.BuildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            string generatedFile = "Metrics.xml";
            if (this.GeneratedFileName != null && !string.IsNullOrEmpty(this.GeneratedFileName.Get(this.ActivityContext)))
            {
                generatedFile = Path.Combine(this.BuildDetail.DropLocation, this.GeneratedFileName.Get(this.ActivityContext));
            }

            if (!this.RunCodeMetrics(generatedFile))
            {
                return;
            }

            IActivityTracking currentTracking = this.ActivityContext.GetExtension<IBuildLoggingExtension>().GetActivityTracking(this.ActivityContext);
            IBuildInformationNode rootNode = AddTextNode("Processing metrics", currentTracking.Node);

            string fileName = Path.GetFileName(generatedFile);
            string pathToFileInDropFolder = Path.Combine(this.BuildDetail.DropLocation, fileName);
            AddLinkNode(fileName, new Uri(pathToFileInDropFolder), rootNode);

            CodeMetricsReport result = CodeMetricsReport.LoadFromFile(generatedFile);
            if (result == null)
            {
                LogBuildMessage("Could not load metric result file ");
                return;
            }

            foreach (var target in result.Targets)
            {
                var targetNode = AddTextNode("Target: " + target.Name, rootNode);
                foreach (var module in target.Modules)
                {
                    var moduleNode = AddTextNode("Module: " + module.Name, targetNode);
                    this.ProcessMetrics(module.Name, module.Metrics, moduleNode, CodeMetricsThresholds.GetForAssembly(this, this.ActivityContext));
                    foreach (var ns in module.Namespaces)
                    {
                        var namespaceNode = AddTextNode("Namespace: " + ns.Name, moduleNode);
                        this.ProcessMetrics(ns.Name, ns.Metrics, namespaceNode, CodeMetricsThresholds.GetForNamespace(this, this.ActivityContext));
                        foreach (var type in ns.Types)
                        {
                            var typeNode = AddTextNode("Type: " + type.Name, namespaceNode);
                            this.ProcessMetrics(type.Name, type.Metrics, typeNode, CodeMetricsThresholds.GetForType(this, this.ActivityContext)); 
                            foreach (var member in type.Members)
                            {
                                var memberNode = AddTextNode("Member: " + member.Name, typeNode);
                                this.ProcessMetrics(member.Name, member.Metrics, memberNode, CodeMetricsThresholds.GetForMember(this, this.ActivityContext), type.Name); 
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Override for base.CacheMetadata
        /// </summary>
        /// <param name="metadata">CodeActivityMetadata</param>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension(typeof(IBuildDetail));
        }

        /// <summary>
        /// Path to Program Files environment directory.
        /// </summary>
        /// <returns>Path to Program Files directory (C:\Program Files or C:\Program Files (x86)).</returns>
        public static string ProgramFilesX86()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private bool RunCodeMetrics(string output)
        {
            string metricsExePath = Path.Combine(ProgramFilesX86(), @"Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\metrics.exe");
            if (!File.Exists(metricsExePath))
            {
                LogBuildError("Could not locate " + metricsExePath + ". Please download Visual Studio Code Metrics PowerTool 10.0 at http://www.microsoft.com/downloads/en/details.aspx?FamilyID=edd1dfb0-b9fe-4e90-b6a6-5ed6f6f6e615");
                return false;
            }

            string metricsExeArguments = GetFilesToProcess().Aggregate(string.Empty, (current, file) => current + string.Format(" /f:\"{0}\"", file));
            metricsExeArguments += string.Format(" /out:\"{0}\"", output);
            if (this.SearchGac.Get(this.ActivityContext))
            {
                metricsExeArguments += string.Format(" /searchgac");
            }

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = metricsExePath;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = metricsExeArguments;
                proc.StartInfo.WorkingDirectory = BinariesDirectory.Get(this.ActivityContext);
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                if (outputStream.Length > 0)
                {
                    this.LogBuildMessage(outputStream);
                }

                string errorStream = proc.StandardError.ReadToEnd();
                if (errorStream.Length > 0)
                {
                    this.LogBuildError(errorStream);
                }

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    this.LogBuildError(proc.ExitCode.ToString(CultureInfo.CurrentCulture));
                    return false;
                }

                if (!File.Exists(output))
                {
                    LogBuildError("Could not locate file " + output);
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<string> GetFilesToProcess()
        {
            var metricsFiles = new CodeMetricsFilesToProcess(this, this.ActivityContext);
            return metricsFiles.Get();
        }

        /// <summary>
        /// Analyzes the resulting metrics file and compares the Maintainability Index and Cyclomatic Complexity against the threshold values
        /// </summary>
        /// <param name="member">Name of the member (namespace, module, type...)</param>
        /// <param name="metrics">The metrics for this member</param>
        /// <param name="parent">The parent node in the build log</param>
        /// <param name="thresholds"> The thresholds for this level, member</param>
        private void ProcessMetrics(string member, IEnumerable<Metric> metrics, IBuildInformationNode parent, SpecificMetricThresholds thresholds)
        {
            ProcessMetrics(member, metrics, parent, thresholds, string.Empty);
        }

        /// <summary>
        /// Analyzes the resulting metrics file and compares the Maintainability Index and Cyclomatic Complexity against the threshold values
        /// </summary>
        /// <param name="member">Name of the member (namespace, module, type...)</param>
        /// <param name="metrics">The metrics for this member</param>
        /// <param name="parent">The parent node in the build log</param>
        /// <param name="thresholds"> The thresholds for this level, member</param>
        /// <param name="memberRootDesc"></param>
        private void ProcessMetrics(string member, IEnumerable<Metric> metrics, IBuildInformationNode parent, SpecificMetricThresholds thresholds, string memberRootDesc)
        {
            foreach (var metric in metrics)
            {
                int metricValue;
                if (metric != null && !string.IsNullOrEmpty(metric.Value) && int.TryParse(metric.Value, out metricValue))
                {
                    if (metric.Name == MaintainabilityIndex && Convert.ToInt32(metric.Value) < thresholds.MaintainabilityIndexErrorThreshold)
                    {
                        this.FailCurrentBuild();
                        LogBuildError(string.Format("{0} for {1} is {2} which is below threshold ({3}){4}", MaintainabilityIndex, member, metric.Value, thresholds.MaintainabilityIndexErrorThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    if (metric.Name == MaintainabilityIndex && metricValue < thresholds.MaintainabilityIndexWarningThreshold)
                    {
                        this.PartiallyFailCurrentBuild();
                        LogBuildError(string.Format("{0} for {1} is {2} which is below threshold ({3}){4}", MaintainabilityIndex, member, metric.Value, thresholds.MaintainabilityIndexWarningThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    if (metric.Name == CyclomaticComplexity && metricValue > thresholds.CyclomaticComplexityErrorThreshold)
                    {
                        this.FailCurrentBuild();
                        this.LogBuildError(string.Format("{0} for {1} is {2} which is above threshold ({3}){4}", CyclomaticComplexity, member, metric.Value, thresholds.CyclomaticComplexityErrorThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    if (metric.Name == CyclomaticComplexity && metricValue > thresholds.CyclomaticComplexityWarningThreshold)
                    {
                        this.PartiallyFailCurrentBuild();
                        this.LogBuildError(string.Format("{0} for {1} is {2} which is above threshold ({3}){4}", CyclomaticComplexity, member, metric.Value, thresholds.CyclomaticComplexityWarningThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    AddTextNode(metric.Name + ": " + metric.Value, parent);
                }
            }
        }

        private string GetMemberRootForOutput(string memberRootDesc)
        {
            if (!string.IsNullOrWhiteSpace(memberRootDesc))
                return string.Format(" [Root:{0}]", memberRootDesc);
            return string.Empty;
        }

        private void PartiallyFailCurrentBuild()
        {
            this.BuildDetail.Status = BuildStatus.PartiallySucceeded;
            this.BuildDetail.Save();
        }

        private void FailCurrentBuild()
        {
            this.BuildDetail.Status = BuildStatus.Failed;
            this.BuildDetail.Save();
        }
    }
}
