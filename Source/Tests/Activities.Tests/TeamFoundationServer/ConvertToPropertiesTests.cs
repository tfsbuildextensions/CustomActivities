//-----------------------------------------------------------------------
// <copyright file="HelloTest.cs"> Copy as you feel fit! </copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities;
    using TfsBuildExtensions.Activities.TeamFoundationServer;

    [TestClass]
    public class ConvertToPropertiesTests
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A test for ensuring that msbuild properties will be broken out by key/value as passed.
        /// It also verifies that later properties will take precendence over prior properties.
        /// Please note that it does not validate that the value is handled by the msbuild engine properly.
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertMSBuildToProperties()
        {
            // Initialize Instance
            var target = new ConvertToProperties { Properties = "/p:Property2=Init /p:Property1=\"V a l u e 1\" /p:Property2=~!@#$%^&*()_=+`-", InputType = PropertiesType.MSBuild, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            string property1Value="";
            Assert.IsTrue(actual.TryGetValue("Property1", out property1Value));
            Assert.AreEqual(property1Value, "\"V a l u e 1\"");
            Assert.IsTrue(actual.TryGetValue("Property2", out property1Value));
            Assert.AreEqual(property1Value, "~!@#$%^&*()_=+`-");
        }


        /// <summary>
        /// A test for ensuring that ntshell properties will be broken out by key/value as passed.
        /// Ntshell parameters must retain key names in some way (i.e. key1 value1 key2 value2)
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertNtshellToProperties()
        {
            // Initialize Instance
            var target = new ConvertToProperties { Properties = "Property1 \"V a l u e 1\" Property2 ~!@#$%^&*()_=+`-", InputType = PropertiesType.NTShell, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            string property1Value = "";
            Assert.IsTrue(actual.TryGetValue("Property1", out property1Value));
            Assert.AreEqual(property1Value, "\"V a l u e 1\"");
            Assert.IsTrue(actual.TryGetValue("Property2", out property1Value));
            Assert.AreEqual(property1Value, "~!@#$%^&*()_=+`-");
        }


        /// <summary>
        /// A test for ensuring that ntshell properties will be broken out by key/value as passed.
        /// Ntshell parameters must retain key names in some way (i.e. key1 value1 key2 value2)
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ConvertPowershellToProperties()
        {
            // Initialize Instance
            var target = new ConvertToProperties { Properties = "-Property1 \"V a l u e 1\" -Property2 ~!@#$%^&*()_=+`-", InputType = PropertiesType.PowerShell, FailBuildOnError = true, IgnoreExceptions = false, TreatWarningsAsErrors = true, LogExceptionStack = true };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            string property1Value = "";
            Assert.IsTrue(actual.TryGetValue("Property1", out property1Value));
            Assert.AreEqual(property1Value, "\"V a l u e 1\"");
            Assert.IsTrue(actual.TryGetValue("Property2", out property1Value));
            Assert.AreEqual(property1Value, "~!@#$%^&*()_=+`-");
        }
    }
}
