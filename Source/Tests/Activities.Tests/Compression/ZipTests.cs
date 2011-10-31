//-----------------------------------------------------------------------
// <copyright file="ZipTests.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Tests
{
    using System.Activities;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TfsBuildExtensions.Activities.Compression;

    /// <summary>
    /// ZipTests
    /// </summary>
    [TestClass]
    public class ZipTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Zip Files by Path
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ZipPathTest()
        {
            // Initialise Instance
            var target = new Zip { Action = ZipAction.Create, CompressPath = @"D:\Projects\teambuild2010contrib\MAIN\Source\Activities.Tests\Compression\TestFiles", ZipFileName = @"D:\a\newZipByPath.zip" };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            WorkflowInvoker invoker = new WorkflowInvoker(target);
            var actual = invoker.Invoke();

            // Test the result
            Assert.IsTrue(System.IO.File.Exists(@"d:\a\newZipByPath.zip"));
        }

        /// <summary>
        /// Zip files by collection
        /// </summary>
        [TestMethod]
        [DeploymentItem("TfsBuildExtensions.Activities.dll")]
        public void ZipFilesTest()
        {
            // Initialise Instance
            var target = new Zip { Action = ZipAction.Create, ZipFileName = @"D:\a\newZipByFiles.zip" };
            
            // Declare additional parameters
            var parameters = new Dictionary<string, object>
            {
                { "Files", new[] { @"D:\Projects\teambuild2010contrib\MAIN\Source\Activities.Tests\Compression\TestFiles\TestFiles (1).txt" } },
            };

            // Create a WorkflowInvoker and add the IBuildDetail Extension
            // Invoke the Workflow
            WorkflowInvoker.Invoke(target, parameters);

            // Test the result
            Assert.IsTrue(System.IO.File.Exists(@"d:\a\newZipByFiles.zip"));
        }
    }
}
