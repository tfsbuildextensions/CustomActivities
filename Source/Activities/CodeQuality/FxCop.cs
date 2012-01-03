//-----------------------------------------------------------------------
// <copyright file="FxCop.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities;

    /// <summary>
    /// The FxCop activity provides a basic wrapper over FxCopCmd.exe. See http://msdn.microsoft.com/en-gb/library/bb429449(VS.80).aspx for more details.
    /// <para/>
    /// <b>Required: </b> Project and / or Files, OutputFile <b>Optional: </b>DependencyDirectories, Imports, Rules, ShowSummary, UpdateProject, Verbose, UpdateProject, LogToConsole, Types, FxCopPath, ReportXsl, OutputFile, ConsoleXsl, Project, SearchGac, IgnoreInvalidTargets, Quiet, ForceOutput, AspNetOnly, IgnoreGeneratedCode, OverrideRuleVisibilities, FailOnMissingRules, SuccessFile, Dictionary, Ruleset, RulesetDirectory <b>Output: </b>AnalysisFailed, OutputText, ExitCode<para/>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <tac3:FxCop AnalysisFailed="{x:Null}" AspNetOnly="{x:Null}" ConsoleXsl="{x:Null}" DependencyDirectories="{x:Null}" Dictionary="{x:Null}" ExitCode="{x:Null}" FailBuildOnError="{x:Null}" FailOnMissingRules="{x:Null}" ForceOutput="{x:Null}" IgnoreExceptions="{x:Null}" IgnoreGeneratedCode="{x:Null}" IgnoreInvalidTargets="{x:Null}" Imports="{x:Null}" OutputText="{x:Null}" OverrideRuleVisibilities="{x:Null}" Project="{x:Null}" Quiet="{x:Null}" ReportXsl="{x:Null}" Ruleset="{x:Null}" RulesetDirectory="{x:Null}" SearchGac="{x:Null}" SuccessFile="{x:Null}" Timeout="{x:Null}" TreatWarningsAsErrors="{x:Null}" Types="{x:Null}" UpdateProject="{x:Null}" Verbose="{x:Null}" Files="[New String() {&quot;C:\FxCopFailTest.dll&quot;}]" FxCopPath="D:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe" sap:VirtualizedContainerService.HintSize="526,22" LogExceptionStack="True" LogToConsole="True" OutputFile="D:\a\tfsfail.txt" Rules="[New String() {&quot;+D:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\Rules\DesignRules.dll&quot;}]" ShowSummary="True" />
    /// ]]></code>    
    /// </example>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class FxCop : BaseCodeActivity
    {
        private InArgument<bool> logToConsole = true;
        private InArgument<bool> showSummary = true;

        /// <summary>
        /// Sets the Item Collection of assemblies to analyse (/file option)
        /// </summary>
        public InArgument<IEnumerable<string>> Files { get; set; }

        /// <summary>
        /// Sets the DependencyDirectories :(/directory option)
        /// </summary>
        public InArgument<IEnumerable<string>> DependencyDirectories { get; set; }

        /// <summary>
        /// Sets the name of an analysis report or project file to import (/import option)
        /// </summary>
        public InArgument<IEnumerable<string>> Imports { get; set; }

        /// <summary>
        /// Sets the location of rule libraries to load (/rule option). Prefix the Rules path with ! to treat warnings as errors
        /// </summary>
        public InArgument<IEnumerable<string>> Rules { get; set; }

        /// <summary>
        /// Set to true to display a summary (/summary option). Default is true
        /// </summary>
        public InArgument<bool> ShowSummary
        {
            get { return this.showSummary; }
            set { this.showSummary = value; }
        }

        /// <summary>
        /// Set to true to search the GAC for missing assembly references (/gac option). Default is false
        /// </summary>
        public InArgument<bool> SearchGac { get; set; }

        /// <summary>
        /// Set to true to create .lastcodeanalysissucceeded file in output report directory if no build-breaking messages occur during analysis. Default is false
        /// </summary>
        public InArgument<bool> SuccessFile { get; set; }

        /// <summary>
        /// Set to true to run all overridable rules against all targets. Default is false
        /// </summary>
        public InArgument<bool> OverrideRuleVisibilities { get; set; }

        /// <summary>
        /// Set the override timeout for analysis deadlock detection. Analysis will be aborted when analysis of a single item by a single rule exceeds the specified amount of time. Default is 0 to disable deadlock detection.
        /// </summary>
        public InArgument<int> Timeout { get; set; }

        /// <summary>
        /// Set to true to treat missing rules or rule sets as an error and halt execution. Default is false
        /// </summary>
        public InArgument<bool> FailOnMissingRules { get; set; }

        /// <summary>
        /// Set to true to suppress analysis results against generated code. Default is false
        /// </summary>
        public InArgument<bool> IgnoreGeneratedCode { get; set; }

        /// <summary>
        /// Set to true to analyze only ASP.NET-generated binaries and honor global suppressions in App_Code.dll for all assemblies under analysis. Default is false
        /// </summary>
        public InArgument<bool> AspNetOnly { get; set; }

        /// <summary>
        /// Set to true to silently ignore invalid target files. Default is false
        /// </summary>
        public InArgument<bool> IgnoreInvalidTargets { get; set; }

        /// <summary>
        /// Set to true to suppress all console output other than the reporting implied by /console or /consolexsl. Default is false
        /// </summary>
        public InArgument<bool> Quiet { get; set; }

        /// <summary>
        /// Set to true to write output XML and project files even in the case where no violations occurred. Default is false
        /// </summary>
        public InArgument<bool> ForceOutput { get; set; }

        /// <summary>
        /// Set to true to output verbose information during analysis (/verbose option)
        /// </summary>
        public InArgument<bool> Verbose { get; set; }

        /// <summary>
        /// Saves the results of the analysis in the project file. This option is ignored if the /project option is not specified (/update option)
        /// </summary>
        public InArgument<bool> UpdateProject { get; set; }

        /// <summary>
        /// Set to true to direct analysis output to the console (/console option). Default is true
        /// </summary>
        public InArgument<bool> LogToConsole
        {
            get { return this.logToConsole; }
            set { this.logToConsole = value; }
        }

        /// <summary>
        /// Specifies the types to analyze
        /// </summary>
        public InArgument<string> Types { get; set; }

        /// <summary>
        /// Specifies the directory to search for rule set files that are specified by the Ruleset switch or are included by one of the specified rule sets.
        /// </summary>
        public InArgument<string> RulesetDirectory { get; set; }

        /// <summary>
        /// Specifies the Rule set to be used for the analysis. It can be a file path to the rule set file or the file name of 
        /// a built-in rule set. '+' enables all rules in the rule set; '-' disables all rules in the rule set; '=' sets rules 
        /// to match the rule set and disables all rules that are not enabled in the rule set
        /// </summary>
        public InArgument<string> Ruleset { get; set; }

        /// <summary>
        /// Sets the path to FxCopCmd.exe. Default is [Program Files]\Microsoft FxCop 1.36\FxCopCmd.exe
        /// </summary>
        public InArgument<string> FxCopPath { get; set; }

        /// <summary>
        /// Sets the ReportXsl (/outXsl: option)
        /// </summary>
        public InArgument<string> ReportXsl { get; set; }

        /// <summary>
        /// Set the name of the file for the analysis report
        /// </summary>
        [RequiredArgument]
        public InArgument<string> OutputFile { get; set; }

        /// <summary>
        /// Sets the ConsoleXsl (/consoleXsl option)
        /// </summary>
        public InArgument<string> ConsoleXsl { get; set; }

        /// <summary>
        /// Sets the custom dictionary used by spelling rules.Default is no custom dictionary
        /// </summary>
        public InArgument<string> Dictionary { get; set; }

        /// <summary>
        /// Set the name of the .fxcop project to use
        /// </summary>
        public InArgument<string> Project { get; set; }

        /// <summary>
        /// Gets AnalysisFailed. True if FxCop logged Code Analysis errors to the Output file.
        /// </summary>
        public OutArgument<bool> AnalysisFailed { get; set; }

        /// <summary>
        /// The exit code returned from FxCop
        /// </summary>
        public OutArgument<int> ExitCode { get; set; }

        /// <summary>
        /// Gets the OutputText emitted during analysis
        /// </summary>
        public OutArgument<string> OutputText { get; set; }
        
        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        protected override void InternalExecute()
        {
            string fxcopIntPath;

            if (string.IsNullOrEmpty(this.FxCopPath.Get(this.ActivityContext)))
            {
                string programFilePath = Environment.GetEnvironmentVariable("ProgramFiles");
                if (string.IsNullOrEmpty(programFilePath))
                {
                    this.LogBuildError("Failed to read a value from the ProgramFiles Environment Variable");
                    return;
                }

                if (System.IO.File.Exists(programFilePath + @"\Microsoft FxCop 1.36\FxCopCmd.exe"))
                {
                    fxcopIntPath = programFilePath + @"\Microsoft FxCop 1.36\FxCopCmd.exe";
                }
                else if (System.IO.File.Exists(programFilePath + @"\Microsoft FxCop 10.0\FxCopCmd.exe"))
                {
                    fxcopIntPath = programFilePath + @"\Microsoft FxCop 10.0\FxCopCmd.exe";
                }
                else
                {
                    this.LogBuildError(string.Format(CultureInfo.CurrentCulture, "FxCopCmd.exe was not found in the default location. Use FxCopPath to specify it. Searched at: {0}", programFilePath + @"\Microsoft FxCop 1.36 and \Microsoft FxCop 10.0"));
                    return;
                }
            }
            else
            {
                fxcopIntPath = this.FxCopPath.Get(this.ActivityContext);
            }

            string arguments = string.Empty;

            if (!string.IsNullOrEmpty(this.ReportXsl.Get(this.ActivityContext)))
            {
                arguments += "/applyoutXsl /outXsl:\"" + this.ReportXsl.Get(this.ActivityContext) + "\"";
            }

            if (this.LogToConsole.Get(this.ActivityContext))
            {
                arguments += " /console";

                if (!string.IsNullOrEmpty(this.ConsoleXsl.Get(this.ActivityContext)))
                {
                    arguments += " /consoleXsl:\"" + this.ConsoleXsl.Get(this.ActivityContext) + "\"";
                }
            }

            if (!string.IsNullOrEmpty(this.Ruleset.Get(this.ActivityContext)))
            {
                arguments += " /ruleset:\"" + this.Ruleset.Get(this.ActivityContext) + "\"";
            }

            if (!string.IsNullOrEmpty(this.RulesetDirectory.Get(this.ActivityContext)))
            {
                arguments += " /rulesetdirectory:\"" + this.RulesetDirectory.Get(this.ActivityContext) + "\"";
            }

            if (this.UpdateProject.Get(this.ActivityContext))
            {
                arguments += " /update";
            }

            if (this.SearchGac.Get(this.ActivityContext))
            {
                arguments += " /gac";
            }

            if (this.SuccessFile.Get(this.ActivityContext))
            {
                arguments += " /successfile";
            }

            if (this.FailOnMissingRules.Get(this.ActivityContext))
            {
                arguments += " /failonmissingrules";
            }

            if (this.IgnoreGeneratedCode.Get(this.ActivityContext))
            {
                arguments += " /ignoregeneratedcode";
            }

            if (this.OverrideRuleVisibilities.Get(this.ActivityContext))
            {
                arguments += " /overriderulevisibilities";
            }

            if (this.AspNetOnly.Get(this.ActivityContext))
            {
                arguments += " /aspnet";
            }

            if (this.IgnoreInvalidTargets.Get(this.ActivityContext))
            {
                arguments += " /ignoreinvalidtargets";
            }

            if (this.Timeout.Get(this.ActivityContext) > 0)
            {
                arguments += " /timeout:" + this.Timeout.Get(this.ActivityContext);
            }

            if (this.Quiet.Get(this.ActivityContext))
            {
                arguments += " /quiet";
            }

            if (this.ForceOutput.Get(this.ActivityContext))
            {
                arguments += " /forceoutput";
            }

            if (!string.IsNullOrEmpty(this.Dictionary.Get(this.ActivityContext)))
            {
                arguments += " /dictionary:\"" + this.Dictionary.Get(this.ActivityContext) + "\"";
            }

            if (this.ShowSummary.Get(this.ActivityContext))
            {
                arguments += " /summary";
            }

            if (this.Verbose.Get(this.ActivityContext))
            {
                arguments += " /verbose";
            }

            if (!string.IsNullOrEmpty(this.Types.Get(this.ActivityContext)))
            {
                arguments += " /types:\"" + this.Types.Get(this.ActivityContext) + "\"";
            }

            if (this.DependencyDirectories.Get(this.ActivityContext) != null)
            {
                foreach (var i in this.DependencyDirectories.Get(this.ActivityContext))
                {
                    string path = i;
                    if (path.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        path = path.Substring(0, path.Length - 1);
                    }

                    arguments += " /directory:\"" + path + "\"";
                }
            }

            if (this.Imports.Get(this.ActivityContext) != null)
            {
                arguments = this.Imports.Get(this.ActivityContext).Aggregate(arguments, (current, import) => current + (" /import:\"" + import + "\""));
            }

            if (this.Rules.Get(this.ActivityContext) != null)
            {
                arguments = this.Rules.Get(this.ActivityContext).Aggregate(arguments, (current, rule) => current + (" /rule:\"" + rule + "\""));
            }

            if (string.IsNullOrEmpty(this.Project.Get(this.ActivityContext)) && this.Files.Get(this.ActivityContext) == null)
            {
                this.LogBuildError("A Project and / or Files collection must be passed.");
                return;
            }

            if (!string.IsNullOrEmpty(this.Project.Get(this.ActivityContext)))
            {
                arguments += " /project:\"" + this.Project.Get(this.ActivityContext) + "\"";
            }

            if (this.Files.Get(this.ActivityContext) != null)
            {
                arguments = this.Files.Get(this.ActivityContext).Aggregate(arguments, (current, file) => current + (" /file:\"" + file + "\""));
            }

            arguments += " /out:\"" + this.OutputFile.Get(this.ActivityContext) + "\"";

            // if the output file exists, delete it.
            if (System.IO.File.Exists(this.OutputFile.Get(this.ActivityContext)))
            {
                System.IO.File.Delete(this.OutputFile.Get(this.ActivityContext));
            }

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = fxcopIntPath;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
                proc.Start();

                string outputStream = proc.StandardOutput.ReadToEnd();
                if (outputStream.Length > 0)
                {
                    this.LogBuildMessage(outputStream);
                    this.OutputText.Set(this.ActivityContext, outputStream);
                }

                string errorStream = proc.StandardError.ReadToEnd();
                if (errorStream.Length > 0)
                {
                    this.LogBuildError(errorStream);
                }

                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    this.ExitCode.Set(this.ActivityContext, proc.ExitCode);
                    this.LogBuildError(proc.ExitCode.ToString(CultureInfo.CurrentCulture));
                    this.AnalysisFailed.Set(this.ActivityContext, true);
                    return;
                }

                this.AnalysisFailed.Set(this.ActivityContext, System.IO.File.Exists(this.OutputFile.Get(this.ActivityContext)));
            }
        }
    }
}