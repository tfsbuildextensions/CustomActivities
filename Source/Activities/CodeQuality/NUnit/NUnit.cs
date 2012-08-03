//-----------------------------------------------------------------------
// <copyright file="NUnit.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
    using System.Xml;
    using System.Xml.Xsl;
    using Microsoft.TeamFoundation.Build.Client;

    using TfsBuildExtensions.Activities.CodeQuality.Extended;

    /// <summary>
    /// Executes Test Cases using NUnit (Tested using v2.5.7)
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <!-- Run NUnit on all binaries (*.dll) for the the current configuration/platform-->    
    /// <Sequence>
    /// <Sequence.Variables>
    /// <Variable x:TypeArguments="scg2:IEnumerable(x:String)" Name="NUnitTestAssemblies" />
    ///  <Variable x:TypeArguments="x:Int32" Name="Total" />
    /// <Variable x:TypeArguments="x:Int32" Name="Errors" />
    /// </Sequence.Variables>
    /// <mtbwa:FindMatchingFiles DisplayName="Find NUnitTest assemblies" MatchPattern="[String.Format(&quot;{0}\\**.dll&quot;, BinariesDirectory)]" Result="[NUnitTestAssemblies]" />
    /// <tan:NUnit PublishTestResults="True" Flavor="[platformConfiguration.Configuration]" Platform="[platformConfiguration.Platform]" Configuration="{x:Null}" Domain="{x:Null}" ErrorOutputFile="{x:Null}" ExcludeCategory="{x:Null}" FailBuildOnError="{x:Null}" Failures="{x:Null}" Framework="{x:Null}" Ignored="{x:Null}" IncludeCategory="{x:Null}" Inconclusive="{x:Null}" Invalid="{x:Null}" Labels="{x:Null}" NoShadow="{x:Null}" NoThread="{x:Null}" NotRun="{x:Null}" OutputFile="{x:Null}" Process="{x:Null}" Run="{x:Null}" Skipped="{x:Null}" TestTimeout="{x:Null}" TimeTaken="{x:Null}" TreatWarningsAsErrors="{x:Null}" Use32Bit="{x:Null}" Assemblies="[NUnitTestAssemblies]" Errors="[Errors]" LogExceptionStack="True" OutputXmlFile="NUnitResults.xml" ToolPath="C:\Program Files (x86)\NUnit 2.5.10\bin\net-2.0\" Total="[Total]" Version="2.5.7" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [System.ComponentModel.Description("Activity to run NUnit tests as part of a TFS Build")]
    public class NUnit : BaseCodeActivity
    {
        /// <summary>
        /// Initializes a new instance of the NUnit class
        /// </summary>
        public NUnit()
        {
            this.Version = "2.5.7";
        }

        /// <summary>
        /// The version of NUnit to run. Default is 2.5.7
        /// </summary>
        [Browsable(true)]
        [Description("The version of NUnit to run. Default is 2.5.7")]
        public InArgument<string> Version { get; set; }

        /// <summary>
        /// Gets or sets the ToolPath (defaults to %ProgramFiles%\Nunit {version}\bin\nunit-console[-86].exe
        /// </summary>
        [Browsable(true)]
        [Description(@"Gets or sets the ToolPath (defaults to %ProgramFiles%\Nunit {version}\bin\nunit-console[-86].exe")]
        public InArgument<string> ToolPath { get; set; }

        /// <summary>
        /// The assemblies to process.
        /// </summary>
        [Browsable(true)]
        [Description("The assemblies to process")]
        [RequiredArgument]
        public InArgument<IEnumerable<string>> Assemblies { get; set; }

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
        /// Set to true to run nunit-console-x86.exe
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to run nunit-console-x86.exe")]
        public InArgument<bool> Use32Bit { get; set; }

        /// <summary>
        /// Comma separated list of categories to include.
        /// </summary>
        [Browsable(true)]
        [Description("Comma separated list of categories to include")]
        public InArgument<string> IncludeCategory { get; set; }

        /// <summary>
        /// Comma separated list of categories to exclude.
        /// </summary>
        [Browsable(true)]
        [Description("Comma separated list of categories to exclude")]
        public InArgument<string> ExcludeCategory { get; set; }

        /// <summary>
        /// Sets the OutputXmlFile name
        /// </summary>
        [Browsable(true)]
        [Description("Sets the OutputXmlFile name")]
        public InArgument<string> OutputXmlFile { get; set; }

        /// <summary>
        /// Sets the File to receive test error output
        /// </summary>
        [Browsable(true)]
        [Description("Sets the File to receive test error output")]
        public InArgument<string> ErrorOutputFile { get; set; }

        /// <summary>
        /// File to receive test output
        /// </summary>
        [Browsable(true)]
        [Description("File to receive test output")]
        public InArgument<string> OutputFile { get; set; }

        /// <summary>
        /// Disable use of a separate thread for tests. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Disable use of a separate thread for tests. Default is false")]
        public InArgument<bool> NoThread { get; set; }

        /// <summary>
        /// Gets the Failures count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Failures count")]
        public OutArgument<int> Failures { get; set; }

        /// <summary>
        /// Gets the NotRun count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the NotRun count")]
        public OutArgument<int> NotRun { get; set; }

        /// <summary>
        /// Gets the Total count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Total count")]
        public OutArgument<int> Total { get; set; }

        /// <summary>
        /// Gets the Errors count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Errors count")]
        public OutArgument<int> Errors { get; set; }

        /// <summary>
        /// Gets the Inconclusive count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Inconclusive count")]
        public OutArgument<int> Inconclusive { get; set; }

        /// <summary>
        /// Gets the Ignored count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Ignored count")]
        public OutArgument<int> Ignored { get; set; }

        /// <summary>
        /// Gets the Skipped count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Skipped count")]
        public OutArgument<int> Skipped { get; set; }

        /// <summary>
        /// Gets the Invalid count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Invalid count")]
        public OutArgument<int> Invalid { get; set; }

        /// <summary>
        /// Disable shadow copy when running in separate domain. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Disable shadow copy when running in separate domain. Default is false")]
        public InArgument<bool> NoShadow { get; set; }

        /// <summary>
        /// Sets the Project configuration (e.g.: Debug) to load
        /// </summary>
        [Browsable(true)]
        [Description("Sets the Project configuration (e.g.: Debug) to load")]
        public InArgument<string> Configuration { get; set; }

        /// <summary>
        /// Gets the Time taken to run the tests
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Time taken to run the tests")]
        public InArgument<DateTime> TimeTaken { get; set; }

        /// <summary>
        /// Process model for tests. Supports Single, Separate, Multiple. Single is the Default
        /// </summary>
        [Browsable(true)]
        [Description("Process model for tests. Supports Single, Separate, Multiple. Single is the Default")]
        public InArgument<string> Process { get; set; }

        /// <summary>
        /// AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.
        /// </summary>
        [Browsable(true)]
        [Description("AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.")]
        public InArgument<string> Domain { get; set; }

        /// <summary>
        /// Framework version to be used for tests
        /// </summary>
        [Browsable(true)]
        [Description("Framework version to be used for tests")]
        public InArgument<string> Framework { get; set; }

        /// <summary>
        /// Set timeout for each test case in milliseconds
        /// </summary>
        [Browsable(true)]
        [Description("Set timeout for each test case in milliseconds")]
        public InArgument<int> TestTimeout { get; set; }

        /// <summary>
        /// Label each test in stdOut. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Label each test in stdOut. Default is false")]
        public InArgument<bool> Labels { get; set; }

        /// <summary>
        /// Name of the test case(s), fixture(s) or namespace(s) to run
        /// </summary>
        [Browsable(true)]
        [Description("Name of the test case(s), fixture(s) or namespace(s) to run")]
        public InArgument<string> Run { get; set; }

        /// <summary>
        /// Name of the test case(s), fixture(s) or namespace(s) to run
        /// </summary>
        [Browsable(true)]
        [Description("ExitCode for the NUnit process")]
        public OutArgument<int> ExitCode { get; set; }

        /// <summary>
        /// Executes the logic for this custom activity
        /// </summary>
        protected override void InternalExecute()
        {
            string fullPath = this.GenerateFullPathToTool(this.ActivityContext);
            if (!File.Exists(fullPath))
            {
                this.LogBuildError(string.Format(fullPath + " was not found. Use ToolPath to specify it."));
                return;
            }

            if (this.Assemblies.Get(this.ActivityContext).Count() == 0)
            {
                this.LogBuildMessage("No unit test assemblies passed to NUnit actitity. No tests will be executed");
                return;
            }

            string workingDirectory = Path.GetDirectoryName(this.Assemblies.Get(this.ActivityContext).First());
            int exitCode = this.RunProcess(fullPath, workingDirectory, this.GenerateCommandLineCommands(this.ActivityContext, workingDirectory));
            this.ExitCode.Set(this.ActivityContext, exitCode);
            this.ProcessXmlResultsFile(this.ActivityContext, workingDirectory);
            this.PublishTestResultsToTFS(this.ActivityContext, workingDirectory);
        }

        private static int GetAttributeInt32Value(string name, XmlNode node)
        {
            if (node.Attributes[name] != null)
            {
                return Convert.ToInt32(node.Attributes[name].Value, CultureInfo.InvariantCulture);
            }

            return 0;
        }

        private string GenerateFullPathToTool(ActivityContext context)
        {
            string toolName = this.Use32Bit.Get(context) ? "nunit-console-x86.exe" : "nunit-console.exe";
            if (string.IsNullOrEmpty(this.ToolPath.Get(context)))
            {
                this.ToolPath.Set(context, string.Format(CultureInfo.InvariantCulture, System.Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Nunit {0}\bin"), this.Version.Get(context)));
            }

            return string.IsNullOrEmpty(this.ToolPath.Get(context)) ? toolName : Path.Combine(this.ToolPath.Get(context), toolName);
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

            this.TransformNUnitToMSTest(filename, resultTrxFile);

            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            string collectionUrl = buildDetail.BuildServer.TeamProjectCollection.Uri.ToString();
            string buildNumber = buildDetail.BuildNumber;
            string teamProject = buildDetail.TeamProject;
            string platform = this.Platform.Get(context);
            string flavor = this.Flavor.Get(context);
            this.PublishMSTestResults(resultTrxFile, collectionUrl, buildNumber, teamProject, platform, flavor);
        }

        private void PublishMSTestResults(string resultTrxFile, string collectionUrl, string buildNumber, string teamProject, string platform, string flavor)
        {
            string argument = string.Format("/publish:\"{0}\" /publishresultsfile:\"{1}\" /publishbuild:\"{2}\" /teamproject:\"{3}\" /platform:\"{4}\" /flavor:\"{5}\"", collectionUrl, resultTrxFile, buildNumber, teamProject, platform, flavor);
            this.RunProcess(Environment.ExpandEnvironmentVariables(@"%VS100COMNTOOLS%\..\IDE\MSTest.exe"), null, argument);
        }

        private void TransformNUnitToMSTest(string nunitResultFile, string mstestResultFile)
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("TfsBuildExtensions.Activities.CodeQuality.NUnit.NUnitToMSTest.xslt");
            if (s == null)
            {
                this.LogBuildError("Could not load NUnitToMSTest.xslt from embedded resources");
                return;
            }

            using (var reader = new XmlTextReader(s))
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(reader);
                transform.Transform(nunitResultFile, mstestResultFile);
            }
        }

        private string GenerateCommandLineCommands(ActivityContext context, string outputFolder)
        {
            SimpleCommandLineBuilder builder = new SimpleCommandLineBuilder();
            builder.AppendSwitch("/nologo");
            if (this.NoShadow.Get(context))
            {
                builder.AppendSwitch("/noshadow");
            }

            if (this.NoThread.Get(context))
            {
                builder.AppendSwitch("/nothread");
            }

            if (this.Labels.Get(context))
            {
                builder.AppendSwitch("/labels");
            }

            builder.AppendFileNamesIfNotNull(this.Assemblies.Get(context).ToArray(), " ");
            builder.AppendSwitchIfNotNull("/run=", this.Run.Get(context));
            builder.AppendSwitchIfNotNull("/config=", this.Configuration.Get(context));
            builder.AppendSwitchIfNotNull("/include=", this.IncludeCategory.Get(context));
            builder.AppendSwitchIfNotNull("/exclude=", this.ExcludeCategory.Get(context));
            builder.AppendSwitchIfNotNull("/process=", this.Process.Get(context));
            builder.AppendSwitchIfNotNull("/domain=", this.Domain.Get(context));
            builder.AppendSwitchIfNotNull("/framework=", this.Framework.Get(context));
            builder.AppendSwitchIfNotNull("/xml=", Path.Combine(outputFolder, this.OutputXmlFile.Get(context)));
            builder.AppendSwitchIfNotNull("/err=", this.ErrorOutputFile.Get(context));
            builder.AppendSwitchIfNotNull("/out=", this.OutputFile.Get(context));
            return builder.ToString();
        }

        private void ProcessXmlResultsFile(ActivityContext context, string folder)
        {
            string filename = Path.Combine(folder, this.GetResultFileName(context));
            this.LogBuildMessage("Processing " + filename, BuildMessageImportance.High);
            if (File.Exists(filename))
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(filename);
                }
                catch (Exception ex)
                {
                    LogBuildError(ex.Message);
                    return;
                }

                XmlNode root = doc.DocumentElement;
                if (root == null)
                {
                    this.LogBuildError("Failed to load the OutputXmlFile");
                    return;
                }

                this.Total.Set(context, GetAttributeInt32Value("total", root));
                this.NotRun.Set(context, GetAttributeInt32Value("not-run", root));
                this.Errors.Set(context, GetAttributeInt32Value("errors", root));
                this.Failures.Set(context, GetAttributeInt32Value("failures", root));
                this.Inconclusive.Set(context, GetAttributeInt32Value("inconclusive", root));
                this.Ignored.Set(context, GetAttributeInt32Value("ignored", root));
                this.Skipped.Set(context, GetAttributeInt32Value("skipped", root));
                this.Invalid.Set(context, GetAttributeInt32Value("invalid", root));

                if (this.Errors.Get(context) > 0 || this.Failures.Get(context) > 0)
                {
                    var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                    buildDetail.Status = BuildStatus.PartiallySucceeded;
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
