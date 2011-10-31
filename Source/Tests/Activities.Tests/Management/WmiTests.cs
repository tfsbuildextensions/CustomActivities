//-----------------------------------------------------------------------
// <copyright file="WmiTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Management;

    /// <summary>
    /// WmiTests
    /// </summary>
    [TestClass]
    public class WmiTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A test for ExecuteScalar
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void WmiTest()
        {
            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.Management.Wmi()
                {
                    Action = WmiAction.Execute,
                    Class = "Win32_Process",
                    Method = "Create",
                    Namespace = @"\root\CIMV2"
                };
                
                // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "MethodParameters", new[] { "CommandLine#~#calc.exe" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke(parameters);

            // Assert
            Assert.IsTrue(actual["ReturnValue"].ToString() == "0");
        }
    }
}
