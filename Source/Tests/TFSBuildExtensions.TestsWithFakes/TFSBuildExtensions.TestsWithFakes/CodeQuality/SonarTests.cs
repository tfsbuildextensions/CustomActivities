using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TfsBuildExtensions.Activities.CodeQuality;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using TfsBuildExtensions.Activities;
using Microsoft.TeamFoundation.Build.Client.Fakes;
using Microsoft.TeamFoundation.VersionControl.Client.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TfsBuildExtensions.TestsWithFakes.CodeQuality
{
    /// <summary>
    /// Units tests for the Sonar activity.
    /// All tests use the workflow invoker with fake a Workspace
    /// </summary>
    [TestClass]
    public class SonarTests
    {
        private IDisposable shimsContext;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            String defaultPropertiesFile = Path.Combine(TestContext.TestDeploymentDir, "sonar-project.properties");
            if (File.Exists(defaultPropertiesFile))
            {
                File.Delete(defaultPropertiesFile);
            }

            this.shimsContext = ShimsContext.Create();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (this.shimsContext != null)
            {
                shimsContext.Dispose();
            }
        }

        private String invokeWorkflow(IBuildDetail BuildDetail,
                   String SonarRunnerPath = null,
                   String LocalSolutionPath = null,
                   bool GeneratePropertiesIfMissing = false,
                   String TemplatePropertiesPath = null,
                   bool FailBuildOnError = true,
                   bool FailBuildOnAlert = false,
                   String RunnerCmdFile = "fake-runner.cmd",
                   StringList SonarProperties = null,
                   String BinariesDirectory = "BinDir")
        {

            // Default values that work
            if (SonarRunnerPath == null) SonarRunnerPath = Path.Combine(TestContext.DeploymentDirectory, RunnerCmdFile);
            if (LocalSolutionPath == null) LocalSolutionPath = Path.Combine(TestContext.DeploymentDirectory, "Dummy.sln");
            if (TemplatePropertiesPath == null) TemplatePropertiesPath = Path.Combine(TestContext.DeploymentDirectory, "sonar-properties.template");

            ShimWorkspace workpace = new ShimWorkspace()
            {
                GetLocalItemForServerItemString = (s) => LocalSolutionPath
            };

            // constants (literals)
            var activity = new Sonar
            {
                SonarRunnerPath = SonarRunnerPath,
                FailBuildOnError = FailBuildOnError,
                GeneratePropertiesIfMissing = GeneratePropertiesIfMissing,
                SonarPropertiesTemplatePath = TemplatePropertiesPath,
                FailBuildOnAlert = FailBuildOnAlert,
                BinariesDirectory = BinariesDirectory
            };


            // object variables
            var parameters = new Dictionary<string, object>
            {
                { "BuildWorkspace", workpace.Instance },
                { "ProjectsToAnalyze", new StringList("dummy.sln") }
            };
            if (SonarProperties == null)
                SonarProperties = new StringList();
            if (SonarProperties != null) parameters.Add("SonarProperties", SonarProperties);

            var workflowLogger = new BuildMessageTrackingParticipant();
            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(activity);
            invoker.Extensions.Add(BuildDetail);
            invoker.Extensions.Add(workflowLogger);
            invoker.Invoke(parameters);
            return workflowLogger.ToString();
        }

        private IBuildDetail generateWorkingBuildDetailStubs()
        {
            // provide build details stub for build failure
            var buildDefinition = new StubIBuildDefinition
            {
                NameGet = () => "My Dummy Build"
            };

            var buildDetail = new StubIBuildDetail
            {
                BuildNumberGet = () => "My Dummy Build_20130612.4",
                BuildDefinitionGet = () => buildDefinition,
                LogLocationGet = () => Path.Combine(TestContext.TestDeploymentDir, "build.log")
            };

            return buildDetail;
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        public void SonarActivity_BadWorkingDirectory_ShouldFail()
        {
                BuildStatus usedBuildStatus = BuildStatus.None;

                // provide build details stub for build failure
                var buildDetail = new StubIBuildDetail
                {
                    StatusSetBuildStatus = (status) => usedBuildStatus = status
                };

                try
                {
                    invokeWorkflow(buildDetail,
                        SonarRunnerPath: @"fake-runner.cmd",
                        LocalSolutionPath: @"C:\IDontExist\NoIDont.sln");
                }
                catch (FailingBuildException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("C:\\IDontExist"), "Missing directory path in exception message");
                    Assert.IsTrue(ex.Message.Contains("not found"), "Missing \"not found\" in the exception message");
                }
                Assert.AreEqual(BuildStatus.Failed, usedBuildStatus, "Build status should have been set to Failed");
        }

        [TestMethod]
        public void SonarActivity_WrongSonnarRunnerPath_ShouldFail()
        {
            BuildStatus usedBuildStatus = BuildStatus.None;
            // provide build details stub for build failure
            var buildDetail = new StubIBuildDetail
            {
                StatusSetBuildStatus = (status) => usedBuildStatus = status
            };

            string badRunnerPath = @"C:\BadSonarRunnerPath\BadBad.cmd";
            // this file exists, and its parent folder too
            string currentAssemblyPath = Assembly.GetExecutingAssembly().GetName().FullName;
            try
            {
                invokeWorkflow(buildDetail,
                    SonarRunnerPath: badRunnerPath,
                    LocalSolutionPath: currentAssemblyPath);
            }
            catch (FailingBuildException ex)
            {
                Assert.IsTrue(ex.Message.Contains("not found"), "Exception message should contain \"not found\"");
                Assert.IsTrue(ex.Message.Contains(badRunnerPath), "Exception message should contain the file name");
            }
            Assert.AreEqual(BuildStatus.Failed, usedBuildStatus, "Build status should have been set to Failed");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        public void SonarActivity_WithFakeRunner_ShouldPassAndLog()
        {
            // provide build details stub for build failure
            var buildDetail = generateWorkingBuildDetailStubs();

            invokeWorkflow(buildDetail);

            String logFile = Path.Combine(TestContext.TestDeploymentDir, "Sonar.log");
            Assert.IsTrue(File.Exists(logFile), logFile + " should have been created.");
            Assert.IsTrue(File.ReadAllText(logFile).Contains("I'm the fake Sonar runner"), "The log file content is invalid.");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\sonar-properties.template")]
        public void SonarActivity_WhenMissingPropertiesFile_ShouldGenerateTheFile()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            invokeWorkflow(buildDetail,
                GeneratePropertiesIfMissing: true);

            String propertiesFile = Path.Combine(TestContext.TestDeploymentDir, "sonar-project.properties");
            Assert.IsTrue(File.Exists(propertiesFile), propertiesFile + " should have been created.");
            Assert.IsTrue(!String.IsNullOrWhiteSpace(File.ReadAllText(propertiesFile)), "properties file must not be empty");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner-with-alerts.cmd")]
        public void SonarActivity_SonarRaisesAlertsAndAlertsFailTheBuild_ShouldFailTheBuild()
        {
            BuildStatus usedBuildStatus = BuildStatus.None;
            // provide build details stub for build failure
            var buildDetail = new StubIBuildDetail
            {
                StatusSetBuildStatus = (status) => usedBuildStatus = status
            };

            try
            {
                invokeWorkflow(buildDetail,
                    RunnerCmdFile: "fake-runner-with-alerts.cmd",
                    GeneratePropertiesIfMissing: false,
                    FailBuildOnAlert: true);
            }
            catch (FailingBuildException)
            {
                // ignore any FailingBuildExceptions here
            }
            catch (Exception)
            {
                // other exceptions types should fail the test
                throw;
            }

            Assert.AreEqual(BuildStatus.Failed, usedBuildStatus, "Build status should have been set to Failed");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner-that-fails.cmd")]
        public void SonarActivity_SonarAnalysisReturnsError_ShouldLogAnError()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            var result = invokeWorkflow(buildDetail,
                GeneratePropertiesIfMissing: false,
                FailBuildOnError: false,
                RunnerCmdFile: "fake-runner-that-fails.cmd");

            Assert.IsTrue(result.Contains("Sonar analysis has failed"), "The error message should have been logged");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner-with-alerts.cmd")]
        public void SonarActivity_SonarRaisesAlerts_ShouldDisplayInBuildLog()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            var result = invokeWorkflow(buildDetail,
                GeneratePropertiesIfMissing: false,
                FailBuildOnError: false,
                RunnerCmdFile: "fake-runner-with-alerts.cmd");

            Assert.IsTrue(result.Contains("Sonar analysis has raised critical alerts"), "The error message should have been logged");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\all-properties.template")]
        public void SonarActivity_WhenGeneratingProperties_ShouldGenerateProperVariables()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            string localSolutionPath = Path.Combine(TestContext.DeploymentDirectory, "MyDummySolution.sln");
            invokeWorkflow(buildDetail,
                LocalSolutionPath: localSolutionPath,
                TemplatePropertiesPath: Path.Combine(TestContext.DeploymentDirectory, "all-properties.template"),
                GeneratePropertiesIfMissing: true,
                BinariesDirectory: @"C:\Build\30\My Project\My Build Def\Binaries\");

            String content = File.ReadAllText(Path.Combine(TestContext.TestDeploymentDir, "sonar-project.properties"));
            Assert.IsTrue(content.Contains("BUILD_DEFINITION = My Dummy Build\r\n"), "Wrong BUILD_DEFINITION in properties file");
            Assert.IsTrue(content.Contains("BUILD_DEFINITION_UNDERSCORE = My_Dummy_Build\r\n"), "Wrong BUILD_DEFINITION_UNDERSCORE in properties file");
            Assert.IsTrue(content.Contains("BUILD_NUMBER = My Dummy Build_20130612.4\r\n"), "Wrong BUILD_NUMBER in properties file");
            Assert.IsTrue(content.Contains("BUILD_NUMBER_UNDERSCORE = My_Dummy_Build_20130612.4\r\n"), "Wrong BUILD_NUMBER_UNDERSCORE in properties file");
            Assert.IsTrue(content.Contains("BUILD_NUMBER_DEFAULT_SUFFIX = 20130612.4\r\n"), "Wrong BUILD_NUMBER_DEFAULT_SUFFIX in properties file");
            Assert.IsTrue(content.Contains("SOLUTION_FILE = MyDummySolution.sln\r\n"), "Wrong SOLUTION_FILE in properties file");
            Assert.IsTrue(content.Contains("SOLUTION_FILE_PATH = " + localSolutionPath + "\r\n"), "Wrong SOLUTION_FILE_PATH in properties file");
            Assert.IsTrue(content.Contains("SOLUTION_DIRECTORY_PATH = " + TestContext.DeploymentDirectory + "\r\n"), "Wrong SOLUTION_DIRECTORY_PATH in properties file");
            Assert.IsTrue(content.Contains("BINARIES_DIRECTORY_PATH = C:\\Build\\30\\My Project\\My Build Def\\Binaries\r\n"), "Wrong BINARIES_DIRECTORY_PATH in properties file");
            Assert.IsTrue(content.Contains("BINARIES_DIRECTORY_PATH_SLASH = C:/Build/30/My Project/My Build Def/Binaries\r\n"), "Wrong BINARIES_DIRECTORY_PATH_SLASH in properties file");
        }



        private static void AssertContainsOnce(List<KeyValuePair<string, string>> content, string name, string value)
        {
            int nbFound = content.Where(x => x.Key == name).Count();

            if (nbFound == 0) Assert.Fail("Result file must contain the following line: " + name);
            if (nbFound >= 2) Assert.Fail("Result file must contain " + name + " only once");
        }

        private static List<KeyValuePair<string, string>> ParseContent(string content)
        {
            var result = new List<KeyValuePair<string, string>>();
            Regex r = new Regex("^[\\s]*(?<name>[^#=]+)=(?<value>.*)", RegexOptions.Multiline);
            foreach (System.Text.RegularExpressions.Match m in r.Matches(content))
            {
                result.Add(new KeyValuePair<string, string>(m.Groups["name"].Value.Trim(), m.Groups["value"].Value.Trim()));
            }
            return result;
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\custom-properties.template")]
        public void SonarActivity_WhenGeneratingProperties_AddsPropertiesToFile()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            // theses properties are missing in the template
            StringList sonarProperties = new StringList
            {
                "sonar.fxcop.installDirectory=D:/Sonar.Net/Microsoft Fxcop 10.0",
                "sonar.fxcop.assemblyDependencyDirectories = $(SolutionDir)/../Binaries",
                "sonar.sources=."
            };

            string localSolutionPath = Path.Combine(TestContext.DeploymentDirectory, "MyDummySolution.sln");
            invokeWorkflow(buildDetail,
                LocalSolutionPath: localSolutionPath,
                TemplatePropertiesPath: Path.Combine(TestContext.DeploymentDirectory, "custom-properties.template"),
                GeneratePropertiesIfMissing: true,
                SonarProperties: sonarProperties);

            String content = File.ReadAllText(Path.Combine(TestContext.TestDeploymentDir, "sonar-project.properties"));
            Assert.IsTrue(content.StartsWith("# Project identification"), "Result file wrong beginning");
            var parsedContent = ParseContent(content);
            // the properties should be found in the result file
            AssertContainsOnce(parsedContent, "sonar.projectVersion", "3.2.1");
            AssertContainsOnce(parsedContent, "sonar.fxcop.installDirectory", "D:/Sonar.Net/Microsoft Fxcop 10.0");
            AssertContainsOnce(parsedContent, "sonar.fxcop.assemblyDependencyDirectories", "$(SolutionDir)/../Binaries");
            AssertContainsOnce(parsedContent, "sonar.sources", ".");
        }

        [TestMethod]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\fake-runner.cmd")]
        [DeploymentItem(@"Tests\TFSBuildExtensions.TestsWithFakes\TFSBuildExtensions.TestsWithFakes\CodeQuality\Deploy\custom-properties.template")]
        public void SonarActivity_WhenGeneratingProperties_OverwritesPropertiesToFileWhenFound()
        {
            var buildDetail = generateWorkingBuildDetailStubs();

            // these properties exist already in the template file
            StringList sonarProperties = new StringList
            {
                "sonar.gallio.coverage.tool=Dot Cover 8.2.1",
                "sonar.opencover.installDirectory  =  C:/Here",
            };

            string localSolutionPath = Path.Combine(TestContext.DeploymentDirectory, "MyDummySolution.sln");
            invokeWorkflow(buildDetail,
                LocalSolutionPath: localSolutionPath,
                TemplatePropertiesPath: Path.Combine(TestContext.DeploymentDirectory, "custom-properties.template"),
                GeneratePropertiesIfMissing: true,
                SonarProperties: sonarProperties);

            String content = File.ReadAllText(Path.Combine(TestContext.TestDeploymentDir, "sonar-project.properties"));
            Assert.IsTrue(content.StartsWith("# Project identification"), "Result file wrong beginning");
            var parsedContent = ParseContent(content);
            AssertContainsOnce(parsedContent, "sonar.gallio.coverage.tool", "Dot Cover 8.2.1");
            AssertContainsOnce(parsedContent, "sonar.opencover.installDirectory", "C:/Here");
            // these ones must be left unchanged
            AssertContainsOnce(parsedContent, "sonar.projectVersion", "3.2.1");
            AssertContainsOnce(parsedContent, "sonar.gallio.runner", "IsolatedProcess");
        }
    }
}
