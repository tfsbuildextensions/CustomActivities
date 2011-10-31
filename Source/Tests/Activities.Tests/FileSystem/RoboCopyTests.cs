//-----------------------------------------------------------------------
// <copyright file="RoboCopyTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.FileSystem;

    /// <summary>
    /// This is a test class
    /// </summary>
    [TestClass]
    public class RoboCopyTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A test
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void RoboCopyTest()
        {
            if (Directory.Exists(@"C:\a destination"))
            {
                Directory.Delete(@"C:\a destination");    
            }

            // Initialise Instance
            var target = new TfsBuildExtensions.Activities.FileSystem.RoboCopy { Action = RoboCopyAction.Copy, Source = @"C:\a source", Destination = @"C:\a destination", Options = "/E" };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            invoker.Invoke();

            Assert.IsTrue(Directory.GetFiles(@"C:\a destination").Length > 0);
        }
    }
}
