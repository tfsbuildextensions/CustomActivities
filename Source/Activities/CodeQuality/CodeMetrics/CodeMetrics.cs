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
    using System.Threading;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Services;
    using TfsBuildExtensions.Activities.CodeMetrics.Extended;
    using TfsBuildExtensions.Activities.CodeQuality.Extended;

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
        private InArgument<bool> analyzeMetricsResult = true;

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
        /// Optional: Enables/Disables analysis of code metrics results. Default true
        /// </summary>
        [Description("Optional: Enables/Disables analysis of code metrics results. Default true")]
        public InArgument<bool> AnalyzeMetricsResult
        {
            get { return this.analyzeMetricsResult; }
            set { this.analyzeMetricsResult = value; }
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
        public InArgument<string> AssemblyThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Namespace Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Namespace Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> NamespaceThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Type Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> TypeThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Description("Optional: Overrides the global thresholds for the Member Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> MemberThresholdsString { get; set; }

        /// <summary>
        /// Gets or sets ta value indicating if code metrics should be logged.
        /// </summary>
        public InArgument<bool> LogCodeMetrics { get; set; }

        /// <summary>
        /// Optional: Indicates whether to ignore elements with the GeneratedCode attribute. Default false
        /// </summary>
        [Description("Indicates whether to ignore elements with the GeneratedCode attribute. Default false")]
        public InArgument<bool> IgnoreGeneratedCode { get; set; }

        private IBuildDetail BuildDetail { get; set; }

        /// <summary>
        /// Path to Program Files environment directory.
        /// </summary>
        /// <returns>Path to Program Files directory (C:\Program Files or C:\Program Files (x86)).</returns>
        public static string ProgramFilesX86()
        {
            if (8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

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

            if (!this.AnalyzeMetricsResult.Get(this.ActivityContext))
            {
                AddTextNode("Skipped code metrics analysis", currentTracking.Node);
                return;
            }

            IBuildInformationNode rootNode = AddTextNode("Analyzing code metrics results", currentTracking.Node);

            string fileName = Path.GetFileName(generatedFile);
            string pathToFileInDropFolder = Path.Combine(this.BuildDetail.DropLocation, fileName);
            AddLinkNode(fileName, new Uri(pathToFileInDropFolder), rootNode);

            CodeMetricsReport result = CodeMetricsReport.LoadFromFile(generatedFile);
            if (result == null)
            {
                LogBuildMessage("Could not load metric result file ");
                return;
            }

            // Get thresholds for each level.
            SpecificMetricThresholds assemblyMetricThresholds = CodeMetricsThresholds.GetForAssembly(this, this.ActivityContext);
            SpecificMetricThresholds namespaceMetricThresholds = CodeMetricsThresholds.GetForNamespace(this, this.ActivityContext);
            SpecificMetricThresholds typeMetricThresholds = CodeMetricsThresholds.GetForType(this, this.ActivityContext);
            SpecificMetricThresholds memberMetricThresholds = CodeMetricsThresholds.GetForMember(this, this.ActivityContext);

            // Check if metrics should be logged.
            bool logCodeMetrics = this.ActivityContext.GetValue(this.LogCodeMetrics);

            foreach (var target in result.Targets)
            {
                var targetNode = logCodeMetrics ? AddTextNode("Target: " + target.Name, rootNode) : null;
                foreach (var module in target.Modules)
                {
                    var moduleNode = logCodeMetrics ? AddTextNode("Module: " + module.Name, targetNode) : null;
                    this.ProcessMetrics(module.Name, module.Metrics, moduleNode, assemblyMetricThresholds);
                    foreach (var ns in module.Namespaces)
                    {
                        var namespaceNode = logCodeMetrics ? AddTextNode("Namespace: " + ns.Name, moduleNode) : null;
                        this.ProcessMetrics(ns.Name, ns.Metrics, namespaceNode, namespaceMetricThresholds);
                        foreach (var type in ns.Types)
                        {
                            var typeNode = logCodeMetrics ? AddTextNode("Type: " + type.Name, namespaceNode) : null;
                            this.ProcessMetrics(type.Name, type.Metrics, typeNode, typeMetricThresholds);
                            foreach (var member in type.Members)
                            {
                                var memberNode = logCodeMetrics ? AddTextNode("Member: " + member.Name, typeNode) : null;
                                this.ProcessMetrics(member.Name, member.Metrics, memberNode, memberMetricThresholds, type.Name);
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

        private static string GetMemberRootForOutput(string memberRootDesc)
        {
            if (!string.IsNullOrWhiteSpace(memberRootDesc))
            {
                return string.Format(" [Root:{0}]", memberRootDesc);
            }

            return string.Empty;
        }

        private bool RunCodeMetrics(string output)
        {
            string metricsExePath = Path.Combine(ProgramFilesX86(), @"Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\metrics.exe");
            if (!File.Exists(metricsExePath))
            {
                LogBuildError("Could not locate " + metricsExePath + ". Please download Visual Studio Code Metrics PowerTool 10.0 at http://www.microsoft.com/downloads/en/details.aspx?FamilyID=edd1dfb0-b9fe-4e90-b6a6-5ed6f6f6e615");
                return false;
            }

            string metricsExeArguments = this.GetFilesToProcess().Aggregate(string.Empty, (current, file) => current + string.Format(" /f:\"{0}\"", file));
            metricsExeArguments += string.Format(" /out:\"{0}\"", output);
            if (this.SearchGac.Get(this.ActivityContext))
            {
                metricsExeArguments += string.Format(" /searchgac");
            }

            if (this.IgnoreGeneratedCode.Get(this.ActivityContext))
            {
                metricsExeArguments += " /ignoregeneratedcode";
            }

            ProcessStartInfo psi = new ProcessStartInfo { FileName = metricsExePath, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, Arguments = metricsExeArguments, WorkingDirectory = this.BinariesDirectory.Get(this.ActivityContext) };
            this.LogBuildMessage("Running " + psi.FileName + " " + psi.Arguments);

            using (Process process = Process.Start(psi))
            {
                using (ManualResetEvent mreOut = new ManualResetEvent(false), mreErr = new ManualResetEvent(false))
                {
                    process.OutputDataReceived += (o, e) =>
                                                      {
                                                          if (e.Data == null)
                                                          {
                                                              mreOut.Set();
                                                          }
                                                          else
                                                          {
                                                              LogBuildMessage(e.Data);
                                                          }
                                                      };
                    process.BeginOutputReadLine();
                    process.ErrorDataReceived += (o, e) =>
                                                     {
                                                         if (e.Data == null) 
                                                         {
                                                             mreErr.Set(); 
                                                         } 
                                                         else
                                                         {
                                                             LogBuildMessage(e.Data);
                                                         }
                                                     };
                    process.BeginErrorReadLine();
                    process.StandardInput.Close();
                    process.WaitForExit();

                    mreOut.WaitOne();
                    mreErr.WaitOne();

                    if (process.ExitCode != 0)
                    {
                        this.LogBuildError(process.ExitCode.ToString(CultureInfo.CurrentCulture));
                        return false;
                    }

                    if (!File.Exists(output))
                    {
                        LogBuildError("Could not locate file " + output);
                        return false;
                    }
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
            this.ProcessMetrics(member, metrics, parent, thresholds, string.Empty);
        }

        /// <summary>
        /// Analyzes the resulting metrics file and compares the Maintainability Index and Cyclomatic Complexity against the threshold values
        /// </summary>
        /// <param name="member">Name of the member (namespace, module, type...)</param>
        /// <param name="metrics">The metrics for this member</param>
        /// <param name="parent">The parent node in the build log</param>
        /// <param name="thresholds"> The thresholds for this level, member</param>
        /// <param name="memberRootDesc">The memberRootDesc</param>
        private void ProcessMetrics(string member, IEnumerable<Metric> metrics, IBuildInformationNode parent, SpecificMetricThresholds thresholds, string memberRootDesc)
        {
            foreach (var metric in metrics)
            {
                int metricValue;
                if (metric != null && !string.IsNullOrEmpty(metric.Value) && int.TryParse(metric.Value, out metricValue))
                {
                    if (metric.Name == MaintainabilityIndex && metricValue < thresholds.MaintainabilityIndexErrorThreshold)
                    {
                        LogBuildError(string.Format("{0} for {1} is {2} which is below threshold ({3}){4}", MaintainabilityIndex, member, metric.Value, thresholds.MaintainabilityIndexErrorThreshold, GetMemberRootForOutput(memberRootDesc)));
                        if (this.FailBuildOnError.Get(this.ActivityContext))
                        {
                            this.FailCurrentBuild();
                        }
                    }

                    if (metric.Name == MaintainabilityIndex && metricValue < thresholds.MaintainabilityIndexWarningThreshold)
                    {
                        LogBuildWarning(string.Format("{0} for {1} is {2} which is below threshold ({3}){4}", MaintainabilityIndex, member, metric.Value, thresholds.MaintainabilityIndexWarningThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    if (metric.Name == CyclomaticComplexity && metricValue > thresholds.CyclomaticComplexityErrorThreshold)
                    {
                        this.LogBuildError(string.Format("{0} for {1} is {2} which is above threshold ({3}){4}", CyclomaticComplexity, member, metric.Value, thresholds.CyclomaticComplexityErrorThreshold, GetMemberRootForOutput(memberRootDesc)));
                        if (this.FailBuildOnError.Get(this.ActivityContext))
                        {
                            this.FailCurrentBuild();
                        }
                    }

                    if (metric.Name == CyclomaticComplexity && metricValue > thresholds.CyclomaticComplexityWarningThreshold)
                    {
                        this.LogBuildWarning(string.Format("{0} for {1} is {2} which is above threshold ({3}){4}", CyclomaticComplexity, member, metric.Value, thresholds.CyclomaticComplexityWarningThreshold, GetMemberRootForOutput(memberRootDesc)));
                    }

                    if (this.LogCodeMetrics.Get(this.ActivityContext))
                    {
                        AddTextNode(metric.Name + ": " + metric.Value, parent);
                    }
                }
            }
        }

        private void FailCurrentBuild()
        {
            this.BuildDetail.Status = BuildStatus.Failed;
            this.BuildDetail.Save();
        }
    }
}
