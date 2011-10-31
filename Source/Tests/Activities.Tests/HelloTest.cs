//-----------------------------------------------------------------------
// <copyright file="HelloTest.cs"> Copy as you feel fit! </copyright>
//-----------------------------------------------------------------------
namespace CodeActivitySamples
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Tests;

    [TestClass]
    public class HelloTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ExecuteTest()
        {
            // Initialise Instance
            var target = new Hello { Message = "World" };

            // Invoke the Workflow
            var actual = WorkflowInvoker.Invoke(target);

            // Test the result
            Assert.AreEqual("Hello World", actual);
        }

        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ExecuteTest2()
        {
            // Initialise Instance
            var target = new Hello { Message = "World" };

            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Message2", " hope you are well" },
            };

            // Create the Workflow object
            WorkflowInvoker invoker = new WorkflowInvoker(target);

            // Create a Mock build detail object
            IBuildDetail t = new MockIBuildDetail { BuildNumber = "MyBuildNumber" };
            invoker.Extensions.Add(t);

            // Invoke the workflow
            var actual = invoker.Invoke(parameters);

            // Test the result wich is now accessed via the named Result key
            Assert.AreEqual("Hello World hope you are well from MyBuildNumber", actual["Result"]);
        }
    }
}
