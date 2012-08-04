//-----------------------------------------------------------------------
// <copyright file="HelloTest.cs"> Copy as you feel fit! </copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities;
    using TfsBuildExtensions.Activities.TeamFoundationServer;

    [TestClass]
    public class ConvertFromPropertiesTests
    {
        public TestContext TestContext { get; set; }
        Dictionary<string, string> properties = new Dictionary<string, string>();

        [TestInitialize]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void InitializeEveryTest()
        {
            properties.Add("Property1", "\"V a l u e 1\"");
            properties.Add("Property2", "~!@#$%^&*()_=+`-");
        }

        /// <summary>
        /// A test for ensuring that msbuild properties will be broken out by key/value as passed.
        /// It also verifies that later properties will take precendence over prior properties.
        /// Please note that it does not validate that the value is handled by the msbuild engine properly.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertMSBuildFromProperties()
        {
            // Initialize Instance
            var target = new ConvertFromProperties { Properties = new InArgument<Dictionary<string, string>>((env) => this.properties), OutputType = PropertiesType.MSBuild, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            Assert.AreEqual(actual, "/p:Property1=\"V a l u e 1\" /p:Property2=~!@#$%^&*()_=+`-");
        }


        /// <summary>
        /// A test for ensuring that ntshell properties will be broken out by key/value as passed.
        /// Ntshell parameters must retain key names in some way (i.e. key1 value1 key2 value2)
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertNtshellFromProperties()
        {
            // Initialize Instance
            var target = new ConvertFromProperties { Properties = new InArgument<Dictionary<string, string>>((env) => this.properties), OutputType = PropertiesType.NTShell, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            Assert.AreEqual(actual, "Property1 \"V a l u e 1\" Property2 ~!@#$%^&*()_=+`-");
        }


        /// <summary>
        /// A test for ensuring that ntshell properties will be broken out by key/value as passed.
        /// Ntshell parameters must retain key names in some way (i.e. key1 value1 key2 value2)
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertPowershellFromProperties()
        {
            // Initialize Instance
            var target = new ConvertFromProperties { Properties = new InArgument<Dictionary<string, string>>((env) => this.properties), OutputType = PropertiesType.PowerShell, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            Assert.AreEqual(actual, "-Property1 \"V a l u e 1\" -Property2 ~!@#$%^&*()_=+`-");
        }
    }
}
