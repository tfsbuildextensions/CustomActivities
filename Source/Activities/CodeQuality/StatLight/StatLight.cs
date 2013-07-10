//-----------------------------------------------------------------------
// <copyright file="StatLight.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;
    using Microsoft.TeamFoundation.Build.Client;
    using TfsBuildExtensions.Activities.CodeQuality.Extended;
    using TfsBuildExtensions.Activities.VisualStudio;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public class StatLight : BaseCodeActivity
    {
        /// <summary>
        /// Initializes a new instance of the StatLight class.
        /// </summary>
        public StatLight()
        {
            this.VisualStudioVersion = VSVersion.VS2010;
        }

        /// <summary>
        /// The assemblies to process.
        /// </summary>
        [Browsable(true)]
        [Description("The test assembly to execute tests in")]
        [RequiredArgument]
        public InArgument<string> TestAssembly { get; set; }

        /// <summary>
        /// Gets or sets the ToolPath. Defaults to %ProgramFiles%\StatLight\StatLight.exe
        /// </summary>
        [Browsable(true)]
        [Description(@"Gets or sets the ToolPath (defaults to %ProgramFiles%\StatLight\StatLight.exe")]
        public InArgument<string> ToolPath { get; set; }

        /// <summary>
        /// Set to true to publish test results back to TFS
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to publish test results back to TFS")]
        public InArgument<bool> PublishTestResults { get; set; }

        /// <summary>
        /// Which platform to publish test results for (ex. Any CPU)
        /// </summary>
        [Browsable(true)]
        [Description("Which platform to publish test results for (ex. Any CPU)")]
        public InArgument<string> Platform { get; set; }

        /// <summary>
        /// Which flavor to publish test results for (ex. Debug)
        /// </summary>
        [Browsable(true)]
        [Description("Which flavor to publish test results for (ex. Debug)")]
        public InArgument<string> Flavor { get; set; }

        /// <summary>
        /// Sets the OutputXmlFile name for the test result
        /// </summary>
        [Browsable(true)]
        [Description("Sets the OutputXmlFile name")]
        public InArgument<string> OutputXmlFile { get; set; }

        /// <summary>
        /// The version of Visual Studio used to run unit tests. Default is VS2012
        /// </summary>
        [Browsable(true)]
        [Description("The version of Visual Studio. Default is VS2012")]
        public InArgument<VSVersion> VisualStudioVersion { get; set; }

        /// <summary>
        /// Provide additional arguments to Statlight
        /// </summary>
        [Browsable(true)]
        [Description("Provide additional arguments to Statlight")]
        public InArgument<string> AdditionalArguments { get; set; }

        /// <summary>
        /// Executes the logic for this custom activity
        /// </summary>
        protected override void InternalExecute()
        {
            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();

            if (string.IsNullOrEmpty(this.ToolPath.Get(this.ActivityContext)))
            {
                this.ToolPath.Set(this.ActivityContext, string.Format(CultureInfo.InvariantCulture, System.Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\StatLight\StatLight.exe")));
            }

            string fullPath = this.ToolPath.Get(this.ActivityContext);

            if (!File.Exists(fullPath))
            {
                this.LogBuildError(string.Format(fullPath + " was not found. Use ToolPath to specify it."));
                return;
            }

            string workingDirectory = Path.GetDirectoryName(this.TestAssembly.Get(this.ActivityContext));
            string additionalArguments = this.AdditionalArguments.Get(this.ActivityContext);

            this.RunProcess(fullPath, workingDirectory, this.GenerateCommandLineCommands(this.ActivityContext, workingDirectory, additionalArguments));
            this.PublishTestResultsToTFS(this.ActivityContext, workingDirectory);
        }

        private int RunProcess(string fullPath, string workingDirectory, string arguments)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = fullPath;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments, BuildMessageImportance.High);

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    proc.StartInfo.WorkingDirectory = workingDirectory;
                }

                proc.Start();

                string textLine = proc.StandardOutput.ReadLine();
                while (textLine != null)
                {
                    if (!string.IsNullOrEmpty(textLine))
                    {
                        this.LogBuildMessage(textLine, BuildMessageImportance.High);                        
                    }

                    textLine = proc.StandardOutput.ReadLine();
                }

                textLine = proc.StandardError.ReadLine();
                while (textLine != null)
                {
                    if (!string.IsNullOrEmpty(textLine))
                    {
                        this.LogBuildError(textLine);
                    }

                    textLine = proc.StandardError.ReadLine();
                }

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                    buildDetail.TestStatus = BuildPhaseStatus.Failed;
                }

                return proc.ExitCode;
            }
        }

        private void PublishTestResultsToTFS(ActivityContext context, string folder)
        {
            if (!this.PublishTestResults.Get(context))
            {
                return;
            }

            if (string.IsNullOrEmpty(this.Platform.Get(context)) || string.IsNullOrEmpty(this.Flavor.Get(context)))
            {
                this.LogBuildError("When publishing test results, both Platform and Flavor must be specified");
                return;
            }

            string filename = Path.Combine(folder, this.GetResultFileName(context));

            string resultTrxFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(this.GetResultFileName(context)) + ".trx");
            if (!File.Exists(filename))
            {
                return;
            }

            string userName = Environment.UserName;
            string machineName = Environment.MachineName;
            this.TransformStatLightTestToMsTest(userName, machineName, filename, resultTrxFile);

            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            string collectionUrl = buildDetail.BuildServer.TeamProjectCollection.Uri.ToString();
            string buildNumber = buildDetail.BuildNumber;
            string teamProject = buildDetail.TeamProject;
            string platform = this.Platform.Get(context);
            string flavor = this.Flavor.Get(context);
            this.PublishMsTestResults(resultTrxFile, collectionUrl, buildNumber, teamProject, platform, flavor);
        }

        private string GenerateCommandLineCommands(ActivityContext context, string outputFolder, string additionalArguments)
        {
            SimpleCommandLineBuilder builder = new SimpleCommandLineBuilder();

            builder.AppendSwitchIfNotNull("-x=", this.TestAssembly.Get(context));
            builder.AppendSwitchIfNotNull("-r=", Path.Combine(outputFolder, this.OutputXmlFile.Get(context)));
            builder.AppendSwitch(additionalArguments);

            return builder.ToString();
        }

        private void PublishMsTestResults(string resultTrxFile, string collectionUrl, string buildNumber, string teamProject, string platform, string flavor)
        {
            int visualStudioVersion = (int)this.VisualStudioVersion.Get(this.ActivityContext);
            string argument = string.Format("/publish:\"{0}\" /publishresultsfile:\"{1}\" /publishbuild:\"{2}\" /teamproject:\"{3}\" /platform:\"{4}\" /flavor:\"{5}\"", collectionUrl, resultTrxFile, buildNumber, teamProject, platform, flavor);
            this.RunProcess(Environment.ExpandEnvironmentVariables(string.Format(@"%VS{0}COMNTOOLS%\..\IDE\MSTest.exe", visualStudioVersion)), null, argument);
        }

        private void TransformStatLightTestToMsTest(string userName, string machineName, string statLightResultFile, string mstestResultFile)
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("TfsBuildExtensions.Activities.CodeQuality.StatLight.StatLightToMSTest.xslt");
            if (s == null)
            {
                this.LogBuildError("Could not load StatLightToMSTest.xslt from embedded resources");
                return;
            }

            using (var reader = new XmlTextReader(s))
            {
                XsltArgumentList argsList = new XsltArgumentList();
                argsList.AddParam("runUser", string.Empty, userName);
                argsList.AddParam("machineName", string.Empty, machineName);
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(reader);

                this.LogBuildMessage(string.Format("Transforming StatLight restult file {0} to MSTest file {1}.", statLightResultFile, mstestResultFile));

                using (StreamWriter sw = new StreamWriter(mstestResultFile))
                {
                    transform.Transform(statLightResultFile, argsList, sw);
                }
            }
        }

        private string GetResultFileName(ActivityContext context)
        {
            string filename = "TestResult.xml";
            if (this.OutputXmlFile.Get(context) != null && !string.IsNullOrEmpty(this.OutputXmlFile.Get(context)))
            {
                filename = this.OutputXmlFile.Get(context);
            }

            return filename;
        }
    }
}
