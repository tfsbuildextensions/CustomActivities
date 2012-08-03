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
        private const string ClassCoupling = "ClassCoupling";
        private const string DepthOfInheritance = "DepthOfInheritance";
        private const string LinesOfCode = "LinesOfCode";

        private InArgument<bool> searchGac = false;
        private InArgument<bool> analyzeMetricsResult = true;
        private InArgument<string> generatedFileName = "Metrics.xml";
        private InArgument<int> maintainabilityIndexWarningThreshold = 40;
        private InArgument<int> maintainabilityIndexErrorThreshold = 20;
        private InArgument<int> cyclomaticComplexityErrorThreshold = 40;
        private InArgument<int> cyclomaticComplexityWarningThreshold = 20;
        private InArgument<int> couplingWarningThreshold = 20;
        private InArgument<int> couplingErrorThreshold = 40;
        private InArgument<int> linesOfCodeWarningThreshold = 20;
        private InArgument<int> linesOfCodeErrorThreshold = 40;
        private InArgument<int> depthOfInheritanceWarningThreshold = 5;
        private InArgument<int> depthOfInheritanceErrorThreshold = 10;
        private InArgument<bool> ignoreGeneratedCode = true;
        private InArgument<bool> logCodeMetricsArg = false;

        private bool logCodeMetrics;

        private delegate void LogToBuild(string message);

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
        public InArgument<string> GeneratedFileName
        {
            get
            {
                return this.generatedFileName;
            }

            set
            {
                this.generatedFileName = value;
            }
        }

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
        [Description("Sets threshold for MaintainabilityIndex Error, default = 20")]
        public InArgument<int> MaintainabilityIndexErrorThreshold
        {
            get
            {
                return this.maintainabilityIndexErrorThreshold;
            }

            set
            {
                this.maintainabilityIndexErrorThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Maintainability Index should partially fail the build on methods and types
        /// </summary>
        [RequiredArgument]
        [Description("Sets threshold for MaintainabilityIndex Warning, default = 40")]
        public InArgument<int> MaintainabilityIndexWarningThreshold
        {
            get
            {
                return this.maintainabilityIndexWarningThreshold;
            }

            set
            {
                this.maintainabilityIndexWarningThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should fail the build on methods
        /// </summary>
        [RequiredArgument]
        [Description("Sets threshold for Cyclomatic Complexity Error, default = 40")]
        public InArgument<int> CyclomaticComplexityErrorThreshold
        {
            get
            {
                return this.cyclomaticComplexityErrorThreshold;
            }

            set
            {
                this.cyclomaticComplexityErrorThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Cyclomatic Complexity should partially fail the build on methods
        /// </summary>
        [RequiredArgument]
        [Description("Sets threshold for Cyclomatic Complexity Warning, default = 20")]
        public InArgument<int> CyclomaticComplexityWarningThreshold
        {
            get
            {
                return this.cyclomaticComplexityWarningThreshold;
            }

            set
            {
                this.cyclomaticComplexityWarningThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Coupling should partially fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Class Coupling warning, default = 20")]
        public InArgument<int> CouplingWarningThreshold
        {
            get
            {
                return this.couplingWarningThreshold;
            }

            set
            {
                this.couplingWarningThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Coupling should fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Class Coupling error, default = 40")]
        public InArgument<int> CouplingErrorThreshold
        {
            get
            {
                return this.couplingErrorThreshold;
            }

            set
            {
                this.couplingErrorThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Lines Of Code should partially fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Lines of Code, warning, default = 20")]
        public InArgument<int> LinesOfCodeWarningThreshold
        {
            get
            {
                return this.linesOfCodeWarningThreshold;
            }

            set
            {
                this.linesOfCodeWarningThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Lines Of Code should fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Lines of Code, error, default = 40")]
        public InArgument<int> LinesOfCodeErrorThreshold
        {
            get
            {
                return this.linesOfCodeErrorThreshold;
            }

            set
            {
                this.linesOfCodeErrorThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Depth Of Inheritance should partially fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Depth of Inheritance, warning, default = 5")]
        public InArgument<int> DepthOfInheritanceWarningThreshold
        {
            get
            {
                return this.depthOfInheritanceWarningThreshold;
            }

            set
            {
                this.depthOfInheritanceWarningThreshold = value;
            }
        }

        /// <summary>
        /// Threshold value for what Depth Of Inheritance should fail the build on methods
        /// </summary>
        [Description("Optional: Sets threshold for Depth of Inheritance, error, default = 10")]
        public InArgument<int> DepthOfInheritanceErrorThreshold
        {
            get
            {
                return this.depthOfInheritanceErrorThreshold;
            }

            set
            {
                this.depthOfInheritanceErrorThreshold = value;
            }
        }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Obsolete("Use specific arguments instead of this string based.This is now ignored in the code")]
        [Description("Optional: Overrides the global thresholds for the Assembly Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> AssemblyThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Namespace Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Obsolete("Use specific arguments instead of this string based.This is now ignored in the code")]
        [Description("Optional: Overrides the global thresholds for the Namespace Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> NamespaceThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Obsolete("Use specific arguments instead of this string based.This is now ignored in the code")]
        [Description("Optional: Overrides the global thresholds for the Type Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> TypeThresholdsString { get; set; }

        /// <summary>
        /// Overrides the global thresholds for the Assembly Metric Level by specific one.  
        /// When a level is not overrides (value of 0), the global thresholds are used.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Raise this exception if the string contains a part that is not an integer.
        /// </exception>
        [Obsolete("Use specific arguments instead of this string based. This is now ignored in the code")]
        [Description("Optional: Overrides the global thresholds for the Member Metric Level by specific one.  The expected format is 9999;9999;9999;9999 where the values are metric's thresholds for the Maintainability Index Error, Maintainability Index Warning, Cyclo Complexity Error and Cyclo Complexity Warning.")]
        public InArgument<string> MemberThresholdsString { get; set; }

        /// <summary>
        /// Gets or sets ta value indicating if code metrics should be logged in detail. Normally keep this false. 
        /// When enabled, all detailed metrics are logged, not only those that fails/warns. 
        /// </summary>
        public InArgument<bool> LogCodeMetrics
        {
            get
            {
                return this.logCodeMetricsArg;
            }

            set
            {
                this.logCodeMetricsArg = value;
            }
        }

        /// <summary>
        /// Optional: Indicates whether to ignore elements with the GeneratedCode attribute. Default true
        /// </summary>
        [Description("Indicates whether to ignore elements with the GeneratedCode attribute. Default true")]
        public InArgument<bool> IgnoreGeneratedCode
        {
            get
            {
                return this.ignoreGeneratedCode;
            }

            set
            {
                this.ignoreGeneratedCode = value;
            }
        }

        /// <summary>
        /// ActivityContext for use by Threshold logic
        /// </summary>
        internal CodeActivityContext Context
        {
            get
            {
                return this.ActivityContext;
            }
        }

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
            this.logCodeMetrics = this.LogCodeMetrics.Get(this.ActivityContext);
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
                BaseCodeActivity.AddTextNode("Skipped code metrics analysis (not set to run)", currentTracking.Node);
                return;
            }

            IBuildInformationNode rootNode = AddTextNode("Analyzing code metrics results", currentTracking.Node);

            string fileName = Path.GetFileName(generatedFile);
            string pathToFileInDropFolder = Path.Combine(this.BuildDetail.DropLocation, fileName);
            BaseCodeActivity.AddLinkNode(fileName, new Uri(pathToFileInDropFolder), rootNode);

            CodeMetricsReport result = CodeMetricsReport.LoadFromFile(generatedFile);
            if (result == null)
            {
                this.LogBuildMessage("Could not load metric result file ");
                return;
            }

            // Get thresholds for each level.
            var memberMetricThresholds = new MethodMetricsThresholds(this);
            var typeMetricThresholds = new TypeMetricsThresholds(this, memberMetricThresholds);

            // var namespaceMetricThresholds = new NameSpaceMetricsThresholds(this);  //Uncomment in this if you want to perform metric checks on namespaces
            // var assemblyMetricThresholds = new AssemblyMetricsThresholds(this);  // Uncomment in this if you want to perform metric checks on assemblies
            int noOfTypeViolations = 0;
            int noOfMethodViolations = 0;
            foreach (var target in result.Targets)
            {
                var targetNode = this.logCodeMetrics ? AddTextNode("Target: " + target.Name, rootNode) : null;
                foreach (var module in target.Modules)
                {
                    var moduleNode = this.logCodeMetrics ? AddTextNode("Module: " + module.Name, targetNode) : null;

                    foreach (var ns in module.Namespaces)
                    {
                        var namespaceNode = this.logCodeMetrics ? AddTextNode("Namespace: " + ns.Name, moduleNode) : null;

                        foreach (var type in ns.Types)
                        {
                            var typeNode = this.logCodeMetrics ? AddTextNode("Type: " + type.Name, namespaceNode) : null;
                            var typeInformation = new MemberInformation(null, type, ns, module);
                            noOfTypeViolations += this.ProcessMetrics(typeNode, typeInformation.TheClass.Metrics, typeMetricThresholds, typeInformation.FullyQualifiedName, typeInformation.TheClass.Name);
                            noOfMethodViolations += (from member in type.Members let memberNode = this.logCodeMetrics ? AddTextNode("Member: " + member.Name + " " + member.MetricsInformation, typeNode) : null let memberInformation = new MemberInformation(member, type, ns, module) select this.ProcessMetrics(memberNode, memberInformation.TheMember.Metrics, memberMetricThresholds, memberInformation.FullyQualifiedName, memberInformation.TheMember.Name)).Sum();
                        }
                    }
                }
            }

            var numberMessageTypes = string.Format("Number of Code Metric warnings/errors on types: {0}", noOfTypeViolations);
            var numberMessageMethods = string.Format("Number of Code Metric warnings/errors on methods: {0}", noOfMethodViolations);
            BaseCodeActivity.AddTextNode(numberMessageTypes, rootNode);
            BaseCodeActivity.AddTextNode(numberMessageMethods, rootNode);
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
        /// Analyzes the resulting metrics file and compares the metrics against the threshold values
        /// </summary>
        /// <param name="parent">The parent node in the build log</param>
        /// <param name="metrics">List of metric values</param>
        /// <param name="thresholds">Thresholds for errors and warnings </param>
        /// <param name="fullyQualifiedName">FQN for the method/type under test</param>
        /// <param name="currentName">Name of current method/type </param>
        /// <returns>Number if violations</returns>
        private int ProcessMetrics(IBuildInformationNode parent, IEnumerable<Metric> metrics, SpecificMetricThresholds thresholds, string fullyQualifiedName, string currentName)
        {
            var thecomplainlist = new List<string>();
            foreach (var metric in metrics.Where(metric => metric != null && !string.IsNullOrEmpty(metric.Value)))
            {
                switch (metric.Name)
                {
                    case MaintainabilityIndex:
                        this.CheckLimits(metric, thresholds.MIMetricChecker, fullyQualifiedName, thecomplainlist);
                        break;
                    case CyclomaticComplexity:
                        this.CheckLimits(metric, thresholds.CCMetricChecker, fullyQualifiedName, thecomplainlist);
                        break;
                    case ClassCoupling:
                        this.CheckLimits(metric, thresholds.COMetricChecker, fullyQualifiedName, thecomplainlist);
                        break;
                    case DepthOfInheritance:
                        this.CheckLimits(metric, thresholds.DOIMetricChecker, fullyQualifiedName, thecomplainlist);
                        break;
                    case LinesOfCode:
                        this.CheckLimits(metric, thresholds.LOCMetricChecker, fullyQualifiedName, thecomplainlist);
                        break;
                    default:
                        throw new UnknownMetricNameException(metric.Name);
                }
            }

            if (this.logCodeMetrics && parent != null && thecomplainlist.Count > 0)
            {
                var result = "Metrics for " + currentName + @"\n";
                thecomplainlist.ForEach(s => result += s + @"\n");
                BaseCodeActivity.AddTextNode(result, parent);
            }

            return thecomplainlist.Count;
        }

        private bool RunCodeMetrics(string output)
        {
            string metricsExePath = Path.Combine(ProgramFilesX86(), @"Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\metrics.exe");
            if (!File.Exists(metricsExePath))
            {
                this.LogBuildError("Could not locate " + metricsExePath + ". Please download Visual Studio Code Metrics PowerTool 10.0 at http://www.microsoft.com/downloads/en/details.aspx?FamilyID=edd1dfb0-b9fe-4e90-b6a6-5ed6f6f6e615");
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
                                                              this.LogBuildMessage(e.Data);
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
                                                             this.LogBuildMessage(e.Data);
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
                        this.LogBuildError("Could not locate file " + output);
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

        private void CheckLimits(Metric metric, MetricCheck metricCheck, string fullyQualifiedName, ICollection<string> thecomplainlist)
        {
            if (metricCheck == null)
            {
                return;
            }

            int metricValue;
            if (!int.TryParse(metric.Value, out metricValue))
            {
                return;
            }

            if (metricCheck.CheckError(metricValue))
            {
                this.CreateErrorLogs(metricCheck, metricValue, BuildStatus.Failed, this.LogBuildError, fullyQualifiedName, thecomplainlist);
            }
            else if (metricCheck.CheckWarning(metricValue))
            {
                this.CreateErrorLogs(metricCheck, metricValue, BuildStatus.PartiallySucceeded, this.LogBuildWarning, fullyQualifiedName, thecomplainlist);
            }
        }

        private void ChangeStatusCurrentBuild(BuildStatus newStatus)
        {
            this.BuildDetail.Status = newStatus;
            this.BuildDetail.Save();
        }

        private void CreateErrorLogs(MetricCheck metricCheck, int metricValue, BuildStatus status, LogToBuild logToBuild, string fullyQualifiedName, ICollection<string> thecomplainlist)
        {
            string typeFail = (status == BuildStatus.Failed) ? "error" : "warning";
            string message = string.Format(metricCheck.Format, fullyQualifiedName, metricValue, typeFail, metricCheck.LimitThatFailed(status));
            logToBuild(message);
            this.ChangeStatusCurrentBuild(status);
            thecomplainlist.Add(metricCheck.Name + ": " + metricValue + " fails on " + message);
        }
    }
}
