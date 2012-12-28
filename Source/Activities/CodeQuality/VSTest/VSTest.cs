//-----------------------------------------------------------------------
// <copyright file="VSTest.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Executes Test Cases using VSTest.Console.exe. 
    /// The activity is needed for scenarios where the tests are written using Visual Studio 2012 and the TFS server is TFS 2010.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [Description("Activity to run unit tests using vstest.console.exe")]
    public sealed class VSTest : BaseCodeActivity
    {
        /// <summary>
        /// The path of VSTEST.CONSOLE.EXE
        /// </summary>
        private const string VSTestConsoleExecutablePath = @"%VS110COMNTOOLS%\..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";
        
        /// <summary>
        /// The default Platform Setting of Any CPU
        /// </summary>
        private const string PlatformAnyCPU = @"Any CPU";

        /// <summary>
        /// The default build flavor of Release
        /// </summary>
        private const string FlavorToBuildRelease = "Release";

        private readonly IProcess process;

        /// <summary>
        /// Initializes a new instance of the VSTest class
        /// </summary>
        public VSTest() : this(new WindowsProcess())
        {
        }

        /// <summary>
        /// Internal constructor taking in an IProcess interface for dependency injection
        /// </summary>
        /// <param name="process">The process interface to create the windows process</param>
        internal VSTest(IProcess process)
        {
            this.process = process;
            this.Platform = PlatformAnyCPU;
            this.Flavor = FlavorToBuildRelease;
        }

        /// <summary>
        /// Sets the Build Details object of the build under which the activity is running in.
        /// </summary>
        [Browsable(true)]
        [Description("The Build Details object of the build under which the activity is running in.")]
        public InArgument<IBuildDetail> Build { get; set; }

        /// <summary>
        /// Sets the name of the test assemblies containing test cases to execute."
        /// </summary>
        [Browsable(true)]
        [RequiredArgument]
        [Description("Sets the name of the test assemblies containing test cases to execute.")]
        public InArgument<string[]> TestAssemblies { get; set; }

        /// <summary>
        /// Run tests with additional settings such as data collectors.
        /// </summary>
        [Browsable(true)]
        [Description("Run tests with additional settings such as data collectors.")]
        public InArgument<string> Settings { get; set; }

        /// <summary>
        /// Run tests that match the given expression. Expression is of the format property=value
        /// </summary>
        [Browsable(true)]
        [Description("Run tests that match the given expression. Expression is of the format property=value")]
        public InArgument<string> TestCaseFilter { get; set; }

        /// <summary>
        /// Run tests with names that match the provided values.
        /// Example: TestMethod1,testMethod2
        /// </summary>
        [Browsable(true)]
        [Description("Run tests with names that match the provided values.")]
        public InArgument<string> TestNames { get; set; }

        /// <summary>
        /// Set to true to publish test results to TFS.
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to publish test results to TFS.")]
        public InArgument<bool> PublishTestResults { get; set; }

        /// <summary>
        /// Set to true to publish unit tests code coverage to TFS
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to publish unit tests code coverage to TFS.")]
        public InArgument<bool> EnableCodeCoverage { get; set; }

        /// <summary>
        /// Which platform to publish test results for (ex. Any CPU). Default to Any CPU
        /// </summary>
        [Browsable(true)]
        [Description("Which platform to publish test results for (ex. Any CPU). Default to Any CPU")]
        public InArgument<string> Platform { get; set; }

        /// <summary>
        /// Which flavor to publish test results for (ex. Debug). Defaults to Release.
        /// </summary>
        [Browsable(true)]
        [Description("Which flavor to publish test results for (ex. Debug). Defaults to Release.")]
        public InArgument<string> Flavor { get; set; }

        /// <summary>
        /// Sets the URL of the Team Foundation Server's Project Collection URL
        /// </summary>
        [Browsable(true)]
        [Description("Sets the URL of the Team Foundation Server's Project Collection URL.")]
        public InArgument<string> ProjectCollectionUrl { get; set; }

        /// <summary>
        /// Sets the Working Directory for the VSTEST.CONSOLE.EXE to run in.
        /// </summary>
        [Browsable(true)]
        [Description("Sets the Working Directory for the VSTEST.CONSOLE.EXE to run in.")]
        public InArgument<string> WorkingDirectory { get; set; }

        /// <summary>
        /// Whether or not the tests ran successfully.
        /// </summary>
        [Description("Whether or not the tests ran successfully.")]
        public OutArgument<bool> ExitCode { get; set; }
        
        /// <summary>
        /// Executes the logic of the custom activity
        /// </summary>
        protected override void InternalExecute()
        {
            var fullPath = Environment.ExpandEnvironmentVariables(VSTestConsoleExecutablePath);
            if (!File.Exists(fullPath))
            {
                this.SetBuildError(string.Format(fullPath + " was not found. Use ToolPath to specify it."));
                return;
            }

            var commandLineArguments = this.ConstructCommandLineArguments();
            if (string.IsNullOrWhiteSpace(commandLineArguments))
            {
                return;                
            }
            
            bool executedSuccessfully = this.ExecuteTests(fullPath, commandLineArguments, this.WorkingDirectory.Get(ActivityContext));
            if (!executedSuccessfully)
            {
                this.SetBuildError("One or more unit tests did not succeed.");
            }

            this.ExitCode.Set(this.ActivityContext, true);
        }

        /// <summary>
        /// Creates the command line argument to be passed to the executable VSTEST.CONSOLE.EXE.
        /// </summary>
        /// <returns>Returns the command line arguments to be passed to VSTEST.CONSOLE.EXE</returns>
        private string ConstructCommandLineArguments()
        {
            var testAssemblies = this.TestAssemblies.Get(this.ActivityContext);
            if (testAssemblies == null || testAssemblies.Length == 0)
            {
                this.LogBuildError("No test assemblies were passed to the vstest.console.exe.");
                return string.Empty;
            }

            var commandLine = "\"" + testAssemblies.Aggregate((first, second) => first + "\" \"" + second) + "\"";

            if (!string.IsNullOrWhiteSpace(this.TestNames.Get(this.ActivityContext)))
            {
                commandLine = string.Format("{0} /Tests:\"{1}\"", commandLine, this.TestNames.Get(this.ActivityContext));
            }

            if (!string.IsNullOrWhiteSpace(this.Settings.Get(this.ActivityContext)))
            {
                commandLine = string.Format("{0} /Settings:\"{1}\"", commandLine, this.Settings.Get(this.ActivityContext));
            }

            if (!string.IsNullOrWhiteSpace(this.TestCaseFilter.Get(this.ActivityContext)))
            {
                commandLine = string.Format("{0} /TestCaseFilter:\"{1}\"", commandLine, this.TestCaseFilter.Get(this.ActivityContext));
            }

            if (this.EnableCodeCoverage.Get(this.ActivityContext))
            {
                commandLine = string.Format("{0} /EnableCodeCoverage", commandLine);
            }

            if (this.PublishTestResults.Get(this.ActivityContext))
            {
                IBuildDetail buildDetail = this.Build.Get(this.ActivityContext);
                commandLine = string.Format(
                    "{0} /Logger:TfsPublisher;Collection=\"{1}\";BuildName=\"{2}\";TeamProject=\"{3}\";Flavor=\"{4}\";Platform=\"{5}\"",
                    commandLine,
                    this.ProjectCollectionUrl.Get(this.ActivityContext),
                    buildDetail.BuildNumber,
                    buildDetail.TeamProject,
                    this.Flavor.Get(this.ActivityContext),
                    this.Platform.Get(this.ActivityContext));
            }

            return commandLine;
        }

        /// <summary>
        /// Executes unit tests  
        /// </summary>
        /// <param name="exeFilePath">The path to VSTEST.CONSOLE.EXE.</param>
        /// <param name="commandLineArguments">The test assemblies and options passed as command line parameter.</param>
        /// <param name="workingDirectory">The working directory to execute the executable file from.</param>
        /// <returns>Returns True if the executable runs without errors. False otherwise.</returns>
        private bool ExecuteTests(string exeFilePath, string commandLineArguments, string workingDirectory)
        {
            this.LogBuildMessage(string.Format("Executing {0} {1}", exeFilePath, commandLineArguments));
            return this.process.Execute(exeFilePath, commandLineArguments, workingDirectory);
        }

        /// <summary>
        /// Sets the exitCode and Log the build error.
        /// </summary>
        /// <param name="errorDescription">The build error description.</param>
        private void SetBuildError(string errorDescription)
        {
            this.ExitCode.Set(this.ActivityContext, false);
            this.LogBuildError(errorDescription);
        }
    }
}
